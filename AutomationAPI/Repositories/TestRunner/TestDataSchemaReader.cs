using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Loader;
using AutomationAPI.Repositories.Models;

namespace AutomationAPI.Repositories.TestRunner
{
    // Reads a test-data model's structure (flow -> sections -> fields) from the model
    // assembly deployed in a Release folder. Rules - kept in sync with
    // Selenium.BaseComponents.Utilities.TestData, which binds data back onto the model:
    //   - class-typed property      -> single-row section named after the property
    //   - List<class> property      -> multi-row section named after the property
    //   - scalar property on model  -> field of a section named after the model class
    //   - scalar property on a section class -> field of that section
    //
    // The assembly is read from bytes into a collectible AssemblyLoadContext and unloaded
    // straight away, so (unlike NUnit's Explore) it never locks the DLL file - a release
    // can be redeployed while the API is running. Only type metadata is read; no model
    // code is executed. Results are cached per (file, last-write time, type).
    public static class TestDataSchemaReader
    {
        private static readonly ConcurrentDictionary<(string Path, DateTime LastWrite, string Type), TestDataFlowSchema> _cache = new();

        // testDataFlow is the [TestDataFlow] property value: "Namespace.Type, Assembly".
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

            // Default values = whatever a freshly constructed model holds (property
            // initializers). This is the one place model code runs - plain POCO
            // construction; if it fails, the schema is still returned without defaults.
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

        // Same text format the Portal saves (dates yyyy-MM-dd, invariant numbers).
        // A value type's own default (0, false, DateTime.MinValue) means "not set", not a
        // real default - otherwise every int field would show 0.
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

        // Declaration order (MetadataToken), so the screen matches the class as written.
        private static IEnumerable<PropertyInfo> OrderedProperties(Type type) =>
            type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.GetIndexParameters().Length == 0)
                .OrderBy(p => p.MetadataToken);

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
                    _ when t == typeof(int) || t == typeof(long) || t == typeof(short) || t == typeof(byte) => "integer",
                    _ => "text"
                }
            };
            if (t.IsEnum) field.Options = Enum.GetNames(t).ToList();
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

        // Resolves the model assembly's own dependencies from the same Release folder
        // (also from bytes); framework assemblies fall through to the default context.
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
                // Framework assemblies always come from the default context, even when a
                // full publish output in the folder has its own copies - otherwise
                // typeof(string)/List<> from the model wouldn't match the API's own.
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
