using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RadiusConnect.Api.Models.Domain;

[Table("radreply")]
public class RadReply
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("username")]
    [MaxLength(64)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [Column("attribute")]
    [MaxLength(64)]
    public string Attribute { get; set; } = string.Empty;

    [Required]
    [Column("op")]
    [MaxLength(2)]
    public string Op { get; set; } = "=";

    [Required]
    [Column("value")]
    [MaxLength(253)]
    public string Value { get; set; } = string.Empty;
}