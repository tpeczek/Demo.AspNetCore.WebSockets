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
        private int _receivePayloadBufferSize;
        #endregion

        #region Properties
        public Guid Id => Guid.NewGuid();

        public WebSocketCloseStatus? CloseStatus { get; private set; } = null;

        public string CloseStatusDescription { get; private set; } = null;
        #endregion

        #region Events
        public event EventHandler<string> Receive;
        #endregion

        #region Constructor
        public WebSocketConnection(WebSocket webSocket, IWebSocketCompressionProvider webSocketCompressionProvider, ITextWebSocketSubprotocol subProtocol, int receivePayloadBufferSize)
        {
            _webSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
            _webSocketCompressionProvider = webSocketCompressionProvider ?? throw new ArgumentNullException(nameof(webSocketCompressionProvider));
            _subProtocol = subProtocol ?? throw new ArgumentNullException(nameof(subProtocol));
            _receivePayloadBufferSize = receivePayloadBufferSize;
        }
        #endregion

        #region Methods
        public Task SendAsync(string message, CancellationToken cancellationToken)
        {
            return _subProtocol.SendAsync(message, _webSocket, _webSocketCompressionProvider, cancellationToken);
        }

        public async Task ReceiveMessagesUntilCloseAsync()
        {
            try
            {
                byte[] receivePayloadBuffer = new byte[_receivePayloadBufferSize];
                WebSocketReceiveResult webSocketReceiveResult = await _webSocket.ReceiveAsync(new ArraySegment<byte>(receivePayloadBuffer), CancellationToken.None);
                while (webSocketReceiveResult.MessageType != WebSocketMessageType.Close)
                {
                    if (webSocketReceiveResult.MessageType == WebSocketMessageType.Binary)
                    {
                        await _webSocketCompressionProvider.DecompressBinaryMessageAsync(_webSocket, webSocketReceiveResult, receivePayloadBuffer);
                    }
                    else
                    {
                        string webSocketMessage = await _webSocketCompressionProvider.DecompressTextMessageAsync(_webSocket, webSocketReceiveResult, receivePayloadBuffer);
                        OnReceive(webSocketMessage);
                    }

                    webSocketReceiveResult = await _webSocket.ReceiveAsync(new ArraySegment<byte>(receivePayloadBuffer), CancellationToken.None);
                }

                CloseStatus = webSocketReceiveResult.CloseStatus.Value;
                CloseStatusDescription = webSocketReceiveResult.CloseStatusDescription;
            }
            catch (WebSocketException wsex) when (wsex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
            { }
        }

        private void OnReceive(string webSocketMessage)
        {
            string message = _subProtocol.Read(webSocketMessage);

            Receive?.Invoke(this, message);
        }
        #endregion
    }
}
