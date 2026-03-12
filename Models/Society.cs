using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SocietyVaccinations.Models;

[Table("societies")]
public partial class Society
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("id_card_number")]
    [StringLength(8)]
    public string IdCardNumber { get; set; } = null!;

    [Column("password")]
    [StringLength(255)]
    public string Password { get; set; } = null!;

    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Column("born_date")]
    public DateOnly BornDate { get; set; }

    [Column("gender")]
    [StringLength(6)]
    [Unicode(false)]
    public string Gender { get; set; } = null!;

    [Column("address", TypeName = "text")]
    public string Address { get; set; } = null!;

    [Column("regional_id")]
    public long RegionalId { get; set; }

    [Column("login_tokens", TypeName = "text")]
    public string LoginTokens { get; set; } = null!;

    [InverseProperty("Society")]
    public virtual ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();

    [ForeignKey("RegionalId")]
    [InverseProperty("Societies")]
    public virtual Regional Regional { get; set; } = null!;

    [InverseProperty("Society")]
    public virtual ICollection<Vaccination> Vaccinations { get; set; } = new List<Vaccination>();
}

public class SocietyLoginDTO
{
    [Required]
    [MinLength(8)]
    public string id_card_number { get; set; }

    [Required]
    [MinLength(6)]
    public string password { get; set; }
}

public class SocietyInputDTO
{
    [Required]
    public string name { get; set; } = null!;

    [Required]
    [RegularExpression(@"^\d{8}$", ErrorMessage = "ID Card Number not valid")]
    
    public string id_card_number { get; set; } = null!;

    public string? password { get; set; }

    [Required]
    public DateOnly born_date { get; set; }

    [Required]
    public string address { get; set; } = null!;

    [Required]
    [RegularExpression(@"^male|female$", ErrorMessage = "Gender must be male or female")]
    public string gender { get; set; }

    [Required]
    public long regionalId { get; set;  }

    public Society ToEntity()
    {
        return new Society { Name = name, Password = Helper.sha256(password), IdCardNumber = id_card_number, BornDate = born_date, Address = address, Gender = gender, RegionalId = regionalId, LoginTokens = "" };
    }

    public Society ToEntity(long id, string password)
    {
        return new Society { Id = id, Name = name, Password = password, IdCardNumber = id_card_number, BornDate = born_date, Address = address, Gender = gender, RegionalId = regionalId, LoginTokens = "" };
    }
}
