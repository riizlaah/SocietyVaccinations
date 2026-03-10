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
            await dbc.Consultations.AddAsync(new Consultation
            {
                SocietyId = userId,
                CurrentSymptoms = input.current_symptoms,
                DiseaseHistory = input.disease_history,
                DoctorId = null,
                DoctorNotes = "",
                Status = "pending"
            });
            await dbc.SaveChangesAsync();
            return Ok();
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
    }
}
