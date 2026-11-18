using System.Reflection;
using System.Runtime.Loader;
using System.Text.Encodings.Web;
using System.Text.Json;

// Usage: SuiteManifestGenerator <settings file>
// The settings file (written by AutomationTests\Directory.Build.targets) has one key=value per line:
// Assembly, Output, TestSuiteName, Version, Description, Author.
// Reads NUnit attributes from the built test DLL without running anything and writes
// suite-manifest.json. The file is only rewritten when its content changes.

if (args.Length != 1 || !File.Exists(args[0]))
{
    Console.Error.WriteLine("SuiteManifestGenerator: pass the path of a settings file.");
    return 1;
}

var settings = File.ReadAllLines(args[0])
    .Where(l => l.Contains('='))
    .Select(l => l.Split('=', 2))
    .ToDictionary(p => p[0].Trim(), p => p[1].Trim(), StringComparer.OrdinalIgnoreCase);

string Setting(string name) => settings.TryGetValue(name, out var v) ? v : "";

var assemblyPath = Path.GetFullPath(Setting("Assembly"));
var outputPath = Path.GetFullPath(Setting("Output"));
if (!File.Exists(assemblyPath))
{
    Console.Error.WriteLine($"SuiteManifestGenerator: assembly not found: {assemblyPath}");
    return 1;
}

var assemblyDir = Path.GetDirectoryName(assemblyPath)!;
var context = new AssemblyLoadContext("SuiteManifest", isCollectible: true);
context.Resolving += (ctx, name) =>
{
    var candidate = Path.Combine(assemblyDir, name.Name + ".dll");
    return File.Exists(candidate) ? ctx.LoadFromAssemblyPath(candidate) : null;
};

try
{
    var assembly = context.LoadFromAssemblyPath(assemblyPath);
    var testCases = new List<TestCaseEntry>();
    var namespaces = new List<string>();

    foreach (var type in LoadableTypes(assembly).Where(t => t.IsClass && !t.IsAbstract).OrderBy(t => t.FullName, StringComparer.Ordinal))
    {
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            .Where(IsTestMethod)
            .OrderBy(m => Order(m) ?? int.MaxValue)
            .ThenBy(m => m.MetadataToken)
            .ToList();
        if (methods.Count == 0) continue;

        if (!string.IsNullOrEmpty(type.Namespace)) namespaces.Add(type.Namespace);

        foreach (var method in methods)
        {
            var properties = Properties(method);
            testCases.Add(new TestCaseEntry
            {
                TestCaseId = properties.GetValueOrDefault("TestCaseId") ?? method.Name,
                TestCaseName = method.Name,
                TestCaseDescription = properties.GetValueOrDefault("Description") ?? DescriptionAttribute(method) ?? "",
                EntryClass = type.FullName ?? type.Name,
                EntryMethod = method.Name,
                DisplayOrder = testCases.Count + 1,
                EstimatedDurationSeconds = DurationSeconds(method),
                TestCaseType = TestCaseType(method),
            });
        }
    }

    var manifest = new SuiteManifest
    {
        TestSuiteName = Setting("TestSuiteName"),
        Version = Setting("Version"),
        Description = Setting("Description"),
        EntryAssembly = Path.GetFileName(assemblyPath),
        EntryType = namespaces.GroupBy(n => n).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault() ?? "",
        Author = Setting("Author"),
        TestCases = testCases,
    };

    var json = JsonSerializer.Serialize(manifest, new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    }) + Environment.NewLine;

    if (!File.Exists(outputPath) || File.ReadAllText(outputPath) != json)
        File.WriteAllText(outputPath, json);

    Console.WriteLine($"Suite manifest: {outputPath} ({testCases.Count} test cases)");
    return 0;
}
finally
{
    context.Unload();
}

static IEnumerable<Type> LoadableTypes(Assembly assembly)
{
    try { return assembly.GetTypes(); }
    catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t != null)!; }
}

static IEnumerable<CustomAttributeData> Attributes(MemberInfo member)
{
    try { return member.GetCustomAttributesData(); }
    catch (Exception) { return Array.Empty<CustomAttributeData>(); }
}

static bool IsTestMethod(MethodInfo method) =>
    Attributes(method).Any(a => a.AttributeType.FullName is "NUnit.Framework.TestAttribute"
        or "NUnit.Framework.TestCaseAttribute" or "NUnit.Framework.TestCaseSourceAttribute"
        or "NUnit.Framework.TheoryAttribute");

static Dictionary<string, string> Properties(MethodInfo method)
{
    var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    foreach (var attribute in Attributes(method).Where(a => a.AttributeType.FullName == "NUnit.Framework.PropertyAttribute"))
    {
        if (attribute.ConstructorArguments.Count == 2 && attribute.ConstructorArguments[0].Value is string name)
            result[name] = attribute.ConstructorArguments[1].Value?.ToString() ?? "";
    }
    return result;
}

static string? DescriptionAttribute(MethodInfo method) =>
    Attributes(method).FirstOrDefault(a => a.AttributeType.FullName == "NUnit.Framework.DescriptionAttribute")
        ?.ConstructorArguments.FirstOrDefault().Value as string;

static int? Order(MethodInfo method) =>
    Attributes(method).FirstOrDefault(a => a.AttributeType.FullName == "NUnit.Framework.OrderAttribute")
        ?.ConstructorArguments.FirstOrDefault().Value as int?;

static int? DurationSeconds(MethodInfo method)
{
    var timeout = Attributes(method).FirstOrDefault(a => a.AttributeType.FullName is "NUnit.Framework.CancelAfterAttribute" or "NUnit.Framework.TimeoutAttribute");
    return timeout?.ConstructorArguments.FirstOrDefault().Value is int milliseconds ? milliseconds / 1000 : null;
}

static string TestCaseType(MethodInfo method)
{
    var category = Attributes(method).FirstOrDefault(a => a.AttributeType.FullName == "NUnit.Framework.CategoryAttribute")
        ?.ConstructorArguments.FirstOrDefault().Value as string;
    if (string.IsNullOrWhiteSpace(category)) return "";
    return category.EndsWith("Tests", StringComparison.OrdinalIgnoreCase) && category.Length > 5 ? category[..^5] : category;
}

class SuiteManifest
{
    public string TestSuiteName { get; set; } = "";
    public string Version { get; set; } = "";
    public string Description { get; set; } = "";
    public string EntryAssembly { get; set; } = "";
    public string EntryType { get; set; } = "";
    public string Author { get; set; } = "";
    public List<TestCaseEntry> TestCases { get; set; } = new();
}

class TestCaseEntry
{
    public string TestCaseId { get; set; } = "";
    public string TestCaseName { get; set; } = "";
    public string TestCaseDescription { get; set; } = "";
    public string EntryClass { get; set; } = "";
    public string EntryMethod { get; set; } = "";
    public int DisplayOrder { get; set; }
    public int? EstimatedDurationSeconds { get; set; }
    public string TestCaseType { get; set; } = "";
}
