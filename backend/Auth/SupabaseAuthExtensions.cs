using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Auth
{
    public static class SupabaseAuthExtensions
    {
        public static IServiceCollection AddSupabaseAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var symmetricSecurityKeyBytes = Encoding.UTF8.GetBytes(configuration["Authentication:JwtSecret"]!);
            services.AddAuthentication().AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(symmetricSecurityKeyBytes),
                    ValidAudience = configuration["Authentication:ValidAudience"],
                    ValidIssuer = configuration["Authentication:ValidIssuer"]
                };
            });
            return services;
        }
    }
}
