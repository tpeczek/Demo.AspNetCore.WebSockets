using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using Newtonsoft.Json;

namespace Demo.AspNetCore.WebSockets.Infrastructure
{
    public class JsonWebSocketSubprotocol : TextWebSocketSubprotocolBase, ITextWebSocketSubprotocol
    {
        public string SubProtocol => "aspnetcore-ws.json";

        public override Task SendAsync(string message, WebSocket webSocket, CancellationToken cancellationToken)
        {
            string jsonMessage = JsonConvert.SerializeObject(new { message, timestamp = DateTime.UtcNow });

            return base.SendAsync(jsonMessage, webSocket, cancellationToken);
        }
    }
}
