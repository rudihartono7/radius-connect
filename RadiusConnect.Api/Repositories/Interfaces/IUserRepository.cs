using RadiusConnect.Api.Models.Domain;

namespace RadiusConnect.Api.Repositories.Interfaces;

public interface IUserRepository : IRepository<AppUser>
{
    Task<AppUser?> GetByUsernameAsync(string username);
    Task<AppUser?> GetByEmailAsync(string email);
    Task<AppUser?> GetWithRolesAsync(Guid id);
    Task<IEnumerable<AppUser>> GetUsersWithRolesAsync();
    Task<bool> IsUsernameExistsAsync(string username);
    Task<bool> IsEmailExistsAsync(string email);
    Task<IEnumerable<AppUser>> SearchUsersAsync(string searchTerm, int page = 1, int pageSize = 10);
    Task<int> GetTotalUsersCountAsync();
    Task<IEnumerable<AppUser>> GetUsersByRoleAsync(string roleName);
}