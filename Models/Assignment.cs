using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurkcellCampaignOptimizer.Models;

public class Assignment
{
    [Key]
    public int AssignmentId { get; set; }

    [Required]
    [MaxLength(50)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string CampaignId { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Score { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "ASSIGNED"; // ASSIGNED, USED, EXPIRED

    [Required]
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [ForeignKey(nameof(CampaignId))]
    public Campaign Campaign { get; set; } = null!;
}
