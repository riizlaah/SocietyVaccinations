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
    public byte Serve { get; set; } // for vaccination type maybe? e.g first = 1, second = 2, both = 3

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

    public void updateVaccines(List<long> vaccineIds)
    {
        foreach (var vId in vaccineIds)
        {
            if (SpotVaccines.Any(sv => sv.VaccineId == vId)) continue;
            SpotVaccines.Add(new SpotVaccine { VaccineId = vId, SpotId = vId });
        }
        var toRemoved = SpotVaccines.Where(sv => !vaccineIds.Contains(sv.VaccineId)).ToList();
        foreach(var sv in toRemoved)
        {
            SpotVaccines.Remove(sv);
        }
    }
}

public class SpotInputDTO
{
    [Required]
    public string name { get; set; } = null!;

    [Required]
    public string address { get; set; } = null!;

    [Required]
    [Range(1, 3)]
    public byte serve { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int capacity { get; set; }

    [Required]
    public long regionalId { get; set; }

    [Required]
    public List<long> available_vacccines { get; set; } = new List<long>();

    public Spot ToEntity()
    {
        return new Spot { Name = name, Address = address, Serve = serve, Capacity = capacity, RegionalId = regionalId};
    }

    
}
