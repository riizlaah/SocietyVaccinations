using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocietyVaccinations.Models;

namespace SocietyVaccinations.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class VaccinesController: ControllerBase
    {
        private readonly SVContext _context;

        public VaccinesController(SVContext context)
        {
            _context = context;
        }

        // GET: api/Vaccines
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var datas = await _context.Vaccines.Select(v => new
            {
                id = v.Id,
                name = v.Name,
                vaccination_count = v.Vaccinations.Count
            }).ToListAsync();
            return Ok(datas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            var data = await _context.Vaccines.Where(v => v.Id == id).Select(v => new
            {
                id = v.Id,
                name = v.Name,
                vaccination_count = v.Vaccinations.Count
            }).FirstAsync();

            if (data == null)
            {
                return NotFound();
            }

            return Ok(data);
        }

        // PUT: api/Vaccines/5
        [HttpPut("{id}")]
        [Authorize(Roles = "officer")]
        public async Task<IActionResult> Update(long id, VaccineInputDTO input)
        {

            _context.Entry(input.ToEntity(id)).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Exists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok();
        }

        // POST: api/Vaccines
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "officer")]
        public async Task<ActionResult<Vaccine>> Create(VaccineInputDTO input)
        {
            var newData = input.ToEntity();
            _context.Vaccines.Add(newData);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Get", new { id = newData.Id }, newData);
        }

        // DELETE: api/Vaccines/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "officer")]
        public async Task<IActionResult> Delete(long id)
        {
            var record = await _context.Vaccines.FindAsync(id);
            if (record == null)
            {
                return NotFound();
            }

            _context.Vaccines.Remove(record);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private bool Exists(long id)
        {
            return _context.Vaccines.Any(e => e.Id == id);
        }
    }
}
