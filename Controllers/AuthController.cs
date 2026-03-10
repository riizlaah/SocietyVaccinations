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
            var user = await dbc.Societies.Include(s => s.Regional).Where(s => s.IdCardNumber == input.id_card_number).FirstOrDefaultAsync();
            if (user == null) return Helper.err("ID Card Number or password incorrect");
            if (!verifyHash(input.password, user.Password)) return Helper.err("ID Card Number or password incorrect");
            var token = GenToken(user.Id.ToString());
            (await dbc.Societies.FindAsync(user.Id)).LoginTokens = "," + token + ",";
            await dbc.SaveChangesAsync();
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
        //[Authorize]
        async public Task<IActionResult> Logout(string token)
        {
            var society = await dbc.Societies.Where(s => EF.Functions.Like(s.LoginTokens, $"%,{token},%")).FirstOrDefaultAsync();
            if (society == null) return Helper.err("Invalid token");
            society.LoginTokens = society.LoginTokens.Replace($"{token},", "");
            await dbc.SaveChangesAsync();
            return Ok(new {message = "Logout success" });
        }

        private string GenToken(string id)
        {
            using(var alg = MD5.Create())
            {
                var bytes = alg.ComputeHash(Encoding.UTF8.GetBytes(id));
                var sb = new StringBuilder();
                foreach(var b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            } 
            //var claims = new Claim[]
            //{
            //    new Claim(ClaimTypes.NameIdentifier, id),
            //    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            //};
            //var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(conf["Jwt:Key"]));
            //var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //var token = new JwtSecurityToken(
            //    issuer: conf["Jwt:Issuer"],
            //    audience: conf["Jwt:Audience"],
            //    claims: claims,
            //    expires: DateTime.Now.AddHours(1),
            //    signingCredentials: creds);

            //return new JwtSecurityTokenHandler().WriteToken(token);
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
