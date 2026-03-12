using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocietyVaccinations.Models;
using System.Security.Claims;

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
                if(await dbc.Vaccinations.AnyAsync(v => EF.Functions.DateDiffDay(v.Date.ToDateTime(TimeOnly.MinValue), DateTime.Now.Date) < 30 && v.SocietyId == userId))
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

        [HttpGet("all")]
        [Authorize(Roles = "officer")]
        async public Task<IActionResult> GetAvailable()
        {
            var officerId = Convert.ToInt64(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var officer = await dbc.Medicals.Where(v => v.Id == officerId).Include(m => m.Spot.Regional).FirstAsync();
            var vaccinations = await dbc.Vaccinations.AsQueryable()
                .Where(v => v.OfficerId == null || v.OfficerId == officerId)
                .Where(v => v.SpotId == officer.SpotId)
                .Include(v => v.Vaccine)
                .Include(v => v.Doctor)
                .Include(v => v.Society)
                .OrderBy(v => (v.VaccineId == null || v.DoctorId == null) ? 0 : 1)
                .AsSplitQuery()
                .ToListAsync();
            return Ok(vaccinations.Select(v => new
            {
                id = v.Id,
                dose = v.Dose,
                date = v.Date,
                society = new
                {
                    id = v.SocietyId,
                    name = v.Society.Name,
                    address = v.Society.Address
                },
                vaccine = v.VaccineId == null ? null : new
                {
                    id = v.VaccineId,
                    name = v.Vaccine.Name
                },
                vaccinator = v.DoctorId == null ? null : new
                {
                    id = v.DoctorId,
                    name = v.Doctor.Name,
                    role = v.Doctor.Role
                },
                officer = new
                {
                    id = officerId,
                    name = officer.Name,
                    role = officer.Role
                },
                spot = new
                {
                    id = officer.SpotId,
                    name = officer.Spot.Name,
                    address = officer.Spot.Address,
                    regional = new
                    {
                        id = officer.Spot.RegionalId,
                        provine = officer.Spot.Regional.Province,
                        district = officer.Spot.Regional.District
                    }
                }
            }));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "officer")]
        async public Task<IActionResult> Get(long id)
        {
            var record = await dbc.Vaccinations.AsQueryable().AsNoTracking()
                .Include(v => v.Vaccine)
                .Include(v => v.Doctor)
                .Include(v => v.Society)
                .Include(v => v.Officer)
                .Include(v => v.Spot.Regional)
                .FirstOrDefaultAsync(v => v.Id == id);
            if (record == null) return Helper.err("Vaccination not found", 404);
            return Ok(new
            {
                id = record.Id,
                dose = record.Dose,
                date = record.Date,
                society_id = record.SocietyId,
                vaccine_id = record.VaccineId,
                doctor_id = record.DoctorId,
                officer_id = record.OfficerId,
                spot_id = record.SpotId,
                society = new
                {
                    id = record.SocietyId,
                    name = record.Society.Name,
                    address = record.Society.Address
                },
                vaccine = record.VaccineId == null ? null : new
                {
                    id = record.VaccineId,
                    name = record.Vaccine.Name
                },
                vaccinator = record.DoctorId == null ? null : new
                {
                    id = record.DoctorId,
                    name = record.Doctor.Name,
                    role = record.Doctor.Role
                },
                officer = new
                {
                    id = record.OfficerId,
                    name = record.Officer.Name,
                    role = record.Officer.Role
                },
                spot = new
                {
                    id = record.SpotId,
                    name = record.Spot.Name,
                    address = record.Spot.Address,
                    regional = new
                    {
                        id = record.Spot.RegionalId,
                        provine = record.Spot.Regional.Province,
                        district = record.Spot.Regional.District
                    }
                }
            });
        }


        [HttpPost("create")]
        [Authorize(Roles = "officer")]
        async public Task<IActionResult> DetailedCreate(VaccinationInputDTO input)
        {
            if (!await dbc.Societies.AnyAsync(s => s.Id == input.society_id)) return Helper.err("Society not found");
            if (!await dbc.Spots.AnyAsync(s => s.Id == input.spot_id)) return Helper.err("Spot not found");
            var vaccination = await dbc.Vaccinations.CountAsync(v => v.SocietyId == input.society_id);
            if (vaccination == 2) return Helper.err("Society has been 2x vaccinated");
            if (await dbc.Vaccinations.AnyAsync(v => EF.Functions.DateDiffDay(v.Date.ToDateTime(TimeOnly.MinValue), DateTime.Now.Date) < 30 && v.SocietyId == input.society_id))
            {
                return Helper.err("Wait at least +30 days from 1st Vaccination");
            }
            var officerId = Convert.ToInt64(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var record = input.ToEntity(officerId);
            if (input.doctor_id.HasValue)
            {
                if (!await dbc.Medicals.AnyAsync(m => m.Id == input.doctor_id.Value)) return Helper.err("Doctor not found");
                record.DoctorId = input.doctor_id.Value;
            }
            if (input.vaccine_id.HasValue)
            {
                if (!await dbc.Vaccines.AnyAsync(m => m.Id == input.vaccine_id.Value)) return Helper.err("Vaccine not found");
                record.VaccineId = input.vaccine_id.Value;
            }

            dbc.Vaccinations.Add(record);
            await dbc.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "officer")]
        async public Task<IActionResult> DetailedUpdate(long id, VaccinationInputDTO input)
        {
            if (!await dbc.Vaccinations.AnyAsync(m => m.Id == id)) return Helper.err("Vaccination not found", 404);
            if (!await dbc.Societies.AnyAsync(s => s.Id == input.society_id)) return Helper.err("Society not found");
            if (!await dbc.Spots.AnyAsync(s => s.Id == input.spot_id)) return Helper.err("Spot not found");
            
            var officerId = Convert.ToInt64(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if ((await dbc.Vaccinations.FindAsync(id)).OfficerId != officerId) return Helper.err("Forbidden", 403);
            var record = input.ToEntity(id, officerId);
            if (input.doctor_id.HasValue)
            {
                if (!await dbc.Medicals.AnyAsync(m => m.Id == input.doctor_id.Value)) return Helper.err("Doctor not found");
                record.DoctorId = input.doctor_id.Value;
            }
            if (input.vaccine_id.HasValue)
            {
                if (!await dbc.Vaccines.AnyAsync(m => m.Id == input.vaccine_id.Value)) return Helper.err("Vaccine not found");
                record.VaccineId = input.vaccine_id.Value;
            }
            dbc.Entry(record).State = EntityState.Modified;
            await dbc.SaveChangesAsync();
            return Ok();
        }

        [HttpPatch("{id}")]
        [Authorize]
        async public Task<IActionResult> UpdateVaccinations(long id, VaccinationUpdateDTO input)
        {
            if (input == null) return Helper.err("Not valid");
            if(input.doctor_id.HasValue)
            {
                if (!await dbc.Medicals.AnyAsync(m => m.Id == input.doctor_id.Value)) return Helper.err("Doctor not found");
            }
            if(input.vaccine_id.HasValue)
            {
                if (!await dbc.Vaccines.AnyAsync(m => m.Id == input.vaccine_id.Value)) return Helper.err("Vaccine not found");
            }
            var vaccination = await dbc.Vaccinations.FindAsync(id);
            if (vaccination == null) return Helper.err("Vaccination not found", 404);
            var officerId = Convert.ToInt64(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (vaccination.OfficerId == null) vaccination.OfficerId = officerId;
            if(input.doctor_id.HasValue) vaccination.DoctorId = input.doctor_id;
            if (input.vaccine_id.HasValue) vaccination.VaccineId = input.vaccine_id;
            await dbc.SaveChangesAsync();
            return Ok(new {message = "Vaccination updated"});
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "officer")]
        async public Task<IActionResult> Delete(long id)
        {
            var record = await dbc.Vaccinations.FindAsync(id);
            if (record == null) return Helper.err("Vaccination not found", 404);
            dbc.Vaccinations.Remove(record);
            await dbc.SaveChangesAsync();
            return Ok();
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
