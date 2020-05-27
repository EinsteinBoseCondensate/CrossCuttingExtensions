using CrossCuttingExtensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrossCuttingExtensions.Extensions
{
    public static class ApiSignalRExtensions
    {
        public static IServiceCollection ConfigureSignalR(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSignalR(hubConfiguration => hubConfiguration.EnableDetailedErrors = configuration.Get<SignalRConfig>()?.SignalRSection?.EnableDetailedErrors ?? false);
            return services;
        }
        public static IApplicationBuilder UseAppDefaults<T>(this IApplicationBuilder app, string hubRoom) where T : Hub
        {
            app.UseRouting().UseCors(InfrastructureConstants.DefaultCorsPolicy)//.UseHttpsRedirection()
                .Use((context, next) =>
                {
                    context.Items["__CorsMiddlewareInvoked"] = true;
                    return next();
                })
                .UseAuthentication().UseAuthorization().UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapHub<T>(hubRoom);
                });
            return app;
        }
    }
}
