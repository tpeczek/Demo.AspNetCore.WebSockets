using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using Lib.AspNetCore.WebSocketsCompression.Providers;

namespace Demo.AspNetCore.WebSockets.Infrastructure
{
    internal abstract class TextWebSocketSubprotocolBase
    {
        public virtual Task SendAsync(string message, WebSocket webSocket, IWebSocketCompressionProvider webSocketCompressionProvider, CancellationToken cancellationToken)
        {
            return webSocketCompressionProvider.CompressTextMessageAsync(webSocket, message, cancellationToken);
        }

        public virtual string Read(string webSocketMessage)
        {
            return webSocketMessage;
        }
    }
}
