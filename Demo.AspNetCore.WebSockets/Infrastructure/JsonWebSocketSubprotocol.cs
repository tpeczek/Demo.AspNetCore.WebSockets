using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using Newtonsoft.Json;

namespace Demo.AspNetCore.WebSockets.Infrastructure
{
    public class JsonWebSocketSubprotocol : ITextWebSocketSubprotocol
    {
        public string SubProtocol => "aspnetcore-ws.json";

        public async Task SendAsync(string message, WebSocket webSocket, CancellationToken cancellationToken)
        {
            if (webSocket.State == WebSocketState.Open)
            {
                string jsonMessage = JsonConvert.SerializeObject(new { message, timestamp = DateTime.UtcNow });
                ArraySegment<byte> buffer = new ArraySegment<byte>(Encoding.ASCII.GetBytes(jsonMessage), 0, jsonMessage.Length);

                await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken);
            }
        }
    }
}
