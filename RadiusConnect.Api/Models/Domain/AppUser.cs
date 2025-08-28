using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace RadiusConnect.Api.Models.Domain;

[Table("app_users")]
public class AppUser : IdentityUser<Guid>
{
    [Column("first_name")]
    [MaxLength(100)]
    public string? FirstName { get; set; }

    [Column("last_name")]
    [MaxLength(100)]
    public string? LastName { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("totp_enabled")]
    public bool TotpEnabled { get; set; } = false;

    [Column("is_totp_enabled")]
    public bool IsTotpEnabled { get; set; } = false;

    // Alias for TwoFactorEnabled (used by UserService)
    [NotMapped]
    public bool TwoFactorEnabled 
    { 
        get => IsTotpEnabled; 
        set => IsTotpEnabled = value; 
    }

    [Column("totp_secret")]
    [MaxLength(32)]
    public string? TotpSecret { get; set; }

    [Column("last_login")]
    public DateTime? LastLogin { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<RbacUserRole> UserRoles { get; set; } = new List<RbacUserRole>();
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}