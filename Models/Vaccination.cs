using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SocietyVaccinations.Models;

[Table("vaccinations")]
public partial class Vaccination
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("dose")]
    public byte Dose { get; set; }

    [Column("date")]
    public DateOnly Date { get; set; }

    [Column("society_id")]
    public long SocietyId { get; set; }

    [Column("spot_id")]
    public long SpotId { get; set; }

    [Column("vaccine_id")]
    public long? VaccineId { get; set; }

    [Column("doctor_id")]
    public long? DoctorId { get; set; }

    [Column("officer_id")]
    public long? OfficerId { get; set; }

    [ForeignKey("DoctorId")]
    [InverseProperty("VaccinationDoctors")]
    public virtual Medical? Doctor { get; set; } = null!;

    [ForeignKey("OfficerId")]
    [InverseProperty("VaccinationOfficers")]
    public virtual Medical? Officer { get; set; } = null!;

    [ForeignKey("SocietyId")]
    [InverseProperty("Vaccinations")]
    public virtual Society Society { get; set; } = null!;

    [ForeignKey("SpotId")]
    [InverseProperty("Vaccinations")]
    public virtual Spot Spot { get; set; } = null!;

    [ForeignKey("VaccineId")]
    [InverseProperty("Vaccinations")]
    public virtual Vaccine? Vaccine { get; set; } = null!;
}


public class VaccinationRegisterDTO
{
    [Required]
    [Range(1, long.MaxValue, ErrorMessage = "Id not valid")]
    public long spot_id { get; set; }

    [Required]
    public DateOnly date { get; set; }
}

public class VaccinationUpdateDTO
{
    public long? doctor_id { get; set; } = null;

    public long? vaccine_id { get; set; } = null;
}