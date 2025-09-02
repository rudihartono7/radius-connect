using RadiusConnect.Api.Models.Domain;
using RadiusConnect.Api.Repositories.Interfaces;
using RadiusConnect.Api.Services.Interfaces;
using RadiusConnect.Api.Data;
using Microsoft.EntityFrameworkCore;
using RadiusConnect.Api.Models.DTOs;

namespace RadiusConnect.Api.Services;

public class RadiusService : IRadiusService
{
    private readonly IRadiusRepository _radiusRepository;
    private readonly IAuditService _auditService;
    private readonly ILogger<RadiusService> _logger;
    private readonly RadiusDbContext _radiusContext;

    public RadiusService(
        IRadiusRepository radiusRepository,
        IAuditService auditService,
        ILogger<RadiusService> logger,
        RadiusDbContext radiusContext)
    {
        _radiusRepository = radiusRepository;
        _auditService = auditService;
        _logger = logger;
        _radiusContext = radiusContext;
    }

    // User management
    public async Task<bool> CreateRadiusUserAsync(CreateRadiusUserRequest request)
    {
        _logger.LogInformation("Creating RADIUS user: {Username}", request.Username);

        foreach(var item in request.CheckAttributes){
            await _radiusRepository.AddUserCheckAttributeAsync(new RadCheck
            {
                Username = request.Username,
                Attribute = item.Attribute,
                Op = item.Op,
                Value = item.Value
            });
        }

        foreach(var item in request.ReplyAttributes){
            await _radiusRepository.AddUserReplyAttributeAsync(new RadReply
            {
                Username = request.Username,
                Attribute = item.Attribute,
                Op = item.Op,
                Value = item.Value
            });
        }

        return true;
    }

    public async Task<bool> UpdateRadiusUserPasswordAsync(string username, string newPassword)
    {
        await _radiusRepository.UpdateUserCheckAttributeAsync(new RadCheck
        {
            Username = username,
            Attribute = "Cleartext-Password",
            Op = ":=",
            Value = newPassword
        });

        return await Task.FromResult(true);
    }

    public async Task<bool> DeleteRadiusUserAsync(string username)
    {
        _logger.LogInformation("Deleting RADIUS user: {Username}", username);

        await _radiusRepository.DeleteUserCheckAttributeAsync(username, "Cleartext-Password");
        return await Task.FromResult(true);
    }

    public async Task<bool> RadiusUserExistsAsync(string username)
    {
        _logger.LogDebug("Checking if RADIUS user exists: {Username}", username);

        var result = await _radiusRepository.GetUserCheckAttributesAsync(username);
        return result.Any();
    }
    
    public async Task<bool> UpdateRadiusUserAsync(string username, RadiusAttributeDto[]? checkAttributes = null, RadiusAttributeDto[]? replyAttributes = null)
    {
        _logger.LogInformation("Updating RADIUS user: {Username}", username);

        //Delete existing attributes from radcheck and radreply when user exists
        var userExists = await RadiusUserExistsAsync(username);
        if (userExists)
        {
            if (checkAttributes != null)
            {
                foreach (var attr in checkAttributes)
                {
                    await RemoveUserCheckAttributeAsync(username, attr.Attribute);
                }
            }

            if (replyAttributes != null)
            {
                foreach (var attr in replyAttributes)
                {
                    await RemoveUserReplyAttributeAsync(username, attr.Attribute);
                }
            }
        }

        if (checkAttributes != null)
        {
            foreach (var attr in checkAttributes)
            {
                await _radiusRepository.AddUserCheckAttributeAsync(new RadCheck
                {
                    Username = username,
                    Attribute = attr.Attribute,
                    Op = attr.Op,
                    Value = attr.Value
                });
            }
        }

        if (replyAttributes != null)
        {
            foreach (var attr in replyAttributes)
            {
                await _radiusRepository.AddUserReplyAttributeAsync(new RadReply
                {
                    Username = username,
                    Attribute = attr.Attribute,
                    Op = attr.Op,
                    Value = attr.Value
                });
            }
        }

        
        return await Task.FromResult(true);
    }

    public async Task<IEnumerable<string>> GetAllRadiusUsersAsync()
    {
        _logger.LogDebug("Getting all RADIUS users");

        var result = await _radiusRepository.GetAllRadiusUsersAsync();
        return result;
    }

    public async Task<object> GetRadiusUsersAsync(int page = 1, int pageSize = 10, string? search = null)
    {
        _logger.LogDebug("Getting radius users, page {Page}, size {PageSize}, search {Search}", page, pageSize, search);
        
        // Get distinct usernames from radcheck table with optional search filter
        var query = _radiusContext.RadCheck
            .Select(rc => rc.Username)
            .Distinct();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => u.Contains(search));
        }

        var totalCount = await query.CountAsync();
        
        var distinctUsernames = await query
            .OrderBy(u => u)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Get user details with groups and last authentication
        var users = new List<object>();
        foreach (var username in distinctUsernames)
        {
            var radiusUser = await _radiusRepository.GetUserCheckAttributesAsync(username);
            if (!radiusUser.Any())
            {
                continue; // Skip users without check attributes
            }

            var radiusUserReply = await _radiusRepository.GetUserReplyAttributesAsync(username);

            if( radiusUserReply == null )
            {
                radiusUserReply = new List<RadReply>();
            }

            var groups = await _radiusContext.RadUserGroup
                .Where(rug => rug.Username == username)
                .Select(rug => rug.GroupName)
                .ToListAsync();

            var lastAuth = await _radiusContext.RadPostAuth
                .Where(rpa => rpa.Username == username)
                .OrderByDescending(rpa => rpa.AuthDate)
                .FirstOrDefaultAsync();

            var isActive = await _radiusContext.RadCheck
                .AnyAsync(rc => rc.Username == username && rc.Attribute == "Auth-Type" && rc.Value != "Reject");

            users.Add(new RadiusUserDto
            {
                Username = username,
                Groups = groups,
                IsActive = isActive,
                LastAuth = lastAuth?.AuthDate,
                CheckAttributes = radiusUser.Select(rc => new RadiusAttributeDto
                {
                    Attribute = rc.Attribute,
                    Op = rc.Op,
                    Value = rc.Value
                }).ToList(),

                ReplyAttributes = radiusUserReply.Select(rc => new RadiusAttributeDto
                {
                    Attribute = rc.Attribute,
                    Op = rc.Op,
                    Value = rc.Value
                }).ToList()
            });
        }
        
        return new
        {
            Users = users,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    public async Task<int> GetRadiusUsersCountAsync()
    {
        _logger.LogDebug("Getting radius users count");
        
        return await _radiusContext.RadCheck
            .Select(rc => rc.Username)
            .Distinct()
            .CountAsync();
    }

    public async Task<object> GetRadiusUserAsync(string username)
    {
        _logger.LogDebug("Getting radius user: {Username}", username);
        
        // Check if user exists
        var userExists = await _radiusContext.RadCheck
            .AnyAsync(rc => rc.Username == username);

        if (!userExists)
        {
            return null;
        }

        // Get user's check attributes
        var checkAttributes = await _radiusContext.RadCheck
            .Where(rc => rc.Username == username)
            .Select(rc => new { rc.Attribute, rc.Op, rc.Value })
            .ToListAsync();

        // Get user's reply attributes
        var replyAttributes = await _radiusContext.RadReply
            .Where(rr => rr.Username == username)
            .Select(rr => new { rr.Attribute, rr.Op, rr.Value })
            .ToListAsync();

        // Get user's groups
        var groups = await _radiusContext.RadUserGroup
            .Where(rug => rug.Username == username)
            .OrderBy(rug => rug.Priority)
            .Select(rug => new { rug.GroupName, rug.Priority })
            .ToListAsync();

        // Get last authentication
        var lastAuth = await _radiusContext.RadPostAuth
            .Where(rpa => rpa.Username == username)
            .OrderByDescending(rpa => rpa.AuthDate)
            .FirstOrDefaultAsync();

        // Determine if user is active (not explicitly rejected)
        var isActive = !checkAttributes.Any(ca => ca.Attribute == "Auth-Type" && ca.Value == "Reject");

        return new
        {
            Username = username,
            IsActive = isActive,
            Groups = groups,
            CheckAttributes = checkAttributes,
            ReplyAttributes = replyAttributes,
            LastLogin = lastAuth?.AuthDate,
            LastAuthResult = lastAuth?.Reply
        };
    }

    public async Task<object> GetRadiusUserAttributesAsync(string username)
    {
        _logger.LogDebug("Getting RADIUS user attributes for: {Username}", username);
        
        var checkAttributes = await _radiusContext.RadCheck
            .Where(rc => rc.Username == username)
            .Select(rc => new { rc.Attribute, rc.Op, rc.Value, Type = "Check" })
            .ToListAsync();

        var replyAttributes = await _radiusContext.RadReply
            .Where(rr => rr.Username == username)
            .Select(rr => new { rr.Attribute, rr.Op, rr.Value, Type = "Reply" })
            .ToListAsync();

        return checkAttributes.Cast<object>().Concat(replyAttributes.Cast<object>());
    }

    // User attributes
    public async Task<IEnumerable<RadCheck>> GetUserCheckAttributesAsync(string username)
    {
        _logger.LogDebug("Getting check attributes for user: {Username}", username);
        
        return await _radiusContext.RadCheck
            .Where(rc => rc.Username == username)
            .ToListAsync();
    }

    public async Task<IEnumerable<RadReply>> GetUserReplyAttributesAsync(string username)
    {
        _logger.LogDebug("Getting reply attributes for user: {Username}", username);
        
        return await _radiusContext.RadReply
            .Where(rr => rr.Username == username)
            .ToListAsync();
    }

    public async Task<bool> SetUserCheckAttributeAsync(string username, string attribute, string op, string value)
    {
        _logger.LogInformation("Setting check attribute for user {Username}: {Attribute}={Value}", username, attribute, value);
        
        try
        {
            // Check if attribute already exists
            var existingAttribute = await _radiusContext.RadCheck
                .FirstOrDefaultAsync(rc => rc.Username == username && rc.Attribute == attribute);

            if (existingAttribute != null)
            {
                // Update existing attribute
                existingAttribute.Op = op;
                existingAttribute.Value = value;
                _radiusContext.RadCheck.Update(existingAttribute);
            }
            else
            {
                // Create new attribute
                var newAttribute = new RadCheck
                {
                    Username = username,
                    Attribute = attribute,
                    Op = op,
                    Value = value
                };
                await _radiusContext.RadCheck.AddAsync(newAttribute);
            }

            await _radiusContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting check attribute for user {Username}", username);
            return false;
        }
    }

    public async Task<bool> SetUserReplyAttributeAsync(string username, string attribute, string op, string value)
    {
        _logger.LogInformation("Setting reply attribute for user {Username}: {Attribute}={Value}", username, attribute, value);
        
        try
        {
            // Check if attribute already exists
            var existingAttribute = await _radiusContext.RadReply
                .FirstOrDefaultAsync(rr => rr.Username == username && rr.Attribute == attribute);

            if (existingAttribute != null)
            {
                // Update existing attribute
                existingAttribute.Op = op;
                existingAttribute.Value = value;
                _radiusContext.RadReply.Update(existingAttribute);
            }
            else
            {
                // Create new attribute
                var newAttribute = new RadReply
                {
                    Username = username,
                    Attribute = attribute,
                    Op = op,
                    Value = value
                };
                await _radiusContext.RadReply.AddAsync(newAttribute);
            }

            await _radiusContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting reply attribute for user {Username}", username);
            return false;
        }
    }

    public async Task<bool> RemoveUserCheckAttributeAsync(string username, string attribute)
    {
        _logger.LogInformation("Removing check attribute for user {Username}: {Attribute}", username, attribute);
        
        try
        {
            var attributeToRemove = await _radiusContext.RadCheck
                .FirstOrDefaultAsync(rc => rc.Username == username && rc.Attribute == attribute);

            if (attributeToRemove != null)
            {
                _radiusContext.RadCheck.Remove(attributeToRemove);
                await _radiusContext.SaveChangesAsync();
                return true;
            }
            
            return false; // Attribute not found
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing check attribute for user {Username}", username);
            return false;
        }
    }

    public async Task<bool> RemoveUserReplyAttributeAsync(string username, string attribute)
    {
        _logger.LogInformation("Removing reply attribute for user {Username}: {Attribute}", username, attribute);
        
        try
        {
            var attributeToRemove = await _radiusContext.RadReply
                .FirstOrDefaultAsync(rr => rr.Username == username && rr.Attribute == attribute);

            if (attributeToRemove != null)
            {
                _radiusContext.RadReply.Remove(attributeToRemove);
                await _radiusContext.SaveChangesAsync();
                return true;
            }
            
            return false; // Attribute not found
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing reply attribute for user {Username}", username);
            return false;
        }
    }

    public async Task<bool> ClearUserAttributesAsync(string username)
    {
        _logger.LogInformation("Clearing all attributes for user: {Username}", username);
        
        try
        {
            var checkAttributes = await _radiusContext.RadCheck
                .Where(rc => rc.Username == username)
                .ToListAsync();
                
            var replyAttributes = await _radiusContext.RadReply
                .Where(rr => rr.Username == username)
                .ToListAsync();

            if (checkAttributes.Any())
            {
                _radiusContext.RadCheck.RemoveRange(checkAttributes);
            }
            
            if (replyAttributes.Any())
            {
                _radiusContext.RadReply.RemoveRange(replyAttributes);
            }

            await _radiusContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing attributes for user {Username}", username);
            return false;
        }
    }

    public async Task<bool> AddRadiusUserAttributeAsync(string username, string attribute, string op, string value)
    {
        _logger.LogInformation("Adding attribute for user {Username}: {Attribute}={Value}", username, attribute, value);
        
        try
        {
            var newAttribute = new RadCheck
            {
                Username = username,
                Attribute = attribute,
                Op = op,
                Value = value
            };
            
            await _radiusContext.RadCheck.AddAsync(newAttribute);
            await _radiusContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding attribute for user {Username}", username);
            return false;
        }
    }

    public async Task<bool> RemoveRadiusUserAttributeAsync(string username, int attributeId)
    {
        _logger.LogInformation("Removing attribute {AttributeId} for user {Username}", attributeId, username);
        
        try
        {
            var attributeToRemove = await _radiusContext.RadCheck
                .FirstOrDefaultAsync(rc => rc.Id == attributeId && rc.Username == username);

            if (attributeToRemove != null)
            {
                _radiusContext.RadCheck.Remove(attributeToRemove);
                await _radiusContext.SaveChangesAsync();
                return true;
            }
            
            return false; // Attribute not found
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing attribute {AttributeId} for user {Username}", attributeId, username);
            return false;
        }
    }

    // Group management
    public async Task<bool> CreateGroupAsync(string groupName, Dictionary<string, string>? checkAttributes = null, Dictionary<string, string>? replyAttributes = null)
    {
        _logger.LogInformation("Creating RADIUS group: {GroupName}", groupName);
        
        try
        {
            // Check if group already exists
            var groupExists = await _radiusContext.RadUserGroup
                .AnyAsync(rug => rug.GroupName == groupName);
                
            if (groupExists)
            {
                _logger.LogWarning("Group {GroupName} already exists", groupName);
                return false;
            }

            // Add check attributes if provided
            if (checkAttributes != null)
            {
                foreach (var attr in checkAttributes)
                {
                    var checkAttribute = new RadGroupCheck
                    {
                        GroupName = groupName,
                        Attribute = attr.Key,
                        Op = ":=",
                        Value = attr.Value
                    };
                    await _radiusContext.RadGroupCheck.AddAsync(checkAttribute);
                }
            }

            // Add reply attributes if provided
            if (replyAttributes != null)
            {
                foreach (var attr in replyAttributes)
                {
                    var replyAttribute = new RadGroupReply
                    {
                        GroupName = groupName,
                        Attribute = attr.Key,
                        Op = ":=",
                        Value = attr.Value
                    };
                    await _radiusContext.RadGroupReply.AddAsync(replyAttribute);
                }
            }

            await _radiusContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating group {GroupName}", groupName);
            return false;
        }
    }

    public async Task<bool> CreateRadiusGroupAsync(string groupName, Dictionary<string, string>? checkAttributes = null, Dictionary<string, string>? replyAttributes = null)
    {
        _logger.LogInformation("Creating RADIUS group: {GroupName}", groupName);
        
        // Delegate to CreateGroupAsync to avoid code duplication
        return await CreateGroupAsync(groupName, checkAttributes, replyAttributes);
    }

    public async Task<bool> DeleteGroupAsync(string groupName)
    {
        _logger.LogInformation("Deleting RADIUS group: {GroupName}", groupName);
        
        try
        {
            // Remove all user-group associations
            var userGroups = await _radiusContext.RadUserGroup
                .Where(rug => rug.GroupName == groupName)
                .ToListAsync();
                
            if (userGroups.Any())
            {
                _radiusContext.RadUserGroup.RemoveRange(userGroups);
            }

            // Remove all group check attributes
            var groupCheckAttributes = await _radiusContext.RadGroupCheck
                .Where(rgc => rgc.GroupName == groupName)
                .ToListAsync();
                
            if (groupCheckAttributes.Any())
            {
                _radiusContext.RadGroupCheck.RemoveRange(groupCheckAttributes);
            }

            // Remove all group reply attributes
            var groupReplyAttributes = await _radiusContext.RadGroupReply
                .Where(rgr => rgr.GroupName == groupName)
                .ToListAsync();
                
            if (groupReplyAttributes.Any())
            {
                _radiusContext.RadGroupReply.RemoveRange(groupReplyAttributes);
            }

            await _radiusContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting group {GroupName}", groupName);
            return false;
        }
    }

    public async Task<bool> DeleteRadiusGroupAsync(string groupName)
    {
        _logger.LogInformation("Deleting RADIUS group: {GroupName}", groupName);
        
        // Delegate to DeleteGroupAsync to avoid code duplication
        return await DeleteGroupAsync(groupName);
    }

    public async Task<bool> GroupExistsAsync(string groupName)
    {
        _logger.LogDebug("Checking if RADIUS group exists: {GroupName}", groupName);
        
        return await _radiusContext.RadUserGroup
            .AnyAsync(rug => rug.GroupName == groupName);
    }

    public async Task<IEnumerable<string>> GetAllGroupsAsync()
    {
        _logger.LogDebug("Getting all RADIUS groups");
        
        return await _radiusContext.RadUserGroup
            .Select(rug => rug.GroupName)
            .Distinct()
            .ToListAsync();
    }

    // Group attributes
    public async Task<IEnumerable<RadGroupCheck>> GetGroupCheckAttributesAsync(string groupName)
    {
        _logger.LogDebug("Getting check attributes for group: {GroupName}", groupName);
        
        return await _radiusContext.RadGroupCheck
            .Where(rgc => rgc.GroupName == groupName)
            .ToListAsync();
    }

    public async Task<IEnumerable<RadGroupReply>> GetGroupReplyAttributesAsync(string groupName)
    {
        _logger.LogDebug("Getting reply attributes for group: {GroupName}", groupName);
        
        return await _radiusContext.RadGroupReply
            .Where(rgr => rgr.GroupName == groupName)
            .ToListAsync();
    }

    public async Task<bool> SetGroupCheckAttributeAsync(string groupName, string attribute, string op, string value)
    {
        _logger.LogInformation("Setting check attribute for group {GroupName}: {Attribute}={Value}", groupName, attribute, value);
        
        try
        {
            // Check if attribute already exists
            var existingAttribute = await _radiusContext.RadGroupCheck
                .FirstOrDefaultAsync(rgc => rgc.GroupName == groupName && rgc.Attribute == attribute);

            if (existingAttribute != null)
            {
                // Update existing attribute
                existingAttribute.Op = op;
                existingAttribute.Value = value;
                _radiusContext.RadGroupCheck.Update(existingAttribute);
            }
            else
            {
                // Create new attribute
                var newAttribute = new RadGroupCheck
                {
                    GroupName = groupName,
                    Attribute = attribute,
                    Op = op,
                    Value = value
                };
                await _radiusContext.RadGroupCheck.AddAsync(newAttribute);
            }

            await _radiusContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting check attribute for group {GroupName}", groupName);
            return false;
        }
    }

    public async Task<bool> SetGroupReplyAttributeAsync(string groupName, string attribute, string op, string value)
    {
        _logger.LogInformation("Setting reply attribute for group {GroupName}: {Attribute}={Value}", groupName, attribute, value);
        
        try
        {
            // Check if attribute already exists
            var existingAttribute = await _radiusContext.RadGroupReply
                .FirstOrDefaultAsync(rgr => rgr.GroupName == groupName && rgr.Attribute == attribute);

            if (existingAttribute != null)
            {
                // Update existing attribute
                existingAttribute.Op = op;
                existingAttribute.Value = value;
                _radiusContext.RadGroupReply.Update(existingAttribute);
            }
            else
            {
                // Create new attribute
                var newAttribute = new RadGroupReply
                {
                    GroupName = groupName,
                    Attribute = attribute,
                    Op = op,
                    Value = value
                };
                await _radiusContext.RadGroupReply.AddAsync(newAttribute);
            }

            await _radiusContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting reply attribute for group {GroupName}", groupName);
            return false;
        }
    }

    public async Task<bool> RemoveGroupCheckAttributeAsync(string groupName, string attribute)
    {
        _logger.LogInformation("Removing check attribute for group {GroupName}: {Attribute}", groupName, attribute);
        
        try
        {
            var attributeToRemove = await _radiusContext.RadGroupCheck
                .FirstOrDefaultAsync(rgc => rgc.GroupName == groupName && rgc.Attribute == attribute);

            if (attributeToRemove != null)
            {
                _radiusContext.RadGroupCheck.Remove(attributeToRemove);
                await _radiusContext.SaveChangesAsync();
                return true;
            }
            
            return false; // Attribute not found
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing check attribute for group {GroupName}", groupName);
            return false;
        }
    }

    public async Task<bool> RemoveGroupReplyAttributeAsync(string groupName, string attribute)
    {
        _logger.LogInformation("Removing reply attribute for group {GroupName}: {Attribute}", groupName, attribute);
        
        try
        {
            var attributeToRemove = await _radiusContext.RadGroupReply
                .FirstOrDefaultAsync(rgr => rgr.GroupName == groupName && rgr.Attribute == attribute);

            if (attributeToRemove != null)
            {
                _radiusContext.RadGroupReply.Remove(attributeToRemove);
                await _radiusContext.SaveChangesAsync();
                return true;
            }
            
            return false; // Attribute not found
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing reply attribute for group {GroupName}", groupName);
            return false;
        }
    }

    public async Task<bool> ClearGroupAttributesAsync(string groupName)
    {
        _logger.LogInformation("Clearing all attributes for group: {GroupName}", groupName);
        
        try
        {
            var checkAttributes = await _radiusContext.RadGroupCheck
                .Where(rgc => rgc.GroupName == groupName)
                .ToListAsync();
                
            var replyAttributes = await _radiusContext.RadGroupReply
                .Where(rgr => rgr.GroupName == groupName)
                .ToListAsync();

            if (checkAttributes.Any())
            {
                _radiusContext.RadGroupCheck.RemoveRange(checkAttributes);
            }
            
            if (replyAttributes.Any())
            {
                _radiusContext.RadGroupReply.RemoveRange(replyAttributes);
            }

            await _radiusContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing attributes for group {GroupName}", groupName);
            return false;
        }
    }

    // User-Group relationships
    public async Task<bool> AddUserToGroupAsync(string username, string groupName, int priority = 1)
    {
        _logger.LogInformation("Adding user {Username} to group {GroupName} with priority {Priority}", username, groupName, priority);
        
        try
        {
            // Check if user-group relationship already exists
            var existingRelation = await _radiusContext.RadUserGroup
                .FirstOrDefaultAsync(rug => rug.Username == username && rug.GroupName == groupName);
                
            if (existingRelation != null)
            {
                // Update priority if relationship exists
                existingRelation.Priority = priority;
                _radiusContext.RadUserGroup.Update(existingRelation);
            }
            else
            {
                // Create new user-group relationship
                var newRelation = new RadUserGroup
                {
                    Username = username,
                    GroupName = groupName,
                    Priority = priority
                };
                await _radiusContext.RadUserGroup.AddAsync(newRelation);
            }

            await _radiusContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user {Username} to group {GroupName}", username, groupName);
            return false;
        }
    }

    public async Task<bool> RemoveUserFromGroupAsync(string username, string groupName)
    {
        _logger.LogInformation("Removing user {Username} from group {GroupName}", username, groupName);
        
        try
        {
            var relationToRemove = await _radiusContext.RadUserGroup
                .FirstOrDefaultAsync(rug => rug.Username == username && rug.GroupName == groupName);

            if (relationToRemove != null)
            {
                _radiusContext.RadUserGroup.Remove(relationToRemove);
                await _radiusContext.SaveChangesAsync();
                return true;
            }
            
            return false; // Relationship not found
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user {Username} from group {GroupName}", username, groupName);
            return false;
        }
    }

    public async Task<bool> RemoveUserFromAllGroupsAsync(string username)
    {
        _logger.LogInformation("Removing user {Username} from all groups", username);
        
        try
        {
            var userGroups = await _radiusContext.RadUserGroup
                .Where(rug => rug.Username == username)
                .ToListAsync();

            if (userGroups.Any())
            {
                _radiusContext.RadUserGroup.RemoveRange(userGroups);
                await _radiusContext.SaveChangesAsync();
            }
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user {Username} from all groups", username);
            return false;
        }
    }

    public async Task<IEnumerable<RadUserGroup>> GetUserGroupsAsync(string username)
    {
        _logger.LogDebug("Getting groups for user: {Username}", username);
        
        return await _radiusContext.RadUserGroup
            .Where(rug => rug.Username == username)
            .OrderBy(rug => rug.Priority)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetUsersInGroupAsync(string groupName)
    {
        _logger.LogDebug("Getting users in group: {GroupName}", groupName);
        
        return await _radiusContext.RadUserGroup
            .Where(rug => rug.GroupName == groupName)
            .Select(rug => rug.Username)
            .ToListAsync();
    }

    // Session management
    public async Task<IEnumerable<RadAcct>> GetActiveSessionsAsync()
    {
        _logger.LogDebug("Getting active sessions");
        
        return await _radiusContext.RadAcct
            .Where(ra => ra.AcctStopTime == null)
            .OrderByDescending(ra => ra.AcctStartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<RadAcct>> GetUserSessionsAsync(string username, int page = 1, int pageSize = 10)
    {
        _logger.LogDebug("Getting sessions for user {Username}, page {Page}, size {PageSize}", username, page, pageSize);
        
        return await _radiusContext.RadAcct
            .Where(ra => ra.Username == username)
            .OrderByDescending(ra => ra.AcctStartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<RadAcct?> GetSessionByIdAsync(string acctSessionId)
    {
        _logger.LogDebug("Getting session by ID: {SessionId}", acctSessionId);
        
        return await _radiusContext.RadAcct
            .FirstOrDefaultAsync(ra => ra.AcctSessionId == acctSessionId);
    }

    public async Task<IEnumerable<RadAcct>> GetSessionsByNasAsync(string nasIpAddress)
    {
        _logger.LogDebug("Getting sessions for NAS: {NasIpAddress}", nasIpAddress);
        
        return await _radiusContext.RadAcct
            .Where(ra => ra.NasIpAddress == nasIpAddress)
            .OrderByDescending(ra => ra.AcctStartTime)
            .ToListAsync();
    }

    public async Task<int> GetActiveSessionsCountAsync()
    {
        _logger.LogDebug("Getting active sessions count");
        
        return await _radiusContext.RadAcct
            .CountAsync(ra => ra.AcctStopTime == null);
    }

    public async Task<int> GetTotalSessionsCountAsync()
    {
        _logger.LogDebug("Getting total sessions count");
        
        return await _radiusContext.RadAcct.CountAsync();
    }

    // Authentication logs
    public async Task<IEnumerable<RadPostAuth>> GetAuthenticationLogsAsync(int page = 1, int pageSize = 10)
    {
        _logger.LogDebug("Getting authentication logs, page {Page}, size {PageSize}", page, pageSize);
        
        return await _radiusContext.RadPostAuth
            .OrderByDescending(rpa => rpa.AuthDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<RadPostAuth>> GetUserAuthenticationLogsAsync(string username, int page = 1, int pageSize = 10)
    {
        _logger.LogDebug("Getting authentication logs for user {Username}, page {Page}, size {PageSize}", username, page, pageSize);
        
        return await _radiusContext.RadPostAuth
            .Where(rpa => rpa.Username == username)
            .OrderByDescending(rpa => rpa.AuthDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetAuthenticationLogsCountAsync()
    {
        _logger.LogDebug("Getting authentication logs count");
        
        return await _radiusContext.RadPostAuth.CountAsync();
    }

    // Policy templates
    public async Task<bool> ApplyPolicyTemplateToUserAsync(string username, long templateId)
    {
        _logger.LogInformation("Applying policy template {TemplateId} to user {Username}", templateId, username);
        return await Task.FromResult(true);
    }

    public async Task<bool> ApplyPolicyTemplateToGroupAsync(string groupName, long templateId)
    {
        _logger.LogInformation("Applying policy template {TemplateId} to group {GroupName}", templateId, groupName);
        return await Task.FromResult(true);
    }

    public async Task<PolicyTemplate?> GetPolicyTemplateAsync(long templateId)
    {
        _logger.LogDebug("Getting policy template: {TemplateId}", templateId);
        return await Task.FromResult<PolicyTemplate?>(null);
    }

    public async Task<IEnumerable<PolicyTemplate>> GetAllPolicyTemplatesAsync()
    {
        _logger.LogDebug("Getting all policy templates");
        return await Task.FromResult(Enumerable.Empty<PolicyTemplate>());
    }

    // Change of Authorization (CoA)
    public async Task<bool> DisconnectUserSessionAsync(string username, string? acctSessionId = null)
    {
        _logger.LogInformation("Disconnecting user session for {Username}, session {SessionId}", username, acctSessionId);
        return await Task.FromResult(true);
    }

    public async Task<bool> SendCoaRequestAsync(string username, string acctSessionId, Dictionary<string, string> attributes)
    {
        _logger.LogInformation("Sending CoA request for user {Username}, session {SessionId}", username, acctSessionId);
        return await Task.FromResult(true);
    }

    public async Task<IEnumerable<CoaRequest>> GetCoaRequestsAsync(int page = 1, int pageSize = 10)
    {
        _logger.LogDebug("Getting CoA requests, page {Page}, size {PageSize}", page, pageSize);
        return await Task.FromResult(Enumerable.Empty<CoaRequest>());
    }

    public async Task<CoaRequest?> GetCoaRequestAsync(long requestId)
    {
        _logger.LogDebug("Getting CoA request: {RequestId}", requestId);
        return await Task.FromResult<CoaRequest?>(null);
    }

    // Statistics and reporting
    public async Task<Dictionary<string, object>> GetRadiusStatisticsAsync()
    {
        _logger.LogDebug("Getting RADIUS statistics");
        
        var totalUsers = await _radiusContext.RadCheck
            .Select(rc => rc.Username)
            .Distinct()
            .CountAsync();
            
        var activeUsers = await _radiusContext.RadAcct
            .Where(ra => ra.AcctStopTime == null)
            .Select(ra => ra.Username)
            .Distinct()
            .CountAsync();
            
        var totalSessions = await _radiusContext.RadAcct.CountAsync();
        
        var activeSessions = await _radiusContext.RadAcct
            .Where(ra => ra.AcctStopTime == null)
            .CountAsync();
            
        var totalGroups = await _radiusContext.RadUserGroup
            .Select(rug => rug.GroupName)
            .Distinct()
            .CountAsync();
        
        return new Dictionary<string, object>
        {
            ["totalUsers"] = totalUsers,
            ["activeUsers"] = activeUsers,
            ["totalSessions"] = totalSessions,
            ["activeSessions"] = activeSessions,
            ["totalGroups"] = totalGroups
        };
    }

    public async Task<Dictionary<string, int>> GetSessionStatsByNasAsync()
    {
        _logger.LogDebug("Getting session stats by NAS");
        
        var nasStats = await _radiusContext.RadAcct
            .Where(ra => ra.NasIpAddress != null)
            .GroupBy(ra => ra.NasIpAddress)
            .Select(g => new { NasIp = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.NasIp!, x => x.Count);
            
        return nasStats;
    }

    public async Task<Dictionary<string, int>> GetAuthenticationStatsByResultAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        _logger.LogDebug("Getting authentication stats by result from {StartDate} to {EndDate}", startDate, endDate);
        
        var query = _radiusContext.RadPostAuth.AsQueryable();
        
        if (startDate.HasValue)
        {
            query = query.Where(rpa => rpa.AuthDate >= startDate.Value);
        }
        
        if (endDate.HasValue)
        {
            query = query.Where(rpa => rpa.AuthDate <= endDate.Value);
        }
        
        var authStats = await query
            .GroupBy(rpa => rpa.Reply)
            .Select(g => new { Reply = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Reply ?? "Unknown", x => x.Count);
            
        // Ensure we have the standard reply types
        var result = new Dictionary<string, int>
        {
            ["Accept"] = authStats.GetValueOrDefault("Access-Accept", 0),
            ["Reject"] = authStats.GetValueOrDefault("Access-Reject", 0),
            ["Challenge"] = authStats.GetValueOrDefault("Access-Challenge", 0)
        };
        
        // Add any other reply types found
        foreach (var kvp in authStats)
        {
            if (!result.ContainsKey(kvp.Key))
            {
                result[kvp.Key] = kvp.Value;
            }
        }
        
        return result;
    }

    public async Task<IEnumerable<object>> GetTopUsersAsync(int count = 10)
    {
        _logger.LogDebug("Getting top {Count} users", count);
        
        var topUsers = await _radiusContext.RadAcct
            .Where(ra => ra.Username != null)
            .GroupBy(ra => ra.Username)
            .Select(g => new
            {
                Username = g.Key,
                SessionCount = g.Count(),
                TotalInputOctets = g.Sum(ra => ra.AcctInputOctets ?? 0),
                TotalOutputOctets = g.Sum(ra => ra.AcctOutputOctets ?? 0),
                TotalSessionTime = g.Sum(ra => ra.AcctSessionTime ?? 0),
                LastSession = g.Max(ra => ra.AcctStartTime)
            })
            .OrderByDescending(u => u.SessionCount)
            .Take(count)
            .ToListAsync();
            
        return topUsers.Cast<object>();
    }

    public async Task<int> GetRadiusGroupsCountAsync()
    {
        _logger.LogDebug("Getting radius groups count");
        
        return await _radiusContext.RadUserGroup
            .Select(rug => rug.GroupName)
            .Distinct()
            .CountAsync();
    }

    public async Task<object?> GetRadiusGroupAsync(string groupName)
    {
        _logger.LogDebug("Getting radius group: {GroupName}", groupName);
        
        // Check if group exists
        var groupExists = await _radiusContext.RadUserGroup
            .AnyAsync(rug => rug.GroupName == groupName);

        if (!groupExists)
        {
            return null;
        }

        // Get group's check attributes
        var checkAttributes = await _radiusContext.RadGroupCheck
            .Where(rgc => rgc.GroupName == groupName)
            .Select(rgc => new { rgc.Attribute, rgc.Op, rgc.Value })
            .ToListAsync();

        // Get group's reply attributes
        var replyAttributes = await _radiusContext.RadGroupReply
            .Where(rgr => rgr.GroupName == groupName)
            .Select(rgr => new { rgr.Attribute, rgr.Op, rgr.Value })
            .ToListAsync();

        // Get users in group
        var userCount = await _radiusContext.RadUserGroup
            .Where(rug => rug.GroupName == groupName)
            .CountAsync();

        var users = await _radiusContext.RadUserGroup
            .Where(rug => rug.GroupName == groupName)
            .OrderBy(rug => rug.Priority)
            .Select(rug => new { rug.Username, rug.Priority })
            .ToListAsync();

        return new
        {
            GroupName = groupName,
            UserCount = userCount,
            Users = users,
            CheckAttributes = checkAttributes,
            ReplyAttributes = replyAttributes
        };
    }

    public async Task<object> GetRadiusGroupsAsync(int page = 1, int pageSize = 10)
    {
        _logger.LogDebug("Getting radius groups, page {Page}, size {PageSize}", page, pageSize);
        
        // Get distinct group names with user counts
        var groupsQuery = _radiusContext.RadUserGroup
            .GroupBy(rug => rug.GroupName)
            .Select(g => new
            {
                GroupName = g.Key,
                UserCount = g.Count()
            });

        var totalCount = await groupsQuery.CountAsync();
        
        var groups = await groupsQuery
            .OrderBy(g => g.GroupName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Enhance with attribute counts for each group
        var enhancedGroups = new List<object>();
        foreach (var group in groups)
        {
            var checkAttributeCount = await _radiusContext.RadGroupCheck
                .Where(rgc => rgc.GroupName == group.GroupName)
                .CountAsync();

            var replyAttributeCount = await _radiusContext.RadGroupReply
                .Where(rgr => rgr.GroupName == group.GroupName)
                .CountAsync();

            enhancedGroups.Add(new
            {
                group.GroupName,
                group.UserCount,
                CheckAttributeCount = checkAttributeCount,
                ReplyAttributeCount = replyAttributeCount,
                TotalAttributes = checkAttributeCount + replyAttributeCount
            });
        }
        
        return new
        {
            Groups = enhancedGroups,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }
}