using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace RadiusConnect.Api.Models.Domain;

[Table("rbac_roles")]
public class RbacRole
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("name")]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("permissions", TypeName = "json")]
    public string PermissionsJson { get; set; } = "[]";

    [NotMapped]
    public List<string> Permissions
    {
        get => string.IsNullOrEmpty(PermissionsJson) 
            ? new List<string>() 
            : JsonSerializer.Deserialize<List<string>>(PermissionsJson) ?? new List<string>();
        set => PermissionsJson = JsonSerializer.Serialize(value);
    }

    // Navigation properties
    public virtual ICollection<RbacUserRole> UserRoles { get; set; } = new List<RbacUserRole>();
}