using RadiusConnect.Api.Models.Domain;
using System.Security.Claims;

namespace RadiusConnect.Api.Services.Interfaces;

public interface IJwtService
{
    // Token generation
    Task<string> GenerateAccessTokenAsync(AppUser user);
    Task<string> GenerateRefreshTokenAsync(AppUser user);
    Task<(string accessToken, string refreshToken)> GenerateTokenPairAsync(AppUser user);

    // Token validation
    Task<ClaimsPrincipal?> ValidateTokenAsync(string token);
    Task<bool> IsTokenValidAsync(string token);
    Task<bool> IsTokenExpiredAsync(string token);
    Task<DateTime?> GetTokenExpirationAsync(string token);

    // Token refresh
    Task<(string accessToken, string refreshToken)?> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeRefreshTokenAsync(string refreshToken);
    Task<bool> RevokeAllUserTokensAsync(Guid userId);

    // Token claims
    Task<Guid?> GetUserIdFromTokenAsync(string token);
    Task<string?> GetUsernameFromTokenAsync(string token);
    Task<IEnumerable<string>> GetRolesFromTokenAsync(string token);
    Task<Dictionary<string, string>> GetClaimsFromTokenAsync(string token);

    // Token management
    Task<bool> BlacklistTokenAsync(string token);
    Task<bool> IsTokenBlacklistedAsync(string token);
    Task CleanupExpiredTokensAsync();
    Task<IEnumerable<string>> GetActiveTokensForUserAsync(Guid userId);

    // Security
    Task<string> GenerateSecureRandomTokenAsync(int length = 32);
    Task<bool> ValidateTokenSignatureAsync(string token);
    Task<string> GetTokenFingerprintAsync(string token);

    // Configuration
    TimeSpan GetAccessTokenLifetime();
    TimeSpan GetRefreshTokenLifetime();
    string GetTokenIssuer();
    string GetTokenAudience();
}