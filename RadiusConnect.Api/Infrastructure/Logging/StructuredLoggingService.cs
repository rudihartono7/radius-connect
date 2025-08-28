using Serilog;
using Serilog.Context;
using System.Security.Claims;

namespace RadiusConnect.Api.Infrastructure.Logging;

public interface IStructuredLoggingService
{
    void LogUserAction(string userId, string action, string resource, object? details = null);
    void LogSecurityEvent(string eventType, string description, string? userId = null, object? details = null);
    void LogPerformanceMetric(string operation, TimeSpan duration, bool success, object? details = null);
    void LogApiRequest(string method, string path, int statusCode, TimeSpan duration, string? userId = null);
    void LogError(Exception exception, string context, object? details = null);
    void LogBusinessEvent(string eventType, string description, object? details = null);
    IDisposable BeginScope(string operationName, object? properties = null);
}

public class StructuredLoggingService : IStructuredLoggingService
{
    private readonly ILogger<StructuredLoggingService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public StructuredLoggingService(
        ILogger<StructuredLoggingService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public void LogUserAction(string userId, string action, string resource, object? details = null)
    {
        var context = GetRequestContext();
        
        _logger.LogInformation(
            "User action performed: {Action} on {Resource} by user {UserId}",
            action, resource, userId);

        // Use Serilog structured logging
        Log.ForContext("EventType", "UserAction")
           .ForContext("UserId", userId)
           .ForContext("Action", action)
           .ForContext("Resource", resource)
           .ForContext("Details", details, true)
           .ForContext("RequestContext", context, true)
           .Information("User {UserId} performed {Action} on {Resource}", userId, action, resource);
    }

    public void LogSecurityEvent(string eventType, string description, string? userId = null, object? details = null)
    {
        var context = GetRequestContext();
        
        _logger.LogWarning(
            "Security event: {EventType} - {Description} for user {UserId}",
            eventType, description, userId ?? "unknown");

        Log.ForContext("EventType", "SecurityEvent")
           .ForContext("SecurityEventType", eventType)
           .ForContext("UserId", userId)
           .ForContext("Description", description)
           .ForContext("Details", details, true)
           .ForContext("RequestContext", context, true)
           .Warning("Security event {EventType}: {Description}", eventType, description);
    }

    public void LogPerformanceMetric(string operation, TimeSpan duration, bool success, object? details = null)
    {
        var context = GetRequestContext();
        
        if (duration.TotalMilliseconds > 1000) // Log slow operations
        {
            _logger.LogWarning(
                "Slow operation detected: {Operation} took {Duration}ms",
                operation, duration.TotalMilliseconds);
        }
        else
        {
            _logger.LogDebug(
                "Operation completed: {Operation} took {Duration}ms, Success: {Success}",
                operation, duration.TotalMilliseconds, success);
        }

        Log.ForContext("EventType", "PerformanceMetric")
           .ForContext("Operation", operation)
           .ForContext("DurationMs", duration.TotalMilliseconds)
           .ForContext("Success", success)
           .ForContext("Details", details, true)
           .ForContext("RequestContext", context, true)
           .Information("Operation {Operation} completed in {Duration}ms, Success: {Success}", 
               operation, duration.TotalMilliseconds, success);
    }

    public void LogApiRequest(string method, string path, int statusCode, TimeSpan duration, string? userId = null)
    {
        var context = GetRequestContext();
        
        var logLevel = statusCode >= 500 ? LogLevel.Error :
                      statusCode >= 400 ? LogLevel.Warning :
                      LogLevel.Information;

        _logger.Log(logLevel,
            "API Request: {Method} {Path} responded with {StatusCode} in {Duration}ms for user {UserId}",
            method, path, statusCode, duration.TotalMilliseconds, userId ?? "anonymous");

        Log.ForContext("EventType", "ApiRequest")
           .ForContext("HttpMethod", method)
           .ForContext("Path", path)
           .ForContext("StatusCode", statusCode)
           .ForContext("DurationMs", duration.TotalMilliseconds)
           .ForContext("UserId", userId)
           .ForContext("RequestContext", context, true)
           .Information("API {Method} {Path} -> {StatusCode} ({Duration}ms)", 
               method, path, statusCode, duration.TotalMilliseconds);
    }

    public void LogError(Exception exception, string context, object? details = null)
    {
        var requestContext = GetRequestContext();
        
        _logger.LogError(exception,
            "Error occurred in context: {Context}",
            context);

        Log.ForContext("EventType", "Error")
           .ForContext("Context", context)
           .ForContext("ExceptionType", exception.GetType().Name)
           .ForContext("Details", details, true)
           .ForContext("RequestContext", requestContext, true)
           .Error(exception, "Error in {Context}: {Message}", context, exception.Message);
    }

    public void LogBusinessEvent(string eventType, string description, object? details = null)
    {
        var context = GetRequestContext();
        
        _logger.LogInformation(
            "Business event: {EventType} - {Description}",
            eventType, description);

        Log.ForContext("EventType", "BusinessEvent")
           .ForContext("BusinessEventType", eventType)
           .ForContext("Description", description)
           .ForContext("Details", details, true)
           .ForContext("RequestContext", context, true)
           .Information("Business event {EventType}: {Description}", eventType, description);
    }

    public IDisposable BeginScope(string operationName, object? properties = null)
    {
        var scopeProperties = new Dictionary<string, object>
        {
            ["OperationName"] = operationName,
            ["OperationId"] = Guid.NewGuid().ToString(),
            ["StartTime"] = DateTime.UtcNow
        };

        if (properties != null)
        {
            foreach (var prop in properties.GetType().GetProperties())
            {
                scopeProperties[prop.Name] = prop.GetValue(properties) ?? "null";
            }
        }

        return LogContext.PushProperty("OperationScope", scopeProperties, true);
    }

    private object GetRequestContext()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return new { Source = "Background" };
        }

        var user = httpContext.User;
        var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = user?.FindFirst(ClaimTypes.Name)?.Value;
        var roles = user?.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToArray();

        return new
        {
            RequestId = httpContext.TraceIdentifier,
            Method = httpContext.Request.Method,
            Path = httpContext.Request.Path.Value,
            QueryString = httpContext.Request.QueryString.Value,
            UserAgent = httpContext.Request.Headers.UserAgent.ToString(),
            IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
            UserId = userId,
            Username = username,
            Roles = roles,
            Timestamp = DateTime.UtcNow
        };
    }
}

// Extension methods for easier logging
public static class StructuredLoggingExtensions
{
    public static IServiceCollection AddStructuredLogging(this IServiceCollection services)
    {
        services.AddScoped<IStructuredLoggingService, StructuredLoggingService>();
        services.AddHttpContextAccessor();
        return services;
    }
}