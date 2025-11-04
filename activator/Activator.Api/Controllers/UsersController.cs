using Activator.Api.Attributes;
using Activator.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Activator.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _db;
        public UsersController(AppDbContext db) { _db = db; }

        // Only admin may create users
        [HttpPost]
        [RequireRole("Admin")]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            user.Id = Guid.NewGuid();
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        // Clinician: list assigned users
        [HttpGet("assigned")]
        [RequireRole("Clinician")]
        public async Task<IActionResult> GetAssignedUsers()
        {
            var caller = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(caller, out var clinicianId)) return Unauthorized();

            var assigned = from ca in _db.ClinicianAssignments
                           join u in _db.Users on ca.UserId equals u.Id
                           where ca.ClinicianId == clinicianId
                           select u;

            return Ok(await assigned.ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var u = await _db.Users.FindAsync(id);
            if (u == null) return NotFound();
            return Ok(u);
        }
    }
}
