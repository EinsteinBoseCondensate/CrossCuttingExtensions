using CrossCuttingExtensions;
using Microsoft.Extensions.DependencyInjection;

namespace CrossCuttingExtensions.Extensions
{
    public static class CorsExtensions
    {
        public static IServiceCollection ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(o => o.AddPolicy(InfrastructureConstants.DefaultCorsPolicy, builder =>
            {
                builder
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowedToAllowWildcardSubdomains()
                .SetIsOriginAllowed((host) => true).AllowCredentials();
            }));
            return services;
        }
    }
}
