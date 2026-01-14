using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TurkcellCampaignOptimizer.Data;
using TurkcellCampaignOptimizer.DTOs;
using TurkcellCampaignOptimizer.Services;
using TurkcellCampaignOptimizer.Models.DTOs;
using TurkcellCampaignOptimizer.Models;

namespace TurkcellCampaignOptimizer.Controllers;

[ApiController]
[Route("api")]
public class CampaignController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly CampaignEngineService _campaignEngine;
    private readonly ILogger<CampaignController> _logger;

    public CampaignController(
        AppDbContext context,
        CampaignEngineService campaignEngine,
        ILogger<CampaignController> logger)
    {
        _context = context;
        _campaignEngine = campaignEngine;
        _logger = logger;
    }

    /// <summary>
    /// Assign a campaign to a user
    /// </summary>
    [HttpPost("assign")]
    public async Task<ActionResult<AssignCampaignResponse>> AssignCampaign([FromBody] AssignCampaignRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return BadRequest(new AssignCampaignResponse
                {
                    Success = false,
                    Message = "UserId is required"
                });
            }

            var assignment = await _campaignEngine.AssignCampaignToUser(request.UserId);

            if (assignment == null)
            {
                return Ok(new AssignCampaignResponse
                {
                    Success = false,
                    Message = "No eligible campaign found for this user"
                });
            }

            // Load related data
            await _context.Entry(assignment)
                .Reference(a => a.User)
                .LoadAsync();
            await _context.Entry(assignment)
                .Reference(a => a.Campaign)
                .LoadAsync();

            var response = new AssignCampaignResponse
            {
                Success = true,
                Message = "Campaign assigned successfully",
                Assignment = new AssignmentDTO
                {
                    AssignmentId = assignment.AssignmentId,
                    UserId = assignment.UserId,
                    CampaignId = assignment.CampaignId,
                    Score = assignment.Score,
                    Status = assignment.Status,
                    AssignedAt = assignment.AssignedAt,
                    UserName = assignment.User?.Name,
                    CampaignType = assignment.Campaign?.Type
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning campaign");
            return StatusCode(500, new AssignCampaignResponse
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    /// <summary>
    /// Update assignment status
    /// </summary>
    [HttpPatch("status-update")]
    public async Task<ActionResult<UpdateStatusResponse>> UpdateStatus([FromBody] UpdateStatusRequest request)
    {
        try
        {
            if (request.AssignmentId <= 0)
            {
                return BadRequest(new UpdateStatusResponse
                {
                    Success = false,
                    Message = "Invalid AssignmentId"
                });
            }

            if (string.IsNullOrWhiteSpace(request.NewStatus))
            {
                return BadRequest(new UpdateStatusResponse
                {
                    Success = false,
                    Message = "NewStatus is required"
                });
            }

            var success = await _campaignEngine.UpdateAssignmentStatus(request.AssignmentId, request.NewStatus);

            if (!success)
            {
                return Ok(new UpdateStatusResponse
                {
                    Success = false,
                    Message = "Failed to update status. Assignment may not exist or status transition is invalid."
                });
            }

            return Ok(new UpdateStatusResponse
            {
                Success = true,
                Message = "Campaign status updated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status");
            return StatusCode(500, new UpdateStatusResponse
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    /// <summary>
    /// Get all campaigns with pagination
    /// </summary>
    [HttpGet("campaigns")]
    public async Task<ActionResult<PaginatedResponse<CampaignDTO>>> GetCampaigns(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var totalItems = await _context.Campaigns.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var campaigns = await _context.Campaigns
                .OrderBy(c => c.Priority)
                .ThenByDescending(c => c.StartDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CampaignDTO
                {
                    CampaignId = c.CampaignId,
                    Type = c.Type,
                    TargetSegment = c.TargetSegment,
                    Priority = c.Priority,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    IsActive = c.IsActive,
                    AssignmentCount = c.Assignments.Count
                })
                .ToListAsync();

            var response = new PaginatedResponse<CampaignDTO>
            {
                Data = campaigns,
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
            _logger.LogError(ex, "Error getting campaigns");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get all assignments with pagination
    /// </summary>
    [HttpGet("assignments")]
    public async Task<ActionResult<PaginatedResponse<AssignmentDTO>>> GetAssignments(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.Assignments
                .Include(a => a.User)
                .Include(a => a.Campaign)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(a => a.Status == status);
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var assignments = await query
                .OrderByDescending(a => a.AssignedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AssignmentDTO
                {
                    AssignmentId = a.AssignmentId,
                    UserId = a.UserId,
                    CampaignId = a.CampaignId,
                    Score = a.Score,
                    Status = a.Status,
                    AssignedAt = a.AssignedAt,
                    UserName = a.User.Name,
                    CampaignType = a.Campaign.Type
                })
                .ToListAsync();

            var response = new PaginatedResponse<AssignmentDTO>
            {
                Data = assignments,
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
            _logger.LogError(ex, "Error getting assignments");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("campaigns")]
    public async Task<ActionResult<Campaign>> CreateCampaign([FromBody] CreateCampaignRequest request)
    {
        try
        {
            var campaign = new Campaign
            {
                // Generate simple ID
                CampaignId = $"C-{DateTime.Now.Ticks % 9000 + 1000}", 
                Type = request.Type,
                TargetSegment = request.TargetSegment,
                Priority = request.Priority,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsActive = true
            };

            _context.Campaigns.Add(campaign);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCampaigns), new { id = campaign.CampaignId }, campaign);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating campaign");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("campaigns/{id}")]
    public async Task<IActionResult> UpdateCampaign(string id, [FromBody] Campaign campaign)
    {
        if (id != campaign.CampaignId)
        {
            _logger.LogWarning($"Update failed: ID mismatch {id} vs {campaign.CampaignId}");
            return BadRequest("ID mismatch");
        }

        // Only update allowable fields
        var existingCampaign = await _context.Campaigns.FindAsync(id);
        if (existingCampaign == null)
        {
            return NotFound();
        }

        existingCampaign.Type = campaign.Type;
        existingCampaign.TargetSegment = campaign.TargetSegment;
        existingCampaign.Priority = campaign.Priority;
        existingCampaign.StartDate = campaign.StartDate;
        existingCampaign.EndDate = campaign.EndDate;
        existingCampaign.IsActive = campaign.IsActive;

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Campaign {id} updated.");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating campaign {id}");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Toggle campaign active status (Bonus feature - PDF requirement)
    /// </summary>
    [HttpPatch("campaigns/{id}/toggle")]
    public async Task<ActionResult<UpdateStatusResponse>> ToggleCampaignStatus(string id)
    {
        try
        {
            var campaign = await _context.Campaigns.FindAsync(id);
            if (campaign == null)
            {
                return NotFound(new UpdateStatusResponse
                {
                    Success = false,
                    Message = $"Campaign {id} not found"
                });
            }

            campaign.IsActive = !campaign.IsActive;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Campaign {id} status toggled to {(campaign.IsActive ? "Active" : "Inactive")}");

            return Ok(new UpdateStatusResponse
            {
                Success = true,
                Message = $"Campaign status updated to {(campaign.IsActive ? "Active" : "Inactive")}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error toggling campaign {id} status");
            return StatusCode(500, new UpdateStatusResponse
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    /// <summary>
    /// Update campaign priority (Bonus feature - PDF requirement)
    /// </summary>
    [HttpPatch("campaigns/{id}/priority")]
    public async Task<ActionResult<UpdateStatusResponse>> UpdateCampaignPriority(
        string id, 
        [FromBody] UpdatePriorityRequest request)
    {
        try
        {
            if (request.Priority < 1)
            {
                return BadRequest(new UpdateStatusResponse
                {
                    Success = false,
                    Message = "Priority must be greater than 0"
                });
            }

            var campaign = await _context.Campaigns.FindAsync(id);
            if (campaign == null)
            {
                return NotFound(new UpdateStatusResponse
                {
                    Success = false,
                    Message = $"Campaign {id} not found"
                });
            }

            campaign.Priority = request.Priority;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Campaign {id} priority updated to {request.Priority}");

            return Ok(new UpdateStatusResponse
            {
                Success = true,
                Message = $"Campaign priority updated to {request.Priority}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating campaign {id} priority");
            return StatusCode(500, new UpdateStatusResponse
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }
}

