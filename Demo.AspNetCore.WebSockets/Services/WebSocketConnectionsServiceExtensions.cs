using Microsoft.Extensions.DependencyInjection;

namespace Demo.AspNetCore.WebSockets.Services
{
    internal static class WebSocketConnectionsServiceExtensions
    {
        public static IServiceCollection AddWebSocketConnections(this IServiceCollection services)
        {
            services.AddSingleton<WebSocketConnectionsService>();
            services.AddSingleton<IWebSocketConnectionsService>(serviceProvider => serviceProvider.GetService<WebSocketConnectionsService>());

            return services;
        }
    }
}
