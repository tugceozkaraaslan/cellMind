using System.ComponentModel.DataAnnotations;

namespace TurkcellCampaignOptimizer.Models;

public class Campaign
{
    [Key]
    [MaxLength(50)]
    public string CampaignId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Type { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string TargetSegment { get; set; } = string.Empty;

    [Required]
    public int Priority { get; set; } // 1 = Highest priority

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    public bool IsActive { get; set; }

    // Navigation Properties
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}
