using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SocietyVaccinations.Models;

[Table("spots")]
public partial class Spot
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("regional_id")]
    public long RegionalId { get; set; }

    [Column("name")]
    [StringLength(50)]
    public string Name { get; set; } = null!;

    [Column("address")]
    [StringLength(50)]
    public string Address { get; set; } = null!;

    [Column("serve")]
    public byte Serve { get; set; }

    [Column("capacity")]
    public int Capacity { get; set; }

    [InverseProperty("Spot")]
    public virtual ICollection<Medical> Medicals { get; set; } = new List<Medical>();

    [ForeignKey("RegionalId")]
    [InverseProperty("Spots")]
    public virtual Regional Regional { get; set; } = null!;

    [InverseProperty("Spot")]
    public virtual ICollection<SpotVaccine> SpotVaccines { get; set; } = new List<SpotVaccine>();

    [InverseProperty("Spot")]
    public virtual ICollection<Vaccination> Vaccinations { get; set; } = new List<Vaccination>();
}
