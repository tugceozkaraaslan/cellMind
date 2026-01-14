using System.ComponentModel.DataAnnotations;

namespace TurkcellCampaignOptimizer.Models.DTOs;

public class CreateCampaignRequest
{
    [Required]
    public string Type { get; set; } = string.Empty;

    [Required]
    public string TargetSegment { get; set; } = string.Empty;

    [Range(1, 100)]
    public int Priority { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }
}
