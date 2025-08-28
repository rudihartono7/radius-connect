using Microsoft.EntityFrameworkCore;
using RadiusConnect.Api.Data;
using RadiusConnect.Api.Models.Domain;
using RadiusConnect.Api.Repositories.Interfaces;

namespace RadiusConnect.Api.Repositories;

public class UserRepository : Repository<AppUser>, IUserRepository
{
    private readonly AppDbContext _appContext;

    public UserRepository(AppDbContext context) : base(context)
    {
        _appContext = context;
    }

    public async Task<AppUser?> GetByUsernameAsync(string username)
    {
        return await _appContext.AppUsers
            .FirstOrDefaultAsync(u => u.UserName == username);
    }

    public async Task<AppUser?> GetByEmailAsync(string email)
    {
        return await _appContext.AppUsers
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<AppUser?> GetWithRolesAsync(Guid id)
    {
        return await _appContext.AppUsers
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<IEnumerable<AppUser>> GetUsersWithRolesAsync()
    {
        return await _appContext.AppUsers
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ToListAsync();
    }

    public async Task<bool> IsUsernameExistsAsync(string username)
    {
        return await _appContext.AppUsers
            .AnyAsync(u => u.UserName == username);
    }

    public async Task<bool> IsEmailExistsAsync(string email)
    {
        return await _appContext.AppUsers
            .AnyAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<AppUser>> SearchUsersAsync(string searchTerm, int page = 1, int pageSize = 10)
    {
        var query = _appContext.AppUsers
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Where(u => u.UserName.Contains(searchTerm) || 
                       u.Email.Contains(searchTerm));

        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalUsersCountAsync()
    {
        return await _appContext.AppUsers.CountAsync();
    }

    public async Task<IEnumerable<AppUser>> GetUsersByRoleAsync(string roleName)
    {
        return await _appContext.AppUsers
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Where(u => u.UserRoles.Any(ur => ur.Role.Name == roleName))
            .ToListAsync();
    }
}