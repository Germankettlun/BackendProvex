using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;

public static class JwtHelper
{
    public static string GenerateToken(
        string issuer,
        string audience,
        string secretKey,
        IEnumerable<Claim> claims,
        int expiresMinutes)
    {

        try
        {
            Debug.WriteLine($"Opciones de token ussuer : {issuer}, audience : {audience} , key : {secretKey}");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                //Expires = DateTime.UtcNow.AddMinutes(expiresMinutes),
                Expires = DateTime.UtcNow.AddYears(1),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            Debug.WriteLine("Token creado");
            return tokenHandler.WriteToken(token);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al generar Token: {ex.Message}");
            return null; // Added return statement here
        }
    }
}
