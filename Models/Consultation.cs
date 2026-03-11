using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SocietyVaccinations.Models;

[Table("consultations")]
public partial class Consultation
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("society_id")]
    public long SocietyId { get; set; }

    [Column("doctor_id")]
    public long? DoctorId { get; set; }

    [Column("status")]
    [StringLength(10)]
    [Unicode(false)]
    public string Status { get; set; } = null!;

    [Column("disease_history", TypeName = "text")]
    public string DiseaseHistory { get; set; } = null!;

    [Column("current_symptoms", TypeName = "text")]
    public string CurrentSymptoms { get; set; } = null!;

    [Column("doctor_notes", TypeName = "text")]
    public string DoctorNotes { get; set; } = null!;

    [ForeignKey("DoctorId")]
    [InverseProperty("Consultations")]
    public virtual Medical? Doctor { get; set; } = null!;

    [ForeignKey("SocietyId")]
    [InverseProperty("Consultations")]
    public virtual Society Society { get; set; } = null!;
}

public class ConsultationReqDTO
{
    [Required]
    public string disease_history { get; set; } = null!;
    [Required]
    public string current_symptoms { get; set; } = null!;
}

public class ConsultationUpdateDTO
{
    [Required]
    [RegularExpression(@"^accepted|pending|rejected$", ErrorMessage = "Status must be pending|accepted|rejected")]
    public string status { get; set; } = null!;

    [Required]
    public string doctor_notes { get; set; } = null!;
}
