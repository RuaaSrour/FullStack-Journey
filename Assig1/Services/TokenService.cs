using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Assig1.Models;
using Microsoft.IdentityModel.Tokens;

namespace Assig1.Services
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public (string Token, DateTime ExpiresAt) GenerateToken(
            UserAccount user)
        {
            List<Claim> claims = new()
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    user.Username),

                new Claim(
                    ClaimTypes.Role,
                    user.Role.ToString())
            };

            if (user.StudentId.HasValue)
            {
                claims.Add(
                    new Claim(
                        "studentId",
                        user.StudentId.Value.ToString()));
            }

            if (user.TeacherId.HasValue)
            {
                claims.Add(
                    new Claim(
                        "teacherId",
                        user.TeacherId.Value.ToString()));
            }

            IConfigurationSection jwtSettings =
                _configuration.GetSection("Jwt");

            string jwtKey = jwtSettings["Key"]
                ?? throw new InvalidOperationException(
                    "JWT key is missing.");

            string issuer = jwtSettings["Issuer"]
                ?? throw new InvalidOperationException(
                    "JWT issuer is missing.");

            string audience = jwtSettings["Audience"]
                ?? throw new InvalidOperationException(
                    "JWT audience is missing.");

            if (!double.TryParse(
                    jwtSettings["ExpiryMinutes"],
                    out double expiryMinutes))
            {
                throw new InvalidOperationException(
                    "JWT expiry is invalid.");
            }

            SymmetricSecurityKey key = new(
                Encoding.UTF8.GetBytes(jwtKey));

            SigningCredentials credentials = new(
                key,
                SecurityAlgorithms.HmacSha256);

            DateTime expiresAt =
                DateTime.UtcNow.AddMinutes(expiryMinutes);

            JwtSecurityToken token = new(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiresAt,
                signingCredentials: credentials);

            string tokenValue =
                new JwtSecurityTokenHandler().WriteToken(token);

            return (tokenValue, expiresAt);
        }
    }
}