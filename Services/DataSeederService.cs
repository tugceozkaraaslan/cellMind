using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using TurkcellCampaignOptimizer.Data;
using TurkcellCampaignOptimizer.Models;

namespace TurkcellCampaignOptimizer.Services;

public class DataSeederService
{
    private readonly AppDbContext _context;
    private readonly ILogger<DataSeederService> _logger;
    private readonly string _seedDataPath;

    public DataSeederService(AppDbContext context, ILogger<DataSeederService> logger, IWebHostEnvironment env)
    {
        _context = context;
        _logger = logger;
        _seedDataPath = Path.Combine(env.ContentRootPath, "SeedData");
    }

    public async Task SeedDataAsync()
    {
        try
        {
            _logger.LogInformation("Starting data seeding process...");

            // Ensure database is created
            await _context.Database.EnsureCreatedAsync();

            // Seed Users
            await SeedUsersAsync();

            // Seed UserMetrics
            await SeedUserMetricsAsync();

            // Seed Campaigns
            await SeedCampaignsAsync();

            _logger.LogInformation("Data seeding completed successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during data seeding");
            throw;
        }
    }

    private async Task SeedUsersAsync()
    {
        var filePath = Path.Combine(_seedDataPath, "users.csv");
        
        if (!File.Exists(filePath))
        {
            _logger.LogWarning($"Users CSV file not found at: {filePath}");
            return;
        }

        if (await _context.Users.AnyAsync())
        {
            _logger.LogInformation("Users table already contains data. Skipping users seeding.");
            return;
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);

        var records = csv.GetRecords<UserCsvRecord>().ToList();
        
        foreach (var record in records)
        {
            var user = new User
            {
                UserId = record.user_id,
                Name = record.name,
                City = record.city,
                Segment = record.segment
            };

            _context.Users.Add(user);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation($"Seeded {records.Count} users");
    }

    private async Task SeedUserMetricsAsync()
    {
        var filePath = Path.Combine(_seedDataPath, "user_metrics.csv");
        
        if (!File.Exists(filePath))
        {
            _logger.LogWarning($"User metrics CSV file not found at: {filePath}");
            return;
        }

        if (await _context.UserMetrics.AnyAsync())
        {
            _logger.LogInformation("UserMetrics table already contains data. Skipping user metrics seeding.");
            return;
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);

        var records = csv.GetRecords<UserMetricCsvRecord>().ToList();
        
        foreach (var record in records)
        {
            // Check if user exists
            var userExists = await _context.Users.AnyAsync(u => u.UserId == record.user_id);
            if (!userExists)
            {
                _logger.LogWarning($"User {record.user_id} not found. Skipping metric.");
                continue;
            }

            var userMetric = new UserMetric
            {
                UserId = record.user_id,
                MonthlyDataGb = record.monthly_data_gb,
                MonthlySpendTry = record.monthly_spend_try,
                LoyaltyYears = record.loyalty_years
            };

            _context.UserMetrics.Add(userMetric);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation($"Seeded {records.Count} user metrics");
    }

    private async Task SeedCampaignsAsync()
    {
        var filePath = Path.Combine(_seedDataPath, "campaigns.csv");
        
        if (!File.Exists(filePath))
        {
            _logger.LogWarning($"Campaigns CSV file not found at: {filePath}");
            return;
        }

        if (await _context.Campaigns.AnyAsync())
        {
            _logger.LogInformation("Campaigns table already contains data. Skipping campaigns seeding.");
            return;
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);

        var records = csv.GetRecords<CampaignCsvRecord>().ToList();
        
        foreach (var record in records)
        {
            var campaign = new Campaign
            {
                CampaignId = record.campaign_id,
                Type = record.type,
                TargetSegment = record.target_segment,
                Priority = record.priority,
                StartDate = DateTime.Parse(record.start_date),
                EndDate = DateTime.Parse(record.end_date),
                IsActive = record.is_active.ToLower() == "true" || record.is_active == "1"
            };

            _context.Campaigns.Add(campaign);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation($"Seeded {records.Count} campaigns");
    }

    // CSV Record Classes
    private class UserCsvRecord
    {
        public string user_id { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string city { get; set; } = string.Empty;
        public string segment { get; set; } = string.Empty;
    }

    private class UserMetricCsvRecord
    {
        public string user_id { get; set; } = string.Empty;
        public decimal monthly_data_gb { get; set; }
        public decimal monthly_spend_try { get; set; }
        public int loyalty_years { get; set; }
    }

    private class CampaignCsvRecord
    {
        public string campaign_id { get; set; } = string.Empty;
        public string type { get; set; } = string.Empty;
        public string target_segment { get; set; } = string.Empty;
        public int priority { get; set; }
        public string start_date { get; set; } = string.Empty;
        public string end_date { get; set; } = string.Empty;
        public string is_active { get; set; } = string.Empty;
    }
}
