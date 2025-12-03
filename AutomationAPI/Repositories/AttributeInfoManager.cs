using System.Reflection;

namespace AutomationAPI.Repositories
{
    public class LibraryMethodInf
    {
        public string MethodName { get; set; } = string.Empty;
        public bool IsStatic { get; set; }
        public string Signature { get; set; } = string.Empty;
        public IReadOnlyList<AttributeInfo> Attributes { get; set; } = Array.Empty<AttributeInfo>();
    }

    public class AttributeInfo
    {
        public string AttributeTypeName { get; set; } = string.Empty; // FullName
        public string AttributeTypeShortName => AttributeTypeName?.Split('.').Last() ?? AttributeTypeName;
        public object?[] ConstructorArguments { get; set; } = Array.Empty<object?>();
        public IReadOnlyDictionary<string, object?> NamedArguments { get; set; } = new Dictionary<string, object?>();
        public override string ToString()
        {
            var ctor = string.Join(", ", ConstructorArguments.Select(a => a?.ToString() ?? "null"));
            var named = string.Join(", ", NamedArguments.Select(kv => $"{kv.Key}={kv.Value ?? "null"}"));
            return $"{AttributeTypeShortName}({ctor}){{{named}}}";
        }


        public static LibraryMethodInf BuildLibraryMethodInfo(MethodInfo m)
        {
            if (m == null) throw new ArgumentNullException(nameof(m));

            // Build signature (simple human-readable form)
            string BuildSignature(MethodInfo mi)
            {
                var parameters = mi.GetParameters();
                var paramList = string.Join(", ", parameters.Select(p => $"{GetTypeName(p.ParameterType)} {p.Name}"));
                return $"{GetTypeName(mi.ReturnType)} {mi.Name}({paramList})";
            }

            static string GetTypeName(Type t)
            {
                if (t.IsGenericType)
                {
                    var genericName = t.Name.Substring(0, t.Name.IndexOf('`'));
                    var args = string.Join(", ", t.GetGenericArguments().Select(GetTypeName));
                    return $"{genericName}<{args}>";
                }
                return t.Name;
            }

            // Extract CustomAttributeData into AttributeInfo
            AttributeInfo ToAttributeInfo(CustomAttributeData cad)
            {
                var ctorArgs = cad.ConstructorArguments?
                    .Select(FormatTypedArgument)
                    .ToArray() ?? Array.Empty<object?>();

                var namedArgs = cad.NamedArguments?
                    .ToDictionary(na => na.MemberName, na => FormatTypedArgument(na.TypedValue))
                    ?? new Dictionary<string, object?>();

                return new AttributeInfo
                {
                    AttributeTypeName = cad.AttributeType?.FullName ?? cad.AttributeType?.Name ?? string.Empty,
                    ConstructorArguments = ctorArgs,
                    NamedArguments = namedArgs
                };
            }
            static object? FormatTypedArgument(CustomAttributeTypedArgument a)
            {
                if (a.Value == null) return null;

                if (a.ArgumentType != null && a.ArgumentType.IsArray && a.Value is IReadOnlyCollection<CustomAttributeTypedArgument> coll)
                {
                    return coll.Select(FormatTypedArgument).ToArray();
                }


                if (a.ArgumentType != null && a.ArgumentType.IsEnum)
                {
                    return a.Value;
                }
                return a.Value;
            }
            var attrs = CustomAttributeData.GetCustomAttributes(m)
                                                    .Select(ToAttributeInfo)
                                                    .ToList();

            var methodInfo = new LibraryMethodInf
            {
                MethodName = m.Name,
                IsStatic = m.IsStatic,
                Signature = BuildSignature(m),
                Attributes = attrs
            };

            return methodInfo;
        }
    }
}
