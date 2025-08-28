using RadiusConnect.Api.Models.Domain;
using System.Net;

namespace RadiusConnect.Api.Services.Interfaces;

public interface IAuditService
{
    // Audit log retrieval
    Task<IEnumerable<AuditLog>> GetAuditLogsAsync(int page = 1, int pageSize = 10);
    Task<IEnumerable<AuditLog>> GetAuditLogsAsync(int page, int pageSize, string? actor, string? entity, string? action, DateTime? startDate, DateTime? endDate, string? search);
    Task<AuditLog?> GetAuditLogByIdAsync(string id);
    Task<IEnumerable<AuditLog>> GetAuditLogsByActorAsync(Guid actorId, int page = 1, int pageSize = 10);
    Task<IEnumerable<AuditLog>> GetLogsByActorAsync(string actorId, int page, int pageSize, DateTime? startDate, DateTime? endDate);
    Task<IEnumerable<AuditLog>> GetAuditLogsByEntityAsync(string entity, int page = 1, int pageSize = 10);
    Task<IEnumerable<AuditLog>> GetLogsByEntityAsync(string entityType, int page, int pageSize, DateTime? startDate, DateTime? endDate);
    Task<IEnumerable<AuditLog>> GetAuditLogsByEntityIdAsync(string entity, string entityId, int page = 1, int pageSize = 10);
    Task<IEnumerable<AuditLog>> GetAuditLogsByActionAsync(string action, int page = 1, int pageSize = 10);
    Task<IEnumerable<AuditLog>> GetAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate, int page = 1, int pageSize = 10);
    Task<IEnumerable<AuditLog>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate, int page, int pageSize);
    Task<IEnumerable<AuditLog>> SearchAuditLogsAsync(string searchTerm, int page = 1, int pageSize = 10);
    Task<IEnumerable<AuditLog>> SearchAuditLogsAsync(string query, int page, int pageSize, DateTime? startDate, DateTime? endDate);
    Task<int> GetTotalAuditLogsCountAsync();
    Task<int> GetTotalLogsCountAsync(string? actor, string? entity, string? action, DateTime? startDate, DateTime? endDate, string? search);
    Task<int> GetLogsByActorCountAsync(string actorId, DateTime? startDate, DateTime? endDate);
    Task<int> GetLogsByEntityCountAsync(string entityType, DateTime? startDate, DateTime? endDate);
    Task<int> GetLogsByDateRangeCountAsync(DateTime startDate, DateTime endDate);
    Task<int> SearchAuditLogsCountAsync(string query, DateTime? startDate, DateTime? endDate);

    // Audit log creation
    Task LogUserActionAsync(Guid? actorId, string action, string entity, string? entityId = null, object? beforeData = null, object? afterData = null, IPAddress? ipAddress = null, string? userAgent = null);
    Task LogSystemActionAsync(string action, string entity, string? entityId = null, object? beforeData = null, object? afterData = null);
    Task LogAuthenticationAttemptAsync(string username, bool success, IPAddress? ipAddress = null, string? userAgent = null, string? failureReason = null);
    Task LogPasswordChangeAsync(Guid userId, bool success, IPAddress? ipAddress = null, string? userAgent = null);
    Task LogRoleAssignmentAsync(Guid actorId, Guid targetUserId, string roleName, bool isAssignment, IPAddress? ipAddress = null, string? userAgent = null);

    // Specific entity logging
    Task LogUserCreatedAsync(Guid actorId, AppUser user, IPAddress? ipAddress = null, string? userAgent = null);
    Task LogUserUpdatedAsync(Guid actorId, AppUser oldUser, AppUser newUser, IPAddress? ipAddress = null, string? userAgent = null);
    Task LogUserDeletedAsync(Guid actorId, AppUser user, IPAddress? ipAddress = null, string? userAgent = null);
    Task LogRadiusUserCreatedAsync(Guid actorId, string username, IPAddress? ipAddress = null, string? userAgent = null);
    Task LogRadiusUserUpdatedAsync(Guid actorId, string username, object? beforeData, object? afterData, IPAddress? ipAddress = null, string? userAgent = null);
    Task LogRadiusUserDeletedAsync(Guid actorId, string username, IPAddress? ipAddress = null, string? userAgent = null);
    Task LogRadiusUserActionAsync(Guid actorId, string action, string username, IPAddress? ipAddress = null, string? userAgent = null);
    Task LogRadiusGroupActionAsync(Guid actorId, string entity, string action, string groupName, string description, IPAddress? ipAddress = null, string? userAgent = null);
    Task LogGroupCreatedAsync(Guid actorId, string groupName, IPAddress? ipAddress = null, string? userAgent = null);
    Task LogGroupUpdatedAsync(Guid actorId, string groupName, object? beforeData, object? afterData, IPAddress? ipAddress = null, string? userAgent = null);
    Task LogGroupDeletedAsync(Guid actorId, string groupName, IPAddress? ipAddress = null, string? userAgent = null);
    Task LogSessionDisconnectedAsync(Guid actorId, string username, string acctSessionId, IPAddress? ipAddress = null, string? userAgent = null);
    Task LogCoaRequestAsync(Guid actorId, string username, string acctSessionId, string requestType, IPAddress? ipAddress = null, string? userAgent = null);
    Task LogSettingsChangedAsync(Guid actorId, string settingKey, object? oldValue, object? newValue, IPAddress? ipAddress = null, string? userAgent = null);

    // Audit statistics
    Task<Dictionary<string, int>> GetAuditStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<object>> GetLogsByActionStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<object>> GetLogsByEntityStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<object>> GetLogsByDateStatsAsync(DateTime? startDate = null, DateTime? endDate = null, string groupBy = "day");
    Task<IEnumerable<object>> GetMostActiveUsersAsync(int limit = 10, DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<object>> GetAuditTrendsAsync(int days = 30);
    Task<Dictionary<string, int>> GetAuditActionsByTypeAsync();
    Task<IEnumerable<object>> GetMostActiveUsersAsync(int count = 10);
    Task<Dictionary<string, int>> GetAuditsByEntityAsync();
    Task<IEnumerable<object>> GetRecentCriticalActionsAsync(int count = 20);

    // Audit maintenance
    Task CleanupOldAuditLogsAsync(int retentionDays = 365);
    Task<int> CleanupOldLogsAsync(int olderThanDays);
    Task<int> ArchiveOldLogsAsync(int olderThanDays, string archivePath);
    Task ArchiveAuditLogsAsync(DateTime cutoffDate);
    Task<long> GetAuditLogsSizeAsync();

    // Export and reporting
    Task<byte[]> ExportAuditLogsAsync(DateTime startDate, DateTime endDate, string format = "csv");
    Task<string> ExportToCsvAsync(DateTime? startDate, DateTime? endDate, string? actor, string? entity, string? action);
    Task<string> ExportToJsonAsync(DateTime? startDate, DateTime? endDate, string? actor, string? entity, string? action);
    Task<string> ExportToXmlAsync(DateTime? startDate, DateTime? endDate, string? actor, string? entity, string? action);
    Task<Dictionary<string, object>> GenerateAuditReportAsync(DateTime startDate, DateTime endDate);
    Task<object> GenerateAuditSummaryReportAsync(DateTime? startDate, DateTime? endDate);
    Task<object> GenerateDetailedAuditReportAsync(DateTime? startDate, DateTime? endDate, string? actor, string? entity);
    Task<IEnumerable<object>> GetComplianceReportAsync(DateTime startDate, DateTime endDate);

    // Security monitoring
    Task<IEnumerable<object>> GetSuspiciousActivitiesAsync(int hours = 24);
    Task<IEnumerable<object>> GetFailedLoginAttemptsAsync(int hours = 24, int threshold = 5);
    Task<IEnumerable<object>> GetUnauthorizedAccessAttemptsAsync(int hours = 24);
    Task<IEnumerable<object>> GetSecurityAlertsAsync(int limit = 20);
    Task<IEnumerable<object>> DetectAnomaliesAsync(DateTime? startDate, DateTime? endDate);
    Task<bool> CheckForAnomalousActivityAsync(Guid userId, string action);
}