using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RadiusConnect.Api.Services.Interfaces;

namespace RadiusConnect.Api.Controllers;

[Route("api/[controller]")]
public class DashboardController : BaseController
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("overview")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetOverviewStats()
    {
        try
        {
            var stats = await _dashboardService.GetOverviewStatisticsAsync();
            return Success(stats, "Overview statistics retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving overview statistics");
        }
    }

    [HttpGet("system-health")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetSystemHealthStats()
    {
        try
        {
            var stats = await _dashboardService.GetSystemHealthStatsAsync();
            return Success(stats, "System health statistics retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving system health statistics");
        }
    }

    [HttpGet("sessions")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetSessionStats([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var stats = await _dashboardService.GetSessionStatsAsync(startDate, endDate);
            return Success(stats, "Session statistics retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving session statistics");
        }
    }

    [HttpGet("sessions/hourly")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetHourlySessionStats([FromQuery] DateTime? date = null)
    {
        try
        {
            var targetDate = date ?? DateTime.Today;
            var stats = await _dashboardService.GetHourlySessionStatsAsync(targetDate);
            return Success(stats, "Hourly session statistics retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving hourly session statistics");
        }
    }

    [HttpGet("sessions/daily")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetDailySessionStats([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today;
            var stats = await _dashboardService.GetDailySessionStatsAsync(start, end);
            return Success(stats, "Daily session statistics retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving daily session statistics");
        }
    }

    [HttpGet("authentication")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetAuthenticationStats([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var stats = await _dashboardService.GetAuthenticationStatsAsync(startDate, endDate);
            return Success(stats, "Authentication statistics retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving authentication statistics");
        }
    }

    [HttpGet("authentication/hourly")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetHourlyAuthStats([FromQuery] DateTime? date = null)
    {
        try
        {
            var targetDate = date ?? DateTime.Today;
            var stats = await _dashboardService.GetHourlyAuthStatsAsync(targetDate);
            return Success(stats, "Hourly authentication statistics retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving hourly authentication statistics");
        }
    }

    [HttpGet("authentication/daily")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetDailyAuthStats([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today;
            var stats = await _dashboardService.GetDailyAuthStatsAsync(start, end);
            return Success(stats, "Daily authentication statistics retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving daily authentication statistics");
        }
    }

    [HttpGet("users")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetUserStats()
    {
        try
        {
            var stats = await _dashboardService.GetUserStatsAsync();
            return Success(stats, "User statistics retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving user statistics");
        }
    }

    [HttpGet("users/activity")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetUserActivityStats([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var stats = await _dashboardService.GetUserActivityStatsAsync(startDate, endDate);
            return Success(stats, "User activity statistics retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving user activity statistics");
        }
    }

    [HttpGet("users/top-active")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetTopActiveUsers([FromQuery] int limit = 10, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var users = await _dashboardService.GetTopActiveUsersAsync(limit, startDate, endDate);
            return Success(users, "Top active users retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving top active users");
        }
    }

    [HttpGet("groups")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetGroupStats()
    {
        try
        {
            var stats = await _dashboardService.GetGroupStatsAsync();
            return Success(stats, "Group statistics retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving group statistics");
        }
    }

    [HttpGet("groups/distribution")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetGroupDistributionStats()
    {
        try
        {
            var stats = await _dashboardService.GetGroupDistributionStatsAsync();
            return Success(stats, "Group distribution statistics retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving group distribution statistics");
        }
    }

    [HttpGet("network")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetNetworkStats([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var stats = await _dashboardService.GetNetworkStatsAsync(startDate, endDate);
            return Success(stats, "Network statistics retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving network statistics");
        }
    }

    [HttpGet("network/nas-usage")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetNasUsageStats([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var stats = await _dashboardService.GetNasUsageStatsAsync(startDate, endDate);
            return Success(stats, "NAS usage statistics retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving NAS usage statistics");
        }
    }

    [HttpGet("network/bandwidth")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetBandwidthStats([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var stats = await _dashboardService.GetBandwidthStatsAsync(startDate, endDate);
            return Success(stats, "Bandwidth statistics retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving bandwidth statistics");
        }
    }

    [HttpGet("real-time/active-sessions")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetRealTimeActiveSessions()
    {
        try
        {
            var sessions = await _dashboardService.GetRealTimeActiveSessionsAsync();
            return Success(sessions, "Real-time active sessions retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving real-time active sessions");
        }
    }

    [HttpGet("real-time/recent-authentications")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetRealTimeRecentAuthentications([FromQuery] int limit = 20)
    {
        try
        {
            var authentications = await _dashboardService.GetRealTimeRecentAuthenticationsAsync(limit);
            return Success(authentications, "Real-time recent authentications retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving real-time recent authentications");
        }
    }

    [HttpGet("real-time/system-alerts")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetRealTimeSystemAlerts([FromQuery] int limit = 10)
    {
        try
        {
            var alerts = await _dashboardService.GetRealTimeSystemAlertsAsync(limit);
            return Success(alerts, "Real-time system alerts retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving real-time system alerts");
        }
    }

    [HttpGet("reports/session-summary")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetSessionSummaryReport([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var report = await _dashboardService.GetSessionSummaryReportAsync(startDate, endDate);
            return Success(report, "Session summary report retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving session summary report");
        }
    }

    [HttpGet("reports/authentication-summary")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetAuthenticationSummaryReport([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var report = await _dashboardService.GetAuthenticationSummaryReportAsync(startDate, endDate);
            return Success(report, "Authentication summary report retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving authentication summary report");
        }
    }

    [HttpGet("reports/user-activity")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetUserActivityReport([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var report = await _dashboardService.GetUserActivityReportAsync(startDate, endDate);
            return Success(report, "User activity report retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving user activity report");
        }
    }

    [HttpGet("audit")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetAuditStats([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var stats = await _dashboardService.GetAuditStatsAsync(startDate, endDate);
            return Success(stats, "Audit statistics retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving audit statistics");
        }
    }

    [HttpGet("audit/recent-activities")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetRecentAuditActivities([FromQuery] int limit = 20)
    {
        try
        {
            var activities = await _dashboardService.GetRecentActivitiesAsync(limit);
            return Success(activities, "Recent audit activities retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving recent audit activities");
        }
    }

    [HttpGet("audit/top-actors")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetTopAuditActors([FromQuery] int limit = 10, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            // TODO: Implement GetTopAuditActorsAsync in IDashboardService
            var actors = new List<object>(); // Placeholder implementation
            return Success(actors, "Top audit actors retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving top audit actors");
        }
    }
}