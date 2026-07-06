﻿using Infrastructure.Models;

namespace Infrastructure.Services
{
    public interface IJWTManagerRepository
    {
        Tokens Authenticate(string? rolename, int userid, int? roleid, int? branchid, int? companyid);
    }
}
