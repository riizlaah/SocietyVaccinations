using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        async public Task<IActionResult> Create()
        {
            return Ok();
        }

        [HttpGet]
        async public Task<IActionResult> GetAll()
        {
            return Ok();
        }
    }
}
