namespace SocietyVaccinations
{
    public class TokenBlacklister
    {
        private List<string> tokens = new List<string>();

        public void Ban(string token)
        {
            tokens.Add(token);
        }

        public bool IsBanned(string token)
        {
            return tokens.Contains(token);
        }
    }
}
