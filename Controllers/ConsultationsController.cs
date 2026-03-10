using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocietyVaccinations.Models;

namespace SocietyVaccinations.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ConsultationsController : ControllerBase
    {
        SVContext dbc;
        public ConsultationsController(SVContext ctx) { dbc = ctx; }

        [HttpPost]
        [Authorize]
        async public Task<IActionResult> Create()
        {
            return Ok();
        }

        [HttpGet]
        async public Task<IActionResult> Get()
        {
            return Ok();
        }
    }
}
