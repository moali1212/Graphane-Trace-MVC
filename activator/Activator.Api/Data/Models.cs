using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Activator.Api.Data
{
    public enum Role { User, Clinician, Admin }

    public class User
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Username { get; set; } = null!;
        public Role Role { get; set; }
    }

    // linking table: clinicians may be assigned to multiple users
    public class ClinicianAssignment
    {
        [Key]
        public Guid Id { get; set; }
        public Guid ClinicianId { get; set; }
        public Guid UserId { get; set; }
    }

    public class PressureFrame
    {
        [Key]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime Timestamp { get; set; }
        // Store raw CSV payload or compressed bytes; for simplicity keep as string
        public string CsvPayload { get; set; } = null!;

        public virtual List<PressureMetric>? Metrics { get; set; }
        public virtual List<Alert>? Alerts { get; set; }
        public virtual List<Comment>? Comments { get; set; }
    }

    public class PressureMetric
    {
        [Key]
        public Guid Id { get; set; }
        public Guid FrameId { get; set; }
        public double PeakPressureIndex { get; set; }
        public double ContactAreaPercent { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class Alert
    {
        [Key]
        public Guid Id { get; set; }
        public Guid FrameId { get; set; }
        public string Message { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public bool Reviewed { get; set; }
    }

    public class Comment
    {
        [Key]
        public Guid Id { get; set; }
        public Guid FrameId { get; set; }
        public Guid UserId { get; set; }
        public string Text { get; set; } = null!;
        public DateTime Timestamp { get; set; }
        // optional threaded replies
        public Guid? ReplyToCommentId { get; set; }
        public virtual Comment? ReplyTo { get; set; }
        public virtual List<Comment>? Replies { get; set; }
    }
}
