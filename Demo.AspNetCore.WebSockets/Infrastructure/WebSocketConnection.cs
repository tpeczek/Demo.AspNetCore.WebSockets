using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using Lib.AspNetCore.WebSocketsCompression.Providers;

namespace Demo.AspNetCore.WebSockets.Infrastructure
{
    internal class WebSocketConnection
    {
        #region Fields
        private WebSocket _webSocket;
        private IWebSocketCompressionProvider _webSocketCompressionProvider;
        private ITextWebSocketSubprotocol _subProtocol;
        #endregion

        #region Properties
        public Guid Id => Guid.NewGuid();
        #endregion

        #region Events
        public event EventHandler<string> Receive;
        #endregion

        #region Constructor
        public WebSocketConnection(WebSocket webSocket, IWebSocketCompressionProvider webSocketCompressionProvider, ITextWebSocketSubprotocol subProtocol)
        {
            _webSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
            _webSocketCompressionProvider = webSocketCompressionProvider ?? throw new ArgumentNullException(nameof(webSocketCompressionProvider));
            _subProtocol = subProtocol ?? throw new ArgumentNullException(nameof(subProtocol));
        }
        #endregion

        #region Methods
        public Task SendAsync(string message, CancellationToken cancellationToken)
        {
            return _subProtocol.SendAsync(message, _webSocket, _webSocketCompressionProvider, cancellationToken);
        }

        public void OnReceive(string webSocketMessage)
        {
            string message = _subProtocol.Read(webSocketMessage);

            Receive?.Invoke(this, message);
        }
        #endregion
    }
}
