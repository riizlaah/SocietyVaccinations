using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SocietyVaccinations.Models;

namespace SocietyVaccinations.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class SocietiesController : ControllerBase
    {
        private readonly SVContext _context;

        public SocietiesController(SVContext context)
        {
            _context = context;
        }

        // GET: api/Societies
        [HttpGet]
        [Authorize(Roles = "officer")]
        public async Task<IActionResult> GetAll()
        {
            var datas = await _context.Societies.Include(s => s.Regional).Select(s => new
            {
                id = s.Id,
                name = s.Name,
                id_card_number = s.IdCardNumber,
                born_date = s.BornDate,
                address = s.Address,
                regional = new
                {
                    id = s.RegionalId,
                    province = s.Regional.Province,
                    district = s.Regional.District,
                }
            }).ToListAsync();
            return Ok(datas);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "officer")]
        public async Task<IActionResult> Get(long id)
        {
            var data = await _context.Societies.Where(s => s.Id == id).Include(s => s.Regional).Select(s => new
            {
                id = s.Id,
                name = s.Name,
                id_card_number = s.IdCardNumber,
                born_date = s.BornDate,
                address = s.Address,
                regional = new
                {
                    id = s.RegionalId,
                    province = s.Regional.Province,
                    district = s.Regional.District,
                }
            }).FirstAsync();

            if (data == null)
            {
                return NotFound();
            }

            return Ok(data);
        }

        // PUT: api/Societies/5
        [HttpPut("{id}")]
        [Authorize(Roles = "officer")]
        public async Task<IActionResult> Update(long id, SocietyInputDTO input)
        {
            if (!await _context.Regionals.AnyAsync(r => r.Id == input.regionalId)) return Helper.err("Regional not found");
            var data = await _context.Societies.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            if (data == null) return Helper.err("Society not found");
            if (await _context.Societies.AnyAsync(s => s.IdCardNumber == input.id_card_number && s.Id != data.Id)) return Helper.err("Id card number has been used");
            var password = input.password.IsNullOrEmpty() ? data.Password : Helper.sha256(input.password);

            _context.Entry(input.ToEntity(id, password)).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok();
        }

        // POST: api/Societies
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "officer")]
        public async Task<ActionResult<Vaccine>> Create(SocietyInputDTO input)
        {
            if (!await _context.Regionals.AnyAsync(r => r.Id == input.regionalId)) return Helper.err("Regional not found");
            if (input.password.IsNullOrEmpty()) return Helper.err("Password is required");
            if (input.password.Length < 8) return Helper.err("Password must contain at least 8 characters");
            var newData = input.ToEntity();
            _context.Societies.Add(newData);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Get", new { id = newData.Id }, newData);
        }

        // DELETE: api/Societies/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "officer")]
        public async Task<IActionResult> Delete(long id)
        {
            var record = await _context.Societies.FindAsync(id);
            if (record == null)
            {
                return NotFound();
            }

            _context.Societies.Remove(record);
            await _context.SaveChangesAsync();

            return Ok();
        }

    }
}
