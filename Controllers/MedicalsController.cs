using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SocietyVaccinations.Models;

namespace SocietyVaccinations.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class MedicalsController : ControllerBase
    {
        private readonly SVContext _context;

        public MedicalsController(SVContext context)
        {
            _context = context;
        }

        // GET: api/Medicals
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var datas = await _context.Medicals.Include(m => m.Spot).ToListAsync();
            return Ok(datas.Select(d => new {
                id = d.Id,
                name = d.Name,
                role = d.Role,
                spot = new
                {
                    id = d.SpotId,
                    name = d.Spot.Name,
                    address = d.Spot.Address
                }
            }));
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(long id)
        {
            var medical = await _context.Medicals.AsQueryable().AsNoTrackingWithIdentityResolution().Include(m => m.User).Include(m => m.Spot).FirstOrDefaultAsync(m => m.Id == id);

            if (medical == null)
            {
                return NotFound();
            }

            return Ok(new {
                id = medical.Id,
                name = medical.Name,
                role = medical.Role,
                user = new
                {
                    id = medical.UserId,
                    username = medical.User.Username
                },
                spot = new
                {
                    id = medical.SpotId,
                    name = medical.Spot.Name,
                    address = medical.Spot.Address
                }
            });
        }

        // PUT: api/Medicals/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(long id, MedicalInputDTO input)
        {
            var data = await _context.Medicals.AsNoTracking().Include(m => m.User).FirstOrDefaultAsync(m => m.Id == id);
            if (data == null) return Helper.err("Medical not found");
            if (await _context.Users.AnyAsync(u => u.Username == input.username && u.Id != data.UserId)) return Helper.err("Username has been used");
            var password = input.password.IsNullOrEmpty() ? data.User.Password : Helper.sha256(input.password);
            _context.Entry(input.Update(id, data.UserId, password)).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // POST: api/Medicals
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Regional>> Create(MedicalInputDTO input)
        {
            if (!await _context.Spots.AnyAsync(s => s.Id == input.spotId)) return Helper.err("Spot not found");
            if (await _context.Users.AnyAsync(u => u.Username == input.username)) return Helper.err("Username has been used");
            if (input.password.IsNullOrEmpty()) return Helper.err("Password is required");
            if (input.password.Length < 8) return Helper.err("Password must contain at least 8 characters");
            var data = input.New();
            var user = input.NewUser();
            _context.Users.Add(user);
            _context.SaveChanges();
            data.User = user;
            data.UserId = user.Id;
            _context.Medicals.Add(data);
            await _context.SaveChangesAsync();
            return CreatedAtAction("Get", new { id = data.Id }, data);
        }

        // DELETE: api/Medicals/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(long id)
        {
            var medical = await _context.Medicals.FindAsync(id);
            if (medical == null)
            {
                return NotFound();
            }

            _context.Medicals.Remove(medical);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private bool Exists(long id)
        {
            return _context.Medicals.Any(e => e.Id == id);
        }
    }
}
