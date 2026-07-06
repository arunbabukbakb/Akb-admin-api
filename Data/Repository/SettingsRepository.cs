using Data.Repository.IRepository;
using System.Security.Cryptography;

namespace Data.Repository
{
	public class SettingsRepository:ISettingsRepository
	{
		private const int SaltSize = 16;
		private const int HashSize = 32;
		private const int Iterations = 10000;
		private static readonly Random _random = new Random();

		public int CustomerOtp()
		{
			return _random.Next(100000, 1000000);
		}

		public string HashPassword(string password="")
		{
			// Generate a random salt
			byte[] salt;
			//new RNGCryptoServiceProvider().GetBytes(salt = new byte[SaltSize]);
			RandomNumberGenerator.Create().GetBytes(salt = new byte[SaltSize]);

			// Create a hash using PBKDF2
			var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations);
			byte[] hash = pbkdf2.GetBytes(HashSize);

			// Combine the salt and hash
			byte[] hashBytes = new byte[SaltSize + HashSize];
			Array.Copy(salt, 0, hashBytes, 0, SaltSize);
			Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

			// Convert the byte array to a base64-encoded string
			string hashedPassword = Convert.ToBase64String(hashBytes);

			return hashedPassword;
		}

		public bool VerifyPassword(string enteredPassword, string storedPassword)
		{
			// Convert the stored password from base64 to byte array
			byte[] hashBytes = Convert.FromBase64String(storedPassword);

			// Extract the salt from the stored password
			byte[] salt = new byte[SaltSize];
			Array.Copy(hashBytes, 0, salt, 0, SaltSize);

			// Compute the hash using the entered password and the stored salt
			var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, Iterations);
			byte[] hash = pbkdf2.GetBytes(HashSize);

			// Compare the computed hash with the stored hash
			for (int i = 0; i < HashSize; i++)
			{
				if (hashBytes[i + SaltSize] != hash[i])
					return false;
			}

			return true;
		}
	}
}
