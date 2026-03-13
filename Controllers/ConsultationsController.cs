using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SocietyVaccinations.Models;
using System.Diagnostics;
using System.Numerics;
using System.Security.Claims;

namespace SocietyVaccinations.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ConsultationsController : ControllerBase
    {
        SVContext dbc;
        public ConsultationsController(SVContext ctx) { dbc = ctx; }

        [HttpPost]
        //[Authorize]
        async public Task<IActionResult> Create(ConsultationReqDTO input, string token)
        {
            var userId = await dbc.getIdFromToken(token);
            if (userId < 0) return Helper.err("Unauthorized user");
            var consult = await dbc.Consultations.Where(c => c.SocietyId == userId).FirstOrDefaultAsync();
            if(consult == null)
            {
                await dbc.Consultations.AddAsync(new Consultation
                {
                    SocietyId = userId,
                    CurrentSymptoms = input.current_symptoms,
                    DiseaseHistory = input.disease_history,
                    DoctorId = null,
                    DoctorNotes = "",
                    Status = "pending"
                });
            } else
            {
                
                //consult.DiseaseHistory = input.disease_history;
                //consult.CurrentSymptoms = input.current_symptoms;
                //consult.DoctorNotes = "";
            }
            await dbc.SaveChangesAsync();
            return Ok(new {message = "Request consultation sent successful" });
        }

        [HttpGet]
        //[Authorize]
        async public Task<IActionResult> Get(string token)
        {
            var userId = await dbc.getIdFromToken(token);
            if (userId < 0) return Helper.err("Unauthorized user");
            var consult = await dbc.Consultations.Include(c => c.Doctor).Where(c => c.SocietyId == userId).FirstOrDefaultAsync();
            if (consult == null) return Ok("empty");
            var doctor = (consult.Doctor == null) ? null : new
            {
                id = consult.Doctor.Id,
                name = consult.Doctor.Name,
                role = consult.Doctor.Role,
            };
            return Ok(new
            {
                consultation = new
                {
                    id = consult.Id,
                    status = consult.Status,
                    disease_history = consult.DiseaseHistory,
                    current_symptoms = consult.CurrentSymptoms,
                    doctor_notes = consult.DoctorNotes,
                    doctor = doctor,
                }
            });
        }

        [HttpGet("all")]
        [Authorize]
        async public Task<IActionResult> GetAll(string status = "pending")
        {
            var statuses = new string[] { "all", "pending", "accepted", "rejected" };
            if (!statuses.Contains(status)) return Helper.err("Status not valid");
            var doctorId = Convert.ToInt64(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var doctor = await dbc.Medicals.Where(m => m.Id == doctorId).Include(m => m.Spot.Regional).FirstAsync();
            var query = dbc.Consultations.Where(c => c.Society.RegionalId == doctor.Spot.RegionalId).AsQueryable().AsNoTrackingWithIdentityResolution();
            if(status != "all")
            {
                query = query.Where(c => c.Status == status);
            }
            if(User.FindFirstValue(ClaimTypes.Role) == "doctor")
            {
                query = query.Where(c => c.DoctorId == null || c.DoctorId == doctorId);
            }
            var consults = await query.Include(c => c.Society).Include(c => c.Doctor).ToListAsync();
            return Ok(consults.Select(c => new
            {
                id = c.Id,
                society = new
                {
                    id = c.SocietyId,
                    name = c.Society.Name,
                    address = c.Society.Address,
                },
                disease_history = c.DiseaseHistory,
                current_symptoms = c.CurrentSymptoms,
                doctor_notes = c.DoctorNotes,
                status = c.Status,
                doctor = c.Doctor == null ? null : new
                {
                    id = c.DoctorId,
                    name = c.Doctor.Name,
                    role = c.Doctor.Role
                }
            }));
        }

        [HttpGet("{id}")]
        [Authorize]
        async public Task<IActionResult> Get(long id)
        {
            var record = await dbc.Consultations.AsQueryable().Include(c => c.Doctor).Include(c => c.Society).FirstOrDefaultAsync(c => c.Id == id);
            if (record == null) return Helper.err("Consultation not found");
            return Ok(new
            {
                id = record.Id,
                society = new
                {
                    id = record.SocietyId,
                    name = record.Society.Name,
                    address = record.Society.Address,
                },
                disease_history = record.DiseaseHistory,
                current_symptoms = record.CurrentSymptoms,
                doctor_notes = record.DoctorNotes,
                status = record.Status,
                doctor = record.Doctor == null ? null : new
                {
                    id = record.DoctorId,
                    name = record.Doctor.Name,
                    role = record.Doctor.Role
                }
            });
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "doctor")]
        async public Task<IActionResult> Update(int id, ConsultationUpdateDTO input)
        {
            var consult = await dbc.Consultations.FindAsync((long)id);
            if (consult == null) return NotFound(new { message = "Consultation not found" });
            consult.DoctorId = Convert.ToInt64(User.FindFirstValue(ClaimTypes.NameIdentifier));
            consult.Status = input.status;
            consult.DoctorNotes = input.doctor_notes;
            await dbc.SaveChangesAsync();
            return Ok(new { message = "Consultation updated" });
        }

        [HttpPost("create")]
        [Authorize(Roles = "officer")]
        async public Task<IActionResult> DetailedCreate(ConsultationInputDTO input)
        {
            if(input.DoctorId.HasValue)
            {
                if (!await dbc.Medicals.AnyAsync(m => m.Id == input.DoctorId && m.Role == "doctor")) return Helper.err("Doctor not found");
            }
            if (!await dbc.Societies.AnyAsync(s => s.Id == input.SocietyId)) return Helper.err("Society not found");
            if (input.Status.IsNullOrEmpty()) input.Status = "pending";
            var record = input.ToEntity();
            dbc.Consultations.Add(record);
            await dbc.SaveChangesAsync();
            return Ok(record);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "officer")]
        async public Task<IActionResult> DetailedUpdate(long id, ConsultationInputDTO input)
        {
            if (!await dbc.Consultations.AnyAsync(c => c.Id == id)) return Helper.err("Consultation not found");
            if (!await dbc.Medicals.AnyAsync(m => m.Id == input.DoctorId && m.Role == "doctor")) return Helper.err("Doctor not found");
            if (!await dbc.Societies.AnyAsync(s => s.Id == input.SocietyId)) return Helper.err("Society not found");
            if (input.Status.IsNullOrEmpty()) input.Status = "pending";
            var record = input.ToEntity(id);
            dbc.Entry(record).State = EntityState.Modified;
            await dbc.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "officer")]
        async public Task<IActionResult> Delete(long id)
        {
            var record = dbc.Consultations.Find(id);
            if (record == null) return Helper.err("Consultation not found", 404);
            dbc.Consultations.Remove(record);
            await dbc.SaveChangesAsync();
            return Ok();
        }
    }
}
