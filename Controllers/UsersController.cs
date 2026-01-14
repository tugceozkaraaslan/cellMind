using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TurkcellCampaignOptimizer.Data;
using TurkcellCampaignOptimizer.DTOs;
using TurkcellCampaignOptimizer.Services;

namespace TurkcellCampaignOptimizer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly CampaignEngineService _campaignEngine;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        AppDbContext context,
        CampaignEngineService campaignEngine,
        ILogger<UsersController> logger)
    {
        _context = context;
        _campaignEngine = campaignEngine;
        _logger = logger;
    }

    /// <summary>
    /// Get all users with pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<UserDTO>>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? segment = null)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.Users
                .Include(u => u.UserMetric)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(segment))
            {
                query = query.Where(u => u.Segment == segment);
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var users = await query
                .OrderBy(u => u.UserId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserDTO
                {
                    UserId = u.UserId,
                    Name = u.Name,
                    City = u.City,
                    Segment = u.Segment,
                    MonthlyDataGb = u.UserMetric != null ? u.UserMetric.MonthlyDataGb : null,
                    MonthlySpendTry = u.UserMetric != null ? u.UserMetric.MonthlySpendTry : null,
                    LoyaltyYears = u.UserMetric != null ? u.UserMetric.LoyaltyYears : null,
                    Score = u.UserMetric != null ? 
                        Math.Round(((u.UserMetric.MonthlyDataGb * 0.5m) + 
                                   (u.UserMetric.MonthlySpendTry * 0.3m) + 
                                   (u.UserMetric.LoyaltyYears * 0.2m)) / 354m * 100, 2) : null
                })
                .ToListAsync();

            var response = new PaginatedResponse<UserDTO>
            {
                Data = users,
                Pagination = new PaginationMetadata
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get a specific user by ID
    /// </summary>
    [HttpGet("{userId}")]
    public async Task<ActionResult<UserDTO>> GetUser(string userId)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.UserMetric)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound(new { message = $"User {userId} not found" });
            }

            var userDto = new UserDTO
            {
                UserId = user.UserId,
                Name = user.Name,
                City = user.City,
                Segment = user.Segment,
                MonthlyDataGb = user.UserMetric?.MonthlyDataGb,
                MonthlySpendTry = user.UserMetric?.MonthlySpendTry,
                LoyaltyYears = user.UserMetric?.LoyaltyYears,
                Score = user.UserMetric != null ? 
                    _campaignEngine.CalculateSuitabilityScore(user.UserMetric) : null
            };

            return Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting user {userId}");
            return StatusCode(500, "Internal server error");
        }
    }
}
