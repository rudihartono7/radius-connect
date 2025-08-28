using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RadiusConnect.Api.Models.Domain;

[Table("radusergroup")]
public class RadUserGroup
{
    [Required]
    [Column("username")]
    [MaxLength(64)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [Column("groupname")]
    [MaxLength(64)]
    public string GroupName { get; set; } = string.Empty;

    [Column("priority")]
    public int Priority { get; set; } = 1;
}