using Infrastructure.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Services
{
    public class JWTManagerRepository : IJWTManagerRepository
    {
        private readonly IConfiguration iconfiguration;
        public JWTManagerRepository(IConfiguration iconfiguration)
        {
            this.iconfiguration = iconfiguration;
        }
        public Tokens Authenticate(string? rolename, int userid, int? roleid, int? branchid, int? companyid)
        {
            // we generate JSON Web Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.UTF8.GetBytes(iconfiguration["JWT:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
              Subject = new ClaimsIdentity(new Claim[]
              {
                 new Claim(ClaimTypes.Role, rolename??""),
                 new Claim(ClaimTypes.Sid, roleid.ToString()),
                 new Claim(ClaimTypes.NameIdentifier, userid.ToString()),
                 new Claim("BranchId", branchid?.ToString() ?? "0"),
                 new Claim("CompanyId", companyid?.ToString() ?? "0")
              }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return new Tokens { Token = tokenHandler.WriteToken(token) };
        }
    }
}
