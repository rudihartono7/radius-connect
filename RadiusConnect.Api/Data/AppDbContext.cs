using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using RadiusConnect.Api.Models.Domain;
using System.Net;

namespace RadiusConnect.Api.Data;

public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Application-specific tables
    public DbSet<AppUser> AppUsers { get; set; }
    public DbSet<RbacRole> RbacRoles { get; set; }
    public DbSet<RbacUserRole> RbacUserRoles { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<PolicyTemplate> PolicyTemplates { get; set; }
    public DbSet<CoaRequest> CoaRequests { get; set; }
    public DbSet<AppSettings> AppSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // AppUser configuration
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasIndex(e => e.UserName).IsUnique().HasDatabaseName("app_users_username_idx");
            entity.HasIndex(e => e.Email).IsUnique().HasDatabaseName("app_users_email_idx");
            entity.HasIndex(e => e.CreatedAt).HasDatabaseName("app_users_created_at_idx");
            
            entity.HasMany(e => e.UserRoles)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasMany(e => e.AuditLogs)
                .WithOne(e => e.Actor)
                .HasForeignKey(e => e.ActorId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // RbacRole configuration
        modelBuilder.Entity<RbacRole>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique().HasDatabaseName("rbac_roles_name_idx");
            
            entity.HasMany(e => e.UserRoles)
                .WithOne(e => e.Role)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RbacUserRole configuration
        modelBuilder.Entity<RbacUserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId });
            entity.HasIndex(e => e.UserId).HasDatabaseName("rbac_user_roles_user_id_idx");
            entity.HasIndex(e => e.RoleId).HasDatabaseName("rbac_user_roles_role_id_idx");
            entity.HasIndex(e => e.AssignedAt).HasDatabaseName("rbac_user_roles_assigned_at_idx");
        });

        // AuditLog configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(e => e.ActorId).HasDatabaseName("audit_log_actor_id_idx");
            entity.HasIndex(e => e.Action).HasDatabaseName("audit_log_action_idx");
            entity.HasIndex(e => e.Entity).HasDatabaseName("audit_log_entity_idx");
            entity.HasIndex(e => e.EntityId).HasDatabaseName("audit_log_entity_id_idx");
            entity.HasIndex(e => e.Timestamp).HasDatabaseName("audit_log_timestamp_idx");
            
            // Configure IPAddress conversion
            //entity.Property(e => e.IpAddress)
            //    .HasConversion(
            //        v => v != null ? v.ToString() : null,
            //        v => v != null ? IPAddress.Parse(v) : null)
            //    .HasMaxLength(45);
        });

        // PolicyTemplate configuration
        modelBuilder.Entity<PolicyTemplate>(entity =>
        {
            entity.HasIndex(e => e.Name).HasDatabaseName("policy_templates_name_idx");
            entity.HasIndex(e => e.CreatedAt).HasDatabaseName("policy_templates_created_at_idx");
            entity.HasIndex(e => e.UpdatedAt).HasDatabaseName("policy_templates_updated_at_idx");
        });

        // CoaRequest configuration
        modelBuilder.Entity<CoaRequest>(entity =>
        {
            entity.HasIndex(e => e.AcctSessionId).HasDatabaseName("coa_requests_acctsessionid_idx");
            entity.HasIndex(e => e.Username).HasDatabaseName("coa_requests_username_idx");
            entity.HasIndex(e => e.Status).HasDatabaseName("coa_requests_status_idx");
            entity.HasIndex(e => e.RequestedAt).HasDatabaseName("coa_requests_requested_at_idx");
            
            // Configure IPAddress conversion
            entity.Property(e => e.NasIp)
                .HasConversion(
                    v => v.ToString(),
                    v => IPAddress.Parse(v));
        });

        // AppSettings configuration
        modelBuilder.Entity<AppSettings>(entity =>
        {
            entity.HasIndex(e => e.Category).HasDatabaseName("app_settings_category_idx");
            entity.HasIndex(e => e.UpdatedAt).HasDatabaseName("app_settings_updated_at_idx");
        });
    }
}