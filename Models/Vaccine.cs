using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SocietyVaccinations.Models;

[Table("vaccines")]
public partial class Vaccine
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [InverseProperty("Vaccine")]
    public virtual ICollection<SpotVaccine> SpotVaccines { get; set; } = new List<SpotVaccine>();

    [InverseProperty("Vaccine")]
    public virtual ICollection<Vaccination> Vaccinations { get; set; } = new List<Vaccination>();
}

public class VaccineInputDTO
{
    [Required]
    public string name { get; set; } = null!;

    public Vaccine ToEntity()
    {
        return new Vaccine { Name = name };
    }
    public Vaccine ToEntity(long id)
    {
        return new Vaccine { Id = id, Name = name };
    }
}
