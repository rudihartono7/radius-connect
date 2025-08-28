using RadiusConnect.Api.Models.Domain;
using RadiusConnect.Api.Models.DTOs;

namespace RadiusConnect.Api.Services.Interfaces;

public interface IRadiusService
{
    // User management
    Task<bool> CreateRadiusUserAsync(CreateRadiusUserRequest request);
    Task<bool> UpdateRadiusUserAsync(string username, Dictionary<string, string>? checkAttributes = null, Dictionary<string, string>? replyAttributes = null);
    Task<bool> UpdateRadiusUserPasswordAsync(string username, string newPassword);
    Task<bool> DeleteRadiusUserAsync(string username);
    Task<bool> RadiusUserExistsAsync(string username);
    Task<IEnumerable<string>> GetAllRadiusUsersAsync();
    Task<object> GetRadiusUsersAsync(int page = 1, int pageSize = 10, string? search = null);
    Task<int> GetRadiusUsersCountAsync();
    Task<object> GetRadiusUserAsync(string username);
    Task<object> GetRadiusUserAttributesAsync(string username);

    // User attributes
    Task<IEnumerable<RadCheck>> GetUserCheckAttributesAsync(string username);
    Task<IEnumerable<RadReply>> GetUserReplyAttributesAsync(string username);
    Task<bool> SetUserCheckAttributeAsync(string username, string attribute, string op, string value);
    Task<bool> SetUserReplyAttributeAsync(string username, string attribute, string op, string value);
    Task<bool> RemoveUserCheckAttributeAsync(string username, string attribute);
    Task<bool> RemoveUserReplyAttributeAsync(string username, string attribute);
    Task<bool> ClearUserAttributesAsync(string username);
    Task<bool> AddRadiusUserAttributeAsync(string username, string attribute, string op, string value);
    Task<bool> RemoveRadiusUserAttributeAsync(string username, int attributeId);

    // Group management
    Task<bool> CreateGroupAsync(string groupName, Dictionary<string, string>? checkAttributes = null, Dictionary<string, string>? replyAttributes = null);
    Task<bool> CreateRadiusGroupAsync(string groupName, Dictionary<string, string>? checkAttributes = null, Dictionary<string, string>? replyAttributes = null);
    Task<bool> DeleteGroupAsync(string groupName);
    Task<bool> DeleteRadiusGroupAsync(string groupName);
    Task<int> GetRadiusGroupsCountAsync();
    Task<object?> GetRadiusGroupAsync(string groupName);
    Task<object> GetRadiusGroupsAsync(int page = 1, int pageSize = 10);
    Task<bool> GroupExistsAsync(string groupName);
    Task<IEnumerable<string>> GetAllGroupsAsync();

    // Group attributes
    Task<IEnumerable<RadGroupCheck>> GetGroupCheckAttributesAsync(string groupName);
    Task<IEnumerable<RadGroupReply>> GetGroupReplyAttributesAsync(string groupName);
    Task<bool> SetGroupCheckAttributeAsync(string groupName, string attribute, string op, string value);
    Task<bool> SetGroupReplyAttributeAsync(string groupName, string attribute, string op, string value);
    Task<bool> RemoveGroupCheckAttributeAsync(string groupName, string attribute);
    Task<bool> RemoveGroupReplyAttributeAsync(string groupName, string attribute);
    Task<bool> ClearGroupAttributesAsync(string groupName);

    // User-Group relationships
    Task<bool> AddUserToGroupAsync(string username, string groupName, int priority = 1);
    Task<bool> RemoveUserFromGroupAsync(string username, string groupName);
    Task<bool> RemoveUserFromAllGroupsAsync(string username);
    Task<IEnumerable<RadUserGroup>> GetUserGroupsAsync(string username);
    Task<IEnumerable<string>> GetUsersInGroupAsync(string groupName);

    // Session management
    Task<IEnumerable<RadAcct>> GetActiveSessionsAsync();
    Task<IEnumerable<RadAcct>> GetUserSessionsAsync(string username, int page = 1, int pageSize = 10);
    Task<RadAcct?> GetSessionByIdAsync(string acctSessionId);
    Task<IEnumerable<RadAcct>> GetSessionsByNasAsync(string nasIpAddress);
    Task<int> GetActiveSessionsCountAsync();
    Task<int> GetTotalSessionsCountAsync();

    // Authentication logs
    Task<IEnumerable<RadPostAuth>> GetAuthenticationLogsAsync(int page = 1, int pageSize = 10);
    Task<IEnumerable<RadPostAuth>> GetUserAuthenticationLogsAsync(string username, int page = 1, int pageSize = 10);
    Task<int> GetAuthenticationLogsCountAsync();

    // Policy templates
    Task<bool> ApplyPolicyTemplateToUserAsync(string username, long templateId);
    Task<bool> ApplyPolicyTemplateToGroupAsync(string groupName, long templateId);
    Task<PolicyTemplate?> GetPolicyTemplateAsync(long templateId);
    Task<IEnumerable<PolicyTemplate>> GetAllPolicyTemplatesAsync();

    // Change of Authorization (CoA)
    Task<bool> DisconnectUserSessionAsync(string username, string? acctSessionId = null);
    Task<bool> SendCoaRequestAsync(string username, string acctSessionId, Dictionary<string, string> attributes);
    Task<IEnumerable<CoaRequest>> GetCoaRequestsAsync(int page = 1, int pageSize = 10);
    Task<CoaRequest?> GetCoaRequestAsync(long requestId);

    // Statistics and reporting
    Task<Dictionary<string, object>> GetRadiusStatisticsAsync();
    Task<Dictionary<string, int>> GetSessionStatsByNasAsync();
    Task<Dictionary<string, int>> GetAuthenticationStatsByResultAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<object>> GetTopUsersAsync(int count = 10);
}