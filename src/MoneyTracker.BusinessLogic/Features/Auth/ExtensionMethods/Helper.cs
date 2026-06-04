using Microsoft.IdentityModel.Tokens;
using MoneyTracker.BusinessLogic.Features.Auth.Options;
using MoneyTracker.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MoneyTracker.BusinessLogic.Features.Auth.ExtensionMethods
{
    internal static class Helper
    {
        public static string BuildToken(this User user, JwtOptions jwtOptions)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.UtcNow.AddMinutes(jwtOptions.ExpiryMinutes);
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(AuthAuthorization.PermissionClaimType, AuthAuthorization.Permissions.Read),
                new Claim(AuthAuthorization.PermissionClaimType, AuthAuthorization.Permissions.Write),
            };

            var token = new JwtSecurityToken(
                issuer: jwtOptions.Issuer,
                audience: jwtOptions.Audience,
                claims: claims,
                expires: expiry,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
