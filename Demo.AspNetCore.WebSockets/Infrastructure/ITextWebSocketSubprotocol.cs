using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using Lib.AspNetCore.WebSocketsCompression.Providers;

namespace Demo.AspNetCore.WebSockets.Infrastructure
{
    internal interface ITextWebSocketSubprotocol
    {
        string SubProtocol { get; }

        Task SendAsync(string message, WebSocket webSocket, IWebSocketCompressionProvider webSocketCompressionProvider, CancellationToken cancellationToken);

        string Read(string rawMessage);
    }
}
