namespace RadiusConnect.Api.Models.DTOs;

public class DashboardOverviewDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalGroups { get; set; }
    public int ActiveSessions { get; set; }
    public int TotalSessions { get; set; }
    public int AuthenticationAttempts { get; set; }
    public int SuccessfulAuthentications { get; set; }
    public int FailedAuthentications { get; set; }
    public double AuthenticationSuccessRate { get; set; }
    public long TotalDataTransferred { get; set; }
    public double AverageSessionDuration { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class SystemHealthDto
{
    public string Status { get; set; } = string.Empty;
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public double DiskUsage { get; set; }
    public bool DatabaseConnected { get; set; }
    public bool RadiusServerConnected { get; set; }
    public TimeSpan Uptime { get; set; }
    public DateTime LastHealthCheck { get; set; }
    public List<ServiceStatusDto> Services { get; set; } = new();
}

public class ServiceStatusDto
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime LastCheck { get; set; }
    public string? ErrorMessage { get; set; }
}

public class SessionAnalyticsDto
{
    public int ActiveSessions { get; set; }
    public int TotalSessions { get; set; }
    public double AverageSessionDuration { get; set; }
    public long TotalDataTransferred { get; set; }
    public List<SessionTrendDto> SessionTrends { get; set; } = new();
    public List<TopUserDto> TopUsers { get; set; } = new();
    public List<SessionsByLocationDto> SessionsByLocation { get; set; } = new();
}

public class SessionTrendDto
{
    public DateTime Date { get; set; }
    public int SessionCount { get; set; }
    public long DataTransferred { get; set; }
    public double AverageDuration { get; set; }
}

public class SessionsByLocationDto
{
    public string Location { get; set; } = string.Empty;
    public int SessionCount { get; set; }
    public long DataTransferred { get; set; }
}

public class AuthenticationAnalyticsDto
{
    public int TotalAttempts { get; set; }
    public int SuccessfulAttempts { get; set; }
    public int FailedAttempts { get; set; }
    public double SuccessRate { get; set; }
    public List<AuthTrendDto> AuthTrends { get; set; } = new();
    public List<TopFailedUserDto> TopFailedUsers { get; set; } = new();
    public List<AuthByLocationDto> AuthByLocation { get; set; } = new();
}

public class AuthTrendDto
{
    public DateTime Date { get; set; }
    public int SuccessfulAttempts { get; set; }
    public int FailedAttempts { get; set; }
    public double SuccessRate { get; set; }
}

public class AuthByLocationDto
{
    public string Location { get; set; } = string.Empty;
    public int SuccessfulAttempts { get; set; }
    public int FailedAttempts { get; set; }
    public double SuccessRate { get; set; }
}

public class UserAnalyticsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int NewUsersToday { get; set; }
    public int NewUsersThisWeek { get; set; }
    public int NewUsersThisMonth { get; set; }
    public List<UserGrowthDto> UserGrowth { get; set; } = new();
    public List<UserActivityDto> UserActivity { get; set; } = new();
}

public class UserGrowthDto
{
    public DateTime Date { get; set; }
    public int NewUsers { get; set; }
    public int TotalUsers { get; set; }
}

public class UserActivityDto
{
    public string Username { get; set; } = string.Empty;
    public DateTime LastLogin { get; set; }
    public int SessionCount { get; set; }
    public long DataTransferred { get; set; }
}

public class GroupAnalyticsDto
{
    public int TotalGroups { get; set; }
    public List<GroupUsageDto> GroupUsage { get; set; } = new();
    public List<GroupActivityDto> GroupActivity { get; set; } = new();
}

public class GroupUsageDto
{
    public string GroupName { get; set; } = string.Empty;
    public int UserCount { get; set; }
    public int ActiveUsers { get; set; }
    public double UsagePercentage { get; set; }
}

public class GroupActivityDto
{
    public string GroupName { get; set; } = string.Empty;
    public int SessionCount { get; set; }
    public long DataTransferred { get; set; }
    public double AverageSessionDuration { get; set; }
}

public class NetworkAnalyticsDto
{
    public long TotalDataTransferred { get; set; }
    public long DataTransferredToday { get; set; }
    public long DataTransferredThisWeek { get; set; }
    public long DataTransferredThisMonth { get; set; }
    public List<NetworkUsageDto> NetworkUsage { get; set; } = new();
    public List<TopLocationDto> TopLocations { get; set; } = new();
}

public class NetworkUsageDto
{
    public DateTime Date { get; set; }
    public long DataTransferred { get; set; }
    public int SessionCount { get; set; }
}

public class TopLocationDto
{
    public string Location { get; set; } = string.Empty;
    public long DataTransferred { get; set; }
    public int SessionCount { get; set; }
}

public class RealTimeDataDto
{
    public int ActiveSessions { get; set; }
    public int CurrentUsers { get; set; }
    public double CurrentDataRate { get; set; }
    public List<RecentSessionDto> RecentSessions { get; set; } = new();
    public List<RecentAuthDto> RecentAuthentications { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

public class RecentSessionDto
{
    public string Username { get; set; } = string.Empty;
    public string NasIpAddress { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class RecentAuthDto
{
    public string Username { get; set; } = string.Empty;
    public DateTime AuthDate { get; set; }
    public string Reply { get; set; } = string.Empty;
    public string NasIpAddress { get; set; } = string.Empty;
    public string CallingStationId { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
}

public class ReportDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public long? FileSize { get; set; }
}

public class SessionSummaryReportDto
{
    public int TotalSessions { get; set; }
    public int ActiveSessions { get; set; }
    public double AverageSessionDuration { get; set; }
    public long TotalDataTransferred { get; set; }
    public long TotalDataTransfer { get; set; }
    public int PeakConcurrentSessions { get; set; }
    public int UniqueUsers { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<SessionTrendDto> SessionTrends { get; set; } = new();
}

public class AuthenticationSummaryReportDto
{
    public int TotalAttempts { get; set; }
    public int SuccessfulAttempts { get; set; }
    public int FailedAttempts { get; set; }
    public double SuccessRate { get; set; }
    public int UniqueUsers { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<AuthTrendDto> AuthTrends { get; set; } = new();
}

public class UserActivityReportDto
{
    public string Username { get; set; } = string.Empty;
    public int SessionCount { get; set; }
    public int TotalSessions { get; set; }
    public int TotalAuthAttempts { get; set; }
    public int SuccessfulAuths { get; set; }
    public long TotalDataTransferred { get; set; }
    public long TotalDataTransfer { get; set; }
    public double TotalSessionTime { get; set; }
    public DateTime FirstLogin { get; set; }
    public DateTime LastLogin { get; set; }
    public DateTime FirstActivity { get; set; }
    public DateTime LastActivity { get; set; }
    public int AuthenticationAttempts { get; set; }
    public int SuccessfulAuthentications { get; set; }
    public int FailedAuthentications { get; set; }
    public double AverageSessionDuration { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class GroupDistributionItemDto
{
    public string GroupName { get; set; } = string.Empty;
    public int UserCount { get; set; }
    public int ActiveUsers { get; set; }
    public double Percentage { get; set; }
    public long DataTransferred { get; set; }
}

public class SystemHealthStatsDto
{
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public double DiskUsage { get; set; }
    public int ActiveConnections { get; set; }
    public double NetworkLatency { get; set; }
    public bool IsHealthy { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class GroupStatsDto
{
    public string GroupName { get; set; } = string.Empty;
    public int UserCount { get; set; }
    public int ActiveUsers { get; set; }
    public int SessionCount { get; set; }
    public long DataTransferred { get; set; }
    public double AverageSessionDuration { get; set; }
    public int CheckAttributeCount { get; set; }
    public int ReplyAttributeCount { get; set; }
    public bool IsActive { get; set; }
}

public class DailySessionStatsDto
{
    public DateTime Date { get; set; }
    public int SessionCount { get; set; }
    public int ActiveSessions { get; set; }
    public long DataTransferred { get; set; }
    public long TotalDataTransfer { get; set; }
    public double AverageSessionDuration { get; set; }
    public int UniqueUsers { get; set; }
}

public class ActiveSessionDto
{
    public string Username { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string NasIpAddress { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public long DataTransferred { get; set; }
    public double Duration { get; set; }
    public long AcctInputOctets { get; set; }
    public long AcctOutputOctets { get; set; }
    public string FramedIpAddress { get; set; } = string.Empty;
    public long SessionTime { get; set; }
    public long InputOctets { get; set; }
    public long OutputOctets { get; set; }
    public string AcctSessionId { get; set; } = string.Empty;
    public DateTime? AcctStartTime { get; set; }
    public long? AcctSessionTime { get; set; }
}

public class GroupDistributionStatsDto
{
    public List<GroupDistributionItemDto> Groups { get; set; } = new();
    public int TotalGroups { get; set; }
    public int TotalUsers { get; set; }
}

public class NetworkStatsDto
{
    public long TotalInputOctets { get; set; }
    public long TotalOutputOctets { get; set; }
    public double InputRate { get; set; }
    public double OutputRate { get; set; }
    public int ActiveConnections { get; set; }
    public long TotalBytesIn { get; set; }
    public long TotalBytesOut { get; set; }
    public long TotalPacketsIn { get; set; }
    public long TotalPacketsOut { get; set; }
    public double AverageSessionDuration { get; set; }
    public double PeakBandwidthUsage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class BandwidthStatsDto
{
    public double CurrentBandwidth { get; set; }
    public double PeakBandwidth { get; set; }
    public double AverageBandwidth { get; set; }
    public DateTime PeakTime { get; set; }
    public long TotalDataTransferred { get; set; }
    public long TotalBandwidth { get; set; }
}

public class UserStatsDto
{
    public string Username { get; set; } = string.Empty;
    public int TotalSessions { get; set; }
    public int ActiveSessions { get; set; }
    public int TotalAuthAttempts { get; set; }
    public int SuccessfulAuths { get; set; }
    public int FailedAuths { get; set; }
    public double SuccessRate { get; set; }
    public long TotalDataTransferred { get; set; }
    public long TotalSessionTime { get; set; }
    public double AverageSessionDuration { get; set; }
    public DateTime? LastActivity { get; set; }
}

public class UserActivityStatsDto
{
    public string Username { get; set; } = string.Empty;
    public int SessionCount { get; set; }
    public long TotalDataTransfer { get; set; }
    public long TotalSessionTime { get; set; }
    public double AverageSessionDuration { get; set; }
    public DateTime? LastActivity { get; set; }
}

public class HourlyStatsDto
{
    public int Hour { get; set; }
    public int SessionCount { get; set; }
    public long DataTransferred { get; set; }
    public long DataTransfer { get; set; }
    public int UniqueUsers { get; set; }
    public double AverageSessionDuration { get; set; }
    public int AuthenticationCount { get; set; }
    public int SuccessfulAuths { get; set; }
    public int FailedAuths { get; set; }
}

public class DailyStatsDto
{
    public DateTime Date { get; set; }
    public int SessionCount { get; set; }
    public long DataTransferred { get; set; }
    public int UniqueUsers { get; set; }
    public double AverageSessionDuration { get; set; }
    public int ActiveSessions { get; set; }
    public int AuthenticationCount { get; set; }
    public int SuccessfulAuths { get; set; }
    public int FailedAuths { get; set; }
    public double SuccessRate { get; set; }
}