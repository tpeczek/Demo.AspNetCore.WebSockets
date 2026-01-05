using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Demo.AspNetCore.WebSockets.Services;
using Demo.AspNetCore.WebSockets.Middlewares;
using Demo.AspNetCore.WebSockets.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddWebSocketConnections();

builder.Services.AddSingleton<IHostedService, HeartbeatService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

DefaultFilesOptions defaultFilesOptions = new();
defaultFilesOptions.DefaultFileNames.Clear();
defaultFilesOptions.DefaultFileNames.Add("websocket-api.html");

ITextWebSocketSubprotocol textWebSocketSubprotocol = new PlainTextWebSocketSubprotocol();
WebSocketConnectionsOptions webSocketConnectionsOptions = new()
{
    AllowedOrigins = ["http://localhost:63290"],
    SupportedSubProtocols = [new JsonWebSocketSubprotocol(), textWebSocketSubprotocol],
    DefaultSubProtocol = textWebSocketSubprotocol,
    SendSegmentSize = 4 * 1024
};

app.UseDefaultFiles(defaultFilesOptions);
app.UseStaticFiles();

app.UseWebSockets();
app.MapWebSocketConnections("/socket", webSocketConnectionsOptions);

app.Run();