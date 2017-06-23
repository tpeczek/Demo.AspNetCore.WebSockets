using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;

namespace Demo.AspNetCore.WebSockets.Infrastructure
{
    public abstract class TextWebSocketSubprotocolBase
    {
        public event EventHandler<string> Receive;

        public async virtual Task SendAsync(string message, WebSocket webSocket, CancellationToken cancellationToken)
        {
            if (webSocket.State == WebSocketState.Open)
            {
                ArraySegment<byte> buffer = new ArraySegment<byte>(Encoding.ASCII.GetBytes(message), 0, message.Length);

                await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken);
            }
        }

        public virtual string Read(byte[] bytes)
        {
            return Encoding.ASCII.GetString(bytes);
        }
    }
}
