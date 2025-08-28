using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Text.Json;

namespace RadiusConnect.Api.Models.Domain;

[Table("audit_log")]
public class AuditLog
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("actor_id")]
    public Guid? ActorId { get; set; }

    [Required]
    [Column("action")]
    [MaxLength(50)]
    public string Action { get; set; } = string.Empty;

    [Required]
    [Column("entity")]
    [MaxLength(50)]
    public string Entity { get; set; } = string.Empty;

    [Column("entity_id")]
    [MaxLength(255)]
    public string? EntityId { get; set; }

    [Column("before_data", TypeName = "json")]
    public string? BeforeDataJson { get; set; }

    [Column("after_data", TypeName = "json")]
    public string? AfterDataJson { get; set; }

    [Column("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [Column("ip_address")]
    public string? IpAddress { get; set; }

    [Column("user_agent")]
    public string? UserAgent { get; set; }

    [NotMapped]
    public object? BeforeData
    {
        get => string.IsNullOrEmpty(BeforeDataJson) ? null : JsonSerializer.Deserialize<object>(BeforeDataJson);
        set => BeforeDataJson = value == null ? null : JsonSerializer.Serialize(value);
    }

    [NotMapped]
    public object? AfterData
    {
        get => string.IsNullOrEmpty(AfterDataJson) ? null : JsonSerializer.Deserialize<object>(AfterDataJson);
        set => AfterDataJson = value == null ? null : JsonSerializer.Serialize(value);
    }

    // Navigation properties
    [ForeignKey(nameof(ActorId))]
    public virtual AppUser? Actor { get; set; }
}