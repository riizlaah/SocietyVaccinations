using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SocietyVaccinations.Models;

[Table("spot_vaccines")]
public partial class SpotVaccine
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("spot_id")]
    public long SpotId { get; set; }

    [Column("vaccine_id")]
    public long VaccineId { get; set; }

    [ForeignKey("SpotId")]
    [InverseProperty("SpotVaccines")]
    public virtual Spot Spot { get; set; } = null!;

    [ForeignKey("VaccineId")]
    [InverseProperty("SpotVaccines")]
    public virtual Vaccine Vaccine { get; set; } = null!;
}
