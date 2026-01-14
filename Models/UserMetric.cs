using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurkcellCampaignOptimizer.Models;

public class UserMetric
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MonthlyDataGb { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MonthlySpendTry { get; set; }

    [Required]
    public int LoyaltyYears { get; set; }

    // Navigation Property
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}
