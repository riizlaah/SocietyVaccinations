using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocietyVaccinations.Models;

namespace SocietyVaccinations.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class VaccinationsController : ControllerBase
    {
        SVContext dbc;
        public VaccinationsController(SVContext ctx) { dbc = ctx; }

        [HttpPost]
        async public Task<IActionResult> Create(string token, VaccinationRegisterDTO input)
        {
            var userId = await dbc.getIdFromToken(token);
            if (userId < 0) return Helper.err("Unauthorized user");
            var spot = await dbc.Spots.FirstOrDefaultAsync(s => s.Id == input.spot_id);
            if (spot == null) return Helper.err("Spot not found");
            if (!await dbc.Consultations.AnyAsync(c => c.SocietyId == userId && c.Status == "accepted")) return Helper.err("Your consultation must be accepted by doctor before");
            var vaccineCount = await dbc.Vaccinations.CountAsync(v => v.SocietyId == userId);
            var counter = "First";
            if (vaccineCount == 2) return Helper.err("Society has been 2x vaccinated");
            else if(vaccineCount == 1)
            {
                if(await dbc.Vaccinations.AnyAsync(v => EF.Functions.DateDiffDay(v.Date.ToDateTime(TimeOnly.MinValue), DateTime.Now.Date) < 30))
                {
                    return Helper.err("Wait at least +30 days from 1st Vaccination");
                }
                counter = "Second";
            }
            if (counter == "First" && spot.Serve == 2) return Helper.err("This spot is only for second vaccination");
            if (counter == "Second" && spot.Serve == 1) return Helper.err("This spot is only for first vaccination");
            if (await dbc.Vaccinations.CountAsync(v => v.Date == input.date) >= spot.Capacity) return Helper.err("The spot has reached max capacity for the requested date");
            await dbc.Vaccinations.AddAsync(new Vaccination
            {
                SocietyId = userId,
                SpotId = input.spot_id,
                Date = input.date,
                Dose = 1,
            });
            await dbc.SaveChangesAsync();
            return Ok(new {message = $"{counter} vaccination registered successful" });
        }

        [HttpGet]
        async public Task<IActionResult> GetAll(string token)
        {
            var userId = await dbc.getIdFromToken(token);
            if (userId < 0) return Helper.err("Unauthorized user");
            var vaccinations = await dbc.Vaccinations.Where(v => v.SocietyId == userId).Include(v => v.Spot.Regional).Include(v => v.Vaccine).Include(v => v.Doctor).ToListAsync();
            var first = vaccinations.Count > 0 ? await formatVaccination(vaccinations[0]) : null;
            var second = vaccinations.Count > 1 ? await formatVaccination(vaccinations[1]) : null;
            return Ok(new
            {
                vaccinations = new
                {
                    first = first,
                    second = second
                }
            });
        }

        private async Task<object> formatVaccination(Vaccination vacc)
        {
            var vaccinator = vacc.Doctor == null ? null : new
            {
                id = vacc.DoctorId,
                role = vacc.Doctor.Role,
                name = vacc.Doctor.Name
            };
            var vaccine = vacc.Vaccine == null ? null : new
            {
                id = vacc.VaccineId,
                name = vacc.Vaccine.Name
            };
            var queues = await dbc.Vaccinations.Where(v => v.SpotId == vacc.SpotId && v.Date == vacc.Date).OrderBy(v => v.Id).Select(v => v.SocietyId).ToListAsync();
            var order = 0;
            foreach(var q in queues)
            {
                order += 1;
                if (q == vacc.SocietyId) break;
            }
            return new
            {
                queue = order,
                dose = vacc.Dose,
                vaccination_date = vacc.Date,
                spot = new
                {
                    id = vacc.SpotId,
                    name = vacc.Spot.Name,
                    address = vacc.Spot.Address,
                    serve = vacc.Spot.Serve,
                    capacity = vacc.Spot.Capacity,
                    regional = new
                    {
                        id = vacc.Spot.RegionalId,
                        province = vacc.Spot.Regional.Province,
                        district = vacc.Spot.Regional.District
                    },
                    status = (vaccine == null || vaccinator == null) ? "pending" : "done",
                    vaccine = vaccine,
                    vaccinator = vaccinator
                }
            };
        }
    }
}
