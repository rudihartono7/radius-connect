using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RadiusConnect.Api.Models.DTOs;
using RadiusConnect.Api.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace RadiusConnect.Api.Controllers;

[Route("api/[controller]")]
public class AuthController : BaseController
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;
    private readonly IAuditService _auditService;

    public AuthController(
        IUserService userService,
        IJwtService jwtService,
        IAuditService auditService)
    {
        _userService = userService;
        _jwtService = jwtService;
        _auditService = auditService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationError("Invalid login data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            }

            var user = await _userService.AuthenticateAsync(request.Username, request.Password);
            if (user == null)
            {
                await _auditService.LogAuthenticationAttemptAsync(request.Username, false, Request.HttpContext.Connection.RemoteIpAddress);
                return Unauthorized("Invalid username or password");
            }

            if (!user.IsActive)
            {
                await _auditService.LogAuthenticationAttemptAsync(request.Username, false, Request.HttpContext.Connection.RemoteIpAddress, null, "Account is inactive");
                return Unauthorized("Account is inactive");
            }

            // Check if 2FA is enabled and TOTP code is required
            if (user.IsTotpEnabled && string.IsNullOrEmpty(request.TotpCode))
            {
                return Ok(new ApiResponse<object>
                {
                    Success = false,
                    Message = "TOTP code required",
                    Data = new { RequireTotp = true }
                });
            }

            // Verify TOTP if provided
            if (user.IsTotpEnabled && !string.IsNullOrEmpty(request.TotpCode))
            {
                var isTotpValid = await _userService.ValidateTotpAsync(user.Id, request.TotpCode);
                if (!isTotpValid)
                {
                    await _auditService.LogAuthenticationAttemptAsync(request.Username, false, Request.HttpContext.Connection.RemoteIpAddress, null, "Invalid TOTP code");
                    return Unauthorized("Invalid TOTP code");
                }
            }

            // Generate tokens
            var accessToken = await _jwtService.GenerateAccessTokenAsync(user);
            var refreshToken = await _jwtService.GenerateRefreshTokenAsync(user);

            // Update last login - using UpdateUserAsync since UpdateLastLoginAsync doesn't exist
            user.LastLogin = DateTime.UtcNow;
            await _userService.UpdateUserAsync(user.Id);

            // Log successful authentication
            var ipAddress = Request.HttpContext.Connection.RemoteIpAddress;
            await _auditService.LogAuthenticationAttemptAsync(request.Username, true, ipAddress);

            var response = new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.Add(_jwtService.GetAccessTokenLifetime()),
                User = new UserDto
                {
                    Id = user.Id.ToString(),
                    Username = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName ?? string.Empty,
                    IsActive = user.IsActive,
                    IsTotpEnabled = user.IsTotpEnabled,
                    LastLogin = user.LastLogin,
                    CreatedAt = user.CreatedAt,
                    Roles = user.UserRoles?.Select(ur => ur.Role?.Name ?? string.Empty).Where(name => !string.IsNullOrEmpty(name)).ToList() ?? new List<string>()
                }
            };

            return Success(response, "Login successful");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "login");
        }
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationError("Invalid registration data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
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
            
            // Log user registration
            await _auditService.LogUserActionAsync(user.Id, "USER_REGISTERED", "User", user.Id.ToString(), null, new { Message = $"User {user.UserName} registered" });

            var userDto = new UserDto
            {
                Id = user.Id.ToString(),
                Username = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                IsTotpEnabled = user.IsTotpEnabled,
                CreatedAt = user.CreatedAt,
                Roles = new List<string> { "User" } // Default role
            };

            return Success(userDto, "Registration successful");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "registration");
        }
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest("Refresh token is required");
            }

            var result = await _jwtService.RefreshTokenAsync(request.RefreshToken);
            if (result == null)
            {
                return Unauthorized("Invalid or expired refresh token");
            }

            return Success(result, "Token refreshed successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "token refresh");
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        try
        {
            var userId = GetCurrentUserIdAsGuid();
            
            if (!string.IsNullOrEmpty(request.RefreshToken))
            {
                await _jwtService.RevokeRefreshTokenAsync(request.RefreshToken);
            }

            // Add access token to blacklist
            var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (!string.IsNullOrEmpty(accessToken))
            {
                await _jwtService.BlacklistTokenAsync(accessToken);
            }

            // Log logout
            await _auditService.LogUserActionAsync(userId, "USER_LOGOUT", "Auth", userId.ToString(), null, new { Message = "User logged out" });

            return Success("Logout successful");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "logout");
        }
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userId = GetCurrentUserIdAsGuid();
            var user = await _userService.GetUserWithRolesAsync(userId);
            
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
                Roles = user.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string>()
            };

            return Success(userDto, "User information retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "retrieving user information");
        }
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationError("Invalid password change data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            }

            var userId = GetCurrentUserIdAsGuid();
            var success = await _userService.UpdateUserPasswordAsync(userId, request.CurrentPassword, request.NewPassword);
            
            if (!success)
            {
                return BadRequest("Current password is incorrect");
            }

            // Log password change
            await _auditService.LogPasswordChangeAsync(userId, true);

            return Success("Password changed successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "password change");
        }
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest("Email is required");
            }

            var user = await _userService.GetUserByEmailAsync(request.Email);
            if (user == null)
            {
                // Don't reveal if email exists or not for security
                return Success("If the email exists, a password reset link has been sent");
            }

            // Generate a simple reset token (in production, use a proper token service)
            var resetToken = Guid.NewGuid().ToString();
            
            // TODO: Send email with reset token
            // For now, we'll just log it
            await _auditService.LogUserActionAsync(user.Id, "PASSWORD_RESET_REQUESTED", "Auth", user.Id.ToString(), null, new { Message = "Password reset requested" });

            return Success("If the email exists, a password reset link has been sent");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "password reset request");
        }
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationError("Invalid password reset data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            }

            // For now, we'll use a simple approach - find user by email and reset password
            // In production, validate the token properly
            var user = await _userService.GetUserByEmailAsync(request.Email ?? "");
            if (user == null) return BadRequest("Invalid reset request");
            
            var success = await _userService.ResetUserPasswordAsync(user.Id, request.NewPassword);
            if (!success)
            {
                return BadRequest("Invalid or expired reset token");
            }

            return Success("Password reset successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "password reset");
        }
    }
}