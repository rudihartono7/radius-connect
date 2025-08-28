using RadiusConnect.Api.Models.Domain;

namespace RadiusConnect.Api.Repositories.Interfaces;

public interface IRadiusRepository
{
    // RadCheck operations
    Task<IEnumerable<RadCheck>> GetUserCheckAttributesAsync(string username);
    Task<RadCheck?> GetUserCheckAttributeAsync(string username, string attribute);
    Task AddUserCheckAttributeAsync(RadCheck radCheck);
    Task UpdateUserCheckAttributeAsync(RadCheck radCheck);
    Task DeleteUserCheckAttributeAsync(string username, string attribute);
    Task DeleteAllUserCheckAttributesAsync(string username);

    // RadReply operations
    Task<IEnumerable<RadReply>> GetUserReplyAttributesAsync(string username);
    Task<RadReply?> GetUserReplyAttributeAsync(string username, string attribute);
    Task AddUserReplyAttributeAsync(RadReply radReply);
    Task UpdateUserReplyAttributeAsync(RadReply radReply);
    Task DeleteUserReplyAttributeAsync(string username, string attribute);
    Task DeleteAllUserReplyAttributesAsync(string username);

    // RadUserGroup operations
    Task<IEnumerable<RadUserGroup>> GetUserGroupsAsync(string username);
    Task<IEnumerable<string>> GetUsersInGroupAsync(string groupName);
    Task AddUserToGroupAsync(RadUserGroup radUserGroup);
    Task RemoveUserFromGroupAsync(string username, string groupName);
    Task RemoveUserFromAllGroupsAsync(string username);

    // RadGroupCheck operations
    Task<IEnumerable<RadGroupCheck>> GetGroupCheckAttributesAsync(string groupName);
    Task<RadGroupCheck?> GetGroupCheckAttributeAsync(string groupName, string attribute);
    Task AddGroupCheckAttributeAsync(RadGroupCheck radGroupCheck);
    Task UpdateGroupCheckAttributeAsync(RadGroupCheck radGroupCheck);
    Task DeleteGroupCheckAttributeAsync(string groupName, string attribute);
    Task DeleteAllGroupCheckAttributesAsync(string groupName);

    // RadGroupReply operations
    Task<IEnumerable<RadGroupReply>> GetGroupReplyAttributesAsync(string groupName);
    Task<RadGroupReply?> GetGroupReplyAttributeAsync(string groupName, string attribute);
    Task AddGroupReplyAttributeAsync(RadGroupReply radGroupReply);
    Task UpdateGroupReplyAttributeAsync(RadGroupReply radGroupReply);
    Task DeleteGroupReplyAttributeAsync(string groupName, string attribute);
    Task DeleteAllGroupReplyAttributesAsync(string groupName);

    // RadAcct operations
    Task<IEnumerable<RadAcct>> GetActiveSessionsAsync();
    Task<IEnumerable<RadAcct>> GetUserSessionsAsync(string username, int page = 1, int pageSize = 10);
    Task<RadAcct?> GetSessionByIdAsync(string acctSessionId);
    Task<IEnumerable<RadAcct>> GetSessionsByNasAsync(string nasIpAddress);
    Task<int> GetActiveSessionsCountAsync();
    Task<int> GetTotalSessionsCountAsync(DateTime? startDate = null, DateTime? endDate = null);

    // RadPostAuth operations
    Task<IEnumerable<RadPostAuth>> GetAuthenticationLogsAsync(int page = 1, int pageSize = 10);
    Task<IEnumerable<RadPostAuth>> GetUserAuthenticationLogsAsync(string username, int page = 1, int pageSize = 10);
    Task<int> GetAuthenticationLogsCountAsync();
    Task<int> GetSuccessfulAuthenticationsCountAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<int> GetFailedAuthenticationsCountAsync(DateTime? startDate = null, DateTime? endDate = null);

    // Group management
    Task<IEnumerable<string>> GetAllGroupsAsync();
    Task<bool> GroupExistsAsync(string groupName);
    Task DeleteGroupAsync(string groupName);

    // User management
    Task<bool> RadiusUserExistsAsync(string username);
    Task DeleteRadiusUserAsync(string username);
    Task<IEnumerable<string>> GetAllRadiusUsersAsync();
    Task<IEnumerable<string>> GetAllUserGroupsAsync();
}