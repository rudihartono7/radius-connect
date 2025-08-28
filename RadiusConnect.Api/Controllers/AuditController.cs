using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RadiusConnect.Api.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace RadiusConnect.Api.Controllers;

[Route("api/[controller]")]
public class AuditController : BaseController
{
    private readonly IAuditService _auditService;

    public AuditController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet("logs")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? actor = null,
        [FromQuery] string? entity = null,
        [FromQuery] string? action = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? search = null)
    {
        try
        {
            var logs = await _auditService.GetAuditLogsAsync(page, pageSize, actor, entity, action, startDate, endDate, search);
            var totalCount = await _auditService.GetTotalLogsCountAsync(actor, entity, action, startDate, endDate, search);

            return PagedResult(logs, totalCount, page, pageSize, "Audit logs retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving audit logs");
        }
    }

    [HttpGet("logs/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetAuditLog(string id)
    {
        try
        {
            var log = await _auditService.GetAuditLogByIdAsync(id);
            if (log == null)
            {
                return NotFound("Audit log not found");
            }

            return Success(log, "Audit log retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving audit log");
        }
    }

    [HttpGet("logs/actor/{actorId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetLogsByActor(
        string actorId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var logs = await _auditService.GetLogsByActorAsync(actorId, page, pageSize, startDate, endDate);
            var totalCount = await _auditService.GetLogsByActorCountAsync(actorId, startDate, endDate);

            return PagedResult(logs, totalCount, page, pageSize, "Actor audit logs retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving actor audit logs");
        }
    }

    [HttpGet("logs/entity/{entityType}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetLogsByEntity(
        string entityType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var logs = await _auditService.GetLogsByEntityAsync(entityType, page, pageSize, startDate, endDate);
            var totalCount = await _auditService.GetLogsByEntityCountAsync(entityType, startDate, endDate);

            return PagedResult(logs, totalCount, page, pageSize, "Entity audit logs retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving entity audit logs");
        }
    }

    [HttpGet("logs/date-range")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetLogsByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            if (startDate >= endDate)
            {
                return BadRequest("Start date must be before end date");
            }

            var logs = await _auditService.GetLogsByDateRangeAsync(startDate, endDate, page, pageSize);
            var totalCount = await _auditService.GetLogsByDateRangeCountAsync(startDate, endDate);

            return PagedResult(logs, totalCount, page, pageSize, "Date range audit logs retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving date range audit logs");
        }
    }

    [HttpGet("search")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> SearchAuditLogs(
        [FromQuery] string query,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Search query is required");
            }

            var logs = await _auditService.SearchAuditLogsAsync(query, page, pageSize, startDate, endDate);
            var totalCount = await _auditService.SearchAuditLogsCountAsync(query, startDate, endDate);

            return PagedResult(logs, totalCount, page, pageSize, "Search audit logs retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "searching audit logs");
        }
    }

    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetAuditStatistics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var stats = await _auditService.GetAuditStatisticsAsync(startDate, endDate);
            return Success(stats, "Audit statistics retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving audit statistics");
        }
    }

    [HttpGet("statistics/by-action")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetLogsByActionStats(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var stats = await _auditService.GetLogsByActionStatsAsync(startDate, endDate);
            return Success(stats, "Logs by action statistics retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving logs by action statistics");
        }
    }

    [HttpGet("statistics/by-entity")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetLogsByEntityStats(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var stats = await _auditService.GetLogsByEntityStatsAsync(startDate, endDate);
            return Success(stats, "Logs by entity statistics retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving logs by entity statistics");
        }
    }

    [HttpGet("statistics/by-date")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetLogsByDateStats(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string groupBy = "day")
    {
        try
        {
            if (!new[] { "hour", "day", "week", "month" }.Contains(groupBy.ToLower()))
            {
                return BadRequest("GroupBy must be one of: hour, day, week, month");
            }

            var stats = await _auditService.GetLogsByDateStatsAsync(startDate, endDate, groupBy);
            return Success(stats, "Logs by date statistics retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving logs by date statistics");
        }
    }

    [HttpGet("statistics/most-active-users")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetMostActiveUsers(
        [FromQuery] int limit = 10,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var users = await _auditService.GetMostActiveUsersAsync(limit, startDate, endDate);
            return Success(users, "Most active users retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving most active users");
        }
    }

    [HttpPost("cleanup")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CleanupOldLogs([FromBody] CleanupLogsRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationError("Invalid cleanup request", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            }

            if (request.OlderThanDays < 30)
            {
                return BadRequest("Cannot delete logs newer than 30 days for compliance reasons");
            }

            var deletedCount = await _auditService.CleanupOldLogsAsync(request.OlderThanDays);
            
            // Log the cleanup action
            var currentUserId = GetCurrentUserId();
            await _auditService.LogSystemActionAsync(currentUserId, "AuditCleanup", "Execute", null, $"Cleaned up {deletedCount} audit logs older than {request.OlderThanDays} days");

            return Success(new { DeletedCount = deletedCount }, $"Successfully cleaned up {deletedCount} old audit logs");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "cleaning up old logs");
        }
    }

    [HttpPost("archive")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ArchiveOldLogs([FromBody] ArchiveLogsRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationError("Invalid archive request", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            }

            var archivedCount = await _auditService.ArchiveOldLogsAsync(request.OlderThanDays, request.ArchivePath);
            
            // Log the archive action
            var currentUserId = GetCurrentUserId();
            await _auditService.LogSystemActionAsync(currentUserId, "AuditArchive", "Execute", null, $"Archived {archivedCount} audit logs older than {request.OlderThanDays} days to {request.ArchivePath}");

            return Success(new { ArchivedCount = archivedCount }, $"Successfully archived {archivedCount} old audit logs");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "archiving old logs");
        }
    }

    [HttpGet("export/csv")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ExportToCsv(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? actor = null,
        [FromQuery] string? entity = null,
        [FromQuery] string? action = null)
    {
        try
        {
            var csvData = await _auditService.ExportToCsvAsync(startDate, endDate, actor, entity, action);
            
            // Log the export action
            var currentUserId = GetCurrentUserId();
            await _auditService.LogSystemActionAsync(currentUserId, "AuditExport", "CSV", null, $"Exported audit logs to CSV by {GetCurrentUsername()}");

            var fileName = $"audit_logs_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            return File(System.Text.Encoding.UTF8.GetBytes(csvData), "text/csv", fileName);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "exporting audit logs to CSV");
        }
    }

    [HttpGet("export/json")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ExportToJson(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? actor = null,
        [FromQuery] string? entity = null,
        [FromQuery] string? action = null)
    {
        try
        {
            var jsonData = await _auditService.ExportToJsonAsync(startDate, endDate, actor, entity, action);
            
            // Log the export action
            var currentUserId = GetCurrentUserId();
            await _auditService.LogSystemActionAsync(currentUserId, "AuditExport", "JSON", null, $"Exported audit logs to JSON by {GetCurrentUsername()}");

            var fileName = $"audit_logs_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            return File(System.Text.Encoding.UTF8.GetBytes(jsonData), "application/json", fileName);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "exporting audit logs to JSON");
        }
    }

    [HttpGet("export/xml")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ExportToXml(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? actor = null,
        [FromQuery] string? entity = null,
        [FromQuery] string? action = null)
    {
        try
        {
            var xmlData = await _auditService.ExportToXmlAsync(startDate, endDate, actor, entity, action);
            
            // Log the export action
            var currentUserId = GetCurrentUserId();
            await _auditService.LogSystemActionAsync(currentUserId, "AuditExport", "XML", null, $"Exported audit logs to XML by {GetCurrentUsername()}");

            var fileName = $"audit_logs_{DateTime.Now:yyyyMMdd_HHmmss}.xml";
            return File(System.Text.Encoding.UTF8.GetBytes(xmlData), "application/xml", fileName);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "exporting audit logs to XML");
        }
    }

    [HttpGet("reports/summary")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetAuditSummaryReport(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var report = await _auditService.GenerateAuditSummaryReportAsync(startDate, endDate);
            return Success(report, "Audit summary report generated successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "generating audit summary report");
        }
    }

    [HttpGet("reports/detailed")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetDetailedAuditReport(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? actor = null,
        [FromQuery] string? entity = null)
    {
        try
        {
            var report = await _auditService.GenerateDetailedAuditReportAsync(startDate, endDate, actor, entity);
            return Success(report, "Detailed audit report generated successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "generating detailed audit report");
        }
    }

    [HttpGet("security/alerts")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetSecurityAlerts([FromQuery] int limit = 20)
    {
        try
        {
            var alerts = await _auditService.GetSecurityAlertsAsync(limit);
            return Success(alerts, "Security alerts retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving security alerts");
        }
    }

    [HttpGet("security/anomalies")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DetectAnomalies(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var anomalies = await _auditService.DetectAnomaliesAsync(startDate, endDate);
            return Success(anomalies, "Security anomalies detected successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "detecting security anomalies");
        }
    }
}

// DTOs
public class CleanupLogsRequest
{
    [Required]
    [Range(30, int.MaxValue, ErrorMessage = "Must be at least 30 days for compliance reasons")]
    public int OlderThanDays { get; set; }
}

public class ArchiveLogsRequest
{
    [Required]
    [Range(30, int.MaxValue, ErrorMessage = "Must be at least 30 days for compliance reasons")]
    public int OlderThanDays { get; set; }

    [Required]
    public string ArchivePath { get; set; } = string.Empty;
}