using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocietyVaccinations.Models;
using System.Text.RegularExpressions;

namespace SocietyVaccinations.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class SpotsController : ControllerBase
    {
        SVContext dbc;
        public SpotsController(SVContext ctx) { dbc = ctx; }

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
    }
}
