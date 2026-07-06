﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository.IRepository
{
	public interface IUserService
	{
		int GetUserId();
		int GetBranchId();
		int GetCompanyId();
		void LogInsert(string level, string message);
	}
}
