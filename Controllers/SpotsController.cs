using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocietyVaccinations.Models;

namespace SocietyVaccinations.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class SpotsController : ControllerBase
    {
        SVContext dbc;
        public SpotsController(SVContext ctx) { dbc = ctx; }

        [HttpGet]
        async public Task<IActionResult> GetAll()
        {
            return Ok();
        }

        [HttpGet("{id}")]
        async public Task<IActionResult> Get(int id)
        {
            return Ok();
        }
    }
}
