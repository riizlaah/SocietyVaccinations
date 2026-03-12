using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocietyVaccinations.Models;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace SocietyVaccinations.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class SpotsController : ControllerBase
    {
        SVContext dbc;
        public SpotsController(SVContext ctx) { dbc = ctx; }

        [HttpGet("all")]
        [Authorize]
        async public Task<IActionResult> GetAll()
        {
            var userId = Convert.ToInt64(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var regionId = (await dbc.Medicals.Include(m => m.Spot).FirstAsync(m => m.Id == userId)).Spot.RegionalId;
            var spots = await dbc.Spots.Include(s => s.SpotVaccines).Where(s => s.RegionalId == regionId).ToListAsync();
            var vaccines = await dbc.Vaccines.ToListAsync();

            return Ok(new
            {
                spots = spots.Select(s => new
                {
                    id = s.Id,
                    name = s.Name,
                    serve = s.Serve,
                    capacity = s.Capacity,
                    available_vaccines = vaccines.ToDictionary(v => v.Name, v => s.SpotVaccines.Any(sv => sv.VaccineId == v.Id))
                })
            });
        }

        [HttpGet]
        async public Task<IActionResult> GetAll(string token)
        {
            var userId = await dbc.getIdFromToken(token);
            if (userId < 0) return Helper.err("Unauthorized user");
            var regionId = (await dbc.Societies.FindAsync(userId)).RegionalId;
            var spots = await dbc.Spots.Include(s => s.SpotVaccines).Where(s => s.RegionalId == regionId).ToListAsync();
            var vaccines = await dbc.Vaccines.ToListAsync();

            return Ok(new
            {
                spots = spots.Select(s => new
                {
                    id = s.Id,
                    name = s.Name,
                    serve = s.Serve,
                    capacity = s.Capacity,
                    available_vaccines = vaccines.ToDictionary(v => v.Name, v => s.SpotVaccines.Any(sv => sv.VaccineId == v.Id))
                })
            });
        }

        [HttpGet("{id}")]
        async public Task<IActionResult> Get(int id, string token, string? date = null)
        {
            if (!await dbc.isAuthenticated(token)) return Helper.err("Unauthorized user");
            if (!Regex.IsMatch("ok", @"^\d{4}-\d{2}-\d{2}$") && date != null) return Helper.err("Date not valid");
            if (id < 0) return Helper.err("Id not valid");
            var actualDate = date == null ? DateTime.Now.Date : DateTime.Parse(date);
            var spot = await dbc.Spots.Where(s => s.Id == id).Include(s => s.Vaccinations).FirstOrDefaultAsync();
            if (spot == null) return Helper.err("Spot not found", 404);
            var vaccinationCounts = spot.Vaccinations.Count(v => v.Date == DateOnly.FromDateTime(actualDate));
            return Ok(new
            {
                date = actualDate.ToString("MMMM dd, yyyy"),
                spot = new
                {
                    id = spot.Id,
                    name = spot.Name,
                    address = spot.Address,
                    serve = spot.Serve,
                    capacity = spot.Capacity
                },
                vaccinations_count = vaccinationCounts
            });
        }

        // PUT: api/Spots/5
        [HttpPut("{id}")]
        [Authorize(Roles = "officer")]
        public async Task<IActionResult> Update(long id, SpotInputDTO input)
        {
            var spot = await dbc.Spots.Include(s => s.SpotVaccines).FirstOrDefaultAsync(s => s.Id == id);
            if (spot == null) return Helper.err("Spot not found", 404);
            var count = dbc.Vaccines.Count(v => input.available_vacccines.Contains(v.Id));
            if(count != input.available_vacccines.Count) return Helper.err("A Vaccine in the list is not exist");
            if (!await dbc.Regionals.AnyAsync(r => r.Id == input.regionalId)) return Helper.err("Regional not found");
            
            spot.updateVaccines(input.available_vacccines);
            spot.Name = input.name;
            spot.Address = input.address;
            spot.Serve = input.serve;
            spot.Capacity = input.capacity;
            spot.RegionalId = input.regionalId;

            await dbc.SaveChangesAsync();

            return Ok();
        }

        // POST: api/Spots
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "officer")]
        public async Task<ActionResult<Vaccine>> Create(SpotInputDTO input)
        {
            var count = dbc.Vaccines.Count(v => input.available_vacccines.Contains(v.Id));
            if (count != input.available_vacccines.Count) return Helper.err("A Vaccine in the list is not exist");
            if (!await dbc.Regionals.AnyAsync(r => r.Id == input.regionalId)) return Helper.err("Regional not found");
            var newData = input.ToEntity();
            dbc.Spots.Add(newData);
            dbc.SaveChanges();
            newData.updateVaccines(input.available_vacccines);
            await dbc.SaveChangesAsync();

            return Ok(new {message = "Spot created"});
        }

        // DELETE: api/Spots/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "officer")]
        public async Task<IActionResult> Delete(long id)
        {
            var record = await dbc.Spots.FindAsync(id);
            if (record == null)
            {
                return NotFound();
            }

            dbc.Spots.Remove(record);
            await dbc.SaveChangesAsync();

            return Ok();
        }

    }
}
