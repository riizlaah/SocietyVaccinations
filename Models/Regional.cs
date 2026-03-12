using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SocietyVaccinations.Models;

[Table("regionals")]
public partial class Regional
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("province")]
    [StringLength(255)]
    public string Province { get; set; } = null!;

    [Column("district")]
    [StringLength(255)]
    public string District { get; set; } = null!;

    [InverseProperty("Regional")]
    public virtual ICollection<Society> Societies { get; set; } = new List<Society>();

    [InverseProperty("Regional")]
    public virtual ICollection<Spot> Spots { get; set; } = new List<Spot>();
}

public class RegionalInputDTO
{
    [Required]

    public string province { get; set; } = null!;

    [Required]
    public string district { get; set; } = null!;

    public Regional ToRegional(long id)
    {
        return new Regional { Id = id, Province = province, District = district }; 
    }

    public Regional ToRegional()
    {
        return new Regional { Province = province, District = district };
    }
}
