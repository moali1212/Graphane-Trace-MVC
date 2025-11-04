using Activator.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Activator.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ReportsController(AppDbContext db) { _db = db; }

        // GET api/reports/user/{userId}?period=1h|6h|24h
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetSimpleReport(Guid userId, string period = "24h")
        {
            DateTime to = DateTime.UtcNow;
            DateTime from = period switch
            {
                "1h" => to.AddHours(-1),
                "6h" => to.AddHours(-6),
                _ => to.AddHours(-24),
            };

            var frames = await _db.PressureFrames.Where(f => f.UserId == userId && f.Timestamp >= from && f.Timestamp <= to).ToListAsync();
            var frameIds = frames.Select(f => f.Id).ToList();
            var metrics = await _db.PressureMetrics.Where(m => frameIds.Contains(m.FrameId)).ToListAsync();

            if (!metrics.Any()) return Ok(new { message = "No data" });

            double avgContact = metrics.Average(m => m.ContactAreaPercent);
            double maxPpi = metrics.Max(m => m.PeakPressureIndex);

            // previous same-length period
            var prevFrom = from.AddHours(-(to - from).TotalHours);
            var prevTo = from;
            var prevFrames = await _db.PressureFrames.Where(f => f.UserId == userId && f.Timestamp >= prevFrom && f.Timestamp < prevTo).ToListAsync();
            var prevMetrics = await _db.PressureMetrics.Where(m => prevFrames.Select(f => f.Id).Contains(m.FrameId)).ToListAsync();

            double prevAvgContact = prevMetrics.Any() ? prevMetrics.Average(m => m.ContactAreaPercent) : 0;
            double prevMaxPpi = prevMetrics.Any() ? prevMetrics.Max(m => m.PeakPressureIndex) : 0;

            return Ok(new
            {
                period = new { from, to },
                averageContact = avgContact,
                maxPeakPressure = maxPpi,
                previous = new { averageContact = prevAvgContact, maxPeakPressure = prevMaxPpi },
                delta = new { averageContact = avgContact - prevAvgContact, maxPeakPressure = maxPpi - prevMaxPpi }
            });
        }
    }
}
