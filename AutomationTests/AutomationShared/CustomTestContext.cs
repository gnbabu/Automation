using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationShared
{
    public static class CustomTestContext
    {
        private static readonly Dictionary<string, object?> _parameters = new();

        public static void Set(string key, object? value)
        {
            _parameters[key] = value;
        }

        public static object? Get(string key)
        {
            return _parameters.TryGetValue(key, out var value) ? value : null;
        }

        public static IReadOnlyDictionary<string, object?> Parameters => _parameters;
    }
}
