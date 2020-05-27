using CrossCuttingExtensions.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CrossCuttingExtensions.Extensions
{
    public static class JwtExtensions
    {
        public static IServiceCollection ConfigureJwt(this IServiceCollection services, IConfiguration config)
        {
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(config.Get<JwtConfig>().JwtSection.Secret ?? "161803398874989484820456834365147951536641807789541135412469820148204635842")),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
            return services;
        }

        public static string GenerateToken(this string Id, IConfiguration config, string userName = "")
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, Id),
                    new Claim(ClaimTypes.Name, userName)
                }),
                Expires = DateTime.UtcNow.AddMinutes(20),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(config.Get<JwtConfig>()?.JwtSection?.Secret ?? "161803398874989484820456834365147951536641807789541135412469820148204635842")), SecurityAlgorithms.HmacSha256Signature)
            }));
        }
        public static JwtSecurityToken DecryptToken(this Microsoft.AspNetCore.Http.HttpContext context, ILogger logger = null)
        {
            try
            {
                if (context.Request.Headers.TryGetValue("Authorization", out Microsoft.Extensions.Primitives.StringValues value))
                    return new JwtSecurityTokenHandler().ReadJwtToken(value.ToArray()[0].Replace("Bearer ", ""));
                else
                    throw new ArgumentException("Authorization header wasn't present on request");
            }
            catch (Exception e)
            {
                logger?.LogError("Exception while decrypting token in JwtExtensions.DecryptToken", e);
                return null;
            }
        }
    }
}
