using Microsoft.EntityFrameworkCore;
using RadiusConnect.Api.Models.Domain;

namespace RadiusConnect.Api.Data;

public class RadiusDbContext : DbContext
{
    public RadiusDbContext(DbContextOptions<RadiusDbContext> options) : base(options)
    {
    }

    // FreeRADIUS tables
    public DbSet<RadCheck> RadCheck { get; set; }
    public DbSet<RadReply> RadReply { get; set; }
    public DbSet<RadUserGroup> RadUserGroup { get; set; }
    public DbSet<RadGroupCheck> RadGroupCheck { get; set; }
    public DbSet<RadGroupReply> RadGroupReply { get; set; }
    public DbSet<RadAcct> RadAcct { get; set; }
    public DbSet<RadPostAuth> RadPostAuth { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // RadCheck configuration
        modelBuilder.Entity<RadCheck>(entity =>
        {
            entity.HasIndex(e => e.Username).HasDatabaseName("radcheck_username_idx");
            entity.HasIndex(e => new { e.Username, e.Attribute }).HasDatabaseName("radcheck_username_attribute_idx");
        });

        // RadReply configuration
        modelBuilder.Entity<RadReply>(entity =>
        {
            entity.HasIndex(e => e.Username).HasDatabaseName("radreply_username_idx");
            entity.HasIndex(e => new { e.Username, e.Attribute }).HasDatabaseName("radreply_username_attribute_idx");
        });

        // RadUserGroup configuration
        modelBuilder.Entity<RadUserGroup>(entity =>
        {
            entity.HasKey(e => new { e.Username, e.GroupName });
            entity.HasIndex(e => e.Username).HasDatabaseName("radusergroup_username_idx");
            entity.HasIndex(e => e.GroupName).HasDatabaseName("radusergroup_groupname_idx");
        });

        // RadGroupCheck configuration
        modelBuilder.Entity<RadGroupCheck>(entity =>
        {
            entity.HasIndex(e => e.GroupName).HasDatabaseName("radgroupcheck_groupname_idx");
            entity.HasIndex(e => new { e.GroupName, e.Attribute }).HasDatabaseName("radgroupcheck_groupname_attribute_idx");
        });

        // RadGroupReply configuration
        modelBuilder.Entity<RadGroupReply>(entity =>
        {
            entity.HasIndex(e => e.GroupName).HasDatabaseName("radgroupreply_groupname_idx");
            entity.HasIndex(e => new { e.GroupName, e.Attribute }).HasDatabaseName("radgroupreply_groupname_attribute_idx");
        });

        // RadAcct configuration
        modelBuilder.Entity<RadAcct>(entity =>
        {
            entity.HasIndex(e => e.Username).HasDatabaseName("radacct_username_idx");
            entity.HasIndex(e => e.AcctSessionId).HasDatabaseName("radacct_acctsessionid_idx");
            entity.HasIndex(e => e.AcctUniqueId).HasDatabaseName("radacct_acctuniqueid_idx");
            entity.HasIndex(e => e.NasIpAddress).HasDatabaseName("radacct_nasipaddress_idx");
            entity.HasIndex(e => e.AcctStartTime).HasDatabaseName("radacct_acctstarttime_idx");
            entity.HasIndex(e => e.AcctStopTime).HasDatabaseName("radacct_acctstoptime_idx");
        });

        // RadPostAuth configuration
        modelBuilder.Entity<RadPostAuth>(entity =>
        {
            entity.HasIndex(e => e.Username).HasDatabaseName("radpostauth_username_idx");
            entity.HasIndex(e => e.AuthDate).HasDatabaseName("radpostauth_authdate_idx");
        });
    }
}