using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System;

namespace Demo.AspNetCore.WebSockets.Infrastructure
{
    public class WebSocketConnection
    {
        #region Fields
        private WebSocket _webSocket;
        private ITextWebSocketSubprotocol _subProtocol;
        #endregion

        #region Properties
        public Guid Id => Guid.NewGuid();
        #endregion

        #region Constructor
        public WebSocketConnection(WebSocket webSocket, ITextWebSocketSubprotocol subProtocol)
        {
            _webSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
            _subProtocol = subProtocol ?? throw new ArgumentNullException(nameof(subProtocol));
        }
        #endregion

        #region Methods
        public Task SendAsync(string message, CancellationToken cancellationToken)
        {
            return _subProtocol.SendAsync(message, _webSocket, cancellationToken);
        }
        #endregion
    }
}
