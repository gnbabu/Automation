using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutomationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnvironmentController : ControllerBase
    {
        private readonly IEnvironmentRepository _repo;

        public EnvironmentController(IEnvironmentRepository repo)
        {
            _repo = repo;
        }

        // GET: api/environments
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _repo.GetAllAsync());
        }

        // GET: api/environments/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid Environment Id");

            var env = await _repo.GetByIdAsync(id);
            return env == null ? NotFound() : Ok(env);
        }

        // POST: api/environments
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EnvironmentRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var id = await _repo.CreateAsync(request);
            return Ok(new { EnvironmentId = id });
        }

        // PUT: api/environments
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] EnvironmentRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!request.EnvironmentId.HasValue || request.EnvironmentId <= 0)
                return BadRequest("EnvironmentId is required for update");

            var existing = await _repo.GetByIdAsync(request.EnvironmentId.Value);
            if (existing == null)
                return NotFound();

            await _repo.UpdateAsync(request);
            return Ok();
        }

        // SOFT DELETE
        [HttpDelete("{id:int}/soft")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid EnvironmentId");

            var env = await _repo.GetByIdAsync(id);
            if (env == null)
                return NotFound();

            await _repo.SoftDeleteAsync(id);
            return Ok(new { Message = "Environment soft-deleted successfully" });
        }

        // HARD DELETE
        [HttpDelete("{id:int}/hard")]
        public async Task<IActionResult> HardDelete(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid EnvironmentId");

            var env = await _repo.GetByIdAsync(id);
            if (env == null)
                return NotFound();

            await _repo.HardDeleteAsync(id);
            return Ok(new { Message = "Environment permanently deleted" });
        }
    }
}
