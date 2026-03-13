using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace SocietyVaccinations
{
    public class Helper
    {

        public static ObjectResult err(string msg, int code = 401)
        {
            return new ObjectResult(new {message = msg}) {
                StatusCode = code
            }
            ;
        }

        public static string sha256(string s)
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

        public static bool verifyHash(string input, string hash)
        {
            var hashedInput = sha256(input);
            return StringComparer.OrdinalIgnoreCase.Compare(hashedInput, hash) == 0;
        }
    }
}
