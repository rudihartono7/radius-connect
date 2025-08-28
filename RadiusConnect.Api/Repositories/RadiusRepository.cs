using Microsoft.EntityFrameworkCore;
using RadiusConnect.Api.Data;
using RadiusConnect.Api.Models.Domain;
using RadiusConnect.Api.Repositories.Interfaces;

namespace RadiusConnect.Api.Repositories;

public class RadiusRepository : IRadiusRepository
{
    private readonly RadiusDbContext _context;

    public RadiusRepository(RadiusDbContext context)
    {
        _context = context;
    }

    #region RadCheck Operations

    public async Task<IEnumerable<RadCheck>> GetUserCheckAttributesAsync(string username)
    {
        return await _context.RadCheck
            .Where(rc => rc.Username == username)
            .ToListAsync();
    }

    public async Task<RadCheck?> GetUserCheckAttributeAsync(string username, string attribute)
    {
        return await _context.RadCheck
            .FirstOrDefaultAsync(rc => rc.Username == username && rc.Attribute == attribute);
    }

    public async Task AddUserCheckAttributeAsync(RadCheck radCheck)
    {
        await _context.RadCheck.AddAsync(radCheck);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateUserCheckAttributeAsync(RadCheck radCheck)
    {
        _context.RadCheck.Update(radCheck);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteUserCheckAttributeAsync(string username, string attribute)
    {
        var radCheck = await GetUserCheckAttributeAsync(username, attribute);
        if (radCheck != null)
        {
            _context.RadCheck.Remove(radCheck);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAllUserCheckAttributesAsync(string username)
    {
        var radChecks = await GetUserCheckAttributesAsync(username);
        _context.RadCheck.RemoveRange(radChecks);
        await _context.SaveChangesAsync();
    }

    #endregion

    #region RadReply Operations

    public async Task<IEnumerable<RadReply>> GetUserReplyAttributesAsync(string username)
    {
        return await _context.RadReply
            .Where(rr => rr.Username == username)
            .ToListAsync();
    }

    public async Task<RadReply?> GetUserReplyAttributeAsync(string username, string attribute)
    {
        return await _context.RadReply
            .FirstOrDefaultAsync(rr => rr.Username == username && rr.Attribute == attribute);
    }

    public async Task AddUserReplyAttributeAsync(RadReply radReply)
    {
        await _context.RadReply.AddAsync(radReply);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateUserReplyAttributeAsync(RadReply radReply)
    {
        _context.RadReply.Update(radReply);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteUserReplyAttributeAsync(string username, string attribute)
    {
        var radReply = await GetUserReplyAttributeAsync(username, attribute);
        if (radReply != null)
        {
            _context.RadReply.Remove(radReply);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAllUserReplyAttributesAsync(string username)
    {
        var radReplies = await GetUserReplyAttributesAsync(username);
        _context.RadReply.RemoveRange(radReplies);
        await _context.SaveChangesAsync();
    }

    #endregion

    #region RadUserGroup Operations

    public async Task<IEnumerable<RadUserGroup>> GetUserGroupsAsync(string username)
    {
        return await _context.RadUserGroup
            .Where(rug => rug.Username == username)
            .OrderBy(rug => rug.Priority)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetUsersInGroupAsync(string groupName)
    {
        return await _context.RadUserGroup
            .Where(rug => rug.GroupName == groupName)
            .Select(rug => rug.Username)
            .ToListAsync();
    }

    public async Task AddUserToGroupAsync(RadUserGroup radUserGroup)
    {
        await _context.RadUserGroup.AddAsync(radUserGroup);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveUserFromGroupAsync(string username, string groupName)
    {
        var radUserGroup = await _context.RadUserGroup
            .FirstOrDefaultAsync(rug => rug.Username == username && rug.GroupName == groupName);
        
        if (radUserGroup != null)
        {
            _context.RadUserGroup.Remove(radUserGroup);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveUserFromAllGroupsAsync(string username)
    {
        var userGroups = await GetUserGroupsAsync(username);
        _context.RadUserGroup.RemoveRange(userGroups);
        await _context.SaveChangesAsync();
    }

    #endregion

    #region RadGroupCheck Operations

    public async Task<IEnumerable<RadGroupCheck>> GetGroupCheckAttributesAsync(string groupName)
    {
        return await _context.RadGroupCheck
            .Where(rgc => rgc.GroupName == groupName)
            .ToListAsync();
    }

    public async Task<RadGroupCheck?> GetGroupCheckAttributeAsync(string groupName, string attribute)
    {
        return await _context.RadGroupCheck
            .FirstOrDefaultAsync(rgc => rgc.GroupName == groupName && rgc.Attribute == attribute);
    }

    public async Task AddGroupCheckAttributeAsync(RadGroupCheck radGroupCheck)
    {
        await _context.RadGroupCheck.AddAsync(radGroupCheck);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateGroupCheckAttributeAsync(RadGroupCheck radGroupCheck)
    {
        _context.RadGroupCheck.Update(radGroupCheck);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteGroupCheckAttributeAsync(string groupName, string attribute)
    {
        var radGroupCheck = await GetGroupCheckAttributeAsync(groupName, attribute);
        if (radGroupCheck != null)
        {
            _context.RadGroupCheck.Remove(radGroupCheck);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAllGroupCheckAttributesAsync(string groupName)
    {
        var radGroupChecks = await GetGroupCheckAttributesAsync(groupName);
        _context.RadGroupCheck.RemoveRange(radGroupChecks);
        await _context.SaveChangesAsync();
    }

    #endregion

    #region RadGroupReply Operations

    public async Task<IEnumerable<RadGroupReply>> GetGroupReplyAttributesAsync(string groupName)
    {
        return await _context.RadGroupReply
            .Where(rgr => rgr.GroupName == groupName)
            .ToListAsync();
    }

    public async Task<RadGroupReply?> GetGroupReplyAttributeAsync(string groupName, string attribute)
    {
        return await _context.RadGroupReply
            .FirstOrDefaultAsync(rgr => rgr.GroupName == groupName && rgr.Attribute == attribute);
    }

    public async Task AddGroupReplyAttributeAsync(RadGroupReply radGroupReply)
    {
        await _context.RadGroupReply.AddAsync(radGroupReply);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateGroupReplyAttributeAsync(RadGroupReply radGroupReply)
    {
        _context.RadGroupReply.Update(radGroupReply);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteGroupReplyAttributeAsync(string groupName, string attribute)
    {
        var radGroupReply = await GetGroupReplyAttributeAsync(groupName, attribute);
        if (radGroupReply != null)
        {
            _context.RadGroupReply.Remove(radGroupReply);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAllGroupReplyAttributesAsync(string groupName)
    {
        var radGroupReplies = await GetGroupReplyAttributesAsync(groupName);
        _context.RadGroupReply.RemoveRange(radGroupReplies);
        await _context.SaveChangesAsync();
    }

    #endregion

    #region RadAcct Operations

    public async Task<IEnumerable<RadAcct>> GetActiveSessionsAsync()
    {
        return await _context.RadAcct
            .Where(ra => ra.AcctStopTime == null)
            .OrderByDescending(ra => ra.AcctStartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<RadAcct>> GetUserSessionsAsync(string username, int page = 1, int pageSize = 10)
    {
        return await _context.RadAcct
            .Where(ra => ra.Username == username)
            .OrderByDescending(ra => ra.AcctStartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<RadAcct?> GetSessionByIdAsync(string acctSessionId)
    {
        return await _context.RadAcct
            .FirstOrDefaultAsync(ra => ra.AcctSessionId == acctSessionId);
    }

    public async Task<IEnumerable<RadAcct>> GetSessionsByNasAsync(string nasIpAddress)
    {
        return await _context.RadAcct
            .Where(ra => ra.NasIpAddress.ToString() == nasIpAddress)
            .OrderByDescending(ra => ra.AcctStartTime)
            .ToListAsync();
    }

    public async Task<int> GetActiveSessionsCountAsync()
    {
        return await _context.RadAcct
            .CountAsync(ra => ra.AcctStopTime == null);
    }

    public async Task<int> GetTotalSessionsCountAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.RadAcct.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(ra => ra.AcctStartTime >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(ra => ra.AcctStartTime <= endDate.Value);

        return await query.CountAsync();
    }

    #endregion

    #region RadPostAuth Operations

    public async Task<IEnumerable<RadPostAuth>> GetAuthenticationLogsAsync(int page = 1, int pageSize = 10)
    {
        return await _context.RadPostAuth
            .OrderByDescending(rpa => rpa.AuthDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<RadPostAuth>> GetUserAuthenticationLogsAsync(string username, int page = 1, int pageSize = 10)
    {
        return await _context.RadPostAuth
            .Where(rpa => rpa.Username == username)
            .OrderByDescending(rpa => rpa.AuthDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetAuthenticationLogsCountAsync()
    {
        return await _context.RadPostAuth.CountAsync();
    }

    public async Task<int> GetSuccessfulAuthenticationsCountAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.RadPostAuth.Where(rpa => rpa.Reply == "Access-Accept");

        if (startDate.HasValue)
            query = query.Where(rpa => rpa.AuthDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(rpa => rpa.AuthDate <= endDate.Value);

        return await query.CountAsync();
    }

    public async Task<int> GetFailedAuthenticationsCountAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.RadPostAuth.Where(rpa => rpa.Reply == "Access-Reject");

        if (startDate.HasValue)
            query = query.Where(rpa => rpa.AuthDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(rpa => rpa.AuthDate <= endDate.Value);

        return await query.CountAsync();
    }

    #endregion

    #region Group Management

    public async Task<IEnumerable<string>> GetAllGroupsAsync()
    {
        var checkGroups = await _context.RadGroupCheck
            .Select(rgc => rgc.GroupName)
            .Distinct()
            .ToListAsync();

        var replyGroups = await _context.RadGroupReply
            .Select(rgr => rgr.GroupName)
            .Distinct()
            .ToListAsync();

        var userGroups = await _context.RadUserGroup
            .Select(rug => rug.GroupName)
            .Distinct()
            .ToListAsync();

        return checkGroups.Union(replyGroups).Union(userGroups).Distinct().OrderBy(g => g);
    }

    public async Task<bool> GroupExistsAsync(string groupName)
    {
        return await _context.RadGroupCheck.AnyAsync(rgc => rgc.GroupName == groupName) ||
               await _context.RadGroupReply.AnyAsync(rgr => rgr.GroupName == groupName) ||
               await _context.RadUserGroup.AnyAsync(rug => rug.GroupName == groupName);
    }

    public async Task DeleteGroupAsync(string groupName)
    {
        var groupChecks = await GetGroupCheckAttributesAsync(groupName);
        var groupReplies = await GetGroupReplyAttributesAsync(groupName);
        var userGroups = await _context.RadUserGroup
            .Where(rug => rug.GroupName == groupName)
            .ToListAsync();

        _context.RadGroupCheck.RemoveRange(groupChecks);
        _context.RadGroupReply.RemoveRange(groupReplies);
        _context.RadUserGroup.RemoveRange(userGroups);

        await _context.SaveChangesAsync();
    }

    #endregion

    #region User Management

    public async Task<bool> RadiusUserExistsAsync(string username)
    {
        return await _context.RadCheck.AnyAsync(rc => rc.Username == username) ||
               await _context.RadReply.AnyAsync(rr => rr.Username == username) ||
               await _context.RadUserGroup.AnyAsync(rug => rug.Username == username);
    }

    public async Task DeleteRadiusUserAsync(string username)
    {
        await DeleteAllUserCheckAttributesAsync(username);
        await DeleteAllUserReplyAttributesAsync(username);
        await RemoveUserFromAllGroupsAsync(username);
    }

    public async Task<IEnumerable<string>> GetAllRadiusUsersAsync()
    {
        var checkUsers = await _context.RadCheck
            .Select(rc => rc.Username)
            .Distinct()
            .ToListAsync();

        var replyUsers = await _context.RadReply
            .Select(rr => rr.Username)
            .Distinct()
            .ToListAsync();

        var groupUsers = await _context.RadUserGroup
            .Select(rug => rug.Username)
            .Distinct()
            .ToListAsync();

        return checkUsers.Union(replyUsers).Union(groupUsers).Distinct().OrderBy(u => u);
    }

    public async Task<IEnumerable<string>> GetAllUserGroupsAsync()
    {
        return await _context.RadUserGroup
            .Select(rug => rug.GroupName)
            .Distinct()
            .OrderBy(g => g)
            .ToListAsync();
    }

    #endregion
}