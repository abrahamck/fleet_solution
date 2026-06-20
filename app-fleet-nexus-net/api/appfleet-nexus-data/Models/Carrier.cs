using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace appfleet_nexus_data.Models;

[Table("fmcsa_census")]
public class Carrier
{
    [Key]
    [Column("dot_number")]
    public long DotNumber { get; set; }

    [Column("legal_name")]
    [MaxLength(200)]
    public string? LegalName { get; set; }

    [Column("dba_name")]
    [MaxLength(200)]
    public string? DbaName { get; set; }

    [Column("carrier_operation")]
    [MaxLength(2)]
    public string? CarrierOperation { get; set; }

    [Column("hm_flag")]
    public bool? HmFlag { get; set; }

    [Column("pc_flag")]
    public bool? PcFlag { get; set; }

    [Column("phy_street")]
    [MaxLength(200)]
    public string? PhyStreet { get; set; }

    [Column("phy_city")]
    [MaxLength(100)]
    public string? PhyCity { get; set; }

    [Column("phy_state")]
    [MaxLength(2)]
    public string? PhyState { get; set; }

    [Column("phy_zip")]
    [MaxLength(10)]
    public string? PhyZip { get; set; }

    [Column("phy_country")]
    [MaxLength(3)]
    public string? PhyCountry { get; set; }

    [Column("mailing_street")]
    [MaxLength(200)]
    public string? MailingStreet { get; set; }

    [Column("mailing_city")]
    [MaxLength(100)]
    public string? MailingCity { get; set; }

    [Column("mailing_state")]
    [MaxLength(2)]
    public string? MailingState { get; set; }

    [Column("mailing_zip")]
    [MaxLength(10)]
    public string? MailingZip { get; set; }

    [Column("mailing_country")]
    [MaxLength(3)]
    public string? MailingCountry { get; set; }

    [Column("telephone")]
    [MaxLength(20)]
    public string? Telephone { get; set; }

    [Column("fax")]
    [MaxLength(20)]
    public string? Fax { get; set; }

    [Column("email_address")]
    [MaxLength(255)]
    public string? EmailAddress { get; set; }

    [Column("mcs150_date")]
    public DateOnly? Mcs150Date { get; set; }

    [Column("mcs150_mileage")]
    public long? Mcs150Mileage { get; set; }

    [Column("mcs150_mileage_year")]
    public int? Mcs150MileageYear { get; set; }

    [Column("add_date")]
    public DateOnly? AddDate { get; set; }

    [Column("oic_state")]
    [MaxLength(2)]
    public string? OicState { get; set; }

    [Column("nbr_power_unit")]
    public int? NbrPowerUnit { get; set; }

    [Column("driver_total")]
    public int? DriverTotal { get; set; }
}
