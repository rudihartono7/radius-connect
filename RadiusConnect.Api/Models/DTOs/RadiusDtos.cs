using System.ComponentModel.DataAnnotations;

namespace RadiusConnect.Api.Models.DTOs;

public class RadiusUserDto
{
    public string Username { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastAuth { get; set; }
    public List<RadiusAttributeDto> CheckAttributes { get; set; } = new();
    public List<RadiusAttributeDto> ReplyAttributes { get; set; } = new();
    public List<string> Groups { get; set; } = new();
}

public class CreateRadiusUserRequest
{
    [Required]
    [StringLength(64, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(253, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    public List<RadiusAttributeDto> CheckAttributes { get; set; } = new();
    public List<RadiusAttributeDto> ReplyAttributes { get; set; } = new();
    public List<string> Groups { get; set; } = new();
}

public class UpdateRadiusUserRequest
{
    public string? Password { get; set; }
    public List<RadiusAttributeDto> CheckAttributes { get; set; } = new();
    public List<RadiusAttributeDto> ReplyAttributes { get; set; } = new();
    public List<string> Groups { get; set; } = new();
}

public class RadiusGroupDto
{
    public string GroupName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Priority { get; set; }
    public List<RadiusAttributeDto> CheckAttributes { get; set; } = new();
    public List<RadiusAttributeDto> ReplyAttributes { get; set; } = new();
    public List<string> Users { get; set; } = new();
}

public class CreateRadiusGroupRequest
{
    [Required]
    [StringLength(64, MinimumLength = 3)]
    public string GroupName { get; set; } = string.Empty;

    public string? Description { get; set; }
    public int Priority { get; set; } = 1;
    public List<RadiusAttributeDto> CheckAttributes { get; set; } = new();
    public List<RadiusAttributeDto> ReplyAttributes { get; set; } = new();
}

public class UpdateRadiusGroupRequest
{
    public string? Description { get; set; }
    public int Priority { get; set; }
    public List<RadiusAttributeDto> CheckAttributes { get; set; } = new();
    public List<RadiusAttributeDto> ReplyAttributes { get; set; } = new();
}

public class RadiusAttributeDto
{
    [Required]
    public string Attribute { get; set; } = string.Empty;

    [Required]
    public string Op { get; set; } = string.Empty;

    [Required]
    public string Value { get; set; } = string.Empty;
}

public class RadiusSessionDto
{
    public string AcctSessionId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string NasIpAddress { get; set; } = string.Empty;
    public string NasPortId { get; set; } = string.Empty;
    public string CalledStationId { get; set; } = string.Empty;
    public string CallingStationId { get; set; } = string.Empty;
    public DateTime AcctStartTime { get; set; }
    public DateTime? AcctStopTime { get; set; }
    public long AcctSessionTime { get; set; }
    public long AcctInputOctets { get; set; }
    public long AcctOutputOctets { get; set; }
    public string AcctTerminateCause { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string FramedProtocol { get; set; } = string.Empty;
    public string FramedIpAddress { get; set; } = string.Empty;
    public bool IsActive => AcctStopTime == null;
}

public class RadiusAuthLogDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Pass { get; set; } = string.Empty;
    public string Reply { get; set; } = string.Empty;
    public DateTime AuthDate { get; set; }
}

public class CoARequest
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string NasIpAddress { get; set; } = string.Empty;

    [Required]
    public string Action { get; set; } = string.Empty; // "disconnect" or "coa"

    public List<RadiusAttributeDto> Attributes { get; set; } = new();
}

public class RadiusStatsDto
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
}

public class SessionStatsDto
{
    public int ActiveSessions { get; set; }
    public int TotalSessions { get; set; }
    public long TotalDataTransferred { get; set; }
    public long TotalDataTransfer { get; set; }
    public int UniqueUsers { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public double AverageSessionDuration { get; set; }
    public List<SessionsByHourDto> SessionsByHour { get; set; } = new();
    public List<TopUserDto> TopUsers { get; set; } = new();
}

public class SessionsByHourDto
{
    public int Hour { get; set; }
    public int SessionCount { get; set; }
}

public class TopUserDto
{
    public string Username { get; set; } = string.Empty;
    public int SessionCount { get; set; }
    public long TotalDataTransfer { get; set; }
    public long TotalSessionTime { get; set; }
    public DateTime? LastActivity { get; set; }
}

public class AuthStatsDto
{
    public int TotalAttempts { get; set; }
    public int SuccessfulAttempts { get; set; }
    public int FailedAttempts { get; set; }
    public double SuccessRate { get; set; }
    public List<AuthByHourDto> AuthByHour { get; set; } = new();
    public List<TopFailedUserDto> TopFailedUsers { get; set; } = new();
}

public class AuthByHourDto
{
    public int Hour { get; set; }
    public int SuccessfulAttempts { get; set; }
    public int FailedAttempts { get; set; }
}

public class TopFailedUserDto
{
    public string Username { get; set; } = string.Empty;
    public int FailedAttempts { get; set; }
}