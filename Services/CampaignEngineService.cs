using Microsoft.EntityFrameworkCore;
using TurkcellCampaignOptimizer.Data;
using TurkcellCampaignOptimizer.Models;

namespace TurkcellCampaignOptimizer.Services;

public class CampaignEngineService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CampaignEngineService> _logger;
    private readonly ScoringSettings _scoringSettings;

    public CampaignEngineService(
        AppDbContext context, 
        ILogger<CampaignEngineService> logger,
        Microsoft.Extensions.Options.IOptions<ScoringSettings> scoringSettings)
    {
        _context = context;
        _logger = logger;
        _scoringSettings = scoringSettings.Value;
    }

    /// <summary>
    /// Calculates suitability score for a user based on their metrics
    /// Formula: Configurable in appsettings.json
    /// </summary>
    public decimal CalculateSuitabilityScore(UserMetric userMetric)
    {
        try
        {
            // Calculate raw score using configured weights
            decimal rawScore = (userMetric.MonthlyDataGb * _scoringSettings.Weights.DataUsage) +
                              (userMetric.MonthlySpendTry * _scoringSettings.Weights.MonthlySpend) +
                              (userMetric.LoyaltyYears * _scoringSettings.Weights.Loyalty);

            // Normalize to 0-100 range using configured max values
            decimal maxPossibleScore = (_scoringSettings.MaxValues.DataUsage * _scoringSettings.Weights.DataUsage) + 
                                     (_scoringSettings.MaxValues.MonthlySpend * _scoringSettings.Weights.MonthlySpend) + 
                                     (_scoringSettings.MaxValues.Loyalty * _scoringSettings.Weights.Loyalty);
            
            if (maxPossibleScore == 0) return 0;

            decimal normalizedScore = (rawScore / maxPossibleScore) * 100;

            // Ensure score is between 0 and 100
            normalizedScore = Math.Max(0, Math.Min(100, normalizedScore));

            _logger.LogDebug($"Calculated score for user {userMetric.UserId}: {normalizedScore:F2}");

            return Math.Round(normalizedScore, 2);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error calculating score for user {userMetric.UserId}");
            return 0;
        }
    }

    /// <summary>
    /// Assigns the most suitable campaign to a user based on segment, priority, and score
    /// </summary>
    public async Task<Assignment?> AssignCampaignToUser(string userId)
    {
        try
        {
            _logger.LogInformation($"Starting campaign assignment for user {userId}");

            // Get user with metrics
            var user = await _context.Users
                .Include(u => u.UserMetric)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                _logger.LogWarning($"User {userId} not found");
                return null;
            }

            if (user.UserMetric == null)
            {
                _logger.LogWarning($"User {userId} has no metrics");
                return null;
            }

            // Check if user already has an active assignment
            var existingAssignment = await _context.Assignments
                .Where(a => a.UserId == userId && a.Status == "ASSIGNED")
                .FirstOrDefaultAsync();

            if (existingAssignment != null)
            {
                _logger.LogInformation($"User {userId} already has an active assignment");
                return existingAssignment;
            }

            // Calculate user score
            decimal userScore = CalculateSuitabilityScore(user.UserMetric);

            // Get eligible campaigns
            var now = DateTime.UtcNow;
            var eligibleCampaigns = await _context.Campaigns
                .Where(c => c.TargetSegment == user.Segment &&
                           c.IsActive &&
                           c.StartDate <= now &&
                           c.EndDate >= now)
                // Sort by Priority (1 is highest)
                // If priorities are equal, pick the Newest campaign (StartDate)
                // Note: Campaigns do not have individual scores, so User Score is not a differentiator between campaigns.
                .OrderBy(c => c.Priority) 
                .ThenByDescending(c => c.StartDate) // Newest first if priority is equal
                .ToListAsync();

            if (!eligibleCampaigns.Any())
            {
                _logger.LogWarning($"No eligible campaigns found for user {userId} with segment {user.Segment}");
                return null;
            }

            // Select the best campaign (first one after sorting by priority)
            var selectedCampaign = eligibleCampaigns.First();

            // Log suppressed campaigns (Requirement #4)
            foreach (var campaign in eligibleCampaigns.Where(c => c.CampaignId != selectedCampaign.CampaignId))
            {
                _logger.LogInformation($"Campaign {campaign.CampaignId} ({campaign.Type}) suppressed for user {userId} in favor of {selectedCampaign.CampaignId}. Reason: Lower priority or older.");
            }

            // Create assignment
            var assignment = new Assignment
            {
                UserId = userId,
                CampaignId = selectedCampaign.CampaignId,
                Score = userScore,
                Status = "ASSIGNED",
                AssignedAt = DateTime.UtcNow
            };

            _context.Assignments.Add(assignment);
            
            // Create Notification (Requirement #5)
            // Mock logic: HIGH_USAGE segment users get BiP, others get SMS
            string channel = user.Segment == "HIGH_USAGE" ? "BiP" : "SMS";
            
            // Generate detailed message based on campaign type (PDF requirement)
            string message = GenerateCampaignMessage(selectedCampaign.Type, selectedCampaign.CampaignId);

            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                Channel = channel,
                SentAt = DateTime.UtcNow
            };
            
            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Successfully assigned campaign {selectedCampaign.CampaignId} to user {userId}. Notification sent via {channel}.");

            return assignment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error assigning campaign to user {userId}");
            throw;
        }
    }

    /// <summary>
    /// Updates the status of an assignment
    /// </summary>
    public async Task<bool> UpdateAssignmentStatus(int assignmentId, string newStatus)
    {
        try
        {
            var assignment = await _context.Assignments.FindAsync(assignmentId);

            if (assignment == null)
            {
                _logger.LogWarning($"Assignment {assignmentId} not found");
                return false;
            }

            // Validate status transitions
            if (assignment.Status == "USED" || assignment.Status == "EXPIRED")
            {
                _logger.LogWarning($"Cannot update assignment {assignmentId} - status is already {assignment.Status}");
                return false;
            }

            if (newStatus != "USED" && newStatus != "EXPIRED")
            {
                _logger.LogWarning($"Invalid status: {newStatus}");
                return false;
            }

            assignment.Status = newStatus;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Updated assignment {assignmentId} status to {newStatus}");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating assignment {assignmentId} status");
            throw;
        }
    }

    /// <summary>
    /// Gets assignment details with related data
    /// </summary>
    public async Task<Assignment?> GetAssignmentDetails(int assignmentId)
    {
        return await _context.Assignments
            .Include(a => a.User)
            .Include(a => a.Campaign)
            .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);
    }

    /// <summary>
    /// Generates a detailed campaign message based on campaign type (PDF requirement)
    /// </summary>
    private string GenerateCampaignMessage(string campaignType, string campaignId)
    {
        return campaignType switch
        {
            "DATA_BOOST" => "Size özel 5 GB ek internet kampanyası tanımlandı.",
            "LOYALTY_REWARD" => "Size özel sadakat ödülü kampanyası tanımlandı. Özel avantajlar sizi bekliyor!",
            "DISCOUNT_OFFER" => "Size özel indirim kampanyası tanımlandı. Fırsatı kaçırmayın!",
            "UPGRADE_PLAN" => "Size özel plan yükseltme kampanyası tanımlandı. Daha fazla avantaj için yükseltin!",
            "WELCOME_BONUS" => "Size özel hoş geldin bonusu kampanyası tanımlandı. Yeni avantajlar sizi bekliyor!",
            "RETENTION_OFFER" => "Size özel özel teklif kampanyası tanımlandı. Özel fırsatlar sizin için!",
            "PREMIUM_TRIAL" => "Size özel premium deneme kampanyası tanımlandı. Premium deneyimi yaşayın!",
            "BASIC_PLAN" => "Size özel temel plan kampanyası tanımlandı. Hemen kullanmaya başlayın!",
            _ => $"Size özel {campaignType} kampanyası tanımlandı. Hemen kullanmaya başlayın!"
        };
    }
}
