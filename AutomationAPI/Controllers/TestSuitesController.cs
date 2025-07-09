using AutomationAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TestSuitesController : ControllerBase
    {
        private readonly ITestSuitesRepository _testSuitesRepository;

        public TestSuitesController(ITestSuitesRepository testSuitesRepository)
        {
            _testSuitesRepository = testSuitesRepository;
        }

        [HttpGet("libraries")]
        public async Task<IActionResult> GetTestLibraries()
        {
            var libraries = await _testSuitesRepository.GetLibrariesAsync();
            return Ok(libraries);
        }
    }
}
