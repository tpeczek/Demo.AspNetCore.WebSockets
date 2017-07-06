using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Demo.AspNetCore.WebSockets.Infrastructure;
using Demo.AspNetCore.WebSockets.Services;
using Demo.AspNetCore.WebSockets.Middlewares;

namespace Demo.AspNetCore.WebSockets
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddWebSocketConnections();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            DefaultFilesOptions defaultFilesOptions = new DefaultFilesOptions();
            defaultFilesOptions.DefaultFileNames.Clear();
            defaultFilesOptions.DefaultFileNames.Add("websocket-api.html");

            ITextWebSocketSubprotocol textWebSocketSubprotocol = new PlainTextWebSocketSubprotocol();
            WebSocketConnectionsOptions webSocketConnectionsOptions = new WebSocketConnectionsOptions
            {
                AllowedOrigins = new HashSet<string> { "http://localhost:63290" },
                SupportedSubProtocols = new List<ITextWebSocketSubprotocol>
                {
                    new JsonWebSocketSubprotocol(),
                    textWebSocketSubprotocol
                },
                DefaultSubProtocol = textWebSocketSubprotocol
            };

            app.UseDefaultFiles(defaultFilesOptions)
                .UseStaticFiles()
                .UseWebSockets()
                .MapWebSocketConnections("/socket", webSocketConnectionsOptions)
                .Run(async (context) =>
                {
                    await context.Response.WriteAsync("-- Demo.AspNetCore.WebSocket --");
                });

            // Only for demo purposes, don't do this kind of thing to your production
            IWebSocketConnectionsService webSocketConnectionsService = serviceProvider.GetService<IWebSocketConnectionsService>();
            System.Threading.Thread webSocketHeartbeatThread = new System.Threading.Thread(new System.Threading.ThreadStart(() =>
            {
                while (true)
                {
                    webSocketConnectionsService.SendToAllAsync("Demo.AspNetCore.WebSockets Heartbeat", System.Threading.CancellationToken.None).Wait();
                    System.Threading.Thread.Sleep(5000);
                }
            }));
            webSocketHeartbeatThread.Start();
        }
    }
}
