using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Text.Json;

namespace RadiusConnect.Api.Models.Domain;

[Table("coa_requests")]
public class CoaRequest
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("acctsessionid")]
    [MaxLength(64)]
    public string AcctSessionId { get; set; } = string.Empty;

    [Column("username")]
    [MaxLength(64)]
    public string? Username { get; set; }

    [Required]
    [Column("nas_ip")]
    public IPAddress NasIp { get; set; } = IPAddress.None;

    [Required]
    [Column("request_type")]
    [MaxLength(20)]
    public string RequestType { get; set; } = string.Empty; // 'disconnect', 'coa'

    [Required]
    [Column("status")]
    [MaxLength(20)]
    public string Status { get; set; } = "pending"; // 'pending', 'sent', 'success', 'failed'

    [Column("request_data", TypeName = "json")]
    public string? RequestDataJson { get; set; }

    [Column("response_data", TypeName = "json")]
    public string? ResponseDataJson { get; set; }

    [Column("requested_by")]
    public Guid? RequestedBy { get; set; }

    [Column("requested_at")]
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }

    [NotMapped]
    public Dictionary<string, object>? RequestData
    {
        get => string.IsNullOrEmpty(RequestDataJson) ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(RequestDataJson);
        set => RequestDataJson = value == null ? null : JsonSerializer.Serialize(value);
    }

    [NotMapped]
    public Dictionary<string, object>? ResponseData
    {
        get => string.IsNullOrEmpty(ResponseDataJson) ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(ResponseDataJson);
        set => ResponseDataJson = value == null ? null : JsonSerializer.Serialize(value);
    }

    // Navigation properties
    [ForeignKey(nameof(RequestedBy))]
    public virtual AppUser? RequestedByUser { get; set; }
}