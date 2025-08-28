using Microsoft.AspNetCore.Authorization;
using RadiusConnect.Api.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RadiusConnect.Api.Middleware;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtMiddleware> _logger;

    public JwtMiddleware(
        RequestDelegate next,
        ILogger<JwtMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = ExtractTokenFromRequest(context.Request);
        
        if (!string.IsNullOrEmpty(token))
        {
            var jwtService = context.RequestServices.GetRequiredService<IJwtService>();
            await AttachUserToContext(context, token, jwtService);
        }

        await _next(context);
    }

    private string? ExtractTokenFromRequest(HttpRequest request)
    {
        // Check Authorization header
        var authHeader = request.Headers["Authorization"].FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            return authHeader.Substring("Bearer ".Length).Trim();
        }

        // Check query parameter (for WebSocket connections, etc.)
        var tokenFromQuery = request.Query["access_token"].FirstOrDefault();
        if (!string.IsNullOrEmpty(tokenFromQuery))
        {
            return tokenFromQuery;
        }

        // Check cookie
        var tokenFromCookie = request.Cookies["access_token"];
        if (!string.IsNullOrEmpty(tokenFromCookie))
        {
            return tokenFromCookie;
        }

        return null;
    }

    private async Task AttachUserToContext(HttpContext context, string token, IJwtService jwtService)
    {
        try
        {
            // Check if token is blacklisted
            var isBlacklisted = await jwtService.IsTokenBlacklistedAsync(token);
            if (isBlacklisted)
            {
                _logger.LogWarning("Blacklisted token used from IP: {IpAddress}", context.Connection.RemoteIpAddress);
                return;
            }

            // Get principal from token (this also validates the token)
            var principal = await jwtService.ValidateTokenAsync(token);
            if (principal == null)
            {
                _logger.LogWarning("Invalid token provided from IP: {IpAddress}", context.Connection.RemoteIpAddress);
                return;
            }

            // Attach user to context
            {
                context.User = principal;
                
                // Add token to context for potential revocation
                context.Items["Token"] = token;
                
                // Log successful authentication
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var username = principal.FindFirst(ClaimTypes.Name)?.Value;
                
                _logger.LogDebug("User {Username} ({UserId}) authenticated successfully", username, userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing JWT token");
        }
    }
}

public static class JwtMiddlewareExtensions
{
    public static IApplicationBuilder UseJwtMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<JwtMiddleware>();
    }
}