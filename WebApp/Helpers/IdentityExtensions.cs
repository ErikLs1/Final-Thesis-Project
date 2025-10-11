using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace WebApp.Helpers;

public static class IdentityExtensions
{
    private static readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
    
    public static Guid GetUserId(this ClaimsPrincipal claimsPrincipal)
    {
        var userIdStr = claimsPrincipal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        var userId = Guid.Parse(userIdStr);
        return userId;
    }

    public static string GenerateJwt(
        IEnumerable<Claim> claims,
        string key,
        string issuer,
        string audience,
        DateTime expires)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512);
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: signingCredentials
        );
        return _jwtSecurityTokenHandler.WriteToken(token);
    }

    public static bool ValidateJwt(string jwt, string key, string issuer, string audience)
    {
        var validationParams = new TokenValidationParameters()
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ValidateIssuerSigningKey = true,
            
            ValidIssuer = issuer,
            ValidateIssuer = true,
            
            ValidAudience = audience,
            ValidateAudience = true,
            
            ValidateLifetime = false
        };

        try
        {
            new JwtSecurityTokenHandler().ValidateToken(jwt, validationParams, out _);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }
}