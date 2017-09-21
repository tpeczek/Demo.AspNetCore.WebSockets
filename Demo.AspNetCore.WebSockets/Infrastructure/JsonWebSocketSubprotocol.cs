using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using Newtonsoft.Json;
using Lib.AspNetCore.WebSocketsCompression.Providers;

namespace Demo.AspNetCore.WebSockets.Infrastructure
{
    internal class JsonWebSocketSubprotocol : TextWebSocketSubprotocolBase, ITextWebSocketSubprotocol
    {
        public string SubProtocol => "aspnetcore-ws.json";

        public override Task SendAsync(string message, WebSocket webSocket, IWebSocketCompressionProvider webSocketCompressionProvider, CancellationToken cancellationToken)
        {
            string jsonMessage = JsonConvert.SerializeObject(new { message, timestamp = DateTime.UtcNow });

            return base.SendAsync(jsonMessage, webSocket, webSocketCompressionProvider, cancellationToken);
        }
    }
}
