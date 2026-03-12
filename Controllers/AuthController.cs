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
        TokenBlacklister blacklister;
        public AuthController(SVContext ctx, IConfiguration c, TokenBlacklister tb) { dbc = ctx; conf = c; blacklister = tb; }

        [HttpPost("login")]
        async public Task<IActionResult> Login(SocietyLoginDTO input)
        {
            var user = await dbc.Societies.Include(s => s.Regional).Where(s => s.IdCardNumber == input.id_card_number).FirstOrDefaultAsync();
            if (user == null) return Helper.err("ID Card Number or password incorrect");
            if (!Helper.verifyHash(input.password, user.Password)) return Helper.err("ID Card Number or password incorrect");
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

        [HttpPost("medical/login")]
        async public Task<IActionResult> MedicalLogin(MedicalLoginDTO input)
        {
            var medical = await dbc.Medicals.Include(m => m.User).Include(m => m.Spot.Regional).Where(s => s.User.Username == input.username).FirstOrDefaultAsync();
            if (medical == null) return Helper.err("Username or password incorrect");
            if (!Helper.verifyHash(input.password, medical.User.Password)) return Helper.err("Username or password incorrect");
            var token = GenJWT(medical.Id.ToString(), medical.Role);
            await dbc.SaveChangesAsync();
            return Ok(new
            {
                name = medical.Name,
                role = medical.Role,
                token = token,
                spot = new
                {
                    id = medical.SpotId,
                    name = medical.Name,
                    regional = new
                    {
                        id = medical.Spot.RegionalId,
                        province = medical.Spot.Regional.Province,
                        district = medical.Spot.Regional.District
                    }
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

        [HttpPost("medical/logout")]
        [Authorize]
        async public Task<IActionResult> MedicalLogout()
        {
            var jwtId = User.FindFirstValue(JwtRegisteredClaimNames.Jti);
            if (jwtId != null)
            {
                blacklister.Ban(jwtId);
                return Ok(new { message = "Logout success" });
            }
            return Helper.err("Token not valid");
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
        }

        private string GenJWT(string id, string role)
        {
            var claims = new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, id),
                new Claim(ClaimTypes.Role, role),
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

        

    }
}
