using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RadiusConnect.Api.Models.Domain;

[Table("app_settings")]
public class AppSettings
{
    [Key]
    [Column("key")]
    [MaxLength(100)]
    public string Key { get; set; } = string.Empty;

    [Required]
    [Column("value")]
    public string Value { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("category")]
    [MaxLength(50)]
    public string? Category { get; set; }

    [Column("is_encrypted")]
    public bool IsEncrypted { get; set; } = false;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_by")]
    public Guid? UpdatedBy { get; set; }

    // Navigation properties
    [ForeignKey(nameof(UpdatedBy))]
    public virtual AppUser? UpdatedByUser { get; set; }
}