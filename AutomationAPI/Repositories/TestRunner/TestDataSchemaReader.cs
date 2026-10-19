using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Loader;
using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories.TestRunner
{
    public static class TestDataSchemaReader
    {
        private static readonly ConcurrentDictionary<(string Path, DateTime LastWrite, string Type), TestDataFlowSchema> _cache = new();

        public static TestDataFlowSchema Read(string releaseFolderPath, string testDataFlow)
        {
            var (typeName, assemblyName) = ParseTestDataFlow(testDataFlow);
            var dllPath = Path.Combine(releaseFolderPath, assemblyName + ".dll");
            if (!File.Exists(dllPath))
            {
                throw new FileNotFoundException(
                    $"{assemblyName}.dll was not found in the release folder ({releaseFolderPath}). " +
                    "Copy it there along with the test DLLs.", dllPath);
            }

            var key = (dllPath, File.GetLastWriteTimeUtc(dllPath), typeName);
            return _cache.GetOrAdd(key, _ => Load(releaseFolderPath, dllPath, typeName));
        }

        public static (string TypeName, string AssemblyName) ParseTestDataFlow(string testDataFlow)
        {
            var parts = (testDataFlow ?? "").Split(',', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2 || parts[0].Length == 0 || parts[1].Length == 0)
                throw new ArgumentException($"Invalid TestDataFlow value '{testDataFlow}'. Expected 'Namespace.Type, Assembly'.");
            return (parts[0], parts[1]);
        }

        public static string FlowNameOf(string testDataFlow)
        {
            var typeName = ParseTestDataFlow(testDataFlow).TypeName;
            return typeName.Substring(typeName.LastIndexOf('.') + 1);
        }

        private static TestDataFlowSchema Load(string releaseFolderPath, string dllPath, string typeName)
        {
            var context = new FolderLoadContext(releaseFolderPath);
            try
            {
                var assembly = context.LoadFromBytes(dllPath);
                var modelType = assembly.GetType(typeName, throwOnError: false)
                    ?? throw new TypeLoadException($"Type '{typeName}' was not found in {Path.GetFileName(dllPath)}.");
                return BuildSchema(modelType);
            }
            finally
            {
                context.Unload();
            }
        }

        private static TestDataFlowSchema BuildSchema(Type modelType)
        {
            var schema = new TestDataFlowSchema { FlowName = modelType.Name, ModelType = modelType.FullName ?? modelType.Name };
            var rootFields = new List<TestDataFieldSchema>();

            object? defaults = null;
            try
            {
                defaults = Activator.CreateInstance(modelType);
            }
            catch (Exception ex)
            {
                schema.Warnings.Add($"Default values couldn't be read ({ex.GetBaseException().Message}).");
            }

            foreach (var property in OrderedProperties(modelType))
            {
                if (IsScalar(property.PropertyType))
                {
                    rootFields.Add(ToField(property));
                    continue;
                }

                var listItemType = GetListItemType(property.PropertyType);
                var sectionType = listItemType ?? property.PropertyType;
                if (!sectionType.IsClass || sectionType == typeof(string))
                {
                    schema.Warnings.Add($"{property.Name}: lists of simple values aren't supported - skipped.");
                    continue;
                }

                var section = new TestDataSectionSchema { SectionName = property.Name, IsList = listItemType != null };
                var scalarFields = new List<PropertyInfo>();
                foreach (var field in OrderedProperties(sectionType))
                {
                    if (IsScalar(field.PropertyType))
                    {
                        section.Fields.Add(ToField(field));
                        scalarFields.Add(field);
                    }
                    else
                        schema.Warnings.Add($"{property.Name}.{field.Name}: nested objects aren't supported - skipped.");
                }

                var sectionValue = defaults == null ? null : SafeGet(property, defaults);
                if (section.IsList && sectionValue is System.Collections.IEnumerable items)
                {
                    foreach (var item in items)
                        AddDefaultRow(section, scalarFields, item);
                }
                else if (!section.IsList && sectionValue != null)
                {
                    AddDefaultRow(section, scalarFields, sectionValue);
                }

                schema.Sections.Add(section);
            }

            if (rootFields.Count > 0)
            {
                if (schema.Sections.Any(s => s.SectionName == modelType.Name))
                    schema.Warnings.Add($"A property named {modelType.Name} clashes with the section for the model's own fields.");
                else
                {
                    var rootSection = new TestDataSectionSchema { SectionName = modelType.Name, Fields = rootFields };
                    if (defaults != null)
                        AddDefaultRow(rootSection, OrderedProperties(modelType).Where(p => IsScalar(p.PropertyType)).ToList(), defaults);
                    schema.Sections.Add(rootSection);
                }
            }

            return schema;
        }

        private static void AddDefaultRow(TestDataSectionSchema section, List<PropertyInfo> fields, object? source)
        {
            if (source == null) return;

            var row = new Dictionary<string, string?>(StringComparer.Ordinal);
            foreach (var field in fields)
            {
                var text = FormatDefault(SafeGet(field, source));
                if (!string.IsNullOrEmpty(text))
                    row[field.Name] = text;
            }

            if (row.Count > 0)
                section.DefaultRows.Add(row);
        }

        private static object? SafeGet(PropertyInfo property, object target)
        {
            try { return property.CanRead ? property.GetValue(target) : null; }
            catch { return null; }
        }

        private static string? FormatDefault(object? value)
        {
            switch (value)
            {
                case null:
                    return null;
                case string s:
                    return s;
                case DateTime dt:
                    if (dt == default) return null;
                    return dt.TimeOfDay == TimeSpan.Zero
                        ? dt.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)
                        : dt.ToString("yyyy-MM-ddTHH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                case bool b:
                    return b ? "true" : null;
                case Enum e:
                    return e.ToString();
                case IFormattable f:
                    var isDefault = value.Equals(Activator.CreateInstance(value.GetType()));
                    return isDefault ? null : f.ToString(null, System.Globalization.CultureInfo.InvariantCulture);
                default:
                    return value.ToString();
            }
        }

        private static IEnumerable<PropertyInfo> OrderedProperties(Type type) =>
            type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.GetIndexParameters().Length == 0)
                .OrderBy(p => p.MetadataToken);

        private static readonly Dictionary<Type, (long Min, long Max)> IntegerRanges = new()
        {
            [typeof(sbyte)] = (sbyte.MinValue, sbyte.MaxValue),
            [typeof(byte)] = (byte.MinValue, byte.MaxValue),
            [typeof(short)] = (short.MinValue, short.MaxValue),
            [typeof(ushort)] = (ushort.MinValue, ushort.MaxValue),
            [typeof(int)] = (int.MinValue, int.MaxValue),
            [typeof(uint)] = (uint.MinValue, uint.MaxValue),
            [typeof(long)] = (long.MinValue, long.MaxValue),
        };

        private static TestDataFieldSchema ToField(PropertyInfo property)
        {
            var underlying = Nullable.GetUnderlyingType(property.PropertyType);
            var t = underlying ?? property.PropertyType;
            var field = new TestDataFieldSchema
            {
                Name = property.Name,
                Nullable = underlying != null || !t.IsValueType,
                Type = t switch
                {
                    _ when t.IsEnum => "enum",
                    _ when t == typeof(bool) => "boolean",
                    _ when t == typeof(DateTime) || t == typeof(DateTimeOffset) => "date",
                    _ when t == typeof(decimal) || t == typeof(double) || t == typeof(float) => "decimal",
                    _ when IntegerRanges.ContainsKey(t) => "integer",
                    _ => "text"
                }
            };
            if (t.IsEnum) field.Options = Enum.GetNames(t).ToList();
            if (IntegerRanges.TryGetValue(t, out var range))
            {
                field.Min = range.Min;
                field.Max = range.Max;
            }
            return field;
        }

        public static bool IsScalar(Type type)
        {
            var t = Nullable.GetUnderlyingType(type) ?? type;
            return t.IsPrimitive || t.IsEnum || t == typeof(string) || t == typeof(decimal)
                || t == typeof(DateTime) || t == typeof(DateTimeOffset) || t == typeof(Guid);
        }

        private static Type? GetListItemType(Type type)
        {
            if (!type.IsGenericType) return null;
            var definition = type.GetGenericTypeDefinition();
            if (definition != typeof(List<>) && definition != typeof(IList<>) && definition != typeof(IEnumerable<>)
                && definition != typeof(ICollection<>) && definition != typeof(IReadOnlyList<>))
                return null;
            return type.GetGenericArguments()[0];
        }

        private sealed class FolderLoadContext : AssemblyLoadContext
        {
            private readonly string _folder;

            public FolderLoadContext(string folder) : base(isCollectible: true) => _folder = folder;

            public Assembly LoadFromBytes(string path)
            {
                using var stream = new MemoryStream(File.ReadAllBytes(path));
                return LoadFromStream(stream);
            }

            protected override Assembly? Load(AssemblyName assemblyName)
            {
                var name = assemblyName.Name ?? "";
                if (name.StartsWith("System", StringComparison.OrdinalIgnoreCase)
                    || name.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase)
                    || name is "mscorlib" or "netstandard")
                    return null;

                var candidate = Path.Combine(_folder, name + ".dll");
                return File.Exists(candidate) ? LoadFromBytes(candidate) : null;
            }
        }
    }
}
