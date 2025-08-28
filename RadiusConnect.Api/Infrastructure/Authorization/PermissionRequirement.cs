using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace RadiusConnect.Api.Infrastructure.Authorization;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission ?? throw new ArgumentNullException(nameof(permission));
    }
}

public class PermissionRequirementHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ILogger<PermissionRequirementHandler> _logger;

    public PermissionRequirementHandler(ILogger<PermissionRequirementHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            _logger.LogWarning("User is not authenticated");
            context.Fail();
            return Task.CompletedTask;
        }

        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRoles = context.User.Claims
            .Where(c => c.Type == "role" || c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        // Check if user has the specific permission claim
        var hasPermission = context.User.HasClaim("permission", requirement.Permission);
        
        if (hasPermission)
        {
            _logger.LogDebug("User {UserId} has required permission: {Permission}", userId, requirement.Permission);
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Check role-based permissions
        var hasRoleBasedPermission = CheckRoleBasedPermissions(userRoles, requirement.Permission);
        
        if (hasRoleBasedPermission)
        {
            _logger.LogDebug("User {UserId} has required permission through role: {Permission}", userId, requirement.Permission);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("User {UserId} does not have required permission: {Permission}. User roles: {UserRoles}",
                userId, requirement.Permission, string.Join(", ", userRoles));
            context.Fail();
        }

        return Task.CompletedTask;
    }

    private static bool CheckRoleBasedPermissions(List<string> userRoles, string requiredPermission)
    {
        // Define role-based permissions mapping
        var rolePermissions = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Admin"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "users.read", "users.create", "users.update", "users.delete",
                "groups.read", "groups.create", "groups.update", "groups.delete",
                "sessions.read", "sessions.manage", "sessions.disconnect",
                "radius.read", "radius.create", "radius.update", "radius.delete",
                "dashboard.read", "dashboard.manage",
                "audit.read", "audit.export", "audit.manage",
                "system.manage", "system.configure"
            },
            ["Manager"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "users.read", "users.create", "users.update",
                "groups.read", "groups.create", "groups.update",
                "sessions.read", "sessions.manage", "sessions.disconnect",
                "radius.read", "radius.create", "radius.update",
                "dashboard.read",
                "audit.read", "audit.export"
            },
            ["NOC"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "users.read", "users.update",
                "groups.read",
                "sessions.read", "sessions.manage", "sessions.disconnect",
                "radius.read",
                "dashboard.read",
                "audit.read"
            },
            ["Helpdesk"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "users.read", "users.update",
                "groups.read",
                "sessions.read",
                "radius.read",
                "dashboard.read"
            },
            ["Auditor"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "users.read",
                "groups.read",
                "sessions.read",
                "radius.read",
                "dashboard.read",
                "audit.read", "audit.export"
            },
            ["User"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "dashboard.read"
            }
        };

        return userRoles.Any(role => 
            rolePermissions.ContainsKey(role) && 
            rolePermissions[role].Contains(requiredPermission));
    }
}

// Extension methods for easier policy configuration
public static class PermissionPolicyBuilderExtensions
{
    public static AuthorizationPolicyBuilder RequirePermission(this AuthorizationPolicyBuilder builder, string permission)
    {
        return builder.AddRequirements(new PermissionRequirement(permission));
    }
}

// Static class for permission constants
public static class Permissions
{
    public static class Users
    {
        public const string Read = "users.read";
        public const string Create = "users.create";
        public const string Update = "users.update";
        public const string Delete = "users.delete";
    }

    public static class Groups
    {
        public const string Read = "groups.read";
        public const string Create = "groups.create";
        public const string Update = "groups.update";
        public const string Delete = "groups.delete";
    }

    public static class Sessions
    {
        public const string Read = "sessions.read";
        public const string Manage = "sessions.manage";
        public const string Disconnect = "sessions.disconnect";
    }

    public static class Radius
    {
        public const string Read = "radius.read";
        public const string Create = "radius.create";
        public const string Update = "radius.update";
        public const string Delete = "radius.delete";
    }

    public static class Dashboard
    {
        public const string Read = "dashboard.read";
        public const string Manage = "dashboard.manage";
    }

    public static class Audit
    {
        public const string Read = "audit.read";
        public const string Export = "audit.export";
        public const string Manage = "audit.manage";
    }

    public static class System
    {
        public const string Manage = "system.manage";
        public const string Configure = "system.configure";
    }
}