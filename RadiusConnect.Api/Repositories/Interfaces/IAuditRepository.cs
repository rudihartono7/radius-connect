using RadiusConnect.Api.Models.Domain;
using System.Net;

namespace RadiusConnect.Api.Repositories.Interfaces;

public interface IAuditRepository : IRepository<AuditLog>
{
    Task<IEnumerable<AuditLog>> GetAuditLogsAsync(int page = 1, int pageSize = 10);
    Task<IEnumerable<AuditLog>> GetAuditLogsByActorAsync(Guid actorId, int page = 1, int pageSize = 10);
    Task<IEnumerable<AuditLog>> GetAuditLogsByEntityAsync(string entity, int page = 1, int pageSize = 10);
    Task<IEnumerable<AuditLog>> GetAuditLogsByEntityIdAsync(string entity, string entityId, int page = 1, int pageSize = 10);
    Task<IEnumerable<AuditLog>> GetAuditLogsByActionAsync(string action, int page = 1, int pageSize = 10);
    Task<IEnumerable<AuditLog>> GetAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate, int page = 1, int pageSize = 10);
    Task<int> GetTotalAuditLogsCountAsync();
    Task LogActionAsync(Guid? actorId, string action, string entity, string? entityId = null, object? beforeData = null, object? afterData = null, IPAddress? ipAddress = null, string? userAgent = null);
    Task<IEnumerable<AuditLog>> SearchAuditLogsAsync(string searchTerm, int page = 1, int pageSize = 10);
    Task CleanupOldLogsAsync(DateTime cutoffDate);
    Task<IEnumerable<AuditLog>> GetRecentLogsAsync(int count = 10);
    Task<int> GetLogsCountAsync(DateTime? startDate = null, DateTime? endDate = null);
}