using Microsoft.AspNetCore.Authorization;

namespace RadiusConnect.Api.Infrastructure.Authorization;

public class RoleRequirement : IAuthorizationRequirement
{
    public string[] AllowedRoles { get; }

    public RoleRequirement(params string[] allowedRoles)
    {
        AllowedRoles = allowedRoles ?? throw new ArgumentNullException(nameof(allowedRoles));
    }
}

public class RoleRequirementHandler : AuthorizationHandler<RoleRequirement>
{
    private readonly ILogger<RoleRequirementHandler> _logger;

    public RoleRequirementHandler(ILogger<RoleRequirementHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RoleRequirement requirement)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            _logger.LogWarning("User is not authenticated");
            context.Fail();
            return Task.CompletedTask;
        }

        var userRoles = context.User.Claims
            .Where(c => c.Type == "role" || c.Type == System.Security.Claims.ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        if (!userRoles.Any())
        {
            _logger.LogWarning("User {UserId} has no roles assigned", 
                context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            context.Fail();
            return Task.CompletedTask;
        }

        var hasRequiredRole = requirement.AllowedRoles.Any(role => 
            userRoles.Contains(role, StringComparer.OrdinalIgnoreCase));

        if (hasRequiredRole)
        {
            _logger.LogDebug("User {UserId} has required role(s): {RequiredRoles}", 
                context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                string.Join(", ", requirement.AllowedRoles));
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("User {UserId} does not have required role(s): {RequiredRoles}. User roles: {UserRoles}",
                context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                string.Join(", ", requirement.AllowedRoles),
                string.Join(", ", userRoles));
            context.Fail();
        }

        return Task.CompletedTask;
    }
}

// Extension methods for easier policy configuration
public static class AuthorizationPolicyBuilderExtensions
{
    public static AuthorizationPolicyBuilder RequireRoles(this AuthorizationPolicyBuilder builder, params string[] roles)
    {
        return builder.AddRequirements(new RoleRequirement(roles));
    }
}