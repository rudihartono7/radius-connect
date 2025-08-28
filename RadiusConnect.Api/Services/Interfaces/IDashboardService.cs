using RadiusConnect.Api.Models.DTOs;

namespace RadiusConnect.Api.Services.Interfaces;

public interface IDashboardService
{
    Task<OverviewStatistics> GetOverviewStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<UserStatistics> GetUserStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<SessionStatistics> GetSessionStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<AuthenticationStatistics> GetAuthenticationStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<NetworkStatistics> GetNetworkStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<RealTimeData> GetRealTimeDataAsync();
    Task<IEnumerable<RecentActivity>> GetRecentActivitiesAsync(int limit = 20);
    Task<UsageReport> GenerateUsageReportAsync(DateTime startDate, DateTime endDate);
    Task<PerformanceReport> GeneratePerformanceReportAsync(DateTime startDate, DateTime endDate);
    Task<AuditStatistics> GetAuditStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<object> GetSessionSummaryReportAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<object> GetAuthenticationSummaryReportAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<object> GetUserActivityReportAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<object> GetAuditStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<object> GetNasUsageStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<object> GetRealTimeSystemAlertsAsync();
    Task<object> GetRealTimeSystemAlertsAsync(int limit);
    Task<object> GetRealTimeActiveSessionsAsync();
    Task<object> GetSystemHealthStatsAsync();
    Task<object> GetSessionStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<object> GetHourlySessionStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<object> GetHourlyAuthStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<object> GetDailyAuthStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<object> GetUserStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<object> GetUserActivityStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<object> GetAuthenticationStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<object> GetTopActiveUsersAsync(int limit = 10, DateTime? startDate = null, DateTime? endDate = null);
    Task<object> GetGroupStatsAsync();
    Task<object> GetRealTimeRecentAuthenticationsAsync(int limit = 20);
    Task<object> GetNetworkStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<object> GetBandwidthStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<object> GetGroupDistributionStatsAsync();
    Task<object> GetDailySessionStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
}