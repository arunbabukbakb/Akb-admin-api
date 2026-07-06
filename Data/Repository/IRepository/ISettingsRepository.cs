using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository.IRepository
{
	public interface ISettingsRepository
	{
		string HashPassword(string password);
		bool VerifyPassword(string enteredPassword, string storedPassword);
		int CustomerOtp();
	}
}
