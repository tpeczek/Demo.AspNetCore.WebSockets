using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Demo.AspNetCore.WebSockets.Infrastructure;

namespace Demo.AspNetCore.WebSockets.Services
{
    internal class WebSocketConnectionsService : IWebSocketConnectionsService
    {
        #region Fields
        private readonly ConcurrentDictionary<Guid, WebSocketConnection> _connections = new ConcurrentDictionary<Guid, WebSocketConnection>();
        #endregion

        #region Methods
        public void AddConnection(WebSocketConnection connection)
        {
            _connections.TryAdd(connection.Id, connection);
        }

        public void RemoveConnection(Guid connectionId)
        {
            WebSocketConnection connection;

            _connections.TryRemove(connectionId, out connection);
        }

        public Task SendToAllAsync(string message, CancellationToken cancellationToken)
        {
            List<Task> connectionsTasks = new List<Task>();
            foreach (WebSocketConnection connection in _connections.Values)
            {
                connectionsTasks.Add(connection.SendAsync(message, cancellationToken));
            }

            return Task.WhenAll(connectionsTasks);
        }
        #endregion
    }
}
