namespace SocietyVaccinations
{
    public static class TokenBlacklister
    {
        private static List<string> tokens = new List<string>();

        public static void Ban(string token)
        {
            tokens.Add(token);
        }

        public static bool IsBanned(string token)
        {
            return tokens.Contains(token);
        }
    }
}
