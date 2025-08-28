using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RadiusConnect.Api.Models.Domain;

[Table("radpostauth")]
public class RadPostAuth
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("username")]
    [MaxLength(64)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [Column("pass")]
    [MaxLength(64)]
    public string Pass { get; set; } = string.Empty;

    [Required]
    [Column("reply")]
    [MaxLength(32)]
    public string Reply { get; set; } = string.Empty;

    [Column("authdate")]
    public DateTime AuthDate { get; set; } = DateTime.UtcNow;
}