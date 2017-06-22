using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;

namespace Demo.AspNetCore.WebSockets.Infrastructure
{
    public interface ITextWebSocketSubprotocol
    {
        string SubProtocol { get; }

        Task SendAsync(string message, WebSocket webSocket, CancellationToken cancellationToken);
    }
}
