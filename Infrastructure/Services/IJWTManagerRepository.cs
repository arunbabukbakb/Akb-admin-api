using Infrastructure.Models;
using System.Security.Claims;

namespace Infrastructure.Services
{
    public interface IJWTManagerRepository
    {
        Tokens Authenticate(string? rolename, int userid, int? roleid, int? branchid, int? companyid);
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token);
        string GenerateRefreshToken();
    }
}
