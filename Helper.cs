using Microsoft.AspNetCore.Mvc;

namespace SocietyVaccinations
{
    public class Helper
    {

        public static ContentResult err(string msg, int code = 401)
        {
            return new ContentResult
            {
                ContentType = "application/json",
                Content = "{\"message\": \"" + msg + "\"}",
                StatusCode = code
            };
        }
    }
}
