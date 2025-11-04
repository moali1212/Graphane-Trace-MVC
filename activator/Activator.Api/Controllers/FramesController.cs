using Activator.Api.Data;
using Activator.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Activator.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FramesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IPressureAnalysisService _svc;

        public FramesController(AppDbContext db, IPressureAnalysisService svc)
        {
            _db = db;
            _svc = svc;
        }

        [HttpPost("upload")] // payload: userId, timestamp, csv
        public async Task<IActionResult> UploadFrame([FromBody] UploadFrameDto dto)
        {
            var user = await _db.Users.FindAsync(dto.UserId);
            if (user == null) return BadRequest("Invalid user");

            var frame = new PressureFrame
            {
                Id = Guid.NewGuid(),
                UserId = dto.UserId,
                Timestamp = dto.Timestamp,
                CsvPayload = dto.CsvPayload
            };

            _db.PressureFrames.Add(frame);
            await _db.SaveChangesAsync();

            var detailed = await _svc.AnalyzeFrameDetailedAsync(frame);
            var metric = detailed.Metric;
            metric.FrameId = frame.Id;
            _db.PressureMetrics.Add(metric);

            // create alert if service detected one
            if (!string.IsNullOrWhiteSpace(detailed.AlertMessage))
            {
                var alert = new Alert
                {
                    Id = Guid.NewGuid(),
                    FrameId = frame.Id,
                    Message = detailed.AlertMessage,
                    CreatedAt = DateTime.UtcNow,
                    Reviewed = false
                };
                _db.Alerts.Add(alert);
            }

            await _db.SaveChangesAsync();

            return Ok(new { frameId = frame.Id, metric });
        }

        [HttpGet("latest/user/{userId}")]
        public async Task<IActionResult> GetLatestFrameForUser(Guid userId)
        {
            var frame = await _db.PressureFrames.Where(f => f.UserId == userId).OrderByDescending(f => f.Timestamp).FirstOrDefaultAsync();
            if (frame == null) return NotFound();

            var metric = await _db.PressureMetrics.Where(m => m.FrameId == frame.Id).OrderByDescending(m => m.CreatedAt).FirstOrDefaultAsync();

            return Ok(new { frame.Id, frame.UserId, frame.Timestamp, frame.CsvPayload, Metric = metric });
        }
    }

    public record UploadFrameDto(Guid UserId, DateTime Timestamp, string CsvPayload);
}
