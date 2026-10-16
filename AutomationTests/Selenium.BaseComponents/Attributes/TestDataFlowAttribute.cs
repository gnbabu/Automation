using NUnit.Framework;

namespace Selenium.BaseComponents.Attributes
{
    // Declares which test-data model (e.g. TestDataModels.SubmitPAProfessional) a test
    // fixture or test method reads its data from. Deriving from NUnit's PropertyAttribute
    // makes it show up in NUnit's Explore() output as property "TestDataFlow", which the
    // Automation API's test discovery already reads - that's how the Test Data
    // Management screen knows which model's sections/fields to show for a test case.
    // Taking a Type (not a string) keeps it compile-time safe: renaming the model class
    // updates every usage.
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
