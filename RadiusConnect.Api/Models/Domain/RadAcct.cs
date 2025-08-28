using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RadiusConnect.Api.Models.Domain;

[Table("radacct")]
public class RadAcct
{
    [Key]
    [Column("radacctid")]
    public long RadAcctId { get; set; }

    [Required]
    [Column("acctsessionid")]
    [MaxLength(64)]
    public string AcctSessionId { get; set; } = string.Empty;

    [Required]
    [Column("acctuniqueid")]
    [MaxLength(32)]
    public string AcctUniqueId { get; set; } = string.Empty;

    [Required]
    [Column("username")]
    [MaxLength(64)]
    public string Username { get; set; } = string.Empty;

    [Column("groupname")]
    [MaxLength(64)]
    public string? GroupName { get; set; }

    [Column("realm")]
    [MaxLength(64)]
    public string? Realm { get; set; }

    [Required]
    [Column("nasipaddress")]
    [MaxLength(15)]
    public string NasIpAddress { get; set; } = string.Empty;

    [Column("nasportid")]
    [MaxLength(15)]
    public string? NasPortId { get; set; }

    [Column("nasporttype")]
    [MaxLength(32)]
    public string? NasPortType { get; set; }

    [Column("acctstarttime")]
    public DateTime? AcctStartTime { get; set; }

    [Column("acctupdatetime")]
    public DateTime? AcctUpdateTime { get; set; }

    [Column("acctstoptime")]
    public DateTime? AcctStopTime { get; set; }

    [Column("acctinterval")]
    public int? AcctInterval { get; set; }

    [Column("acctsessiontime")]
    public uint? AcctSessionTime { get; set; }

    [Column("acctauthentic")]
    [MaxLength(32)]
    public string? AcctAuthentic { get; set; }

    [Column("connectinfo_start")]
    [MaxLength(50)]
    public string? ConnectInfoStart { get; set; }

    [Column("connectinfo_stop")]
    [MaxLength(50)]
    public string? ConnectInfoStop { get; set; }

    [Column("acctinputoctets")]
    public long? AcctInputOctets { get; set; }

    [Column("acctoutputoctets")]
    public long? AcctOutputOctets { get; set; }

    [Required]
    [Column("calledstationid")]
    [MaxLength(50)]
    public string CalledStationId { get; set; } = string.Empty;

    [Required]
    [Column("callingstationid")]
    [MaxLength(50)]
    public string CallingStationId { get; set; } = string.Empty;

    [Required]
    [Column("acctterminatecause")]
    [MaxLength(32)]
    public string AcctTerminateCause { get; set; } = string.Empty;

    [Column("servicetype")]
    [MaxLength(32)]
    public string? ServiceType { get; set; }

    [Column("framedprotocol")]
    [MaxLength(32)]
    public string? FramedProtocol { get; set; }

    [Required]
    [Column("framedipaddress")]
    [MaxLength(15)]
    public string FramedIpAddress { get; set; } = string.Empty;
}