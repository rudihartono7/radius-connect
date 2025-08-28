using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using RadiusConnect.Api.Services.Interfaces;
using RadiusConnect.Api.Models.Domain;

namespace RadiusConnect.Api.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtService> _logger;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessTokenLifetimeMinutes;
    private readonly int _refreshTokenLifetimeDays;
    private readonly HashSet<string> _blacklistedTokens = new();
    private readonly Dictionary<string, string> _refreshTokens = new();

    public JwtService(
        IConfiguration configuration,
        ILogger<JwtService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        _secretKey = _configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        _issuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
        _audience = _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");
        _accessTokenLifetimeMinutes = int.Parse(_configuration["Jwt:AccessTokenLifetimeMinutes"] ?? "15");
        _refreshTokenLifetimeDays = int.Parse(_configuration["Jwt:RefreshTokenLifetimeDays"] ?? "7");
    }

    public async Task<string> GenerateAccessTokenAsync(AppUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new("firstName", user.FirstName ?? string.Empty),
            new("lastName", user.LastName ?? string.Empty),
            new("isActive", user.IsActive.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        // Add role claims
        if (user.UserRoles != null)
        {
            foreach (var userRole in user.UserRoles)
            {
                if (userRole.Role?.Name != null)
                {
                    claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
                }
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_accessTokenLifetimeMinutes);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        
        _logger.LogInformation("Access token generated for user {UserId}", user.Id);
        return await Task.FromResult(tokenString);
    }

    public async Task<string> GenerateRefreshTokenAsync(AppUser user)
    {
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        
        // Store refresh token in memory (in production, use database)
        _refreshTokens[refreshToken] = user.Id.ToString();
        
        _logger.LogInformation("Refresh token generated for user {UserId}", user.Id);
        return await Task.FromResult(refreshToken);
    }

    public async Task<(string accessToken, string refreshToken)> GenerateTokenPairAsync(AppUser user)
    {
        var accessToken = await GenerateAccessTokenAsync(user);
        var refreshToken = await GenerateRefreshTokenAsync(user);
        return (accessToken, refreshToken);
    }

    public async Task<ClaimsPrincipal?> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);
            
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            
            // Check if token is blacklisted
            var jti = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            if (!string.IsNullOrEmpty(jti) && _blacklistedTokens.Contains(jti))
            {
                _logger.LogWarning("Blacklisted token validation attempted: {Jti}", jti);
                return null;
            }

            return await Task.FromResult(principal);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return await Task.FromResult<ClaimsPrincipal?>(null);
        }
    }

    public async Task<bool> IsTokenValidAsync(string token)
    {
        var principal = await ValidateTokenAsync(token);
        return principal != null;
    }

    public async Task<bool> IsTokenExpiredAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);
            return await Task.FromResult(jsonToken.ValidTo < DateTime.UtcNow);
        }
        catch
        {
            return await Task.FromResult(true);
        }
    }

    public async Task<DateTime?> GetTokenExpirationAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);
            return await Task.FromResult<DateTime?>(jsonToken.ValidTo);
        }
        catch
        {
            return await Task.FromResult<DateTime?>(null);
        }
    }

    public async Task<(string accessToken, string refreshToken)?> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            // Validate refresh token
            if (!_refreshTokens.ContainsKey(refreshToken))
            {
                _logger.LogWarning("Invalid refresh token");
                return null;
            }

            var userId = _refreshTokens[refreshToken];
            
            // In a real implementation, you would fetch the user from database
            // For now, return null as we need user data to generate new tokens
            _logger.LogWarning("Refresh token validation requires user data from database");
            return await Task.FromResult<(string, string)?>(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return await Task.FromResult<(string, string)?>(null);
        }
    }

    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
    {
        var removed = _refreshTokens.Remove(refreshToken);
        _logger.LogInformation("Refresh token revoked: {Success}", removed);
        return await Task.FromResult(removed);
    }

    public async Task<bool> RevokeAllUserTokensAsync(Guid userId)
    {
        var userIdString = userId.ToString();
        var tokensToRemove = _refreshTokens.Where(kvp => kvp.Value == userIdString).Select(kvp => kvp.Key).ToList();
        
        foreach (var token in tokensToRemove)
        {
            _refreshTokens.Remove(token);
        }
        
        _logger.LogInformation("All tokens revoked for user {UserId}, count: {Count}", userId, tokensToRemove.Count);
        return await Task.FromResult(tokensToRemove.Count > 0);
    }

    public async Task<Guid?> GetUserIdFromTokenAsync(string token)
    {
        var principal = await ValidateTokenAsync(token);
        var userIdClaim = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        
        return null;
    }

    public async Task<string?> GetUsernameFromTokenAsync(string token)
    {
        var principal = await ValidateTokenAsync(token);
        return principal?.FindFirst(ClaimTypes.Name)?.Value;
    }

    public async Task<IEnumerable<string>> GetRolesFromTokenAsync(string token)
    {
        var principal = await ValidateTokenAsync(token);
        var roles = principal?.FindAll(ClaimTypes.Role)?.Select(c => c.Value) ?? Enumerable.Empty<string>();
        return await Task.FromResult(roles);
    }

    public async Task<Dictionary<string, string>> GetClaimsFromTokenAsync(string token)
    {
        var principal = await ValidateTokenAsync(token);
        var claims = principal?.Claims?.ToDictionary(c => c.Type, c => c.Value) ?? new Dictionary<string, string>();
        return await Task.FromResult(claims);
    }

    public async Task<bool> BlacklistTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);
            var jti = jsonToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;
            
            if (!string.IsNullOrEmpty(jti))
            {
                _blacklistedTokens.Add(jti);
                _logger.LogInformation("Token blacklisted: {Jti}", jti);
                return await Task.FromResult(true);
            }
            
            return await Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error blacklisting token");
            return await Task.FromResult(false);
        }
    }

    public async Task<bool> IsTokenBlacklistedAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);
            var jti = jsonToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;
            
            return await Task.FromResult(!string.IsNullOrEmpty(jti) && _blacklistedTokens.Contains(jti));
        }
        catch
        {
            return await Task.FromResult(false);
        }
    }

    public async Task CleanupExpiredTokensAsync()
    {
        // In a real implementation, you would clean up expired tokens from database
        // For in-memory implementation, we don't need to do anything special
        _logger.LogInformation("Token cleanup completed");
        await Task.CompletedTask;
    }

    public async Task<IEnumerable<string>> GetActiveTokensForUserAsync(Guid userId)
    {
        var userIdString = userId.ToString();
        var userTokens = _refreshTokens.Where(kvp => kvp.Value == userIdString).Select(kvp => kvp.Key);
        return await Task.FromResult(userTokens);
    }

    public async Task<string> GenerateSecureRandomTokenAsync(int length = 32)
    {
        var randomBytes = RandomNumberGenerator.GetBytes(length);
        return await Task.FromResult(Convert.ToBase64String(randomBytes));
    }

    public async Task<bool> ValidateTokenSignatureAsync(string token)
    {
        var isValid = await IsTokenValidAsync(token);
        return isValid;
    }

    public async Task<string> GetTokenFingerprintAsync(string token)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        return await Task.FromResult(Convert.ToBase64String(hashBytes));
    }

    public TimeSpan GetAccessTokenLifetime()
    {
        return TimeSpan.FromMinutes(_accessTokenLifetimeMinutes);
    }

    public TimeSpan GetRefreshTokenLifetime()
    {
        return TimeSpan.FromDays(_refreshTokenLifetimeDays);
    }

    public string GetTokenIssuer()
    {
        return _issuer;
    }

    public string GetTokenAudience()
    {
        return _audience;
    }
}