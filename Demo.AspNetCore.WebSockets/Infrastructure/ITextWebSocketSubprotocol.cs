using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Demo.AspNetCore.WebSockets.Infrastructure
{
    internal interface ITextWebSocketSubprotocol
    {
        string SubProtocol { get; }

        Task SendAsync(string message, Stream webSocketMessageStream, CancellationToken cancellationToken);

        string Read(Stream webSocketMessageStream);
    }
}
