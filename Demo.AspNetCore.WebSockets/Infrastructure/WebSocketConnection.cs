using System;
using System.IO;
using System.Net.WebSockets;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Demo.AspNetCore.WebSockets.Infrastructure
{
    internal class WebSocketConnection
    {
        #region Fields
        private readonly WebSocket _webSocket;
        private readonly ITextWebSocketSubprotocol _textSubProtocol;
        #endregion

        #region Properties
        public Guid Id { get; } = Guid.NewGuid();

        public WebSocketCloseStatus? CloseStatus { get; private set; } = null;

        public string CloseStatusDescription { get; private set; } = null;
        #endregion

        #region Events
        public event EventHandler<string> Receive;
        #endregion

        #region Constructor
        public WebSocketConnection(WebSocket webSocket, ITextWebSocketSubprotocol textSubProtocol)
        {
            _webSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
            _textSubProtocol = textSubProtocol ?? throw new ArgumentNullException(nameof(textSubProtocol));
        }
        #endregion

        #region Methods
        public Task SendAsync(string message, CancellationToken cancellationToken)
        {
            using Stream webSocketMessageStream = WebSocketStream.CreateWritableMessageStream(_webSocket, WebSocketMessageType.Text);
            return _textSubProtocol.SendAsync(message, webSocketMessageStream, cancellationToken);
        }       

        public async Task ReceiveMessagesUntilCloseAsync()
        {
            try
            {
                while (_webSocket.State != WebSocketState.CloseReceived)
                {
                    using Stream webSocketMessageStream = WebSocketStream.CreateReadableMessageStream(_webSocket);
                    string message = _textSubProtocol.Read(webSocketMessageStream);

                    Receive?.Invoke(this, message);
                }

                CloseStatus = _webSocket.CloseStatus.Value;
                CloseStatusDescription = _webSocket.CloseStatusDescription;
            }
            catch (WebSocketException wsex) when (wsex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
            { }
        }
        #endregion
    }
}
