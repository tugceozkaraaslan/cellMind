using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TurkcellCampaignOptimizer.Data;
using TurkcellCampaignOptimizer.DTOs;
using TurkcellCampaignOptimizer.Services;

namespace TurkcellCampaignOptimizer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly CampaignEngineService _campaignEngine;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        AppDbContext context,
        CampaignEngineService campaignEngine,
        ILogger<DashboardController> logger)
    {
        _context = context;
        _campaignEngine = campaignEngine;
        _logger = logger;
    }

    /// <summary>
    /// Get dashboard summary statistics
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDTO>> GetSummary()
    {
        try
        {
            var totalUsers = await _context.Users.CountAsync();
            
            // Active users are those with at least one assignment
            var activeUsers = await _context.Assignments
                .Select(a => a.UserId)
                .Distinct()
                .CountAsync();

            var totalCampaigns = await _context.Campaigns.CountAsync();
            
            var now = DateTime.UtcNow;
            var activeCampaigns = await _context.Campaigns
                .CountAsync(c => c.IsActive && c.StartDate <= now && c.EndDate >= now);

            var totalAssignments = await _context.Assignments.CountAsync();

            // Success rate: percentage of USED assignments
            var usedAssignments = await _context.Assignments.CountAsync(a => a.Status == "USED");
            var successRate = totalAssignments > 0 
                ? Math.Round((decimal)usedAssignments / totalAssignments * 100, 2) 
                : 0;

            // Average score of all assignments
            var averageScore = await _context.Assignments.AnyAsync()
                ? await _context.Assignments.AverageAsync(a => a.Score)
                : 0;

            var summary = new DashboardSummaryDTO
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                TotalCampaigns = totalCampaigns,
                ActiveCampaigns = activeCampaigns,
                TotalAssignments = totalAssignments,
                SuccessRate = successRate,
                AverageScore = Math.Round(averageScore, 2)
            };

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard summary");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get daily campaign distribution (PDF requirement #7)
    /// </summary>
    [HttpGet("daily-distribution")]
    public async Task<ActionResult<List<DailyDistributionDTO>>> GetDailyDistribution(
        [FromQuery] int days = 7)
    {
        try
        {
            if (days < 1 || days > 30) days = 7;

            var startDate = DateTime.UtcNow.Date.AddDays(-days);
            
            var dailyDistribution = await _context.Assignments
                .Where(a => a.AssignedAt >= startDate)
                .GroupBy(a => a.AssignedAt.Date)
                .Select(g => new DailyDistributionDTO
                {
                    Date = g.Key,
                    TotalAssignments = g.Count(),
                    AssignedCount = g.Count(a => a.Status == "ASSIGNED"),
                    UsedCount = g.Count(a => a.Status == "USED"),
                    ExpiredCount = g.Count(a => a.Status == "EXPIRED")
                })
                .OrderBy(d => d.Date)
                .ToListAsync();

            return Ok(dailyDistribution);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting daily distribution");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get campaign usage rates (PDF requirement #7)
    /// </summary>
    [HttpGet("campaign-usage-rates")]
    public async Task<ActionResult<List<CampaignUsageRateDTO>>> GetCampaignUsageRates()
    {
        try
        {
            var campaignUsageRates = await _context.Campaigns
                .Select(c => new CampaignUsageRateDTO
                {
                    CampaignId = c.CampaignId,
                    CampaignType = c.Type,
                    TargetSegment = c.TargetSegment,
                    TotalAssignments = c.Assignments.Count,
                    AssignedCount = c.Assignments.Count(a => a.Status == "ASSIGNED"),
                    UsedCount = c.Assignments.Count(a => a.Status == "USED"),
                    ExpiredCount = c.Assignments.Count(a => a.Status == "EXPIRED"),
                    UsageRate = c.Assignments.Count > 0 
                        ? Math.Round((decimal)c.Assignments.Count(a => a.Status == "USED") / c.Assignments.Count * 100, 2)
                        : 0
                })
                .OrderByDescending(c => c.UsageRate)
                .ToListAsync();

            return Ok(campaignUsageRates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting campaign usage rates");
            return StatusCode(500, "Internal server error");
        }
    }
}
