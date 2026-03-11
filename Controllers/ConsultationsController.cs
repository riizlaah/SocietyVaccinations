using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocietyVaccinations.Models;
using System.Diagnostics;
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
        [Authorize(Roles = "doctor")]
        async public Task<IActionResult> GetAll(string status = "pending")
        {
            var statuses = new string[] { "all", "pending", "accepted", "rejected" };
            if (!statuses.Contains(status)) return Helper.err("Status not valid");
            var doctorId = Convert.ToInt64(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var doctor = await dbc.Medicals.Where(m => m.Id == doctorId).Include(m => m.Spot.Regional).FirstAsync();
            var query = dbc.Consultations.Where(c => c.Society.RegionalId == doctor.Spot.RegionalId && (c.DoctorId == null || c.DoctorId == doctorId)).AsQueryable();
            if(status != "all")
            {
                query = query.Where(c => c.Status == status);
            }
            var consults = await query.Include(c => c.Society).ToListAsync();
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
                    id = doctorId,
                    name = doctor.Name,
                    role = doctor.Role
                },
                spot = new
                {
                    id = doctor.SpotId,
                    name = doctor.Spot.Name,
                    address = doctor.Spot.Address,
                    regional = new
                    {
                        id = doctor.Spot.RegionalId,
                        province = doctor.Spot.Regional.Province,
                        district = doctor.Spot.Regional.District
                    }
                }
            }));
        }

        [HttpPut("{id}")]
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
    }
}
