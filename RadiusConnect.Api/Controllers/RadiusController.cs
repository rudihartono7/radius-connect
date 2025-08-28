using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RadiusConnect.Api.Models.DTOs;
using RadiusConnect.Api.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace RadiusConnect.Api.Controllers;

[Route("api/[controller]")]
public class RadiusController : BaseController
{
    private readonly IRadiusService _radiusService;
    private readonly IAuditService _auditService;

    public RadiusController(
        IRadiusService radiusService,
        IAuditService auditService)
    {
        _radiusService = radiusService;
        _auditService = auditService;
    }

    #region RADIUS Users

    [HttpGet("users")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetRadiusUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
    {
        try
        {
            if(string.IsNullOrEmpty(search) || search == "undefined")
            {
                search = "";
            }

            var usersResult = await _radiusService.GetRadiusUsersAsync(page, pageSize, search);
            var totalCount = await _radiusService.GetRadiusUsersCountAsync();
            
            // Extract the Users property from the result
            var usersData = ((dynamic)usersResult).Users;
            var users = ((IEnumerable<object>)usersData).ToList();

            return PagedResult<object>(users, totalCount, page, pageSize, "RADIUS users retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving RADIUS users");
        }
    }

    [HttpGet("users/{username}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetRadiusUser(string username)
    {
        try
        {
            var user = await _radiusService.GetRadiusUserAsync(username);
            if (user == null)
            {
                return NotFound("RADIUS user not found");
            }

            return Success(user, "RADIUS user retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving RADIUS user");
        }
    }

    [HttpPost("users")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateRadiusUser([FromBody] CreateRadiusUserRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationError("Invalid RADIUS user data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            }

            // Check if user already exists
            if (await _radiusService.RadiusUserExistsAsync(request.Username))
            {
                return Conflict("RADIUS user already exists");
            }

            // Create user
            var user = await _radiusService.CreateRadiusUserAsync(request);
            
            // Log RADIUS user creation
            var currentUserId = GetCurrentUserId();
            await _auditService.LogRadiusUserActionAsync(Guid.Parse(currentUserId), "RADIUS_USER_CREATED", request.Username);

            return Success(user, "RADIUS user created successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "creating RADIUS user");
        }
    }

    [HttpPut("users/{username}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateRadiusUser(string username, [FromBody] UpdateRadiusUserRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationError("Invalid RADIUS user data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            }

            if (!await _radiusService.RadiusUserExistsAsync(username))
            {
                return NotFound("RADIUS user not found");
            }

            // Update password if provided
            if (!string.IsNullOrEmpty(request.Password))
            {
                await _radiusService.UpdateRadiusUserPasswordAsync(username, request.Password);
            }
            
            // Update attributes if provided
            var user = await _radiusService.UpdateRadiusUserAsync(username, request.Attributes, null);
            
            // Log RADIUS user update
            var currentUserId = GetCurrentUserId();
            await _auditService.LogRadiusUserActionAsync(Guid.Parse(currentUserId), "RADIUS_USER_UPDATED", username);

            return Success(user, "RADIUS user updated successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "updating RADIUS user");
        }
    }

    [HttpDelete("users/{username}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteRadiusUser(string username)
    {
        try
        {
            if (!await _radiusService.RadiusUserExistsAsync(username))
            {
                return NotFound("RADIUS user not found");
            }

            await _radiusService.DeleteRadiusUserAsync(username);
            
            // Log RADIUS user deletion
            var currentUserId = GetCurrentUserId();
            await _auditService.LogRadiusUserActionAsync(Guid.Parse(currentUserId), "RADIUS_USER_DELETED", username);

            return Success("RADIUS user deleted successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "deleting RADIUS user");
        }
    }

    [HttpGet("users/{username}/attributes")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetRadiusUserAttributes(string username)
    {
        try
        {
            if (!await _radiusService.RadiusUserExistsAsync(username))
            {
                return NotFound("RADIUS user not found");
            }

            var attributes = await _radiusService.GetRadiusUserAttributesAsync(username);
            return Success(attributes, "RADIUS user attributes retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving RADIUS user attributes");
        }
    }

    [HttpPost("users/{username}/attributes")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> AddRadiusUserAttribute(string username, [FromBody] AddAttributeRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationError("Invalid attribute data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            }

            if (!await _radiusService.RadiusUserExistsAsync(username))
            {
                return NotFound("RADIUS user not found");
            }

            await _radiusService.AddRadiusUserAttributeAsync(username, request.Attribute, request.Op, request.Value);
            
            // Log attribute addition
            var currentUserId = GetCurrentUserId();
            await _auditService.LogRadiusUserActionAsync(Guid.Parse(currentUserId), "RADIUS_USER_ATTRIBUTE_ADDED", username);

            return Success("RADIUS user attribute added successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "adding RADIUS user attribute");
        }
    }

    [HttpDelete("users/{username}/attributes/{attributeId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> RemoveRadiusUserAttribute(string username, int attributeId)
    {
        try
        {
            if (!await _radiusService.RadiusUserExistsAsync(username))
            {
                return NotFound("RADIUS user not found");
            }

            await _radiusService.RemoveRadiusUserAttributeAsync(username, attributeId);
            
            // Log attribute removal
            var currentUserId = GetCurrentUserId();
            await _auditService.LogRadiusUserActionAsync(Guid.Parse(currentUserId), "RADIUS_USER_ATTRIBUTE_REMOVED", username);

            return Success("RADIUS user attribute removed successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "removing RADIUS user attribute");
        }
    }

    #endregion

    #region RADIUS Groups

    [HttpGet("groups")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetRadiusGroups([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var groupsResult = await _radiusService.GetRadiusGroupsAsync(page, pageSize);
            var totalCount = await _radiusService.GetRadiusGroupsCountAsync();
            
            // Extract the Groups property from the result
            var groupsData = ((dynamic)groupsResult).Groups;
            var groups = ((IEnumerable<object>)groupsData).ToList();

            return PagedResult<object>(groups, totalCount, page, pageSize, "RADIUS groups retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving RADIUS groups");
        }
    }

    [HttpGet("groups/{groupName}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetRadiusGroup(string groupName)
    {
        try
        {
            var group = await _radiusService.GetRadiusGroupAsync(groupName);
            if (group == null)
            {
                return NotFound("RADIUS group not found");
            }

            return Success(group, "RADIUS group retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving RADIUS group");
        }
    }

    [HttpPost("groups")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateRadiusGroup([FromBody] CreateRadiusGroupRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationError("Invalid RADIUS group data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            }

            // Check if group already exists
            if (await _radiusService.GroupExistsAsync(request.GroupName))
            {
                return Conflict("RADIUS group already exists");
            }

            var group = await _radiusService.CreateRadiusGroupAsync(request.GroupName, request.CheckAttributes, request.ReplyAttributes);
            
            // Log RADIUS group creation
            var currentUserId = GetCurrentUserId();
            await _auditService.LogRadiusGroupActionAsync(Guid.Parse(currentUserId), "RadiusGroup", "Create", request.GroupName, $"RADIUS group {request.GroupName} created by {GetCurrentUsername()}");

            return Success(group, "RADIUS group created successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "creating RADIUS group");
        }
    }

    [HttpDelete("groups/{groupName}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteRadiusGroup(string groupName)
    {
        try
        {
            if (!await _radiusService.GroupExistsAsync(groupName))
            {
                return NotFound("RADIUS group not found");
            }

            await _radiusService.DeleteRadiusGroupAsync(groupName);
            
            // Log RADIUS group deletion
            var currentUserId = GetCurrentUserId();
            await _auditService.LogRadiusGroupActionAsync(Guid.Parse(currentUserId), "RadiusGroup", "Delete", groupName, $"RADIUS group {groupName} deleted by {GetCurrentUsername()}");

            return Success("RADIUS group deleted successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "deleting RADIUS group");
        }
    }

    [HttpPost("groups/{groupName}/users/{username}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> AddUserToGroup(string groupName, string username, [FromBody] AddUserToGroupRequest request)
    {
        try
        {
            if (!await _radiusService.RadiusUserExistsAsync(username))
            {
                return NotFound("RADIUS user not found");
            }

            if (!await _radiusService.GroupExistsAsync(groupName))
            {
                return NotFound("RADIUS group not found");
            }

            await _radiusService.AddUserToGroupAsync(username, groupName, request.Priority);
            
            // Log user-group assignment
            var currentUserId = GetCurrentUserId();
            await _auditService.LogUserActionAsync(Guid.Parse(currentUserId), "RADIUS_USER_GROUP_ADDED", "RadiusUserGroup", $"{username}:{groupName}", null, new { Username = username, GroupName = groupName, Priority = request.Priority });

            return Success("User added to RADIUS group successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "adding user to RADIUS group");
        }
    }

    [HttpDelete("groups/{groupName}/users/{username}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> RemoveUserFromGroup(string groupName, string username)
    {
        try
        {
            await _radiusService.RemoveUserFromGroupAsync(username, groupName);
            
            // Log user-group removal
            var currentUserId = GetCurrentUserId();
            await _auditService.LogUserActionAsync(Guid.Parse(currentUserId), "RADIUS_USER_GROUP_REMOVED", "RadiusUserGroup", $"{username}:{groupName}", new { Username = username, GroupName = groupName }, null);

            return Success("User removed from RADIUS group successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "removing user from RADIUS group");
        }
    }

    [HttpGet("groups/{groupName}/users")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetGroupUsers(string groupName)
    {
        try
        {
            if (!await _radiusService.GroupExistsAsync(groupName))
            {
                return NotFound("RADIUS group not found");
            }

            var users = await _radiusService.GetUsersInGroupAsync(groupName);
            return Success(users, "Group users retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving group users");
        }
    }

    [HttpGet("users/{username}/groups")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetUserGroups(string username)
    {
        try
        {
            if (!await _radiusService.RadiusUserExistsAsync(username))
            {
                return NotFound("RADIUS user not found");
            }

            var groups = await _radiusService.GetUserGroupsAsync(username);
            return Success(groups, "User groups retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving user groups");
        }
    }

    #endregion

    #region Sessions

    [HttpGet("sessions")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetActiveSessions([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var sessions = await _radiusService.GetActiveSessionsAsync();
            var totalCount = await _radiusService.GetActiveSessionsCountAsync();

            return PagedResult(sessions, totalCount, page, pageSize, "Active sessions retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving active sessions");
        }
    }

    [HttpGet("sessions/{sessionId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetSession(string sessionId)
    {
        try
        {
            var session = await _radiusService.GetSessionByIdAsync(sessionId);
            if (session == null)
            {
                return NotFound("Session not found");
            }

            return Success(session, "Session retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving session");
        }
    }

    [HttpPost("sessions/{sessionId}/disconnect")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DisconnectSession(string sessionId, [FromBody] DisconnectSessionRequest request)
    {
        try
        {
            var session = await _radiusService.GetSessionByIdAsync(sessionId);
            if (session == null)
            {
                return NotFound("Session not found");
            }

            await _radiusService.DisconnectUserSessionAsync(session.Username, sessionId);
            
            // Log session disconnection
            var currentUserId = GetCurrentUserIdAsGuid();
            await _auditService.LogUserActionAsync(currentUserId, "RADIUS_SESSION_DISCONNECTED", "RadiusSession", sessionId, null, new { Message = $"Session {sessionId} disconnected by {GetCurrentUsername()}: {request.Reason}" });

            return Success("Session disconnected successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "disconnecting session");
        }
    }

    [HttpGet("sessions/user/{username}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetUserSessions(string username, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var sessions = await _radiusService.GetUserSessionsAsync(username, page, pageSize);
            var totalCount = await _radiusService.GetTotalSessionsCountAsync();

            return PagedResult(sessions, totalCount, page, pageSize, "User sessions retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving user sessions");
        }
    }

    #endregion

    #region Authentication Logs

    [HttpGet("auth-logs")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetAuthenticationLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? username = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var logs = username != null 
                ? await _radiusService.GetUserAuthenticationLogsAsync(username, page, pageSize)
                : await _radiusService.GetAuthenticationLogsAsync(page, pageSize);
            var totalCount = await _radiusService.GetAuthenticationLogsCountAsync();

            return PagedResult(logs, totalCount, page, pageSize, "Authentication logs retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving authentication logs");
        }
    }

    #endregion

    #region Statistics

    [HttpGet("stats/overview")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetRadiusOverviewStats()
    {
        try
        {
            var stats = await _radiusService.GetRadiusStatisticsAsync();
            return Success(stats, "RADIUS overview statistics retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving RADIUS overview statistics");
        }
    }

    [HttpGet("stats/sessions")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetSessionStats([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var stats = await _radiusService.GetSessionStatsByNasAsync();
            return Success(stats, "Session statistics retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving session statistics");
        }
    }

    [HttpGet("stats/authentication")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetAuthenticationStats([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var stats = await _radiusService.GetAuthenticationStatsByResultAsync(startDate, endDate);
            return Success(stats, "Authentication statistics retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving authentication statistics");
        }
    }

    #endregion
}

// DTOs

public class UpdateRadiusUserRequest
{
    public string? Password { get; set; }
    public Dictionary<string, string>? Attributes { get; set; }
}

public class CreateRadiusGroupRequest
{
    [Required]
    public string GroupName { get; set; } = string.Empty;

    public Dictionary<string, string>? CheckAttributes { get; set; }
    public Dictionary<string, string>? ReplyAttributes { get; set; }
}

public class AddAttributeRequest
{
    [Required]
    public string Attribute { get; set; } = string.Empty;

    [Required]
    public string Op { get; set; } = string.Empty;

    [Required]
    public string Value { get; set; } = string.Empty;
}

public class AddUserToGroupRequest
{
    public int Priority { get; set; } = 1;
}

public class DisconnectSessionRequest
{
    public string Reason { get; set; } = "Administrative disconnect";
}