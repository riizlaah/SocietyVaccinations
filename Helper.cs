using Microsoft.AspNetCore.Mvc;

namespace SocietyVaccinations
{
    public class Helper
    {

        public static BadRequestObjectResult bad(string msg)
        {
            return new BadRequestObjectResult(new { message = msg });
        }
    }
}
