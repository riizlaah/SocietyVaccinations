using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SocietyVaccinations.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SocietyVaccinations.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        SVContext dbc;
        IConfiguration conf;
        public AuthController(SVContext ctx, IConfiguration c) { dbc = ctx; conf = c; }

        [HttpPost("login")]
        async public Task<IActionResult> Login(SocietyLoginDTO input)
        {
            var user = dbc.Societies.Include(s => s.Regional).Where(s => s.IdCardNumber == input.id_card_number).FirstOrDefault();
            if (user == null) return Helper.bad("ID Card Number or password incorrect.");
            if (!verifyHash(input.password, user.Password)) return Helper.bad("ID Card Number or password incorrect.");
            var token = GenToken(input.id_card_number);
            return Ok(new
            {
                name = user.Name,
                born_date = user.BornDate,
                gender = user.Gender,
                address = user.Address,
                token = token,
                regional = new
                {
                    id = user.RegionalId,
                    province = user.Regional.Province,
                    district = user.Regional.District
                },
            });
        }

        [HttpPost("logout")]
        [Authorize]
        async public Task<IActionResult> Logout()
        {
            TokenBlacklister.Ban(User.FindFirstValue(JwtRegisteredClaimNames.Jti));
            return Ok();
        }

        private string GenToken(string id)
        {
            var claims = new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(conf["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: conf["Jwt:Issuer"],
                audience: conf["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string sha256(string s)
        {
            using (var alg = SHA256.Create())
            {
                var hashedBytes = alg.ComputeHash(Encoding.UTF8.GetBytes(s));
                var sb = new StringBuilder();
                foreach (var b in hashedBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        private bool verifyHash(string input, string hash)
        {
            var hashedInput = sha256(input);
            return StringComparer.OrdinalIgnoreCase.Compare(hashedInput, hash) == 0;
        }

    }
}
