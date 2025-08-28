using System.ComponentModel.DataAnnotations;

namespace RadiusConnect.Api.Models.DTOs;

public class AuditLogDto
{
    public int Id { get; set; }
    public string Actor { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? Details { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string? AdditionalData { get; set; }
}

public class AuditLogSearchRequest
{
    public string? Actor { get; set; }
    public string? Action { get; set; }
    public string? Entity { get; set; }
    public string? EntityId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Severity { get; set; }
    public string? SearchQuery { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string SortBy { get; set; } = "Timestamp";
    public string SortOrder { get; set; } = "desc";
}

public class AuditStatsDto
{
    public int TotalLogs { get; set; }
    public int LogsToday { get; set; }
    public int LogsThisWeek { get; set; }
    public int LogsThisMonth { get; set; }
    public List<AuditStatsByActionDto> LogsByAction { get; set; } = new();
    public List<AuditStatsByEntityDto> LogsByEntity { get; set; } = new();
    public List<AuditStatsByDateDto> LogsByDate { get; set; } = new();
    public List<MostActiveUserDto> MostActiveUsers { get; set; } = new();
}

public class AuditStatsByActionDto
{
    public string Action { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class AuditStatsByEntityDto
{
    public string Entity { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class AuditStatsByDateDto
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}

public class MostActiveUserDto
{
    public string Username { get; set; } = string.Empty;
    public int ActionCount { get; set; }
    public DateTime LastActivity { get; set; }
}

public class AuditCleanupRequest
{
    [Required]
    public DateTime OlderThan { get; set; }
    
    public string? Severity { get; set; }
    public bool DryRun { get; set; } = true;
}

public class AuditCleanupResult
{
    public int RecordsDeleted { get; set; }
    public DateTime CleanupDate { get; set; }
    public bool WasDryRun { get; set; }
    public string Summary { get; set; } = string.Empty;
}

public class AuditArchiveRequest
{
    [Required]
    public DateTime OlderThan { get; set; }
    
    [Required]
    public string ArchivePath { get; set; } = string.Empty;
    
    public string Format { get; set; } = "json"; // "json", "csv", "xml"
    public bool DeleteAfterArchive { get; set; } = false;
}

public class AuditArchiveResult
{
    public int RecordsArchived { get; set; }
    public string ArchiveFilePath { get; set; } = string.Empty;
    public long ArchiveFileSize { get; set; }
    public DateTime ArchiveDate { get; set; }
    public bool DeletedAfterArchive { get; set; }
    public string Summary { get; set; } = string.Empty;
}

public class AuditExportRequest
{
    public string? Actor { get; set; }
    public string? Action { get; set; }
    public string? Entity { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Format { get; set; } = "csv"; // "csv", "json", "xml"
    public bool IncludeDetails { get; set; } = true;
}

public class AuditReportDto
{
    public string ReportId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalRecords { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string Summary { get; set; } = string.Empty;
}

public class SecurityAlertDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Actor { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedBy { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string AlertType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class AnomalyDetectionDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public DateTime DetectedAt { get; set; }
    public string AffectedEntity { get; set; } = string.Empty;
    public Dictionary<string, object> Details { get; set; } = new();
    public List<string> RecommendedActions { get; set; } = new();
}