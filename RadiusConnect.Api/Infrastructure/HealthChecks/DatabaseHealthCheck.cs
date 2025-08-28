using Microsoft.Extensions.Diagnostics.HealthChecks;
using RadiusConnect.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace RadiusConnect.Api.Infrastructure.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly AppDbContext _context;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(AppDbContext context, ILogger<DatabaseHealthCheck> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if database can be connected to
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            
            if (!canConnect)
            {
                _logger.LogWarning("Database connection failed");
                return HealthCheckResult.Unhealthy("Cannot connect to database");
            }

            // Execute a simple query to verify database is responsive
            var userCount = await _context.AppUsers.CountAsync(cancellationToken);
            
            var data = new Dictionary<string, object>
            {
                ["database_connection"] = "healthy",
                ["user_count"] = userCount,
                ["connection_string"] = _context.Database.GetConnectionString()?.Split(';')
                    .Where(part => !part.ToLower().Contains("password"))
                    .FirstOrDefault(part => part.ToLower().Contains("server")) ?? "unknown"
            };

            _logger.LogDebug("Database health check passed. User count: {UserCount}", userCount);
            return HealthCheckResult.Healthy("Database is healthy", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy("Database health check failed", ex);
        }
    }
}

public class RadiusDatabaseHealthCheck : IHealthCheck
{
    private readonly RadiusDbContext _context;
    private readonly ILogger<RadiusDatabaseHealthCheck> _logger;

    public RadiusDatabaseHealthCheck(RadiusDbContext context, ILogger<RadiusDatabaseHealthCheck> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if RADIUS database can be connected to
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            
            if (!canConnect)
            {
                _logger.LogWarning("RADIUS database connection failed");
                return HealthCheckResult.Unhealthy("Cannot connect to RADIUS database");
            }

            // Execute a simple query to verify database is responsive
            var userCount = await _context.RadCheck.CountAsync(cancellationToken);
            var sessionCount = await _context.RadAcct.Where(r => r.AcctStopTime == null).CountAsync(cancellationToken);
            
            var data = new Dictionary<string, object>
            {
                ["radius_database_connection"] = "healthy",
                ["radius_user_count"] = userCount,
                ["active_session_count"] = sessionCount,
                ["connection_string"] = _context.Database.GetConnectionString()?.Split(';')
                    .Where(part => !part.ToLower().Contains("password"))
                    .FirstOrDefault(part => part.ToLower().Contains("server")) ?? "unknown"
            };

            _logger.LogDebug("RADIUS database health check passed. User count: {UserCount}, Active sessions: {SessionCount}", 
                userCount, sessionCount);
            return HealthCheckResult.Healthy("RADIUS database is healthy", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RADIUS database health check failed");
            return HealthCheckResult.Unhealthy("RADIUS database health check failed", ex);
        }
    }
}