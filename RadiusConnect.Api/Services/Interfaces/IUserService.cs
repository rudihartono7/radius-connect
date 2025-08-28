using RadiusConnect.Api.Models.Domain;

namespace RadiusConnect.Api.Services.Interfaces;

public interface IUserService
{
    // User management
    Task<AppUser?> GetUserByIdAsync(Guid id);
    Task<AppUser?> GetUserByUsernameAsync(string username);
    Task<AppUser?> GetUserByEmailAsync(string email);
    Task<AppUser?> GetUserWithRolesAsync(Guid id);
    Task<IEnumerable<AppUser>> GetAllUsersAsync();
    Task<IEnumerable<AppUser>> GetUsersWithRolesAsync();
    Task<IEnumerable<AppUser>> SearchUsersAsync(string? searchTerm, int page = 1, int pageSize = 10);
    Task<int> GetTotalUsersCountAsync();
    Task<IEnumerable<AppUser>> GetUsersByRoleAsync(string roleName);

    // User creation and updates
    Task<AppUser> CreateUserAsync(string username, string email, string password, List<string>? roles = null);
    Task<AppUser> UpdateUserAsync(Guid id, string? email = null, List<string>? roles = null);
    Task<bool> UpdateUserPasswordAsync(Guid id, string currentPassword, string newPassword);
    Task<bool> ResetUserPasswordAsync(Guid id, string newPassword);
    Task<bool> DeleteUserAsync(Guid id);

    // Authentication
    Task<AppUser?> AuthenticateAsync(string username, string password);
    Task<bool> ValidatePasswordAsync(AppUser user, string password);
    Task<string> HashPasswordAsync(string password);

    // TOTP/2FA management
    Task<string> GenerateTotpSecretAsync(Guid userId);
    Task<bool> EnableTotpAsync(Guid userId, string totpCode);
    Task<bool> DisableTotpAsync(Guid userId, string password);
    Task<bool> ValidateTotpAsync(Guid userId, string totpCode);
    Task<string> GenerateQrCodeUriAsync(Guid userId, string issuer = "RadiusConnect");

    // Role management
    Task<bool> AssignRoleToUserAsync(Guid userId, string roleName, Guid? assignedBy = null);
    Task<bool> RemoveRoleFromUserAsync(Guid userId, string roleName);
    Task<IEnumerable<string>> GetUserRolesAsync(Guid userId);
    Task<bool> UserHasRoleAsync(Guid userId, string roleName);
    Task<bool> UserHasAnyRoleAsync(Guid userId, params string[] roles);

    // Validation
    Task<bool> IsUsernameAvailableAsync(string username);
    Task<bool> IsEmailAvailableAsync(string email);
    Task<bool> ValidateUserCredentialsAsync(string username, string password);

    // Account management
    Task<bool> LockUserAccountAsync(Guid userId);
    Task<bool> UnlockUserAccountAsync(Guid userId);
    Task<bool> IsUserAccountLockedAsync(Guid userId);
    Task<bool> ActivateUserAsync(Guid userId);
    Task<bool> DeactivateUserAsync(Guid userId);
}