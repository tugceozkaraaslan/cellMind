using Microsoft.EntityFrameworkCore;
using TurkcellCampaignOptimizer.Models;

namespace TurkcellCampaignOptimizer.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<UserMetric> UserMetrics { get; set; }
    public DbSet<Campaign> Campaigns { get; set; }
    public DbSet<Assignment> Assignments { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.HasIndex(e => e.Segment);
            
            entity.HasOne(e => e.UserMetric)
                .WithOne(e => e.User)
                .HasForeignKey<UserMetric>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserMetric Configuration
        modelBuilder.Entity<UserMetric>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
        });

        // Campaign Configuration
        modelBuilder.Entity<Campaign>(entity =>
        {
            entity.HasKey(e => e.CampaignId);
            entity.HasIndex(e => new { e.TargetSegment, e.Priority, e.IsActive });
        });

        // Assignment Configuration
        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId);
            entity.HasIndex(e => new { e.UserId, e.Status });
            entity.HasIndex(e => e.CampaignId);

            entity.HasOne(e => e.User)
                .WithMany(e => e.Assignments)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Campaign)
                .WithMany(e => e.Assignments)
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Notification Configuration
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId);
            entity.HasIndex(e => new { e.UserId, e.SentAt });

            entity.HasOne(e => e.User)
                .WithMany(e => e.Notifications)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
