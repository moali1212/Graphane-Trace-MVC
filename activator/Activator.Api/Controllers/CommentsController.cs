using Activator.Api.Data;
using Activator.Api.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Activator.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public CommentsController(AppDbContext db) { _db = db; }

        [HttpPost]
        public async Task<IActionResult> PostComment([FromBody] CreateCommentDto dto)
        {
            var frame = await _db.PressureFrames.FindAsync(dto.FrameId);
            if (frame == null) return BadRequest("Invalid frame");
            var user = await _db.Users.FindAsync(dto.UserId);
            if (user == null) return BadRequest("Invalid user");

            var c = new Comment
            {
                Id = Guid.NewGuid(),
                FrameId = dto.FrameId,
                UserId = dto.UserId,
                Text = dto.Text,
                Timestamp = dto.Timestamp
            };
            _db.Comments.Add(c);
            await _db.SaveChangesAsync();
            return Ok(c);
        }

        [HttpGet("frame/{frameId}")]
        public async Task<IActionResult> GetCommentsForFrame(Guid frameId)
        {
            var comments = _db.Comments.Where(c => c.FrameId == frameId).OrderBy(c => c.Timestamp);
            return Ok(await comments.ToListAsync());
        }
    }

    public record CreateCommentDto(Guid FrameId, Guid UserId, string Text, DateTime Timestamp);
}
