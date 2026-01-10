using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Demo.AspNetCore.WebSockets.Infrastructure
{
    internal class JsonWebSocketSubprotocol : TextWebSocketSubprotocolBase, ITextWebSocketSubprotocol
    {
        public string SubProtocol => "aspnetcore-ws.json";

        public override Task SendAsync(string message, Stream webSocketMessageStream, CancellationToken cancellationToken)
        {
            var jsonMessage = new { message, timestamp = DateTime.UtcNow };

            return JsonSerializer.SerializeAsync(webSocketMessageStream, jsonMessage, cancellationToken: cancellationToken);
        }
    }
}
