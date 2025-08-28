using System.Net;

namespace RadiusConnect.Api.Models.DTOs;

public class OverviewStatistics
{
    public int TotalUsers { get; set; }
    public int ActiveSessions { get; set; }
    public int TotalSessions { get; set; }
    public int SuccessfulAuthentications { get; set; }
    public int FailedAuthentications { get; set; }
}

public class UserStatistics
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int NewUsersToday { get; set; }
    public int NewUsersThisWeek { get; set; }
    public int NewUsersThisMonth { get; set; }
}

public class SessionStatistics
{
    public int TotalSessions { get; set; }
    public int ActiveSessions { get; set; }
    public double AverageSessionDuration { get; set; }
    public int SessionsStarted { get; set; }
    public int SessionsEnded { get; set; }
}

public class AuthenticationStatistics
{
    public int TotalAttempts { get; set; }
    public int SuccessfulAuthentications { get; set; }
    public int FailedAuthentications { get; set; }
    public double SuccessRate { get; set; }
}

public class NetworkStatistics
{
    public long TotalDataTransferred { get; set; }
    public long AverageDataPerSession { get; set; }
    public IEnumerable<NasTrafficData> TopNasByTraffic { get; set; } = new List<NasTrafficData>();
}

public class NasTrafficData
{
    public IPAddress NasIp { get; set; } = IPAddress.None;
    public long TotalBytes { get; set; }
    public int SessionCount { get; set; }
}

public class RealTimeData
{
    public int CurrentActiveSessions { get; set; }
    public int AuthenticationsLastMinute { get; set; }
    public int FailedAuthenticationsLastMinute { get; set; }
    public int NewSessionsLastMinute { get; set; }
    public DateTime Timestamp { get; set; }
}

public class RecentActivity
{
    public string Type { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Details { get; set; } = string.Empty;
}

public class UsageReport
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalSessions { get; set; }
    public long TotalDataTransferred { get; set; }
    public int UniqueUsers { get; set; }
    public double AverageSessionDuration { get; set; }
    public DateTime GeneratedAt { get; set; }
}

public class PerformanceReport
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalAuthenticationAttempts { get; set; }
    public int SuccessfulAuthentications { get; set; }
    public int FailedAuthentications { get; set; }
    public double SuccessRate { get; set; }
    public double AverageResponseTime { get; set; }
    public DateTime GeneratedAt { get; set; }
}

public class AuditStatistics
{
    public int TotalLogs { get; set; }
    public Dictionary<string, int> LogsByAction { get; set; } = new();
    public Dictionary<string, int> LogsByEntity { get; set; } = new();
    public IEnumerable<object> MostActiveUsers { get; set; } = new List<object>();
}