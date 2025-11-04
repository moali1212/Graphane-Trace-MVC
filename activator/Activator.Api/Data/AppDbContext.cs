using Microsoft.EntityFrameworkCore;

namespace Activator.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<PressureFrame> PressureFrames { get; set; } = null!;
        public DbSet<PressureMetric> PressureMetrics { get; set; } = null!;
        public DbSet<Alert> Alerts { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
    public DbSet<ClinicianAssignment> ClinicianAssignments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Simple seed admin user
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Username = "admin",
                Role = Role.Admin
            });

            // Comment self-reference
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ReplyTo)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ReplyToCommentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
