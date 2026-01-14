# Turkcell Campaign Optimizer - Test Script
# Bu script API endpoint'lerini test eder

Write-Host "=== Turkcell Campaign Optimizer API Test ===" -ForegroundColor Cyan
Write-Host ""

$baseUrl = "http://localhost:5000/api"

# Test 1: Dashboard Summary
Write-Host "Test 1: Dashboard Summary" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/dashboard/summary" -Method Get
    Write-Host "✓ Success!" -ForegroundColor Green
    Write-Host "  Total Users: $($response.totalUsers)"
    Write-Host "  Active Campaigns: $($response.activeCampaigns)"
    Write-Host "  Success Rate: $($response.successRate)%"
    Write-Host "  Average Score: $($response.averageScore)"
} catch {
    Write-Host "✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 2: Get User U003
Write-Host "Test 2: Get User U003" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/users/U003" -Method Get
    Write-Host "✓ Success!" -ForegroundColor Green
    Write-Host "  Name: $($response.name)"
    Write-Host "  Segment: $($response.segment)"
    Write-Host "  Monthly Data: $($response.monthlyDataGb) GB"
    Write-Host "  Monthly Spend: $($response.monthlySpendTry) TRY"
    Write-Host "  Loyalty Years: $($response.loyaltyYears)"
    Write-Host "  Calculated Score: $($response.score)"
} catch {
    Write-Host "✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 3: Get All Users (First Page)
Write-Host "Test 3: Get All Users (Page 1)" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/users?page=1&pageSize=5" -Method Get
    Write-Host "✓ Success!" -ForegroundColor Green
    Write-Host "  Total Items: $($response.pagination.totalItems)"
    Write-Host "  Page Size: $($response.pagination.pageSize)"
    Write-Host "  Total Pages: $($response.pagination.totalPages)"
    Write-Host "  Users on this page: $($response.data.Count)"
} catch {
    Write-Host "✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 4: Get All Campaigns
Write-Host "Test 4: Get All Campaigns" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/campaigns?page=1&pageSize=10" -Method Get
    Write-Host "✓ Success!" -ForegroundColor Green
    Write-Host "  Total Campaigns: $($response.pagination.totalItems)"
    Write-Host "  Campaigns on this page: $($response.data.Count)"
    if ($response.data.Count -gt 0) {
        Write-Host "  First Campaign: $($response.data[0].campaignId) - $($response.data[0].type)"
    }
} catch {
    Write-Host "✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 5: Assign Campaign to U003
Write-Host "Test 5: Assign Campaign to U003" -ForegroundColor Yellow
try {
    $body = @{
        userId = "U003"
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri "$baseUrl/assign" -Method Post -Body $body -ContentType "application/json"
    
    if ($response.success) {
        Write-Host "✓ Success!" -ForegroundColor Green
        Write-Host "  Message: $($response.message)"
        Write-Host "  Assignment ID: $($response.assignment.assignmentId)"
        Write-Host "  Campaign ID: $($response.assignment.campaignId)"
        Write-Host "  Campaign Type: $($response.assignment.campaignType)"
        Write-Host "  Score: $($response.assignment.score)"
        Write-Host "  Status: $($response.assignment.status)"
        
        $assignmentId = $response.assignment.assignmentId
        
        # Test 6: Update Assignment Status
        Write-Host ""
        Write-Host "Test 6: Update Assignment Status to USED" -ForegroundColor Yellow
        try {
            $updateBody = @{
                assignmentId = $assignmentId
                newStatus = "USED"
            } | ConvertTo-Json

            $updateResponse = Invoke-RestMethod -Uri "$baseUrl/status-update" -Method Patch -Body $updateBody -ContentType "application/json"
            
            if ($updateResponse.success) {
                Write-Host "✓ Success!" -ForegroundColor Green
                Write-Host "  Message: $($updateResponse.message)"
            } else {
                Write-Host "✗ Failed: $($updateResponse.message)" -ForegroundColor Red
            }
        } catch {
            Write-Host "✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
        }
    } else {
        Write-Host "✗ Failed: $($response.message)" -ForegroundColor Red
    }
} catch {
    Write-Host "✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 7: Get All Assignments
Write-Host "Test 7: Get All Assignments" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/assignments?page=1&pageSize=10" -Method Get
    Write-Host "✓ Success!" -ForegroundColor Green
    Write-Host "  Total Assignments: $($response.pagination.totalItems)"
    Write-Host "  Assignments on this page: $($response.data.Count)"
    if ($response.data.Count -gt 0) {
        Write-Host "  Latest Assignment:"
        Write-Host "    User: $($response.data[0].userName) ($($response.data[0].userId))"
        Write-Host "    Campaign: $($response.data[0].campaignType)"
        Write-Host "    Status: $($response.data[0].status)"
        Write-Host "    Score: $($response.data[0].score)"
    }
} catch {
    Write-Host "✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Final Dashboard Check
Write-Host "Test 8: Final Dashboard Check" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/dashboard/summary" -Method Get
    Write-Host "✓ Success!" -ForegroundColor Green
    Write-Host "  Total Assignments: $($response.totalAssignments)"
    Write-Host "  Success Rate: $($response.successRate)%"
    Write-Host "  Average Score: $($response.averageScore)"
} catch {
    Write-Host "✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

Write-Host "=== Test Completed ===" -ForegroundColor Cyan
