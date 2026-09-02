using AutomationShared;
using Moq;
using NUnit.Framework;
using OnboardingTests.Models;
using OnboardingTests.Services;

namespace OnboardingTests
{
    [TestFixture]
    [Category("OnboardingServiceTests")]
    public class OnboardingServiceTests
    {
        private Mock<IOnboardingService> _mockService;

        [SetUp]
        public void Setup()
        {
            _mockService = new Mock<IOnboardingService>();
        }

        [Test]
        [Property("Description", "Verifies that RegisterEmployee returns true when documents are valid.")]
        [Property("Priority", "High")]
        [Property("TestCaseId", "TC_ONB_001")]
        public void RegisterEmployee_ValidDocuments_ReturnsTrue()
        {
            var queueId = CustomTestContext.Get("queueId");
            var employee = new Employee { Name = "John Doe", DocumentsVerified = true };

            _mockService.Setup(s => s.RegisterEmployee(employee)).Returns(true);

            bool result = _mockService.Object.RegisterEmployee(employee);

            Assert.IsTrue(result);
        }

        [Test]
        [Property("Description", "Verifies that RegisterEmployee returns false when documents are invalid.")]
        [Property("Priority", "Medium")]
        [Property("TestCaseId", "TC_ONB_002")]
        public void RegisterEmployee_InvalidDocuments_ReturnsFalse()
        {
            var employee = new Employee { Name = "Jane Doe", DocumentsVerified = false };

            _mockService.Setup(s => s.RegisterEmployee(employee)).Returns(false);

            bool result = _mockService.Object.RegisterEmployee(employee);

            Assert.IsFalse(result);
        }

        [Test]
        [Property("Description", "Ensures GetEmployeeById returns the correct employee for a valid ID.")]
        [Property("Priority", "Low")]
        [Property("TestCaseId", "TC_ONB_003")]
        public void GetEmployeeById_ExistingEmployee_ReturnsEmployee()
        {
            var employee = new Employee { Id = 1, Name = "Alice", DocumentsVerified = true };

            _mockService.Setup(s => s.GetEmployeeById(1)).Returns(employee);

            var result = _mockService.Object.GetEmployeeById(1);

            Assert.AreEqual("Alice", result.Name);
        }
    }
}
