using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace RadiusConnect.Api.Models.Domain;

[Table("policy_templates")]
public class PolicyTemplate
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("name")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("check_attributes", TypeName = "json")]
    public string CheckAttributesJson { get; set; } = "{}";

    [Column("reply_attributes", TypeName = "json")]
    public string ReplyAttributesJson { get; set; } = "{}";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("created_by")]
    public Guid? CreatedBy { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_by")]
    public Guid? UpdatedBy { get; set; }

    [NotMapped]
    public Dictionary<string, object> CheckAttributes
    {
        get => string.IsNullOrEmpty(CheckAttributesJson) 
            ? new Dictionary<string, object>() 
            : JsonSerializer.Deserialize<Dictionary<string, object>>(CheckAttributesJson) ?? new Dictionary<string, object>();
        set => CheckAttributesJson = JsonSerializer.Serialize(value);
    }

    [NotMapped]
    public Dictionary<string, object> ReplyAttributes
    {
        get => string.IsNullOrEmpty(ReplyAttributesJson) 
            ? new Dictionary<string, object>() 
            : JsonSerializer.Deserialize<Dictionary<string, object>>(ReplyAttributesJson) ?? new Dictionary<string, object>();
        set => ReplyAttributesJson = JsonSerializer.Serialize(value);
    }

    // Navigation properties
    [ForeignKey(nameof(CreatedBy))]
    public virtual AppUser? Creator { get; set; }

    [ForeignKey(nameof(UpdatedBy))]
    public virtual AppUser? Updater { get; set; }
}