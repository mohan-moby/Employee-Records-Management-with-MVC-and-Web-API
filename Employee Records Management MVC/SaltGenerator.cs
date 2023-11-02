using System;
using System.Security.Cryptography;
using System.Text;

namespace Employee_Records_Management_MVC
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

        public static string CreateSalt(int size)
        {
            //Generate a cryptographic random number.
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[size];
            rng.GetBytes(buff);

            // Return a Base64 string representation of the random number.
            return Convert.ToBase64String(buff);
        }

    }
}
