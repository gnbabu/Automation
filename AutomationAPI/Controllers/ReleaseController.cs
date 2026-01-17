using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReleaseController : ControllerBase
    {
        private readonly IReleaseRepository _repo;

        public ReleaseController(IReleaseRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var releases = await _repo.GetAllAsync();
            return Ok(releases);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var release = await _repo.GetByIdAsync(id);
            return release == null ? NotFound() : Ok(release);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReleaseRequestDto releaseRequest)
        {
            var id = await _repo.CreateAsync(releaseRequest);
            return Ok(new { ReleaseId = id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ReleaseRequestDto releaseRequest)
        {
            releaseRequest.ReleaseId = id;
            await _repo.UpdateAsync(releaseRequest);
            return Ok();
        }

        [HttpPost("{id}/signoff")]
        public async Task<IActionResult> SignOff(int id)
        {
            await _repo.SignOffAsync(id, User.Identity?.Name ?? "system");
            return Ok();
        }
    }
}
