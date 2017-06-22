using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;

namespace Demo.AspNetCore.WebSockets.Infrastructure
{
    public class PlainTextWebSocketSubprotocol : ITextWebSocketSubprotocol
    {
        public string SubProtocol => "aspnetcore-ws.plaintext";

        public async Task SendAsync(string message, WebSocket webSocket, CancellationToken cancellationToken)
        {
            if (webSocket.State == WebSocketState.Open)
            {
                ArraySegment<byte> buffer = new ArraySegment<byte>(Encoding.ASCII.GetBytes(message), 0, message.Length);

                await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken);
            }
        }
    }
}
