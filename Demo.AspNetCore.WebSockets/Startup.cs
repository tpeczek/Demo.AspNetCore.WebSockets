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

            ITextWebSocketSubprotocol textWebSocketSubprotocol = new PlainTextWebSocketSubprotocol();

            app.UseStaticFiles()
                .UseWebSockets()
                .MapWebSocketConnections("/socket", new WebSocketConnectionsOptions
                {
                    SupportedSubProtocols = new List<ITextWebSocketSubprotocol>
                    {
                        new JsonWebSocketSubprotocol(),
                        textWebSocketSubprotocol
                    },
                    DefaultSubProtocol = textWebSocketSubprotocol
                })
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
