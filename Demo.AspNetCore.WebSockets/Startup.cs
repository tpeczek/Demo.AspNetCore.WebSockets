using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Demo.AspNetCore.WebSockets.Infrastructure;
using Demo.AspNetCore.WebSockets.Services;
using Demo.AspNetCore.WebSockets.Middlewares;

using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Demo.AspNetCore.WebSockets
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddWebSocketCompression();
            services.AddWebSocketConnections();

            services.AddSingleton<IHostedService, HeartbeatService>();
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
                DefaultSubProtocol = textWebSocketSubprotocol,
                SendSegmentSize = 4 * 1024
            };

            app.UseDefaultFiles(defaultFilesOptions)
                .UseStaticFiles()
                .UseWebSocketsCompression()
                .MapWebSocketConnections("/socket", webSocketConnectionsOptions)
                .Run(async (context) =>
                {
                    await context.Response.WriteAsync("-- Demo.AspNetCore.WebSocket --");
                });
        }
    }
}
