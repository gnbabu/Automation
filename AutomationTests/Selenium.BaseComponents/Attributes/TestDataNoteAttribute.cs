using System.Text.Json;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Selenium.BaseComponents.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class TestDataNoteAttribute : NUnitAttribute, IApplyToTest
    {
        public const string PropertyName = "TestDataNote";

        public TestDataNoteAttribute(string message)
            : this(null, message)
        {
        }

        public TestDataNoteAttribute(string section, string message)
        {
            Section = section;
            Message = message;
        }

        public string Section { get; }
        public string Message { get; }
        public string Field { get; set; }

        public void ApplyToTest(Test test)
        {
            var value = JsonSerializer.Serialize(new { section = Section, field = Field, message = Message });
            test.Properties.Add(PropertyName, value);
        }
    }
}
