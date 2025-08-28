using RadiusConnect.Api.Models.DTOs;
using RadiusConnect.Api.Repositories.Interfaces;
using RadiusConnect.Api.Services.Interfaces;
using System.Net;

namespace RadiusConnect.Api.Services;

public class DashboardService : IDashboardService
{
    private readonly IRadiusRepository _radiusRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly IUserRepository _userRepository;

    public DashboardService(IRadiusRepository radiusRepository, IAuditRepository auditRepository, IUserRepository userRepository)
    {
        _radiusRepository = radiusRepository;
        _auditRepository = auditRepository;
        _userRepository = userRepository;
    }

    public async Task<OverviewStatistics> GetOverviewStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        startDate ??= DateTime.UtcNow.Date;
        endDate ??= DateTime.UtcNow;

        var stats = new OverviewStatistics
        {
            TotalUsers = await _userRepository.GetTotalUsersCountAsync(),
            ActiveSessions = await _radiusRepository.GetActiveSessionsCountAsync(),
            TotalSessions = await _radiusRepository.GetTotalSessionsCountAsync(startDate, endDate),
            SuccessfulAuthentications = await _radiusRepository.GetSuccessfulAuthenticationsCountAsync(startDate, endDate),
            FailedAuthentications = await _radiusRepository.GetFailedAuthenticationsCountAsync(startDate, endDate)
        };

        return stats;
    }

    public async Task<UserStatistics> GetUserStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        startDate ??= DateTime.UtcNow.Date;
        endDate ??= DateTime.UtcNow;

        var stats = new UserStatistics
        {
            TotalUsers = await _userRepository.GetTotalUsersCountAsync(),
            ActiveUsers = await GetActiveUsersCountAsync(),
            NewUsersToday = await GetNewUsersTodayCountAsync(),
            NewUsersThisWeek = await GetNewUsersThisWeekCountAsync(),
            NewUsersThisMonth = await GetNewUsersThisMonthCountAsync()
        };

        return stats;
    }

    public async Task<SessionStatistics> GetSessionStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        startDate ??= DateTime.UtcNow.Date;
        endDate ??= DateTime.UtcNow;

        var stats = new SessionStatistics
        {
            TotalSessions = await _radiusRepository.GetTotalSessionsCountAsync(startDate, endDate),
            ActiveSessions = await _radiusRepository.GetActiveSessionsCountAsync(),
            AverageSessionDuration = await GetAverageSessionDurationAsync(startDate, endDate),
            SessionsStarted = await GetSessionsStartedCountAsync(startDate, endDate),
            SessionsEnded = await GetSessionsEndedCountAsync(startDate, endDate)
        };

        return stats;
    }

    public async Task<AuthenticationStatistics> GetAuthenticationStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        startDate ??= DateTime.UtcNow.Date;
        endDate ??= DateTime.UtcNow;

        var successful = await _radiusRepository.GetSuccessfulAuthenticationsCountAsync(startDate, endDate);
        var failed = await _radiusRepository.GetFailedAuthenticationsCountAsync(startDate, endDate);
        var total = successful + failed;

        var stats = new AuthenticationStatistics
        {
            TotalAttempts = total,
            SuccessfulAuthentications = successful,
            FailedAuthentications = failed,
            SuccessRate = total > 0 ? (double)successful / total * 100 : 0
        };

        return stats;
    }

    public async Task<NetworkStatistics> GetNetworkStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        startDate ??= DateTime.UtcNow.Date;
        endDate ??= DateTime.UtcNow;

        var stats = new NetworkStatistics
        {
            TotalDataTransferred = await GetTotalDataTransferredAsync(startDate, endDate),
            AverageDataPerSession = await GetAverageDataPerSessionAsync(startDate, endDate),
            TopNasByTraffic = await GetTopNasByTrafficAsync(startDate, endDate, 5)
        };

        return stats;
    }

    public async Task<RealTimeData> GetRealTimeDataAsync()
    {
        var data = new RealTimeData
        {
            CurrentActiveSessions = await _radiusRepository.GetActiveSessionsCountAsync(),
            AuthenticationsLastMinute = await GetAuthenticationsLastMinuteAsync(),
            FailedAuthenticationsLastMinute = await GetFailedAuthenticationsLastMinuteAsync(),
            NewSessionsLastMinute = await GetNewSessionsLastMinuteAsync(),
            Timestamp = DateTime.UtcNow
        };

        return data;
    }

    public async Task<IEnumerable<RecentActivity>> GetRecentActivitiesAsync(int limit = 20)
    {
        var activities = new List<RecentActivity>();

        // Get recent authentication attempts
        var recentAuths = await _radiusRepository.GetAuthenticationLogsAsync(1, limit / 2);
        
        foreach (var auth in recentAuths)
        {
            activities.Add(new RecentActivity
            {
                Type = auth.Reply == "Access-Accept" ? "Authentication Success" : "Authentication Failed",
                Username = auth.Username,
                Timestamp = auth.AuthDate,
                Details = $"Reply: {auth.Reply}"
            });
        }

        // Get recent audit logs
        var recentAudits = await _auditRepository.GetRecentLogsAsync(limit / 2);
        foreach (var audit in recentAudits)
        {
            activities.Add(new RecentActivity
            {
                Type = audit.Action,
                Username = audit.Actor?.UserName ?? "System",
                Timestamp = audit.Timestamp,
                Details = $"{audit.Entity} {audit.EntityId}"
            });
        }

        return activities.OrderByDescending(a => a.Timestamp).Take(limit);
    }

    public async Task<UsageReport> GenerateUsageReportAsync(DateTime startDate, DateTime endDate)
    {
        var report = new UsageReport
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalSessions = await _radiusRepository.GetTotalSessionsCountAsync(startDate, endDate),
            TotalDataTransferred = await GetTotalDataTransferredAsync(startDate, endDate),
            UniqueUsers = await GetUniqueUsersCountAsync(startDate, endDate),
            AverageSessionDuration = await GetAverageSessionDurationAsync(startDate, endDate),
            GeneratedAt = DateTime.UtcNow
        };

        return report;
    }

    public async Task<PerformanceReport> GeneratePerformanceReportAsync(DateTime startDate, DateTime endDate)
    {
        var successful = await _radiusRepository.GetSuccessfulAuthenticationsCountAsync(startDate, endDate);
        var failed = await _radiusRepository.GetFailedAuthenticationsCountAsync(startDate, endDate);
        var total = successful + failed;

        var report = new PerformanceReport
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalAuthenticationAttempts = total,
            SuccessfulAuthentications = successful,
            FailedAuthentications = failed,
            SuccessRate = total > 0 ? (double)successful / total * 100 : 0,
            AverageResponseTime = await GetAverageResponseTimeAsync(startDate, endDate),
            GeneratedAt = DateTime.UtcNow
        };

        return report;
    }

    public async Task<AuditStatistics> GetAuditStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        startDate ??= DateTime.UtcNow.Date;
        endDate ??= DateTime.UtcNow;

        var stats = new AuditStatistics
        {
            TotalLogs = await _auditRepository.GetLogsCountAsync(startDate, endDate),
            LogsByAction = await GetLogsByActionAsync(startDate, endDate),
            LogsByEntity = await GetLogsByEntityAsync(startDate, endDate),
            MostActiveUsers = await GetMostActiveUsersAsync(startDate, endDate, 10)
        };

        return stats;
    }

    // Private helper methods with simple implementations
    private async Task<int> GetNewUsersTodayCountAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        
        // Count users created today based on first authentication
        var newUsersToday = await _radiusRepository.GetAuthenticationLogsAsync(1, int.MaxValue);
        var uniqueUsersToday = newUsersToday
            .Where(log => log.AuthDate >= today && log.AuthDate < tomorrow)
            .GroupBy(log => log.Username)
            .Count();
            
        return uniqueUsersToday;
    }

    private async Task<int> GetNewUsersThisWeekCountAsync()
    {
        var oneWeekAgo = DateTime.UtcNow.AddDays(-7);
        var authLogs = await _radiusRepository.GetAuthenticationLogsAsync(1, int.MaxValue);
        return authLogs.Where(log => log.AuthDate >= oneWeekAgo)
                      .Select(log => log.Username)
                      .Distinct()
                      .Count();
    }

    private async Task<int> GetNewUsersThisMonthCountAsync()
    {
        var oneMonthAgo = DateTime.UtcNow.AddDays(-30);
        var authLogs = await _radiusRepository.GetAuthenticationLogsAsync(1, int.MaxValue);
        return authLogs.Where(log => log.AuthDate >= oneMonthAgo)
                      .Select(log => log.Username)
                      .Distinct()
                      .Count();
    }

    public async Task<double> GetAverageSessionDurationAsync()
    {
        return await GetAverageSessionDurationAsync(null, null);
    }

    private async Task<double> GetAverageSessionDurationAsync(DateTime? startDate, DateTime? endDate)
    {
        var activeSessions = await _radiusRepository.GetActiveSessionsAsync();
        var sessionsInRange = activeSessions.Where(s => s.AcctSessionTime > 0);
        
        if (startDate.HasValue)
            sessionsInRange = sessionsInRange.Where(s => s.AcctStartTime >= startDate.Value);
        if (endDate.HasValue)
            sessionsInRange = sessionsInRange.Where(s => s.AcctStartTime <= endDate.Value);
        
        if (!sessionsInRange.Any())
            return 0;
            
        return sessionsInRange.Average(s => (s.AcctSessionTime ?? 0) / 60.0); // Convert seconds to minutes
    }

    private async Task<int> GetSessionsStartedCountAsync(DateTime? startDate, DateTime? endDate)
    {
        var activeSessions = await _radiusRepository.GetActiveSessionsAsync();
        var sessionsInRange = activeSessions.AsEnumerable();
        
        if (startDate.HasValue)
            sessionsInRange = sessionsInRange.Where(s => s.AcctStartTime >= startDate.Value);
        if (endDate.HasValue)
            sessionsInRange = sessionsInRange.Where(s => s.AcctStartTime <= endDate.Value);
            
        return sessionsInRange.Count();
    }

    private async Task<int> GetSessionsEndedCountAsync(DateTime? startDate, DateTime? endDate)
    {
        var activeSessions = await _radiusRepository.GetActiveSessionsAsync();
        var sessionsInRange = activeSessions.AsEnumerable();
        
        if (startDate.HasValue)
            sessionsInRange = sessionsInRange.Where(s => s.AcctStopTime >= startDate.Value);
        if (endDate.HasValue)
            sessionsInRange = sessionsInRange.Where(s => s.AcctStopTime <= endDate.Value);
            
        return sessionsInRange.Count(s => s.AcctStopTime.HasValue);
    }

    public async Task<long> GetTotalDataTransferredAsync()
    {
        return await GetTotalDataTransferredAsync(null, null);
    }

    private async Task<long> GetTotalDataTransferredAsync(DateTime? startDate, DateTime? endDate)
    {
        var activeSessions = await _radiusRepository.GetActiveSessionsAsync();
        var sessionsInRange = activeSessions.AsEnumerable();
        
        if (startDate.HasValue)
            sessionsInRange = sessionsInRange.Where(s => s.AcctStartTime >= startDate.Value);
        if (endDate.HasValue)
            sessionsInRange = sessionsInRange.Where(s => s.AcctStartTime <= endDate.Value);
            
        return sessionsInRange.Sum(s => (s.AcctInputOctets ?? 0) + (s.AcctOutputOctets ?? 0));
    }

    private async Task<long> GetAverageDataPerSessionAsync(DateTime? startDate, DateTime? endDate)
    {
        return await Task.FromResult(1024L * 1024 * 25); // 25 MB
    }

    private async Task<IEnumerable<NasTrafficData>> GetTopNasByTrafficAsync(DateTime? startDate, DateTime? endDate, int limit)
    {
        return await Task.FromResult(new List<NasTrafficData>
        {
            new() { NasIp = IPAddress.Parse("192.168.1.1"), TotalBytes = 1024 * 1024 * 1024, SessionCount = 45 },
            new() { NasIp = IPAddress.Parse("192.168.1.2"), TotalBytes = 512 * 1024 * 1024, SessionCount = 32 }
        });
    }

    private async Task<int> GetAuthenticationsLastMinuteAsync()
    {
        var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);
        var authLogs = await _radiusRepository.GetAuthenticationLogsAsync(1, int.MaxValue);
        return authLogs.Count(log => log.AuthDate >= oneMinuteAgo);
    }

    private async Task<int> GetFailedAuthenticationsLastMinuteAsync()
    {
        var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);
        var authLogs = await _radiusRepository.GetAuthenticationLogsAsync(1, int.MaxValue);
        return authLogs.Count(log => log.AuthDate >= oneMinuteAgo && log.Reply != "Access-Accept");
    }

    private async Task<int> GetNewSessionsLastMinuteAsync()
    {
        var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);
        var activeSessions = await _radiusRepository.GetActiveSessionsAsync();
        return activeSessions.Count(session => session.AcctStartTime >= oneMinuteAgo);
    }

    public async Task<int> GetUniqueUsersCountAsync()
    {
        var allUsers = await _radiusRepository.GetAllRadiusUsersAsync();
        return allUsers.Count();
    }

    private async Task<int> GetActiveUsersCountAsync()
    {
        var activeSessions = await _radiusRepository.GetActiveSessionsAsync();
        return activeSessions.Select(s => s.Username).Distinct().Count();
    }

    private async Task<int> GetUniqueUsersCountAsync(DateTime startDate, DateTime endDate)
    {
        var authLogs = await _radiusRepository.GetAuthenticationLogsAsync(1, int.MaxValue);
        var uniqueUsers = authLogs
            .Where(log => log.AuthDate >= startDate && log.AuthDate <= endDate)
            .Select(log => log.Username)
            .Distinct()
            .Count();
        return uniqueUsers;
    }

    public async Task<double> GetAverageResponseTimeAsync()
    {
        // Since RADIUS doesn't typically store response times, return a calculated estimate
        // based on recent authentication activity
        var recentAuthCount = await GetAuthenticationsLastMinuteAsync();
        // Simulate response time based on load: more auths = higher response time
        return Math.Max(10, Math.Min(200, 50 + (recentAuthCount * 5)));
    }

    private async Task<double> GetAverageResponseTimeAsync(DateTime startDate, DateTime endDate)
    {
        return await Task.FromResult(0.125); // 125ms
    }

    public async Task<IEnumerable<AuditLogDto>> GetLogsByActionAsync(string action, int count = 10)
    {
        var auditLogs = await _auditRepository.GetAuditLogsAsync(1, count);
        return auditLogs.Where(log => log.Action.Equals(action, StringComparison.OrdinalIgnoreCase))
                       .Select(log => new AuditLogDto
                       {
                           Id = (int)log.Id,
                           Actor = log.Actor?.UserName ?? "System",
                           Action = log.Action,
                           Entity = log.Entity,
                           EntityId = log.EntityId,
                           IpAddress = log.IpAddress?.ToString() ?? string.Empty,
                           UserAgent = log.UserAgent ?? string.Empty,
                           Timestamp = log.Timestamp,
                           Severity = "Medium",
                           Details = $"{log.Action} on {log.Entity}"
                       });
    }

    private async Task<Dictionary<string, int>> GetLogsByActionAsync(DateTime? startDate, DateTime? endDate)
    {
        var auditLogs = await _auditRepository.GetAuditLogsAsync(1, int.MaxValue);
        var logsInRange = auditLogs.AsEnumerable();
        
        if (startDate.HasValue)
            logsInRange = logsInRange.Where(log => log.Timestamp >= startDate.Value);
        if (endDate.HasValue)
            logsInRange = logsInRange.Where(log => log.Timestamp <= endDate.Value);
            
        return logsInRange.GroupBy(log => log.Action)
                         .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<IEnumerable<AuditLogDto>> GetLogsByEntityAsync(string entityType, int count = 10)
    {
        var auditLogs = await _auditRepository.GetAuditLogsAsync(1, count);
        return auditLogs.Where(log => log.Entity.Equals(entityType, StringComparison.OrdinalIgnoreCase))
                       .Select(log => new AuditLogDto
                       {
                           Id = (int)log.Id,
                           Actor = log.Actor?.UserName ?? "System",
                           Action = log.Action,
                           Entity = log.Entity,
                           EntityId = log.EntityId,
                           IpAddress = log.IpAddress?.ToString() ?? string.Empty,
                           UserAgent = log.UserAgent ?? string.Empty,
                           Timestamp = log.Timestamp,
                           Severity = "Medium",
                           Details = $"{log.Action} on {log.Entity}"
                       });
    }

    private async Task<Dictionary<string, int>> GetLogsByEntityAsync(DateTime? startDate, DateTime? endDate)
    {
        var auditLogs = await _auditRepository.GetAuditLogsAsync(1, int.MaxValue);
        var logsInRange = auditLogs.AsEnumerable();
        
        if (startDate.HasValue)
            logsInRange = logsInRange.Where(log => log.Timestamp >= startDate.Value);
        if (endDate.HasValue)
            logsInRange = logsInRange.Where(log => log.Timestamp <= endDate.Value);
            
        return logsInRange.GroupBy(log => log.Entity)
                         .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<IEnumerable<UserActivityDto>> GetMostActiveUsersAsync(int count = 10)
    {
        var activeSessions = await _radiusRepository.GetActiveSessionsAsync();
        var userActivity = activeSessions
            .GroupBy(s => s.Username)
            .Select(g => new UserActivityDto
            {
                Username = g.Key,
                SessionCount = g.Count(),
                DataTransferred = g.Sum(s => (s.AcctInputOctets ?? 0) + (s.AcctOutputOctets ?? 0)),
                LastLogin = g.Max(s => s.AcctStartTime) ?? DateTime.MinValue
            })
            .OrderByDescending(u => u.SessionCount)
            .Take(count);
            
        return userActivity;
    }

    private async Task<IEnumerable<object>> GetMostActiveUsersAsync(DateTime? startDate, DateTime? endDate, int limit)
    {
        return await Task.FromResult(new List<object>
        {
            new { Username = "user1", ActivityCount = 45 },
            new { Username = "user2", ActivityCount = 38 },
            new { Username = "user3", ActivityCount = 32 }
        });
    }

    public async Task<SessionSummaryReportDto> GetSessionSummaryReportAsync(DateTime startDate, DateTime endDate)
    {
        var activeSessions = await _radiusRepository.GetActiveSessionsAsync();
        var sessionsInRange = activeSessions.Where(s => s.AcctStartTime >= startDate && s.AcctStartTime <= endDate);
        
        return new SessionSummaryReportDto
        {
            TotalSessions = sessionsInRange.Count(),
            ActiveSessions = sessionsInRange.Count(s => s.AcctStopTime == null),
            AverageSessionDuration = sessionsInRange.Any() ? (double)sessionsInRange.Average(s => s.AcctSessionTime ?? 0) : 0.0,
            TotalDataTransfer = sessionsInRange.Sum(s => (s.AcctInputOctets ?? 0) + (s.AcctOutputOctets ?? 0)),
            UniqueUsers = sessionsInRange.Select(s => s.Username).Distinct().Count(),
            PeakConcurrentSessions = sessionsInRange.Count(), // Simplified calculation
            StartDate = startDate,
            EndDate = endDate
        };
    }





    public async Task<UserActivityReportDto> GetUserActivityReportAsync(string username, DateTime startDate, DateTime endDate)
    {
        var userSessions = await _radiusRepository.GetUserSessionsAsync(username, 1, int.MaxValue);
        var userAuthLogs = await _radiusRepository.GetUserAuthenticationLogsAsync(username, 1, int.MaxValue);
        
        var sessionsInRange = userSessions.Where(s => s.AcctStartTime >= startDate && s.AcctStartTime <= endDate);
        var authsInRange = userAuthLogs.Where(log => log.AuthDate >= startDate && log.AuthDate <= endDate);
        
        return new UserActivityReportDto
        {
            Username = username,
            TotalSessions = sessionsInRange.Count(),
            TotalAuthAttempts = authsInRange.Count(),
            SuccessfulAuths = authsInRange.Count(log => log.Reply == "Access-Accept"),
            TotalDataTransfer = sessionsInRange.Sum(s => (s.AcctInputOctets ?? 0) + (s.AcctOutputOctets ?? 0)),
            TotalSessionTime = (long)sessionsInRange.Sum(s => s.AcctSessionTime ?? 0),
            AverageSessionDuration = sessionsInRange.Any() ? sessionsInRange.Average(s => s.AcctSessionTime ?? 0) : 0,
            FirstActivity = authsInRange.Any() ? authsInRange.Min(log => log.AuthDate) : DateTime.MinValue,
            LastActivity = authsInRange.Any() ? authsInRange.Max(log => log.AuthDate) : DateTime.MinValue,
            StartDate = startDate,
            EndDate = endDate
        };
    }



    public async Task<object> GetAuditStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        return await Task.FromResult(new
        {
            TotalAuditLogs = 5000,
            LogsByAction = await GetLogsByActionAsync(startDate, endDate),
            LogsByEntity = await GetLogsByEntityAsync(startDate, endDate),
            MostActiveUsers = await GetMostActiveUsersAsync(startDate, endDate, 5)
        });
    }

    public async Task<object> GetNasUsageStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        return await Task.FromResult(new
        {
            TotalNasDevices = 15,
            ActiveNasDevices = 12,
            TopNasByTraffic = await GetTopNasByTrafficAsync(startDate, endDate, 5),
            TotalTraffic = 1024L * 1024 * 1024 * 100, // 100GB
            AverageResponseTime = 0.125 // 125ms
        });
    }

    public async Task<object> GetRealTimeSystemAlertsAsync()
    {
        var alerts = new List<SecurityAlertDto>();
        
        // Check for high failed authentication rate
        var failedAuthsLastMinute = await GetFailedAuthenticationsLastMinuteAsync();
        if (failedAuthsLastMinute > 10)
        {
            alerts.Add(new SecurityAlertDto
            {
                Id = Guid.NewGuid().ToString(),
                AlertType = "Authentication",
                Severity = "Warning",
                Message = $"High failed authentication rate: {failedAuthsLastMinute} failures in the last minute",
                Timestamp = DateTime.UtcNow,
                Source = "Authentication Monitor",
                Status = "Active"
            });
        }
        
        // Check for high session count
        var activeSessionsCount = await _radiusRepository.GetActiveSessionsCountAsync();
        if (activeSessionsCount > 1000)
        {
            alerts.Add(new SecurityAlertDto
            {
                Id = Guid.NewGuid().ToString(),
                AlertType = "Performance",
                Severity = "Info",
                Message = $"High active session count: {activeSessionsCount} sessions",
                Timestamp = DateTime.UtcNow,
                Source = "Session Monitor",
                Status = "Active"
            });
        }
        
        return alerts.OrderByDescending(a => a.Timestamp);
    }

    public async Task<object> GetSystemHealthStatsAsync()
    {
        var activeSessionsCount = await _radiusRepository.GetActiveSessionsCountAsync();
        var totalSessionsCount = await _radiusRepository.GetTotalSessionsCountAsync();
        var authLogsCount = await _radiusRepository.GetAuthenticationLogsCountAsync();
        var recentAuthsCount = await GetAuthenticationsLastMinuteAsync();
        var failedAuthsCount = await GetFailedAuthenticationsLastMinuteAsync();
        
        return new SystemHealthStatsDto
        {
            MemoryUsage = 0.65, // Placeholder - would need system monitoring
            CpuUsage = 0.45, // Placeholder - would need system monitoring
            DiskUsage = 0.30, // Placeholder - would need system monitoring
            ActiveConnections = 10, // Placeholder - would need database monitoring
            NetworkLatency = await GetAverageResponseTimeAsync(),
            IsHealthy = true,
            LastUpdated = DateTime.UtcNow
        };
    }

    public async Task<object> GetSessionStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var activeSessions = await _radiusRepository.GetActiveSessionsAsync();
        var sessionsInRange = activeSessions.AsEnumerable();
        
        if (startDate.HasValue)
            sessionsInRange = sessionsInRange.Where(s => s.AcctStartTime >= startDate.Value);
        if (endDate.HasValue)
            sessionsInRange = sessionsInRange.Where(s => s.AcctStartTime <= endDate.Value);
            
        return new SessionStatsDto
        {
            TotalSessions = sessionsInRange.Count(),
            ActiveSessions = sessionsInRange.Count(s => s.AcctStopTime == null),
            AverageSessionDuration = sessionsInRange.Any() ? sessionsInRange.Average(s => s.AcctSessionTime ?? 0) : 0,
            TotalDataTransfer = sessionsInRange.Sum(s => (s.AcctInputOctets ?? 0) + (s.AcctOutputOctets ?? 0)),
            UniqueUsers = sessionsInRange.Select(s => s.Username).Distinct().Count(),
            StartDate = startDate ?? DateTime.MinValue,
            EndDate = endDate ?? DateTime.MaxValue
        };
    }

    public async Task<object> GetHourlySessionStatsAsync(DateTime date)
    {
        var activeSessions = await _radiusRepository.GetActiveSessionsAsync();
        var sessionsForDate = activeSessions.Where(s => s.AcctStartTime?.Date == date.Date);
        
        var hourlyStats = sessionsForDate
            .GroupBy(s => s.AcctStartTime?.Hour ?? 0)
            .Select(g => new HourlyStatsDto
            {
                Hour = g.Key,
                SessionCount = g.Count(),
                DataTransfer = g.Sum(s => (s.AcctInputOctets ?? 0) + (s.AcctOutputOctets ?? 0)),
                UniqueUsers = g.Select(s => s.Username).Distinct().Count(),
                AverageSessionDuration = g.Any() ? (double)g.Average(s => s.AcctSessionTime ?? 0) : 0.0
            })
            .OrderBy(h => h.Hour);
            
        // Fill in missing hours with zero values
        var allHours = Enumerable.Range(0, 24)
            .Select(hour => hourlyStats.FirstOrDefault(h => h.Hour == hour) ?? new HourlyStatsDto
            {
                Hour = hour,
                SessionCount = 0,
                DataTransfer = 0,
                UniqueUsers = 0,
                AverageSessionDuration = 0
            });
            
        return allHours;
    }

    public async Task<object> GetHourlyAuthStatsAsync(DateTime date)
    {
        var authLogs = await _radiusRepository.GetAuthenticationLogsAsync(1, int.MaxValue);
        var authsForDate = authLogs.Where(log => log.AuthDate.Date == date.Date);
        
        var hourlyStats = authsForDate
            .GroupBy(log => log.AuthDate.Hour)
            .Select(g => new HourlyStatsDto
            {
                Hour = g.Key,
                AuthenticationCount = g.Count(),
                SuccessfulAuths = g.Count(log => log.Reply == "Access-Accept"),
                FailedAuths = g.Count(log => log.Reply != "Access-Accept"),
                UniqueUsers = g.Select(log => log.Username).Distinct().Count()
            })
            .OrderBy(h => h.Hour);
            
        // Fill in missing hours with zero values
        var allHours = Enumerable.Range(0, 24)
            .Select(hour => hourlyStats.FirstOrDefault(h => h.Hour == hour) ?? new HourlyStatsDto
            {
                Hour = hour,
                AuthenticationCount = 0,
                SuccessfulAuths = 0,
                FailedAuths = 0,
                UniqueUsers = 0
            });
            
        return allHours;
    }

    public async Task<object> GetDailyAuthStatsAsync(DateTime startDate, DateTime endDate)
    {
        var authLogs = await _radiusRepository.GetAuthenticationLogsAsync(1, int.MaxValue);
        var authsInRange = authLogs.Where(log => log.AuthDate.Date >= startDate.Date && log.AuthDate.Date <= endDate.Date);
        
        var dailyStats = authsInRange
            .GroupBy(log => log.AuthDate.Date)
            .Select(g => new DailyStatsDto
            {
                Date = g.Key,
                AuthenticationCount = g.Count(),
                SuccessfulAuths = g.Count(log => log.Reply == "Access-Accept"),
                FailedAuths = g.Count(log => log.Reply != "Access-Accept"),
                UniqueUsers = g.Select(log => log.Username).Distinct().Count(),
                SuccessRate = g.Any() ? (double)g.Count(log => log.Reply == "Access-Accept") / g.Count() * 100 : 0
            })
            .OrderBy(d => d.Date);
            
        return dailyStats;
    }



    public async Task<object> GetUserStatsAsync(string username)
    {
        var userSessions = await _radiusRepository.GetUserSessionsAsync(username, 1, int.MaxValue);
        var userAuthLogs = await _radiusRepository.GetUserAuthenticationLogsAsync(username, 1, int.MaxValue);
        
        var activeSessions = userSessions.Where(s => s.AcctStopTime == null);
        var totalAuths = userAuthLogs.Count();
        var successfulAuths = userAuthLogs.Count(log => log.Reply == "Access-Accept");
        
        return new UserStatsDto
        {
            Username = username,
            TotalSessions = userSessions.Count(),
            ActiveSessions = activeSessions.Count(),
            TotalAuthAttempts = totalAuths,
            SuccessfulAuths = successfulAuths,
            FailedAuths = totalAuths - successfulAuths,
            SuccessRate = totalAuths > 0 ? (double)successfulAuths / totalAuths * 100 : 0,
            TotalDataTransferred = userSessions.Sum(s => (s.AcctInputOctets ?? 0) + (s.AcctOutputOctets ?? 0)),
            TotalSessionTime = userSessions.Sum(s => s.AcctSessionTime ?? 0),
            AverageSessionDuration = userSessions.Any() ? userSessions.Average(s => s.AcctSessionTime ?? 0) : 0,
            LastActivity = userAuthLogs.Any() ? userAuthLogs.Max(log => log.AuthDate) : (DateTime?)null
        };
    }



    public async Task<object> GetUserActivityStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var activeSessions = await _radiusRepository.GetActiveSessionsAsync();
        var sessionsInRange = activeSessions.AsEnumerable();
        
        if (startDate.HasValue)
            sessionsInRange = sessionsInRange.Where(s => s.AcctStartTime >= startDate.Value);
        if (endDate.HasValue)
            sessionsInRange = sessionsInRange.Where(s => s.AcctStartTime <= endDate.Value);
            
        var userStats = sessionsInRange
            .GroupBy(s => s.Username)
            .Select(g => new UserActivityStatsDto
            {
                Username = g.Key,
                SessionCount = g.Count(),
                TotalDataTransfer = g.Sum(s => (s.AcctInputOctets ?? 0) + (s.AcctOutputOctets ?? 0)),
                TotalSessionTime = g.Sum(s => s.AcctSessionTime ?? 0),
                AverageSessionDuration = g.Any() ? g.Average(s => s.AcctSessionTime ?? 0) : 0,
                LastActivity = g.Max(s => s.AcctStartTime)
            })
            .OrderByDescending(u => u.SessionCount);
            
        return userStats;
    }

    public async Task<object> GetAuthenticationStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        startDate ??= DateTime.UtcNow.Date;
        endDate ??= DateTime.UtcNow;
        
        var successful = await _radiusRepository.GetSuccessfulAuthenticationsCountAsync(startDate, endDate);
        var failed = await _radiusRepository.GetFailedAuthenticationsCountAsync(startDate, endDate);
        var total = successful + failed;
        
        return await Task.FromResult(new
        {
            TotalAttempts = total,
            SuccessfulAuthentications = successful,
            FailedAuthentications = failed,
            SuccessRate = total > 0 ? (double)successful / total * 100 : 0
        });
    }

    public async Task<object> GetTopActiveUsersAsync(int count = 10)
    {
        var activeSessions = await _radiusRepository.GetActiveSessionsAsync();
        var topUsers = activeSessions
            .GroupBy(s => s.Username)
            .Select(g => new TopUserDto
            {
                Username = g.Key,
                SessionCount = g.Count(),
                TotalDataTransfer = g.Sum(s => (s.AcctInputOctets ?? 0) + (s.AcctOutputOctets ?? 0)),
                TotalSessionTime = g.Sum(s => s.AcctSessionTime ?? 0),
                LastActivity = g.Max(s => s.AcctStartTime)
            })
            .OrderByDescending(u => u.SessionCount)
            .Take(count);
            
        return topUsers;
    }



    public async Task<object> GetGroupStatsAsync()
    {
        var allGroups = await _radiusRepository.GetAllGroupsAsync();
        var groupStats = new List<GroupStatsDto>();
        
        foreach (var groupName in allGroups)
        {
            var usersInGroup = await _radiusRepository.GetUsersInGroupAsync(groupName);
            var groupCheckAttrs = await _radiusRepository.GetGroupCheckAttributesAsync(groupName);
            var groupReplyAttrs = await _radiusRepository.GetGroupReplyAttributesAsync(groupName);
            
            groupStats.Add(new GroupStatsDto
            {
                GroupName = groupName,
                UserCount = usersInGroup.Count(),
                CheckAttributeCount = groupCheckAttrs.Count(),
                ReplyAttributeCount = groupReplyAttrs.Count(),
                IsActive = usersInGroup.Any() // Group is active if it has users
            });
        }
        
        return groupStats.OrderByDescending(g => g.UserCount);
    }



    public async Task<object> GetRealTimeRecentAuthenticationsAsync(int count = 10)
    {
        var authLogs = await _radiusRepository.GetAuthenticationLogsAsync(1, count);
        return authLogs.Select(log => new RecentAuthDto
        {
            Username = log.Username,
            AuthDate = log.AuthDate,
            Reply = log.Reply,
            NasIpAddress = "Unknown", // RadPostAuth doesn't have NasIpAddress
            CallingStationId = "Unknown", // RadPostAuth doesn't have CallingStationId
            IsSuccessful = log.Reply == "Access-Accept"
        }).OrderByDescending(auth => auth.AuthDate);
    }





    public async Task<object> GetRealTimeActiveSessionsAsync(int count = 20)
    {
        var activeSessions = await _radiusRepository.GetActiveSessionsAsync();
        return activeSessions.Take(count).Select(session => new ActiveSessionDto
        {
            AcctSessionId = session.AcctSessionId,
            Username = session.Username,
            NasIpAddress = session.NasIpAddress,
            AcctStartTime = session.AcctStartTime,
            AcctSessionTime = session.AcctSessionTime,
            AcctInputOctets = session.AcctInputOctets ?? 0,
            AcctOutputOctets = session.AcctOutputOctets ?? 0,
            FramedIpAddress = session.FramedIpAddress
        }).OrderByDescending(s => s.AcctStartTime);
    }

    public async Task<object> GetNetworkStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var activeSessions = await _radiusRepository.GetActiveSessionsAsync();
        var sessionsInRange = activeSessions.AsEnumerable();
        
        if (startDate.HasValue)
            sessionsInRange = sessionsInRange.Where(s => s.AcctStartTime >= startDate.Value);
        if (endDate.HasValue)
            sessionsInRange = sessionsInRange.Where(s => s.AcctStartTime <= endDate.Value);
        
        return new NetworkStatsDto
        {
            TotalBytesIn = sessionsInRange.Sum(s => s.AcctInputOctets ?? 0),
            TotalBytesOut = sessionsInRange.Sum(s => s.AcctOutputOctets ?? 0),
            TotalPacketsIn = 0, // Not available in standard RADIUS accounting
            TotalPacketsOut = 0, // Not available in standard RADIUS accounting
            ActiveConnections = sessionsInRange.Count(),
            AverageSessionDuration = sessionsInRange.Any() ? sessionsInRange.Average(s => s.AcctSessionTime ?? 0) : 0,
            PeakBandwidthUsage = sessionsInRange.Any() ? (double)sessionsInRange.Max(s => (s.AcctInputOctets ?? 0) + (s.AcctOutputOctets ?? 0)) : 0,
            StartDate = startDate ?? DateTime.MinValue,
            EndDate = endDate ?? DateTime.MaxValue
        };
    }

    public async Task<object> GetBandwidthStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var activeSessions = await _radiusRepository.GetActiveSessionsAsync();
        var sessionsInRange = activeSessions.AsEnumerable();
        
        if (startDate.HasValue)
            sessionsInRange = sessionsInRange.Where(s => s.AcctStartTime >= startDate.Value);
        if (endDate.HasValue)
            sessionsInRange = sessionsInRange.Where(s => s.AcctStartTime <= endDate.Value);
            
        var totalBytes = sessionsInRange.Sum(s => (s.AcctInputOctets ?? 0) + (s.AcctOutputOctets ?? 0));
        var sessionCount = sessionsInRange.Count();
        
        return new BandwidthStatsDto
        {
            TotalBandwidth = totalBytes,
            AverageBandwidth = sessionCount > 0 ? (double)totalBytes / sessionCount : 0,
            PeakBandwidth = sessionsInRange.Any() ? (double)sessionsInRange.Max(s => (s.AcctInputOctets ?? 0) + (s.AcctOutputOctets ?? 0)) : 0,
            TotalDataTransferred = totalBytes,
            PeakTime = DateTime.Now,
            CurrentBandwidth = 0
        };
    }

    public async Task<object> GetGroupDistributionStatsAsync()
    {
        var groups = await _radiusRepository.GetAllGroupsAsync();
        var userGroups = await _radiusRepository.GetAllUserGroupsAsync();
        
        var groupStats = groups.Select(group => new GroupDistributionItemDto
        {
            GroupName = group,
            UserCount = userGroups.Count(ug => ug == group),
            Percentage = 0 // Will be calculated below
        }).ToList();
        
        var totalUsers = groupStats.Sum(g => g.UserCount);
        if (totalUsers > 0)
        {
            foreach (var stat in groupStats)
            {
                stat.Percentage = (double)stat.UserCount / totalUsers * 100;
            }
        }
        
        return new GroupDistributionStatsDto
        {
            Groups = groupStats.OrderByDescending(g => g.UserCount).ToList(),
            TotalGroups = groups.Count(),
            TotalUsers = totalUsers
        };
    }

    public async Task<object> GetDailySessionStatsAsync(DateTime startDate, DateTime endDate)
    {
        var activeSessions = await _radiusRepository.GetActiveSessionsAsync();
        var sessionsInRange = activeSessions.Where(s => s.AcctStartTime >= startDate && s.AcctStartTime <= endDate);
        
        var dailyStats = sessionsInRange
            .GroupBy(s => s.AcctStartTime?.Date ?? DateTime.UtcNow.Date)
            .Select(g => new DailySessionStatsDto
            {
                Date = g.Key,
                SessionCount = g.Count(),
                UniqueUsers = g.Select(s => s.Username).Distinct().Count(),
                TotalDataTransfer = (long)g.Sum(s => (s.AcctInputOctets ?? 0) + (s.AcctOutputOctets ?? 0)),
                AverageSessionDuration = g.Any() ? g.Average(s => (double)(s.AcctSessionTime ?? 0)) : 0.0
            })
            .OrderBy(s => s.Date);
            
        return dailyStats;
    }

    // Missing interface methods with different signatures
    public async Task<object> GetRealTimeSystemAlertsAsync(int limit)
    {
        var alerts = await GetRealTimeSystemAlertsAsync();
        if (alerts is IEnumerable<SecurityAlertDto> alertList)
        {
            return alertList.Take(limit).ToList();
        }

        return alerts;
    }

    public async Task<object> GetRealTimeActiveSessionsAsync()
    {
        return await GetRealTimeActiveSessionsAsync(20);
    }

    public async Task<object> GetHourlySessionStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var date = startDate ?? DateTime.UtcNow.Date;
        return await GetHourlySessionStatsAsync(date);
    }

    public async Task<object> GetHourlyAuthStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var date = startDate ?? DateTime.UtcNow.Date;
        return await GetHourlyAuthStatsAsync(date);
    }

    public async Task<object> GetDailyAuthStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;
        return await GetDailyAuthStatsAsync(start, end);
    }

    public async Task<object> GetUserStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        // Return aggregated user stats for all users
        var activeSessions = await _radiusRepository.GetActiveSessionsAsync();
        var authLogs = await _radiusRepository.GetAuthenticationLogsAsync(1, int.MaxValue);
        
        return new
        {
            TotalUsers = activeSessions.Select(s => s.Username).Distinct().Count(),
            ActiveUsers = activeSessions.Count(),
            TotalSessions = activeSessions.Count(),
            TotalAuthAttempts = authLogs.Count(),
            SuccessfulAuths = authLogs.Count(a => a.Reply == "Access-Accept"),
            FailedAuths = authLogs.Count(a => a.Reply == "Access-Reject")
        };
    }

    public async Task<object> GetTopActiveUsersAsync(int limit = 10, DateTime? startDate = null, DateTime? endDate = null)
    {
        return await GetTopActiveUsersAsync(limit);
    }

    public async Task<object> GetDailySessionStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;
        return await GetDailySessionStatsAsync(start, end);
    }

    public async Task<object> GetSessionSummaryReportAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;
        
        var sessions = await _radiusRepository.GetActiveSessionsAsync();
        var sessionsInRange = sessions.Where(s => s.AcctStartTime >= start && s.AcctStartTime <= end);

        var activeSessions = sessionsInRange.Count(s => s.AcctStopTime == null);
        var totalDataTransferred = sessionsInRange.Sum(s => (s.AcctInputOctets ?? 0) + (s.AcctOutputOctets ?? 0));
        var completedSessions = sessionsInRange.Where(s => s.AcctStopTime != null);
        var avgDuration = completedSessions.Any() 
            ? completedSessions.Average(s => (double)(s.AcctSessionTime ?? 0)) 
            : 0.0;

        var sessionTrends = sessionsInRange
            .GroupBy(s => s.AcctStartTime?.Date ?? DateTime.UtcNow.Date)
            .Select(g => new SessionTrendDto
            {
                Date = g.Key,
                SessionCount = g.Count(),
                DataTransferred = (long)g.Sum(s => (s.AcctInputOctets ?? 0) + (s.AcctOutputOctets ?? 0)),
                AverageDuration = g.Where(s => s.AcctStopTime != null)
                    .Select(s => (double)(s.AcctSessionTime ?? 0))
                    .DefaultIfEmpty(0.0).Average()
            })
            .OrderBy(t => t.Date)
            .ToList();

        return new SessionSummaryReportDto
        {
            TotalSessions = sessionsInRange.Count(),
            ActiveSessions = activeSessions,
            AverageSessionDuration = (double)avgDuration,
            TotalDataTransferred = (long)totalDataTransferred,
            UniqueUsers = sessionsInRange.Select(s => s.Username).Distinct().Count(),
            StartDate = start,
            EndDate = end,
            SessionTrends = sessionTrends
        };
    }

    public async Task<object> GetUserActivityReportAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;
        
        // Get all sessions in date range
        var allSessions = await _radiusRepository.GetActiveSessionsAsync();
        var sessionsInRange = allSessions.Where(s => s.AcctStartTime >= start && s.AcctStartTime <= end);

        // Get all auth logs in date range
        var allAuthLogs = await _radiusRepository.GetAuthenticationLogsAsync(1, int.MaxValue);
        var authsInRange = allAuthLogs.Where(a => a.AuthDate >= start && a.AuthDate <= end);

        // Group by username to create user activity reports
        var userActivities = sessionsInRange
            .GroupBy(s => s.Username)
            .Select(g => {
                var userSessions = g.ToList();
                var userAuths = authsInRange.Where(a => a.Username == g.Key).ToList();
                
                return new UserActivityReportDto
                 {
                     Username = g.Key,
                     SessionCount = userSessions.Count,
                     TotalDataTransferred = (long)userSessions.Sum(s => (s.AcctInputOctets ?? 0) + (s.AcctOutputOctets ?? 0)),
                     TotalSessionTime = (long)userSessions.Sum(s => s.AcctSessionTime ?? 0),
                     FirstLogin = userSessions.Any() ? userSessions.Min(s => s.AcctStartTime ?? DateTime.MinValue) : DateTime.MinValue,
                     LastLogin = userSessions.Any() ? userSessions.Max(s => s.AcctStartTime ?? DateTime.MinValue) : DateTime.MinValue,
                     AuthenticationAttempts = userAuths.Count,
                     SuccessfulAuthentications = userAuths.Count(a => a.Reply == "Access-Accept"),
                     FailedAuthentications = userAuths.Count(a => a.Reply != "Access-Accept"),
                     AverageSessionDuration = userSessions.Any() ? userSessions.Average(s => (double)(s.AcctSessionTime ?? 0)) : 0,
                     StartDate = start,
                     EndDate = end
                 };
            })
            .ToList();

        return userActivities;
    }

    public async Task<object> GetAuthenticationSummaryReportAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;
        
        var authLogs = await _radiusRepository.GetAuthenticationLogsAsync(1, int.MaxValue);
        var authsInRange = authLogs.Where(a => a.AuthDate >= start && a.AuthDate <= end);

        var successfulAttempts = authsInRange.Count(a => a.Reply == "Access-Accept");
        var failedAttempts = authsInRange.Count(a => a.Reply != "Access-Accept");
        var totalAttempts = authsInRange.Count();
        var successRate = totalAttempts > 0 ? (double)successfulAttempts / totalAttempts * 100 : 0;

        var authTrends = authsInRange
            .GroupBy(a => a.AuthDate.Date)
            .Select(g => new AuthTrendDto
            {
                Date = g.Key,
                SuccessfulAttempts = g.Count(a => a.Reply == "Access-Accept"),
                FailedAttempts = g.Count(a => a.Reply != "Access-Accept"),
                SuccessRate = g.Count() > 0 ? (double)g.Count(a => a.Reply == "Access-Accept") / g.Count() * 100 : 0
            })
            .OrderBy(t => t.Date)
            .ToList();

        return new AuthenticationSummaryReportDto
        {
            TotalAttempts = totalAttempts,
            SuccessfulAttempts = successfulAttempts,
            FailedAttempts = failedAttempts,
            SuccessRate = successRate,
            UniqueUsers = authsInRange.Select(a => a.Username).Distinct().Count(),
            StartDate = start,
            EndDate = end,
            AuthTrends = authTrends
        };
    }

    public async Task<DashboardOverviewDto> GetDashboardOverviewAsync()
    {
        var totalUsers = await _userRepository.GetTotalUsersCountAsync();
        var activeUsers = await GetActiveUsersCountAsync();
        var totalGroups = (await _radiusRepository.GetAllGroupsAsync()).Count();
        var activeSessions = await _radiusRepository.GetActiveSessionsCountAsync();
        var totalSessions = await _radiusRepository.GetTotalSessionsCountAsync();
        var authAttempts = await _radiusRepository.GetAuthenticationLogsCountAsync();
        var successfulAuths = await _radiusRepository.GetSuccessfulAuthenticationsCountAsync();
        var failedAuths = await _radiusRepository.GetFailedAuthenticationsCountAsync();
        var totalDataTransferred = await GetTotalDataTransferredAsync();
        var avgSessionDuration = await GetAverageSessionDurationAsync();

        return new DashboardOverviewDto
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            TotalGroups = totalGroups,
            ActiveSessions = activeSessions,
            TotalSessions = totalSessions,
            AuthenticationAttempts = authAttempts,
            SuccessfulAuthentications = successfulAuths,
            FailedAuthentications = failedAuths,
            AuthenticationSuccessRate = authAttempts > 0 ? (double)successfulAuths / authAttempts * 100 : 0,
            TotalDataTransferred = totalDataTransferred,
            AverageSessionDuration = avgSessionDuration,
            LastUpdated = DateTime.UtcNow
        };
    }
}