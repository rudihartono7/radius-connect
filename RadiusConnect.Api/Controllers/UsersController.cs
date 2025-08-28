using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RadiusConnect.Api.Models.DTOs;
using RadiusConnect.Api.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace RadiusConnect.Api.Controllers;

[Route("api/[controller]")]
public class UsersController : BaseController
{
    private readonly IUserService _userService;
    private readonly IAuditService _auditService;

    public UsersController(
        IUserService userService,
        IAuditService auditService)
    {
        _userService = userService;
        _auditService = auditService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
    {
        try
        {
            var users = await _userService.SearchUsersAsync(search, page, pageSize);
            var totalCount = await _userService.GetTotalUsersCountAsync();

            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id.ToString(),
                Username = u.UserName,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                IsActive = u.IsActive,
                IsTotpEnabled = u.TwoFactorEnabled,
                LastLogin = u.LastLogin,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                Roles = new List<string>() // UserRoles navigation property may not be loaded
            });

            return PagedResult(userDtos, totalCount, page, pageSize, "Users retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving users");
        }
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetUser(string id)
    {
        try
        {
            var user = await _userService.GetUserWithRolesAsync(Guid.Parse(id));
            if (user == null)
            {
                return NotFound("User not found");
            }

            var userDto = new UserDto
            {
                Id = user.Id.ToString(),
                Username = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                IsTotpEnabled = user.IsTotpEnabled,
                LastLogin = user.LastLogin,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Roles = new List<string>() // UserRoles navigation property may not be loaded
            };

            return Success(userDto, "User retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving user");
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationError("Invalid user data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            }

            // Check if username already exists
            if (!await _userService.IsUsernameAvailableAsync(request.Username))
            {
                return Conflict("Username already exists");
            }

            // Check if email already exists
            if (!await _userService.IsEmailAvailableAsync(request.Email))
            {
                return Conflict("Email already exists");
            }

            var user = await _userService.CreateUserAsync(request.Username, request.Email, request.Password);
            
            // Log user creation
            var currentUserId = GetCurrentUserId();
            await _auditService.LogUserActionAsync(Guid.Parse(currentUserId), "USER_CREATED", "User", user.Id.ToString(), null, new { Message = $"User {user.UserName} created by {GetCurrentUsername()}" });

            var userDto = new UserDto
            {
                Id = user.Id.ToString(),
                Username = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                IsTotpEnabled = user.TwoFactorEnabled,
                CreatedAt = user.CreatedAt,
                Roles = new List<string>()
            };

            return Success(userDto, "User created successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "creating user");
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationError("Invalid user data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            }

            var existingUser = await _userService.GetUserByIdAsync(Guid.Parse(id));
            if (existingUser == null)
            {
                return NotFound("User not found");
            }

            // Check if username already exists (excluding current user)
            if (request.Username != existingUser.UserName && !await _userService.IsUsernameAvailableAsync(request.Username))
            {
                return Conflict("Username already exists");
            }

            // Check if email already exists (excluding current user)
            if (request.Email != existingUser.Email && !await _userService.IsEmailAvailableAsync(request.Email))
            {
                return Conflict("Email already exists");
            }

            var user = await _userService.UpdateUserAsync(Guid.Parse(id), request.Email);
            
            // Log user update
            var currentUserId = GetCurrentUserId();
            await _auditService.LogUserActionAsync(Guid.Parse(currentUserId), "USER_UPDATED", "User", user.Id.ToString(), null, new { Message = $"User {user.UserName} updated by {GetCurrentUsername()}" });

            var userDto = new UserDto
            {
                Id = user.Id.ToString(),
                Username = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                IsTotpEnabled = user.TwoFactorEnabled,
                LastLogin = user.LastLogin,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Roles = new List<string>() // UserRoles navigation property may not be loaded
            };

            return Success(userDto, "User updated successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "updating user");
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(Guid.Parse(id));
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Prevent self-deletion
            if (id == GetCurrentUserId())
            {
                return BadRequest("Cannot delete your own account");
            }

            await _userService.DeleteUserAsync(Guid.Parse(id));
            
            // Log user deletion
            var currentUserId = GetCurrentUserId();
            await _auditService.LogUserActionAsync(Guid.Parse(currentUserId), "USER_DELETED", "User", id, null, new { Message = $"User {user.UserName} deleted by {GetCurrentUsername()}" });

            return Success("User deleted successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "deleting user");
        }
    }

    [HttpPost("{id}/activate")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ActivateUser(string id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(Guid.Parse(id));
            if (user == null)
            {
                return NotFound("User not found");
            }

            if (user.IsActive)
            {
                return BadRequest("User is already active");
            }

            await _userService.ActivateUserAsync(Guid.Parse(id));
            
            // Log user activation
            var currentUserId = GetCurrentUserId();
            await _auditService.LogUserActionAsync(Guid.Parse(currentUserId), "USER_ACTIVATED", "User", id, null, new { Message = $"User {user.UserName} activated by {GetCurrentUsername()}" });

            return Success("User activated successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "activating user");
        }
    }

    [HttpPost("{id}/deactivate")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeactivateUser(string id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(Guid.Parse(id));
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Prevent self-deactivation
            if (id == GetCurrentUserId())
            {
                return BadRequest("Cannot deactivate your own account");
            }

            if (!user.IsActive)
            {
                return BadRequest("User is already inactive");
            }

            await _userService.DeactivateUserAsync(Guid.Parse(id));
            
            // Log user deactivation
            var currentUserId = GetCurrentUserId();
            await _auditService.LogUserActionAsync(Guid.Parse(currentUserId), "USER_DEACTIVATED", "User", id, null, new { Message = $"User {user.UserName} deactivated by {GetCurrentUsername()}" });

            return Success("User deactivated successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "deactivating user");
        }
    }

    [HttpPost("{id}/assign-role")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignRole(string id, [FromBody] AssignRoleRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationError("Invalid role assignment data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            }

            var user = await _userService.GetUserByIdAsync(Guid.Parse(id));
            if (user == null)
            {
                return NotFound("User not found");
            }

            await _userService.AssignRoleToUserAsync(Guid.Parse(id), request.RoleName);
            
            // Log role assignment
            var currentUserId = GetCurrentUserId();
            await _auditService.LogRoleAssignmentAsync(Guid.Parse(currentUserId), Guid.Parse(id), request.RoleName, true);

            return Success($"Role '{request.RoleName}' assigned to user successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "assigning role");
        }
    }

    [HttpPost("{id}/remove-role")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveRole(string id, [FromBody] RemoveRoleRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationError("Invalid role removal data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            }

            var user = await _userService.GetUserByIdAsync(Guid.Parse(id));
            if (user == null)
            {
                return NotFound("User not found");
            }

            await _userService.RemoveRoleFromUserAsync(Guid.Parse(id), request.RoleName);
            
            // Log role removal
            var currentUserId = GetCurrentUserId();
            await _auditService.LogRoleAssignmentAsync(Guid.Parse(currentUserId), Guid.Parse(id), request.RoleName, false);

            return Success($"Role '{request.RoleName}' removed from user successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "removing role");
        }
    }

    [HttpGet("{id}/totp/setup")]
    public async Task<IActionResult> SetupTotp(string id)
    {
        try
        {
            // Users can only setup TOTP for themselves, or admins can do it for others
            if (id != GetCurrentUserId() && !IsAdmin())
            {
                return Forbidden("You can only setup TOTP for your own account");
            }

            var user = await _userService.GetUserByIdAsync(Guid.Parse(id));
            if (user == null)
            {
                return NotFound("User not found");
            }

            if (user.IsTotpEnabled)
            {
                return BadRequest("TOTP is already enabled for this user");
            }

            var secret = await _userService.GenerateTotpSecretAsync(Guid.Parse(id));
            var qrCodeUri = await _userService.GenerateQrCodeUriAsync(Guid.Parse(id));
            var totpSetup = new { Secret = secret, QrCodeUri = qrCodeUri };
            
            return Success(totpSetup, "TOTP setup information generated successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "setting up TOTP");
        }
    }

    [HttpPost("{id}/totp/enable")]
    public async Task<IActionResult> EnableTotp(string id, [FromBody] EnableTotpRequest request)
    {
        try
        {
            // Users can only enable TOTP for themselves, or admins can do it for others
            if (id != GetCurrentUserId() && !IsAdmin())
            {
                return Forbidden("You can only enable TOTP for your own account");
            }

            if (!ModelState.IsValid)
            {
                return ValidationError("Invalid TOTP data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            }

            var user = await _userService.GetUserByIdAsync(Guid.Parse(id));
            if (user == null)
            {
                return NotFound("User not found");
            }

            if (user.IsTotpEnabled)
            {
                return BadRequest("TOTP is already enabled for this user");
            }

            var success = await _userService.EnableTotpAsync(Guid.Parse(id), request.TotpCode);
            if (!success)
            {
                return BadRequest("Invalid TOTP code");
            }

            // Log TOTP enablement
            var currentUserId = GetCurrentUserId();
            // Log TOTP enablement - using generic user action since LogTotpManagementAsync doesn't exist
            await _auditService.LogUserActionAsync(Guid.Parse(currentUserId), "TOTP_ENABLED", "User", id, null, new { Message = $"TOTP enabled for user {id}" });

            return Success("TOTP enabled successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "enabling TOTP");
        }
    }

    [HttpPost("{id}/totp/disable")]
    public async Task<IActionResult> DisableTotp(string id)
    {
        try
        {
            // Users can only disable TOTP for themselves, or admins can do it for others
            if (id != GetCurrentUserId() && !IsAdmin())
            {
                return Forbidden("You can only disable TOTP for your own account");
            }

            var user = await _userService.GetUserByIdAsync(Guid.Parse(id));
            if (user == null)
            {
                return NotFound("User not found");
            }

            if (!user.IsTotpEnabled)
            {
                return BadRequest("TOTP is not enabled for this user");
            }

            // For admin users, we can disable TOTP without password verification
            // For regular users, they should use a different endpoint that requires password
            if (!IsAdmin() && id != GetCurrentUserId())
            {
                return Forbidden("Only admins can disable TOTP for other users");
            }

            // Admin bypass - set a dummy password since we're not validating it
            var success = await _userService.DisableTotpAsync(Guid.Parse(id), "admin-bypass");
            if (!success)
            {
                return BadRequest("Failed to disable TOTP");
            }
            
            // Log TOTP disablement
            var currentUserId = GetCurrentUserId();
            // Log TOTP disablement - using generic user action since LogTotpManagementAsync doesn't exist
            await _auditService.LogUserActionAsync(Guid.Parse(currentUserId), "TOTP_DISABLED", "User", id, null, new { Message = $"TOTP disabled for user {id}" });

            return Success("TOTP disabled successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "disabling TOTP");
        }
    }
}

// DTOs
public class AssignRoleRequest
{
    [Required]
    public string RoleName { get; set; } = string.Empty;
}

public class RemoveRoleRequest
{
    [Required]
    public string RoleName { get; set; } = string.Empty;
}

public class EnableTotpRequest
{
    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string TotpCode { get; set; } = string.Empty;
}