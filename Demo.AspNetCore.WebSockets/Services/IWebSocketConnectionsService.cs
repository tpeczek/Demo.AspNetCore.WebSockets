using System;
using System.Threading;
using System.Threading.Tasks;
using Demo.AspNetCore.WebSockets.Infrastructure;

namespace Demo.AspNetCore.WebSockets.Services
{
    internal interface IWebSocketConnectionsService
    {
        void AddConnection(WebSocketConnection connection);

        void RemoveConnection(Guid connectionId);

        Task SendToAllAsync(string message, CancellationToken cancellationToken);
    }
}
