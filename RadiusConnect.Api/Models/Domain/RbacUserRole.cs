using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RadiusConnect.Api.Models.Domain;

[Table("rbac_user_roles")]
public class RbacUserRole
{
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("role_id")]
    public int RoleId { get; set; }

    [Column("assigned_at")]
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    [Column("assigned_by")]
    public Guid? AssignedBy { get; set; }

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public virtual AppUser User { get; set; } = null!;

    [ForeignKey(nameof(RoleId))]
    public virtual RbacRole Role { get; set; } = null!;

    [ForeignKey(nameof(AssignedBy))]
    public virtual AppUser? AssignedByUser { get; set; }
}