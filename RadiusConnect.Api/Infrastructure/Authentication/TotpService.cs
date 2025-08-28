using System.Security.Cryptography;
using System.Text;
using RadiusConnect.Api.Services.Interfaces;

namespace RadiusConnect.Api.Infrastructure.Authentication;

public interface ITotpService
{
    string GenerateSecret();
    string GenerateQrCodeUri(string email, string secret, string issuer = "RadiusConnect");
    bool ValidateTotp(string secret, string code, int windowSize = 1);
    string GenerateTotp(string secret, DateTime? timestamp = null);
}

public class TotpService : ITotpService
{
    private const int SecretLength = 32;
    private const int CodeLength = 6;
    private const int TimeStep = 30; // 30 seconds
    private const long UnixEpoch = 621355968000000000L;

    public string GenerateSecret()
    {
        var buffer = new byte[SecretLength];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(buffer);
        return Convert.ToBase64String(buffer).TrimEnd('=');
    }

    public string GenerateQrCodeUri(string email, string secret, string issuer = "RadiusConnect")
    {
        var encodedIssuer = Uri.EscapeDataString(issuer);
        var encodedEmail = Uri.EscapeDataString(email);
        var encodedSecret = Uri.EscapeDataString(secret);
        
        return $"otpauth://totp/{encodedIssuer}:{encodedEmail}?secret={encodedSecret}&issuer={encodedIssuer}&digits={CodeLength}&period={TimeStep}";
    }

    public bool ValidateTotp(string secret, string code, int windowSize = 1)
    {
        if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(code))
            return false;

        if (code.Length != CodeLength || !code.All(char.IsDigit))
            return false;

        var currentTimestamp = GetCurrentTimestamp();
        
        // Check current time window and adjacent windows
        for (int i = -windowSize; i <= windowSize; i++)
        {
            var timestamp = currentTimestamp + i;
            var expectedCode = GenerateTotp(secret, DateTimeOffset.FromUnixTimeSeconds(timestamp * TimeStep).DateTime);
            
            if (code == expectedCode)
                return true;
        }

        return false;
    }

    public string GenerateTotp(string secret, DateTime? timestamp = null)
    {
        var time = timestamp ?? DateTime.UtcNow;
        var unixTime = GetTimestamp(time);
        
        var secretBytes = Convert.FromBase64String(PadBase32(secret));
        var timeBytes = BitConverter.GetBytes(unixTime);
        
        if (BitConverter.IsLittleEndian)
            Array.Reverse(timeBytes);

        using var hmac = new HMACSHA1(secretBytes);
        var hash = hmac.ComputeHash(timeBytes);
        
        var offset = hash[hash.Length - 1] & 0x0F;
        var binaryCode = (hash[offset] & 0x7F) << 24
                       | (hash[offset + 1] & 0xFF) << 16
                       | (hash[offset + 2] & 0xFF) << 8
                       | (hash[offset + 3] & 0xFF);
        
        var code = binaryCode % (int)Math.Pow(10, CodeLength);
        return code.ToString().PadLeft(CodeLength, '0');
    }

    private static long GetCurrentTimestamp()
    {
        return GetTimestamp(DateTime.UtcNow);
    }

    private static long GetTimestamp(DateTime dateTime)
    {
        var unixTime = (dateTime.Ticks - UnixEpoch) / TimeSpan.TicksPerSecond;
        return unixTime / TimeStep;
    }

    private static string PadBase32(string base32)
    {
        var padding = (8 - (base32.Length % 8)) % 8;
        return base32 + new string('=', padding);
    }
}

// Base32 encoding extension methods
public static class Base32Extensions
{
    private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
    
    public static string ToBase32String(this byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
            return string.Empty;

        var result = new StringBuilder();
        var buffer = 0;
        var bitsLeft = 0;
        
        foreach (var b in bytes)
        {
            buffer = (buffer << 8) | b;
            bitsLeft += 8;
            
            while (bitsLeft >= 5)
            {
                var index = (buffer >> (bitsLeft - 5)) & 0x1F;
                result.Append(Base32Alphabet[index]);
                bitsLeft -= 5;
            }
        }
        
        if (bitsLeft > 0)
        {
            var index = (buffer << (5 - bitsLeft)) & 0x1F;
            result.Append(Base32Alphabet[index]);
        }
        
        return result.ToString();
    }
    
    public static byte[] FromBase32String(this string base32)
    {
        if (string.IsNullOrEmpty(base32))
            return Array.Empty<byte>();

        base32 = base32.ToUpperInvariant().TrimEnd('=');
        var result = new List<byte>();
        var buffer = 0;
        var bitsLeft = 0;
        
        foreach (var c in base32)
        {
            var index = Base32Alphabet.IndexOf(c);
            if (index < 0)
                throw new ArgumentException($"Invalid Base32 character: {c}");
                
            buffer = (buffer << 5) | index;
            bitsLeft += 5;
            
            if (bitsLeft >= 8)
            {
                result.Add((byte)(buffer >> (bitsLeft - 8)));
                bitsLeft -= 8;
            }
        }
        
        return result.ToArray();
    }
}