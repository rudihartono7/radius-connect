using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RadiusConnect.Api.Data;
using RadiusConnect.Api.Models.Domain;
using RadiusConnect.Api.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace RadiusConnect.Api.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IPasswordHasher<AppUser> _passwordHasher;
    private readonly ILogger<UserService> _logger;

    public UserService(
        AppDbContext context,
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IPasswordHasher<AppUser> passwordHasher,
        ILogger<UserService> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    // User management
    public async Task<AppUser?> GetUserByIdAsync(Guid id)
    {
        _logger.LogDebug("Getting user by ID: {UserId}", id);
        return await _userManager.FindByIdAsync(id.ToString());
    }

    public async Task<AppUser?> GetUserByUsernameAsync(string username)
    {
        _logger.LogDebug("Getting user by username: {Username}", username);
        return await _userManager.FindByNameAsync(username);
    }

    public async Task<AppUser?> GetUserByEmailAsync(string email)
    {
        _logger.LogDebug("Getting user by email: {Email}", email);
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<AppUser?> GetUserWithRolesAsync(Guid id)
    {
        _logger.LogDebug("Getting user with roles by ID: {UserId}", id);
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user != null)
        {
            var userRole = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserName == user.UserName);

            return userRole;
        }
        
        return user;
    }

    public async Task<IEnumerable<AppUser>> GetAllUsersAsync()
    {
        _logger.LogDebug("Getting all users");
        return await _userManager.Users.ToListAsync();
    }

    public async Task<IEnumerable<AppUser>> GetUsersWithRolesAsync()
    {
        _logger.LogDebug("Getting all users with roles");
        return await _userManager.Users.ToListAsync();
    }

    public async Task<IEnumerable<AppUser>> SearchUsersAsync(string? searchTerm, int page = 1, int pageSize = 10)
    {
        _logger.LogDebug("Searching users with term: {SearchTerm}, page: {Page}, size: {PageSize}", searchTerm, page, pageSize);
        
        var query = _userManager.Users.AsQueryable();
        
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(u => u.UserName!.Contains(searchTerm) || u.Email!.Contains(searchTerm));
        }
        
        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalUsersCountAsync()
    {
        _logger.LogDebug("Getting total users count");
        return await _userManager.Users.CountAsync();
    }

    public async Task<IEnumerable<AppUser>> GetUsersByRoleAsync(string roleName)
    {
        _logger.LogDebug("Getting users by role: {RoleName}", roleName);
        return await _userManager.GetUsersInRoleAsync(roleName);
    }

    // User creation and updates
    public async Task<AppUser> CreateUserAsync(string username, string email, string password, List<string>? roles = null)
    {
        _logger.LogInformation("Creating user: {Username}, email: {Email}", username, email);
        
        var user = new AppUser
        {
            UserName = username,
            Email = email,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to create user {Username}: {Errors}", username, errors);
            throw new InvalidOperationException($"Failed to create user: {errors}");
        }

        // Assign roles if provided
        if (roles != null && roles.Any())
        {
            foreach (var role in roles)
            {
                if (await _roleManager.RoleExistsAsync(role))
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
            }
        }

        return user;
    }

    public async Task<AppUser> UpdateUserAsync(Guid id, string? email = null, List<string>? roles = null)
    {
        _logger.LogInformation("Updating user: {UserId}", id);
        
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            throw new ArgumentException($"User with ID {id} not found");
        }

        if (!string.IsNullOrEmpty(email))
        {
            user.Email = email;
            await _userManager.UpdateAsync(user);
        }

        if (roles != null)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            
            foreach (var role in roles)
            {
                if (await _roleManager.RoleExistsAsync(role))
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
            }
        }

        return user;
    }

    public async Task<bool> UpdateUserPasswordAsync(Guid id, string currentPassword, string newPassword)
    {
        _logger.LogInformation("Updating password for user: {UserId}", id);
        
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return false;
        }

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return result.Succeeded;
    }

    public async Task<bool> ResetUserPasswordAsync(Guid id, string newPassword)
    {
        _logger.LogInformation("Resetting password for user: {UserId}", id);
        
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return false;
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        return result.Succeeded;
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        _logger.LogInformation("Deleting user: {UserId}", id);
        
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return false;
        }

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }

    // Authentication
    public async Task<AppUser?> AuthenticateAsync(string username, string password)
    {
        _logger.LogDebug("Authenticating user: {Username}", username);
        
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserName == username);
            
        if (user == null)
        {
            return null;
        }

        var isValidPassword = await _userManager.CheckPasswordAsync(user, password);
        return isValidPassword ? user : null;
    }

    public async Task<bool> ValidatePasswordAsync(AppUser user, string password)
    {
        _logger.LogDebug("Validating password for user: {Username}", user.UserName);
        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<string> HashPasswordAsync(string password)
    {
        _logger.LogDebug("Hashing password");
        var user = new AppUser(); // Temporary user for hashing
        return _passwordHasher.HashPassword(user, password);
    }

    // TOTP/2FA management - Basic implementations
    public async Task<string> GenerateTotpSecretAsync(Guid userId)
    {
        _logger.LogInformation("Generating TOTP secret for user: {UserId}", userId);
        
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new ArgumentException($"User with ID {userId} not found");
        }

        // Generate a simple secret (in production, use a proper TOTP library)
        var secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(20));
        user.TotpSecret = secret;
        await _userManager.UpdateAsync(user);
        
        return secret;
    }

    public async Task<bool> EnableTotpAsync(Guid userId, string totpCode)
    {
        _logger.LogInformation("Enabling TOTP for user: {UserId}", userId);
        
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return false;
        }

        // Basic validation (in production, validate the TOTP code properly)
        user.TwoFactorEnabled = true;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> DisableTotpAsync(Guid userId, string password)
    {
        _logger.LogInformation("Disabling TOTP for user: {UserId}", userId);
        
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return false;
        }

        var isValidPassword = await _userManager.CheckPasswordAsync(user, password);
        if (!isValidPassword)
        {
            return false;
        }

        user.TwoFactorEnabled = false;
        user.TotpSecret = null;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> ValidateTotpAsync(Guid userId, string totpCode)
    {
        _logger.LogDebug("Validating TOTP for user: {UserId}", userId);
        
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null || !user.TwoFactorEnabled)
        {
            return false;
        }

        // Basic validation (in production, implement proper TOTP validation)
        return !string.IsNullOrEmpty(totpCode) && totpCode.Length == 6;
    }

    public async Task<string> GenerateQrCodeUriAsync(Guid userId, string issuer = "RadiusConnect")
    {
        _logger.LogDebug("Generating QR code URI for user: {UserId}", userId);
        
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new ArgumentException($"User with ID {userId} not found");
        }

        // Basic QR code URI format (in production, use proper TOTP URI format)
        return $"otpauth://totp/{issuer}:{user.UserName}?secret={user.TotpSecret}&issuer={issuer}";
    }

    // Role management
    public async Task<bool> AssignRoleToUserAsync(Guid userId, string roleName, Guid? assignedBy = null)
    {
        _logger.LogInformation("Assigning role {RoleName} to user: {UserId}", roleName, userId);
        
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return false;
        }

        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            return false;
        }

        var result = await _userManager.AddToRoleAsync(user, roleName);
        return result.Succeeded;
    }

    public async Task<bool> RemoveRoleFromUserAsync(Guid userId, string roleName)
    {
        _logger.LogInformation("Removing role {RoleName} from user: {UserId}", roleName, userId);
        
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return false;
        }

        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        return result.Succeeded;
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync(Guid userId)
    {
        _logger.LogDebug("Getting roles for user: {UserId}", userId);
        
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return Enumerable.Empty<string>();
        }

        return await _userManager.GetRolesAsync(user);
    }

    public async Task<bool> UserHasRoleAsync(Guid userId, string roleName)
    {
        _logger.LogDebug("Checking if user {UserId} has role: {RoleName}", userId, roleName);
        
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return false;
        }

        return await _userManager.IsInRoleAsync(user, roleName);
    }

    public async Task<bool> UserHasAnyRoleAsync(Guid userId, params string[] roles)
    {
        _logger.LogDebug("Checking if user {UserId} has any of roles: {Roles}", userId, string.Join(", ", roles));
        
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return false;
        }

        foreach (var role in roles)
        {
            if (await _userManager.IsInRoleAsync(user, role))
            {
                return true;
            }
        }

        return false;
    }

    // Validation
    public async Task<bool> IsUsernameAvailableAsync(string username)
    {
        _logger.LogDebug("Checking if username is available: {Username}", username);
        var user = await _userManager.FindByNameAsync(username);
        return user == null;
    }

    public async Task<bool> IsEmailAvailableAsync(string email)
    {
        _logger.LogDebug("Checking if email is available: {Email}", email);
        var user = await _userManager.FindByEmailAsync(email);
        return user == null;
    }

    public async Task<bool> ValidateUserCredentialsAsync(string username, string password)
    {
        _logger.LogDebug("Validating credentials for user: {Username}", username);
        var user = await AuthenticateAsync(username, password);
        return user != null;
    }

    // Account management
    public async Task<bool> LockUserAccountAsync(Guid userId)
    {
        _logger.LogInformation("Locking user account: {UserId}", userId);
        
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return false;
        }

        var result = await _userManager.SetLockoutEnabledAsync(user, true);
        if (result.Succeeded)
        {
            result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        }
        
        return result.Succeeded;
    }

    public async Task<bool> UnlockUserAccountAsync(Guid userId)
    {
        _logger.LogInformation("Unlocking user account: {UserId}", userId);
        
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return false;
        }

        var result = await _userManager.SetLockoutEndDateAsync(user, null);
        return result.Succeeded;
    }

    public async Task<bool> IsUserAccountLockedAsync(Guid userId)
    {
        _logger.LogDebug("Checking if user account is locked: {UserId}", userId);
        
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return false;
        }

        return await _userManager.IsLockedOutAsync(user);
    }

    public async Task<bool> ActivateUserAsync(Guid userId)
    {
        _logger.LogDebug("Activating user: {UserId}", userId);
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;
        
        user.IsActive = true;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> DeactivateUserAsync(Guid userId)
    {
        _logger.LogDebug("Deactivating user: {UserId}", userId);
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;
        
        user.IsActive = false;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }
}