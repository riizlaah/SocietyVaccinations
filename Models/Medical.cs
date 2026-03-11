using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SocietyVaccinations.Models;

[Table("medicals")]
public partial class Medical
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("spot_id")]
    public long SpotId { get; set; }

    [Column("user_id")]
    public long UserId { get; set; }

    [Column("role")]
    [StringLength(8)]
    [Unicode(false)]
    public string Role { get; set; } = null!;

    [Column("name")]
    [StringLength(255)]
    [Unicode(false)]
    public string Name { get; set; } = null!;

    [InverseProperty("Doctor")]
    public virtual ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();

    [ForeignKey("SpotId")]
    [InverseProperty("Medicals")]
    public virtual Spot Spot { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("Medicals")]
    public virtual User User { get; set; } = null!;

    [InverseProperty("Doctor")]
    public virtual ICollection<Vaccination> VaccinationDoctors { get; set; } = new List<Vaccination>();

    [InverseProperty("Officer")]
    public virtual ICollection<Vaccination> VaccinationOfficers { get; set; } = new List<Vaccination>();
}

public class MedicalLoginDTO
{
    [Required]
    [MinLength(3)]
    public string username { get; set; } = null!;

    [Required]
    [MinLength(4)]
    public string password { get; set; } = null!;
}
