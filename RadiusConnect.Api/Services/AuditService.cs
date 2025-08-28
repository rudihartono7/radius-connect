using Microsoft.EntityFrameworkCore;
using RadiusConnect.Api.Data;
using RadiusConnect.Api.Models.Domain;
using RadiusConnect.Api.Services.Interfaces;
using System.Net;
using System.Text;
using System.Text.Json;

namespace RadiusConnect.Api.Services;

public class AuditService : IAuditService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        AppDbContext context,
        ILogger<AuditService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Audit log retrieval methods
    public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(int page = 1, int pageSize = 10)
    {
        _logger.LogDebug("Getting audit logs, page {Page}, pageSize {PageSize}", page, pageSize);
        
        return await _context.AuditLogs
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetAuditLogsByActorAsync(Guid actorId, int page = 1, int pageSize = 10)
    {
        _logger.LogDebug("Getting audit logs for actor {ActorId}, page {Page}, pageSize {PageSize}", actorId, page, pageSize);
        
        return await _context.AuditLogs
            .Where(a => a.ActorId == actorId)
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetAuditLogsByEntityAsync(string entity, int page = 1, int pageSize = 10)
    {
        _logger.LogDebug("Getting audit logs for entity {Entity}, page {Page}, pageSize {PageSize}", entity, page, pageSize);
        
        return await _context.AuditLogs
            .Where(a => a.Entity == entity)
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetAuditLogsByEntityIdAsync(string entity, string entityId, int page = 1, int pageSize = 10)
    {
        _logger.LogDebug("Getting audit logs for entity {Entity}, entityId {EntityId}, page {Page}, pageSize {PageSize}", entity, entityId, page, pageSize);
        
        return await _context.AuditLogs
            .Where(a => a.Entity == entity && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetAuditLogsByActionAsync(string action, int page = 1, int pageSize = 10)
    {
        _logger.LogDebug("Getting audit logs for action {Action}, page {Page}, pageSize {PageSize}", action, page, pageSize);
        
        return await _context.AuditLogs
            .Where(a => a.Action == action)
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate, int page = 1, int pageSize = 10)
    {
        _logger.LogDebug("Getting audit logs from {StartDate} to {EndDate}, page {Page}, pageSize {PageSize}", startDate, endDate, page, pageSize);
        
        return await _context.AuditLogs
            .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> SearchAuditLogsAsync(string searchTerm, int page = 1, int pageSize = 10)
    {
        _logger.LogDebug("Searching audit logs for term {SearchTerm}, page {Page}, pageSize {PageSize}", searchTerm, page, pageSize);
        
        return await _context.AuditLogs
            .Where(a => a.Action.Contains(searchTerm) || 
                       a.Entity.Contains(searchTerm) || 
                       (a.EntityId != null && a.EntityId.Contains(searchTerm)))
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalAuditLogsCountAsync()
    {
        _logger.LogDebug("Getting total audit logs count");
        
        return await _context.AuditLogs.CountAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetRecentAuditLogsAsync(int hours = 24, int page = 1, int pageSize = 50)
    {
        _logger.LogDebug("Getting recent audit logs for {Hours} hours, page {Page}, pageSize {PageSize}", hours, page, pageSize);
        
        var cutoffTime = DateTime.UtcNow.AddHours(-hours);
        
        return await _context.AuditLogs
            .Where(a => a.Timestamp >= cutoffTime)
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetAuditLogsCountAsync(Guid? actorId = null, string? entity = null, string? action = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        _logger.LogDebug("Getting audit logs count with filters");
        
        var query = _context.AuditLogs.AsQueryable();

        if (actorId.HasValue)
            query = query.Where(a => a.ActorId == actorId.Value);

        if (!string.IsNullOrEmpty(entity))
            query = query.Where(a => a.Entity == entity);

        if (!string.IsNullOrEmpty(action))
            query = query.Where(a => a.Action == action);

        if (startDate.HasValue)
            query = query.Where(a => a.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.Timestamp <= endDate.Value);

        return await query.CountAsync();
    }

    // Audit log creation methods
    public async Task LogUserActionAsync(Guid? actorId, string action, string entity, string? entityId = null, object? beforeData = null, object? afterData = null, IPAddress? ipAddress = null, string? userAgent = null)
    {
        _logger.LogInformation("Logging user action: {Action} on {Entity} by {ActorId}", action, entity, actorId);
        
        var auditLog = new AuditLog
        {
            ActorId = actorId,
            Action = action,
            Entity = entity,
            EntityId = entityId,
            BeforeData = beforeData,
            AfterData = afterData,
            IpAddress = ipAddress?.ToString(),
            UserAgent = userAgent,
            Timestamp = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }

    public async Task LogSystemActionAsync(string action, string entity, string? entityId = null, object? beforeData = null, object? afterData = null)
    {
        _logger.LogInformation("Logging system action: {Action} on {Entity}", action, entity);
        
        var auditLog = new AuditLog
        {
            ActorId = null, // System action
            Action = action,
            Entity = entity,
            EntityId = entityId,
            BeforeData = beforeData,
            AfterData = afterData,
            IpAddress = null,
            UserAgent = "System",
            Timestamp = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }

    public async Task LogAuthenticationAttemptAsync(string username, bool success, IPAddress? ipAddress = null, string? userAgent = null, string? failureReason = null)
    {
        _logger.LogInformation("Logging authentication attempt for {Username}, success: {Success}", username, success);
        
        var action = success ? "LOGIN_SUCCESS" : "LOGIN_FAILED";
        var details = new
        {
            Username = username,
            Success = success,
            FailureReason = failureReason
        };

        var auditLog = new AuditLog
        {
            ActorId = null,
            Action = action,
            Entity = "Authentication",
            EntityId = username,
            BeforeData = null,
            AfterData = details,
            IpAddress = ipAddress?.ToString(),
            UserAgent = userAgent,
            Timestamp = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }

    public async Task LogPasswordChangeAsync(Guid userId, bool success, IPAddress? ipAddress = null, string? userAgent = null)
    {
        _logger.LogInformation("Logging password change for user {UserId}, success: {Success}", userId, success);
        
        var action = success ? "PASSWORD_CHANGED" : "PASSWORD_CHANGE_FAILED";
        var details = new
        {
            UserId = userId,
            Success = success
        };

        var auditLog = new AuditLog
        {
            ActorId = userId,
            Action = action,
            Entity = "User",
            EntityId = userId.ToString(),
            BeforeData = null,
            AfterData = details,
            IpAddress = ipAddress?.ToString(),
            UserAgent = userAgent,
            Timestamp = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }

    public async Task LogRoleAssignmentAsync(Guid actorId, Guid targetUserId, string roleName, bool isAssignment, IPAddress? ipAddress = null, string? userAgent = null)
    {
        _logger.LogInformation("Logging role assignment for user {TargetUserId} by {ActorId}, role: {RoleName}, assignment: {IsAssignment}", targetUserId, actorId, roleName, isAssignment);
        
        var action = isAssignment ? "ROLE_ASSIGNED" : "ROLE_REMOVED";
        var details = new
        {
            TargetUserId = targetUserId,
            RoleName = roleName,
            IsAssignment = isAssignment
        };

        var auditLog = new AuditLog
        {
            ActorId = actorId,
            Action = action,
            Entity = "User",
            EntityId = targetUserId.ToString(),
            BeforeData = null,
            AfterData = details,
            IpAddress = ipAddress?.ToString(),
            UserAgent = userAgent,
            Timestamp = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }

    public async Task LogPasswordChangeAsync(Guid actorId, Guid targetUserId, bool success, IPAddress? ipAddress = null, string? userAgent = null)
    {
        _logger.LogInformation("Logging password change for user {TargetUserId} by {ActorId}, success: {Success}", targetUserId, actorId, success);
        
        var details = new
        {
            TargetUserId = targetUserId,
            Success = success
        };

        var auditLog = new AuditLog
        {
            ActorId = actorId,
            Action = success ? "PASSWORD_CHANGED" : "PASSWORD_CHANGE_FAILED",
            Entity = "User",
            EntityId = targetUserId.ToString(),
            AfterData = details,
            IpAddress = ipAddress?.ToString(),
            UserAgent = userAgent,
            Timestamp = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }

    // Specific entity logging
    public async Task LogUserCreatedAsync(Guid actorId, AppUser user, IPAddress? ipAddress = null, string? userAgent = null)
    {
        await LogUserActionAsync(actorId, "USER_CREATED", "User", user.Id.ToString(), null, user, ipAddress, userAgent);
    }

    public async Task LogUserUpdatedAsync(Guid actorId, AppUser oldUser, AppUser newUser, IPAddress? ipAddress = null, string? userAgent = null)
    {
        await LogUserActionAsync(actorId, "USER_UPDATED", "User", newUser.Id.ToString(), oldUser, newUser, ipAddress, userAgent);
    }

    public async Task LogUserDeletedAsync(Guid actorId, AppUser user, IPAddress? ipAddress = null, string? userAgent = null)
    {
        await LogUserActionAsync(actorId, "USER_DELETED", "User", user.Id.ToString(), user, null, ipAddress, userAgent);
    }

    public async Task LogRadiusUserCreatedAsync(Guid actorId, string username, IPAddress? ipAddress = null, string? userAgent = null)
    {
        await LogUserActionAsync(actorId, "RADIUS_USER_CREATED", "RadiusUser", username, null, new { Username = username }, ipAddress, userAgent);
    }

    public async Task LogRadiusUserUpdatedAsync(Guid actorId, string username, object? beforeData, object? afterData, IPAddress? ipAddress = null, string? userAgent = null)
    {
        await LogUserActionAsync(actorId, "RADIUS_USER_UPDATED", "RadiusUser", username, beforeData, afterData, ipAddress, userAgent);
    }

    public async Task LogRadiusUserDeletedAsync(Guid actorId, string username, IPAddress? ipAddress = null, string? userAgent = null)
    {
        await LogUserActionAsync(actorId, "RADIUS_USER_DELETED", "RadiusUser", username, new { Username = username }, null, ipAddress, userAgent);
    }

    public async Task LogRadiusUserActionAsync(Guid actorId, string action, string username, IPAddress? ipAddress = null, string? userAgent = null)
    {
        await LogUserActionAsync(actorId, action, "RadiusUser", username, null, new { Username = username, Action = action }, ipAddress, userAgent);
    }

    public async Task LogRadiusGroupActionAsync(Guid actorId, string entity, string action, string groupName, string description, IPAddress? ipAddress = null, string? userAgent = null)
    {
        await LogUserActionAsync(actorId, action, entity, groupName, null, new { GroupName = groupName, Description = description }, ipAddress, userAgent);
    }

    public async Task LogGroupCreatedAsync(Guid actorId, string groupName, IPAddress? ipAddress = null, string? userAgent = null)
    {
        await LogUserActionAsync(actorId, "GROUP_CREATED", "RadiusGroup", groupName, null, new { GroupName = groupName }, ipAddress, userAgent);
    }

    public async Task LogGroupUpdatedAsync(Guid actorId, string groupName, object? beforeData, object? afterData, IPAddress? ipAddress = null, string? userAgent = null)
    {
        await LogUserActionAsync(actorId, "GROUP_UPDATED", "RadiusGroup", groupName, beforeData, afterData, ipAddress, userAgent);
    }

    public async Task LogGroupDeletedAsync(Guid actorId, string groupName, IPAddress? ipAddress = null, string? userAgent = null)
    {
        await LogUserActionAsync(actorId, "GROUP_DELETED", "RadiusGroup", groupName, new { GroupName = groupName }, null, ipAddress, userAgent);
    }

    public async Task LogSessionDisconnectedAsync(Guid actorId, string username, string acctSessionId, IPAddress? ipAddress = null, string? userAgent = null)
    {
        var details = new
        {
            Username = username,
            AcctSessionId = acctSessionId
        };

        await LogUserActionAsync(actorId, "SESSION_DISCONNECTED", "RadiusSession", acctSessionId, null, details, ipAddress, userAgent);
    }

    public async Task LogCoaRequestAsync(Guid actorId, string username, string acctSessionId, string requestType, IPAddress? ipAddress = null, string? userAgent = null)
    {
        var details = new
        {
            Username = username,
            AcctSessionId = acctSessionId,
            RequestType = requestType
        };

        await LogUserActionAsync(actorId, "COA_REQUEST", "RadiusSession", acctSessionId, null, details, ipAddress, userAgent);
    }

    public async Task LogSettingsChangedAsync(Guid actorId, string settingKey, object? oldValue, object? newValue, IPAddress? ipAddress = null, string? userAgent = null)
    {
        var beforeData = new { Key = settingKey, Value = oldValue };
        var afterData = new { Key = settingKey, Value = newValue };

        await LogUserActionAsync(actorId, "SETTINGS_CHANGED", "Settings", settingKey, beforeData, afterData, ipAddress, userAgent);
    }

    // Audit statistics
    public async Task<Dictionary<string, int>> GetAuditStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        _logger.LogDebug("Getting audit statistics");
        
        var query = _context.AuditLogs.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(a => a.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.Timestamp <= endDate.Value);

        var stats = await query
            .GroupBy(a => a.Action)
            .Select(g => new { Action = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Action, x => x.Count);

        return stats;
    }

    public async Task<IEnumerable<object>> GetAuditTrendsAsync(int days = 30)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);

        var trends = await _context.AuditLogs
            .Where(a => a.Timestamp >= startDate)
            .GroupBy(a => a.Timestamp.Date)
            .Select(g => new
            {
                Date = g.Key,
                Count = g.Count(),
                Actions = g.GroupBy(x => x.Action).Select(ag => new { Action = ag.Key, Count = ag.Count() })
            })
            .OrderBy(x => x.Date)
            .ToListAsync();

        return trends;
    }

    public async Task<Dictionary<string, int>> GetAuditActionsByTypeAsync()
    {
        var actionsByType = await _context.AuditLogs
            .GroupBy(a => a.Action)
            .Select(g => new { Action = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Action, x => x.Count);

        return actionsByType;
    }

    public async Task<IEnumerable<object>> GetMostActiveUsersAsync(int count = 10)
    {
        var activeUsers = await _context.AuditLogs
            .Where(a => a.ActorId != null)
            .GroupBy(a => a.ActorId)
            .Select(g => new
            {
                ActorId = g.Key,
                ActionCount = g.Count(),
                LastActivity = g.Max(x => x.Timestamp)
            })
            .OrderByDescending(x => x.ActionCount)
            .Take(count)
            .ToListAsync();

        return activeUsers;
    }

    public async Task<Dictionary<string, int>> GetAuditsByEntityAsync()
    {
        var auditsByEntity = await _context.AuditLogs
            .GroupBy(a => a.Entity)
            .Select(g => new { Entity = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Entity, x => x.Count);

        return auditsByEntity;
    }

    public async Task<IEnumerable<object>> GetRecentCriticalActionsAsync(int count = 20)
    {
        var criticalActions = new[] { "LOGIN_FAILED", "UNAUTHORIZED_ACCESS", "PASSWORD_CHANGED", "ROLE_ASSIGNED", "USER_DELETED", "SETTINGS_CHANGED" };

        var recentCritical = await _context.AuditLogs
            .Where(a => criticalActions.Contains(a.Action))
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .Select(a => new
            {
                a.Id,
                a.Action,
                a.Entity,
                a.EntityId,
                a.ActorId,
                a.Timestamp,
                a.IpAddress
            })
            .ToListAsync();

        return recentCritical;
    }

    public async Task<Dictionary<string, int>> GetEntityStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        _logger.LogDebug("Getting entity statistics");
        
        var query = _context.AuditLogs.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(a => a.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.Timestamp <= endDate.Value);

        var stats = await query
            .GroupBy(a => a.Entity)
            .Select(g => new { Entity = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Entity, x => x.Count);

        return stats;
    }

    public async Task<Dictionary<string, int>> GetUserActivityStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        _logger.LogDebug("Getting user activity statistics");
        
        var query = _context.AuditLogs.Where(a => a.ActorId.HasValue);

        if (startDate.HasValue)
            query = query.Where(a => a.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.Timestamp <= endDate.Value);

        var stats = await query
            .GroupBy(a => a.ActorId)
            .Select(g => new { UserId = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId!, x => x.Count);

        return stats;
    }

    // Audit maintenance
    public async Task CleanupOldAuditLogsAsync(int retentionDays = 365)
    {
        _logger.LogInformation("Cleaning up audit logs older than {RetentionDays} days", retentionDays);
        
        var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
        
        var oldLogs = await _context.AuditLogs
            .Where(a => a.Timestamp < cutoffDate)
            .ToListAsync();

        if (oldLogs.Any())
        {
            _context.AuditLogs.RemoveRange(oldLogs);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Cleaned up {Count} old audit logs", oldLogs.Count);
        }
    }

    public async Task ArchiveAuditLogsAsync(DateTime cutoffDate)
    {
        _logger.LogInformation("Archiving audit logs before {CutoffDate}", cutoffDate);
        
        // In a real implementation, you might move logs to an archive table or external storage
        // For now, we'll just log the action
        var logsToArchive = await _context.AuditLogs
            .Where(a => a.Timestamp < cutoffDate)
            .CountAsync();

        _logger.LogInformation("Would archive {Count} audit logs", logsToArchive);
    }

    public async Task<long> GetAuditLogsSizeAsync()
    {
        _logger.LogDebug("Getting audit logs size");
        
        // This is a simplified calculation - in reality you'd want to calculate actual storage size
        var count = await _context.AuditLogs.CountAsync();
        
        // Estimate average size per log entry (rough calculation)
        const int averageSizePerLog = 500; // bytes
        
        return count * averageSizePerLog;
    }

    // Export and reporting
    public async Task<byte[]> ExportAuditLogsAsync(DateTime startDate, DateTime endDate, string format = "csv")
    {
        _logger.LogInformation("Exporting audit logs from {StartDate} to {EndDate} in {Format} format", startDate, endDate, format);
        
        var logs = await _context.AuditLogs
            .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
            .OrderBy(a => a.Timestamp)
            .ToListAsync();

        if (format.ToLower() == "csv")
        {
            var csv = new StringBuilder();
            csv.AppendLine("Timestamp,Actor,Action,Entity,EntityId,BeforeData,AfterData,IpAddress,UserAgent");
            
            foreach (var log in logs)
            {
                csv.AppendLine($"{log.Timestamp:yyyy-MM-dd HH:mm:ss},{log.ActorId},{log.Action},{log.Entity},{log.EntityId},\"{log.BeforeDataJson?.Replace("\"", "\"\"")}\",\"{log.AfterDataJson?.Replace("\"", "\"\"")}\",{log.IpAddress},{log.UserAgent}");
            }
            
            return Encoding.UTF8.GetBytes(csv.ToString());
        }
        
        // Default to JSON if format not recognized
        var json = JsonSerializer.Serialize(logs, new JsonSerializerOptions { WriteIndented = true });
        return Encoding.UTF8.GetBytes(json);
    }

    public async Task<Dictionary<string, object>> GenerateAuditReportAsync(DateTime startDate, DateTime endDate)
    {
        _logger.LogInformation("Generating audit report from {StartDate} to {EndDate}", startDate, endDate);
        
        var logs = await _context.AuditLogs
            .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
            .ToListAsync();

        var report = new Dictionary<string, object>
        {
            ["period"] = new { startDate, endDate },
            ["totalEvents"] = logs.Count,
            ["uniqueActors"] = logs.Where(l => l.ActorId.HasValue).Select(l => l.ActorId).Distinct().Count(),
            ["actionBreakdown"] = logs.GroupBy(l => l.Action).ToDictionary(g => g.Key, g => g.Count()),
            ["entityBreakdown"] = logs.GroupBy(l => l.Entity).ToDictionary(g => g.Key, g => g.Count()),
            ["dailyActivity"] = logs.GroupBy(l => l.Timestamp.Date).ToDictionary(g => g.Key.ToString("yyyy-MM-dd"), g => g.Count())
        };

        return report;
    }

    public async Task<IEnumerable<object>> GetComplianceReportAsync(DateTime startDate, DateTime endDate)
    {
        _logger.LogInformation("Generating compliance report from {StartDate} to {EndDate}", startDate, endDate);
        
        var logs = await _context.AuditLogs
            .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
            .ToListAsync();

        var complianceEvents = logs.Where(l => 
            l.Action.Contains("LOGIN") || 
            l.Action.Contains("PASSWORD") || 
            l.Action.Contains("ROLE") || 
            l.Action.Contains("PERMISSION") ||
            l.Action.Contains("ACCESS"))
            .Select(l => new
            {
                l.Timestamp,
                l.ActorId,
                l.Action,
                l.Entity,
                l.EntityId,
                l.IpAddress,
                ComplianceCategory = GetComplianceCategory(l.Action)
            });

        return complianceEvents;
    }

    // Security monitoring
    public async Task<IEnumerable<object>> GetSuspiciousActivitiesAsync(int hours = 24)
    {
        _logger.LogInformation("Getting suspicious activities for the last {Hours} hours", hours);
        
        var cutoffTime = DateTime.UtcNow.AddHours(-hours);
        
        var suspiciousLogs = await _context.AuditLogs
            .Where(a => a.Timestamp >= cutoffTime && 
                       (a.Action.Contains("FAILED") || 
                        a.Action.Contains("UNAUTHORIZED") || 
                        a.Action.Contains("BLOCKED")))
            .GroupBy(a => new { a.ActorId, a.IpAddress })
            .Where(g => g.Count() > 5) // More than 5 suspicious activities
            .Select(g => new
            {
                ActorId = g.Key.ActorId,
                IpAddress = g.Key.IpAddress,
                Count = g.Count(),
                FirstOccurrence = g.Min(x => x.Timestamp),
                LastOccurrence = g.Max(x => x.Timestamp),
                Actions = g.Select(x => x.Action).Distinct()
            })
            .ToListAsync();

        return suspiciousLogs;
    }

    public async Task<IEnumerable<object>> GetFailedLoginAttemptsAsync(int hours = 24, int threshold = 5)
    {
        _logger.LogInformation("Getting failed login attempts for the last {Hours} hours with threshold {Threshold}", hours, threshold);
        
        var cutoffTime = DateTime.UtcNow.AddHours(-hours);
        
        var failedLogins = await _context.AuditLogs
            .Where(a => a.Timestamp >= cutoffTime && a.Action == "LOGIN_FAILED")
            .GroupBy(a => new { a.ActorId, a.IpAddress })
            .Where(g => g.Count() >= threshold)
            .Select(g => new
            {
                ActorId = g.Key.ActorId,
                IpAddress = g.Key.IpAddress,
                FailedAttempts = g.Count(),
                FirstAttempt = g.Min(x => x.Timestamp),
                LastAttempt = g.Max(x => x.Timestamp)
            })
            .ToListAsync();

        return failedLogins;
    }

    public async Task<IEnumerable<object>> GetUnauthorizedAccessAttemptsAsync(int hours = 24)
    {
        _logger.LogInformation("Getting unauthorized access attempts for the last {Hours} hours", hours);
        
        var cutoffTime = DateTime.UtcNow.AddHours(-hours);
        
        var unauthorizedAttempts = await _context.AuditLogs
            .Where(a => a.Timestamp >= cutoffTime && 
                       (a.Action.Contains("UNAUTHORIZED") || a.Action.Contains("ACCESS_DENIED")))
            .Select(a => new
            {
                a.Timestamp,
                a.ActorId,
                a.Action,
                a.Entity,
                a.EntityId,
                a.IpAddress,
                a.UserAgent
            })
            .ToListAsync();

        return unauthorizedAttempts;
    }

    public async Task<bool> CheckForAnomalousActivityAsync(Guid userId, string action)
    {
        _logger.LogDebug("Checking for anomalous activity for user {UserId} and action {Action}", userId, action);
        
        var recentActivity = await _context.AuditLogs
            .Where(a => a.ActorId == userId && a.Timestamp >= DateTime.UtcNow.AddHours(-1))
            .CountAsync();

        // Simple anomaly detection: more than 50 actions in the last hour
        var isAnomalous = recentActivity > 50;
        
        if (isAnomalous)
        {
            _logger.LogWarning("Anomalous activity detected for user {UserId}: {Count} actions in the last hour", userId, recentActivity);
        }

        return isAnomalous;
    }

    private string GetComplianceCategory(string action)
    {
        return action switch
        {
            var a when a.Contains("LOGIN") => "Authentication",
            var a when a.Contains("PASSWORD") => "Password Management",
            var a when a.Contains("ROLE") || a.Contains("PERMISSION") => "Authorization",
            var a when a.Contains("ACCESS") => "Access Control",
            _ => "General"
        };
    }

    // Additional audit log retrieval methods for AuditController
    public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(int page, int pageSize, string? actor, string? entity, string? action, DateTime? startDate, DateTime? endDate, string? search)
    {
        _logger.LogDebug("Getting filtered audit logs");
        
        var query = _context.AuditLogs.AsQueryable();

        if (!string.IsNullOrEmpty(actor))
            query = query.Where(a => a.ActorId.ToString().Contains(actor) || (a.Actor != null && a.Actor.UserName.Contains(actor)));

        if (!string.IsNullOrEmpty(entity))
            query = query.Where(a => a.Entity == entity);

        if (!string.IsNullOrEmpty(action))
            query = query.Where(a => a.Action == action);

        if (startDate.HasValue)
            query = query.Where(a => a.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.Timestamp <= endDate.Value);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(a => a.BeforeDataJson != null && a.BeforeDataJson.Contains(search) || 
                                   a.AfterDataJson != null && a.AfterDataJson.Contains(search) || 
                                   (a.Actor != null && a.Actor.UserName.Contains(search)));

        return await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<AuditLog?> GetAuditLogByIdAsync(string id)
    {
        _logger.LogDebug("Getting audit log by ID {Id}", id);
        
        if (long.TryParse(id, out var longId))
        {
            return await _context.AuditLogs.FirstOrDefaultAsync(a => a.Id == longId);
        }
        
        return null;
    }

    public async Task<IEnumerable<AuditLog>> GetLogsByActorAsync(string actorId, int page, int pageSize, DateTime? startDate, DateTime? endDate)
    {
        _logger.LogDebug("Getting logs by actor {ActorId}", actorId);
        
        var query = _context.AuditLogs.AsQueryable();

        if (Guid.TryParse(actorId, out var guidActorId))
            query = query.Where(a => a.ActorId == guidActorId);
        else
            query = query.Where(a => a.Actor != null && a.Actor.UserName.Contains(actorId));

        if (startDate.HasValue)
            query = query.Where(a => a.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.Timestamp <= endDate.Value);

        return await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetLogsByEntityAsync(string entityType, int page, int pageSize, DateTime? startDate, DateTime? endDate)
    {
        _logger.LogDebug("Getting logs by entity {EntityType}", entityType);
        
        var query = _context.AuditLogs.Where(a => a.Entity == entityType);

        if (startDate.HasValue)
            query = query.Where(a => a.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.Timestamp <= endDate.Value);

        return await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate, int page, int pageSize)
    {
        _logger.LogDebug("Getting logs by date range {StartDate} to {EndDate}", startDate, endDate);
        
        return await _context.AuditLogs
            .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> SearchAuditLogsAsync(string query, int page, int pageSize, DateTime? startDate, DateTime? endDate)
    {
        _logger.LogDebug("Searching audit logs with query {Query}", query);
        
        var auditQuery = _context.AuditLogs.AsQueryable();

        auditQuery = auditQuery.Where(a => 
            a.BeforeDataJson != null && a.BeforeDataJson.Contains(query) || 
            a.AfterDataJson != null && a.AfterDataJson.Contains(query) ||
            (a.Actor != null && a.Actor.UserName.Contains(query)) ||
            a.Action.Contains(query) ||
            a.Entity.Contains(query));

        if (startDate.HasValue)
            auditQuery = auditQuery.Where(a => a.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            auditQuery = auditQuery.Where(a => a.Timestamp <= endDate.Value);

        return await auditQuery
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    // Count methods
    public async Task<int> GetTotalLogsCountAsync(string? actor, string? entity, string? action, DateTime? startDate, DateTime? endDate, string? search)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (!string.IsNullOrEmpty(actor))
            query = query.Where(a => a.ActorId.ToString().Contains(actor) || (a.Actor != null && a.Actor.UserName.Contains(actor)));

        if (!string.IsNullOrEmpty(entity))
            query = query.Where(a => a.Entity == entity);

        if (!string.IsNullOrEmpty(action))
            query = query.Where(a => a.Action == action);

        if (startDate.HasValue)
            query = query.Where(a => a.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.Timestamp <= endDate.Value);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(a => a.BeforeDataJson != null && a.BeforeDataJson.Contains(search) || 
                                   a.AfterDataJson != null && a.AfterDataJson.Contains(search) || 
                                   (a.Actor != null && a.Actor.UserName.Contains(search)));

        return await query.CountAsync();
    }

    public async Task<int> GetLogsByActorCountAsync(string actorId, DateTime? startDate, DateTime? endDate)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (Guid.TryParse(actorId, out var guidActorId))
            query = query.Where(a => a.ActorId == guidActorId);
        else
            query = query.Where(a => a.Actor != null && a.Actor.UserName.Contains(actorId));

        if (startDate.HasValue)
            query = query.Where(a => a.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.Timestamp <= endDate.Value);

        return await query.CountAsync();
    }

    public async Task<int> GetLogsByEntityCountAsync(string entityType, DateTime? startDate, DateTime? endDate)
    {
        var query = _context.AuditLogs.Where(a => a.Entity == entityType);

        if (startDate.HasValue)
            query = query.Where(a => a.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.Timestamp <= endDate.Value);

        return await query.CountAsync();
    }

    public async Task<int> GetLogsByDateRangeCountAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.AuditLogs
            .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
            .CountAsync();
    }

    public async Task<int> SearchAuditLogsCountAsync(string query, DateTime? startDate, DateTime? endDate)
    {
        var auditQuery = _context.AuditLogs.AsQueryable();

        auditQuery = auditQuery.Where(a => 
            a.BeforeDataJson != null && a.BeforeDataJson.Contains(query) || 
            a.AfterDataJson != null && a.AfterDataJson.Contains(query) ||
            (a.Actor != null && a.Actor.UserName.Contains(query)) ||
            a.Action.Contains(query) ||
            a.Entity.Contains(query));

        if (startDate.HasValue)
            auditQuery = auditQuery.Where(a => a.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            auditQuery = auditQuery.Where(a => a.Timestamp <= endDate.Value);

        return await auditQuery.CountAsync();
    }

    // Statistics methods
    public async Task<IEnumerable<object>> GetLogsByActionStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(a => a.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.Timestamp <= endDate.Value);

        return await query
            .GroupBy(a => a.Action)
            .Select(g => new { Action = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();
    }

    public async Task<IEnumerable<object>> GetLogsByEntityStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(a => a.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.Timestamp <= endDate.Value);

        return await query
            .GroupBy(a => a.Entity)
            .Select(g => new { Entity = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();
    }

    public async Task<IEnumerable<object>> GetLogsByDateStatsAsync(DateTime? startDate = null, DateTime? endDate = null, string groupBy = "day")
    {
        var query = _context.AuditLogs.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(a => a.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.Timestamp <= endDate.Value);

        return groupBy.ToLower() switch
        {
            "hour" => await query
                .GroupBy(a => new { a.Timestamp.Year, a.Timestamp.Month, a.Timestamp.Day, a.Timestamp.Hour })
                .Select(g => new { Date = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day, g.Key.Hour, 0, 0), Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToListAsync(),
            "week" => await query
                .GroupBy(a => new { Year = a.Timestamp.Year, Week = (a.Timestamp.DayOfYear - 1) / 7 })
                .Select(g => new { Week = $"{g.Key.Year}-W{g.Key.Week + 1}", Count = g.Count() })
                .OrderBy(x => x.Week)
                .ToListAsync(),
            "month" => await query
                .GroupBy(a => new { a.Timestamp.Year, a.Timestamp.Month })
                .Select(g => new { Date = new DateTime(g.Key.Year, g.Key.Month, 1), Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToListAsync(),
            _ => await query
                .GroupBy(a => a.Timestamp.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToListAsync()
        };
    }

    public async Task<IEnumerable<object>> GetMostActiveUsersAsync(int limit = 10, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(a => a.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.Timestamp <= endDate.Value);

        return await query
            .Where(a => a.ActorId.HasValue)
            .GroupBy(a => new { a.ActorId, ActorUsername = a.Actor != null ? a.Actor.UserName : null })
            .Select(g => new { 
                UserId = g.Key.ActorId, 
                Username = g.Key.ActorUsername ?? "Unknown", 
                ActivityCount = g.Count() 
            })
            .OrderByDescending(x => x.ActivityCount)
            .Take(limit)
            .ToListAsync();
    }

    // Cleanup and archive methods
    public async Task<int> CleanupOldLogsAsync(int olderThanDays)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);
        var logsToDelete = await _context.AuditLogs
            .Where(a => a.Timestamp < cutoffDate)
            .ToListAsync();

        _context.AuditLogs.RemoveRange(logsToDelete);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Cleaned up {Count} audit logs older than {Days} days", logsToDelete.Count, olderThanDays);
        return logsToDelete.Count;
    }

    public async Task<int> ArchiveOldLogsAsync(int olderThanDays, string archivePath)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);
        var logsToArchive = await _context.AuditLogs
            .Where(a => a.Timestamp < cutoffDate)
            .ToListAsync();

        if (logsToArchive.Any())
        {
            // Create archive directory if it doesn't exist
            Directory.CreateDirectory(Path.GetDirectoryName(archivePath) ?? "");

            // Export to JSON for archiving
            var jsonData = JsonSerializer.Serialize(logsToArchive, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(archivePath, jsonData);

            // Remove from database
            _context.AuditLogs.RemoveRange(logsToArchive);
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("Archived {Count} audit logs older than {Days} days to {Path}", logsToArchive.Count, olderThanDays, archivePath);
        return logsToArchive.Count;
    }

    // Export methods
    public async Task<string> ExportToCsvAsync(DateTime? startDate, DateTime? endDate, string? actor, string? entity, string? action)
    {
        var logs = await GetAuditLogsAsync(1, int.MaxValue, actor, entity, action, startDate, endDate, null);
        
        var csv = new StringBuilder();
        csv.AppendLine("Id,Timestamp,ActorId,ActorUsername,Action,Entity,EntityId,BeforeData,AfterData,IpAddress,UserAgent");
        
        foreach (var log in logs)
        {
            csv.AppendLine($"{log.Id},{log.Timestamp:yyyy-MM-dd HH:mm:ss},{log.ActorId},{log.Actor?.UserName},{log.Action},{log.Entity},{log.EntityId},{log.BeforeDataJson},{log.AfterDataJson},{log.IpAddress},{log.UserAgent}");
        }
        
        return csv.ToString();
    }

    public async Task<string> ExportToJsonAsync(DateTime? startDate, DateTime? endDate, string? actor, string? entity, string? action)
    {
        var logs = await GetAuditLogsAsync(1, int.MaxValue, actor, entity, action, startDate, endDate, null);
        return JsonSerializer.Serialize(logs, new JsonSerializerOptions { WriteIndented = true });
    }

    public async Task<string> ExportToXmlAsync(DateTime? startDate, DateTime? endDate, string? actor, string? entity, string? action)
    {
        var logs = await GetAuditLogsAsync(1, int.MaxValue, actor, entity, action, startDate, endDate, null);
        
        var xml = new StringBuilder();
        xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        xml.AppendLine("<AuditLogs>");
        
        foreach (var log in logs)
        {
            xml.AppendLine("  <AuditLog>");
            xml.AppendLine($"    <Id>{log.Id}</Id>");
            xml.AppendLine($"    <Timestamp>{log.Timestamp:yyyy-MM-dd HH:mm:ss}</Timestamp>");
            xml.AppendLine($"    <ActorId>{log.ActorId}</ActorId>");
            xml.AppendLine($"    <ActorUsername>{log.Actor?.UserName}</ActorUsername>");
            xml.AppendLine($"    <Action>{log.Action}</Action>");
            xml.AppendLine($"    <Entity>{log.Entity}</Entity>");
            xml.AppendLine($"    <EntityId>{log.EntityId}</EntityId>");
            xml.AppendLine($"    <BeforeData>{log.BeforeDataJson}</BeforeData>");
            xml.AppendLine($"    <AfterData>{log.AfterDataJson}</AfterData>");
            xml.AppendLine($"    <IpAddress>{log.IpAddress}</IpAddress>");
            xml.AppendLine($"    <UserAgent>{log.UserAgent}</UserAgent>");
            xml.AppendLine("  </AuditLog>");
        }
        
        xml.AppendLine("</AuditLogs>");
        return xml.ToString();
    }

    // Report generation methods
    public async Task<object> GenerateAuditSummaryReportAsync(DateTime? startDate, DateTime? endDate)
    {
        var stats = await GetAuditStatisticsAsync(startDate, endDate);
        var actionStats = await GetLogsByActionStatsAsync(startDate, endDate);
        var entityStats = await GetLogsByEntityStatsAsync(startDate, endDate);
        var activeUsers = await GetMostActiveUsersAsync(10, startDate, endDate);

        return new
        {
            Period = new { StartDate = startDate, EndDate = endDate },
            Statistics = stats,
            ActionBreakdown = actionStats,
            EntityBreakdown = entityStats,
            MostActiveUsers = activeUsers,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<object> GenerateDetailedAuditReportAsync(DateTime? startDate, DateTime? endDate, string? actor, string? entity)
    {
        var logs = await GetAuditLogsAsync(1, 1000, actor, entity, null, startDate, endDate, null);
        var totalCount = await GetTotalLogsCountAsync(actor, entity, null, startDate, endDate, null);
        var stats = await GetAuditStatisticsAsync(startDate, endDate);

        return new
        {
            Period = new { StartDate = startDate, EndDate = endDate },
            Filters = new { Actor = actor, Entity = entity },
            TotalRecords = totalCount,
            Statistics = stats,
            Logs = logs.Take(1000), // Limit for performance
            GeneratedAt = DateTime.UtcNow
        };
    }

    // Security monitoring methods
    public async Task<IEnumerable<object>> GetSecurityAlertsAsync(int limit = 20)
    {
        var recentFailures = await _context.AuditLogs
            .Where(a => a.Action.Contains("FAILED") || a.Action.Contains("UNAUTHORIZED") || a.Action.Contains("BLOCKED"))
            .OrderByDescending(a => a.Timestamp)
            .Take(limit)
            .Select(a => new
            {
                a.Id,
                a.Timestamp,
                a.Action,
                ActorUsername = a.Actor != null ? a.Actor.UserName : null,
                a.IpAddress,
                a.BeforeDataJson,
                a.AfterDataJson,
                Severity = a.Action.Contains("UNAUTHORIZED") ? "High" : "Medium"
            })
            .ToListAsync();

        return recentFailures;
    }

    public async Task<IEnumerable<object>> DetectAnomaliesAsync(DateTime? startDate, DateTime? endDate)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(a => a.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.Timestamp <= endDate.Value);

        // Detect unusual activity patterns
        var suspiciousActivity = await query
            .Where(a => a.ActorId.HasValue)
            .GroupBy(a => new { a.ActorId, Hour = a.Timestamp.Hour })
            .Where(g => g.Count() > 50) // More than 50 actions per hour
            .Select(g => new
            {
                UserId = g.Key.ActorId,
                Hour = g.Key.Hour,
                ActionCount = g.Count(),
                AnomalyType = "High Activity Volume",
                Severity = "Medium"
            })
            .ToListAsync();

        return suspiciousActivity;
    }
}