using System.Security.Cryptography;
using System.Text;

namespace Employee_Records_Management_Web_API.Repositories
{
    public static class SaltGenerator
    {
        public static string GenerateSalt()
        {
            const int saltLength = 10; // 10-digit salt

            const string validChars = "0123456789";

            StringBuilder salt = new StringBuilder(saltLength);
            RandomNumberGenerator rng = RandomNumberGenerator.Create();

            byte[] randomBytes = new byte[saltLength];
            rng.GetBytes(randomBytes);

            foreach (byte b in randomBytes)
            {
                salt.Append(validChars[b % validChars.Length]);
            }

            return salt.ToString();
        }
    }
}