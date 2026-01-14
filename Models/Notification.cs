using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurkcellCampaignOptimizer.Models;

public class Notification
{
    [Key]
    public int NotificationId { get; set; }

    [Required]
    [MaxLength(50)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Channel { get; set; } = string.Empty; // SMS, EMAIL, PUSH

    [Required]
    [MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    [Required]
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}
