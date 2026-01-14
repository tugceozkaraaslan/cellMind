using System.ComponentModel.DataAnnotations;

namespace TurkcellCampaignOptimizer.Models;

public class User
{
    [Key]
    [MaxLength(50)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Segment { get; set; } = string.Empty;

    // Navigation Properties
    public UserMetric? UserMetric { get; set; }
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
