namespace TurkcellCampaignOptimizer.Models;

public class ScoringSettings
{
    public Weights Weights { get; set; } = new();
    public MaxValues MaxValues { get; set; } = new();
}

public class Weights
{
    public decimal DataUsage { get; set; }
    public decimal MonthlySpend { get; set; }
    public decimal Loyalty { get; set; }
}

public class MaxValues
{
    public decimal DataUsage { get; set; }
    public decimal MonthlySpend { get; set; }
    public decimal Loyalty { get; set; }
}
