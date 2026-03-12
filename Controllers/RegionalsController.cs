using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocietyVaccinations.Models;

namespace SocietyVaccinations.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class RegionalsController : ControllerBase
    {
        private readonly SVContext _context;

        public RegionalsController(SVContext context)
        {
            _context = context;
        }

        // GET: api/Regionals
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var regionals = await _context.Regionals.ToListAsync();
            return Ok(regionals.Select(r => new
            {
                id = r.Id,
                province = r.Province,
                district = r.District
            }));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Regional>> Get(long id)
        {
            var regional = await _context.Regionals.FindAsync(id);

            if (regional == null)
            {
                return NotFound();
            }

            return regional;
        }

        // PUT: api/Regionals/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(long id, RegionalInputDTO regional)
        {

            _context.Entry(regional.ToRegional(id)).State = EntityState.Modified;

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

        // POST: api/Regionals
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Regional>> Create(RegionalInputDTO regional)
        {
            var reg = regional.ToRegional();
            _context.Regionals.Add(reg);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Get", new { id = reg.Id }, reg);
        }

        // DELETE: api/Regionals/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(long id)
        {
            var regional = await _context.Regionals.FindAsync(id);
            if (regional == null)
            {
                return NotFound();
            }

            _context.Regionals.Remove(regional);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private bool Exists(long id)
        {
            return _context.Regionals.Any(e => e.Id == id);
        }
    }
}
