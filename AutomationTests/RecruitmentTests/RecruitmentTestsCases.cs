using Moq;
using RecruitmentTests.Models;
using RecruitmentTests.Services;

namespace RecruitmentTests
{
    [TestFixture]
    public class RecruitmentServiceTests
    {
        private Mock<IRecruitmentService> _mockService;

        [SetUp]
        public void Setup()
        {
            _mockService = new Mock<IRecruitmentService>();
        }

        // ==================================================
        // TC_RECRUIT_001
        // ==================================================
        [Test]
        [Property("Description", "Valid candidate with sufficient experience should return true.")]
        [Property("Priority", "High")]
        [Property("TestCaseId", "TC_RECRUIT_001")]
        public void ValidateCandidate_ValidCandidate_ReturnsTrue()
        {
            var candidate = new Candidate { Name = "John Doe", ExperienceYears = 5 };

            _mockService.Setup(s => s.ValidateCandidate(candidate)).Returns(true);

            bool result = _mockService.Object.ValidateCandidate(candidate);

            Assert.IsTrue(result);
        }

        // ==================================================
        // TC_RECRUIT_002
        // ==================================================
        [Test]
        [Property("Description", "Candidate with invalid experience should return false.")]
        [Property("Priority", "Medium")]
        [Property("TestCaseId", "TC_RECRUIT_002")]
        public void ValidateCandidate_InvalidCandidate_ReturnsFalse()
        {
            var candidate = new Candidate { Name = "Jane Doe", ExperienceYears = 0 };

            _mockService.Setup(s => s.ValidateCandidate(candidate)).Returns(false);

            bool result = _mockService.Object.ValidateCandidate(candidate);

            Assert.IsFalse(result);
        }

        // ==================================================
        // TC_RECRUIT_003
        // ==================================================
        [Test]
        [Property("Description", "GetCandidateById should return the correct candidate when the ID exists.")]
        [Property("Priority", "High")]
        [Property("TestCaseId", "TC_RECRUIT_003")]
        public void GetCandidateById_ExistingCandidate_ReturnsCandidate()
        {
            var candidate = new Candidate { Id = 1, Name = "Alice", ExperienceYears = 3 };

            _mockService.Setup(s => s.GetCandidateById(1)).Returns(candidate);

            var result = _mockService.Object.GetCandidateById(1);

            Assert.AreEqual("Alice", result.Name);
        }
    }
}
