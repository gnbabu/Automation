using NUnit.Framework;

namespace Selenium.BaseComponents.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class TestDataFlowAttribute : PropertyAttribute
    {
        public const string PropertyName = "TestDataFlow";

        public TestDataFlowAttribute(Type modelType)
            : base(PropertyName, $"{modelType.FullName}, {modelType.Assembly.GetName().Name}")
        {
            ModelType = modelType;
        }

        public Type ModelType { get; }
    }
}
