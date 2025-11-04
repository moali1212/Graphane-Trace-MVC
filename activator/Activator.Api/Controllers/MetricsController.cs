using Activator.Api.Data;
using Activator.Api.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Activator.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MetricsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public MetricsController(AppDbContext db) { _db = db; }

        [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetMetricsForUser(Guid userId, DateTime? since, string? period)
        {
            // period can be '1h','6h','24h' or since timestamp
            DateTime from = since ?? (period switch
            {
                "1h" => DateTime.UtcNow.AddHours(-1),
                "6h" => DateTime.UtcNow.AddHours(-6),
                "24h" => DateTime.UtcNow.AddHours(-24),
                _ => DateTime.UtcNow.AddHours(-24)
            });

            var metrics = await _db.PressureMetrics
                .Where(m => m.CreatedAt >= from && _db.PressureFrames.Any(f => f.Id == m.FrameId && f.UserId == userId))
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();

            return Ok(metrics);
        }
    }
}
