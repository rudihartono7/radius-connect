using Microsoft.EntityFrameworkCore;
using RadiusConnect.Api.Data;
using RadiusConnect.Api.Models.Domain;
using RadiusConnect.Api.Repositories.Interfaces;
using System.Net;

namespace RadiusConnect.Api.Repositories;

public class AuditRepository : Repository<AuditLog>, IAuditRepository
{
    private readonly AppDbContext _appContext;

    public AuditRepository(AppDbContext context) : base(context)
    {
        _appContext = context;
    }

    public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(int page = 1, int pageSize = 10)
    {
        return await _appContext.AuditLogs
            .Include(al => al.Actor)
            .OrderByDescending(al => al.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetAuditLogsByActorAsync(Guid actorId, int page = 1, int pageSize = 10)
    {
        return await _appContext.AuditLogs
            .Include(al => al.Actor)
            .Where(al => al.ActorId == actorId)
            .OrderByDescending(al => al.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetAuditLogsByEntityAsync(string entity, int page = 1, int pageSize = 10)
    {
        return await _appContext.AuditLogs
            .Include(al => al.Actor)
            .Where(al => al.Entity == entity)
            .OrderByDescending(al => al.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetAuditLogsByEntityIdAsync(string entity, string entityId, int page = 1, int pageSize = 10)
    {
        return await _appContext.AuditLogs
            .Include(al => al.Actor)
            .Where(al => al.Entity == entity && al.EntityId == entityId)
            .OrderByDescending(al => al.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetAuditLogsByActionAsync(string action, int page = 1, int pageSize = 10)
    {
        return await _appContext.AuditLogs
            .Include(al => al.Actor)
            .Where(al => al.Action == action)
            .OrderByDescending(al => al.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate, int page = 1, int pageSize = 10)
    {
        return await _appContext.AuditLogs
            .Include(al => al.Actor)
            .Where(al => al.Timestamp >= startDate && al.Timestamp <= endDate)
            .OrderByDescending(al => al.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalAuditLogsCountAsync()
    {
        return await _appContext.AuditLogs.CountAsync();
    }

    public async Task LogActionAsync(Guid? actorId, string action, string entity, string? entityId = null, object? beforeData = null, object? afterData = null, IPAddress? ipAddress = null, string? userAgent = null)
    {
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

        await _appContext.AuditLogs.AddAsync(auditLog);
        await _appContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<AuditLog>> SearchAuditLogsAsync(string searchTerm, int page = 1, int pageSize = 10)
    {
        return await _appContext.AuditLogs
            .Include(al => al.Actor)
            .Where(al => al.Action.Contains(searchTerm) ||
                        al.Entity.Contains(searchTerm) ||
                        (al.EntityId != null && al.EntityId.Contains(searchTerm)) ||
                        (al.Actor != null && al.Actor.UserName.Contains(searchTerm)))
            .OrderByDescending(al => al.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task CleanupOldLogsAsync(DateTime cutoffDate)
    {
        var oldLogs = await _appContext.AuditLogs
            .Where(al => al.Timestamp < cutoffDate)
            .ToListAsync();

        _appContext.AuditLogs.RemoveRange(oldLogs);
        await _appContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetRecentLogsAsync(int count = 10)
    {
        return await _appContext.AuditLogs
            .Include(al => al.Actor)
            .OrderByDescending(al => al.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    public async Task<int> GetLogsCountAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _appContext.AuditLogs.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(al => al.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(al => al.Timestamp <= endDate.Value);

        return await query.CountAsync();
    }
}