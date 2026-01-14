namespace TurkcellCampaignOptimizer.DTOs;

public class DashboardSummaryDTO
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalCampaigns { get; set; }
    public int ActiveCampaigns { get; set; }
    public int TotalAssignments { get; set; }
    public decimal SuccessRate { get; set; }
    public decimal AverageScore { get; set; }
}

public class PaginationMetadata
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}

public class PaginatedResponse<T>
{
    public List<T> Data { get; set; } = new();
    public PaginationMetadata Pagination { get; set; } = new();
}

public class AssignCampaignRequest
{
    public string UserId { get; set; } = string.Empty;
}

public class AssignCampaignResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public AssignmentDTO? Assignment { get; set; }
}

public class AssignmentDTO
{
    public int AssignmentId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string CampaignId { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
    public string? UserName { get; set; }
    public string? CampaignType { get; set; }
}

public class UpdateStatusRequest
{
    public int AssignmentId { get; set; }
    public string NewStatus { get; set; } = string.Empty;
}

public class UpdateStatusResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class UserDTO
{
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Segment { get; set; } = string.Empty;
    public decimal? MonthlyDataGb { get; set; }
    public decimal? MonthlySpendTry { get; set; }
    public int? LoyaltyYears { get; set; }
    public decimal? Score { get; set; }
}

public class CampaignDTO
{
    public string CampaignId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string TargetSegment { get; set; } = string.Empty;
    public int Priority { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public int AssignmentCount { get; set; }
}

public class DailyDistributionDTO
{
    public DateTime Date { get; set; }
    public int TotalAssignments { get; set; }
    public int AssignedCount { get; set; }
    public int UsedCount { get; set; }
    public int ExpiredCount { get; set; }
}

public class CampaignUsageRateDTO
{
    public string CampaignId { get; set; } = string.Empty;
    public string CampaignType { get; set; } = string.Empty;
    public string TargetSegment { get; set; } = string.Empty;
    public int TotalAssignments { get; set; }
    public int AssignedCount { get; set; }
    public int UsedCount { get; set; }
    public int ExpiredCount { get; set; }
    public decimal UsageRate { get; set; }
}

public class UpdatePriorityRequest
{
    public int Priority { get; set; }
}
