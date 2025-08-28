using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using RadiusConnect.Api.Models.DTOs;

namespace RadiusConnect.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public abstract class BaseController : ControllerBase
{
    protected string GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    }

    protected Guid GetCurrentUserIdAsGuid()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdString, out var userId) ? userId : Guid.Empty;
    }

    protected string GetCurrentUsername()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
    }

    protected string GetCurrentUserEmail()
    {
        return User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
    }

    protected IEnumerable<string> GetCurrentUserRoles()
    {
        return User.FindAll(ClaimTypes.Role).Select(c => c.Value);
    }

    protected bool IsInRole(string role)
    {
        return User.IsInRole(role);
    }

    protected bool IsAdmin()
    {
        return IsInRole("Admin");
    }

    protected bool IsManager()
    {
        return IsInRole("Manager") || IsAdmin();
    }

    protected IActionResult HandleException(Exception ex, string operation)
    {
        // Log the exception here if needed
        return StatusCode(500, new ApiResponse<object>
        {
            Success = false,
            Message = $"An error occurred during {operation}",
            Data = null,
            Errors = new[] { ex.Message }
        });
    }

    protected IActionResult Success<T>(T data, string message = "Operation completed successfully")
    {
        return Ok(new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        });
    }

    protected IActionResult Success(string message = "Operation completed successfully")
    {
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = message,
            Data = null
        });
    }

    protected IActionResult BadRequest(string message, IEnumerable<string>? errors = null)
    {
        return BadRequest(new ApiResponse<object>
        {
            Success = false,
            Message = message,
            Data = null,
            Errors = errors
        });
    }

    protected IActionResult NotFound(string message = "Resource not found")
    {
        return NotFound(new ApiResponse<object>
        {
            Success = false,
            Message = message,
            Data = null
        });
    }

    protected IActionResult Unauthorized(string message = "Unauthorized access")
    {
        return Unauthorized(new ApiResponse<object>
        {
            Success = false,
            Message = message,
            Data = null
        });
    }

    protected IActionResult Forbidden(string message = "Access forbidden")
    {
        return StatusCode(403, new ApiResponse<object>
        {
            Success = false,
            Message = message,
            Data = null
        });
    }

    protected IActionResult Conflict(string message = "Resource conflict")
    {
        return Conflict(new ApiResponse<object>
        {
            Success = false,
            Message = message,
            Data = null
        });
    }

    protected IActionResult ValidationError(string message, IEnumerable<string> errors)
    {
        return BadRequest(new ApiResponse<object>
        {
            Success = false,
            Message = message,
            Data = null,
            Errors = errors
        });
    }

    protected IActionResult PagedResult<T>(IEnumerable<T> data, int totalCount, int page, int pageSize, string message = "Data retrieved successfully")
    {
        return Ok(new PagedApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }
}