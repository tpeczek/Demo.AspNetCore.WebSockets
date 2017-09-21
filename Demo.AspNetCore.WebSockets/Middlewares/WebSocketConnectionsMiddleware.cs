using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Demo.AspNetCore.WebSockets.Infrastructure;
using Demo.AspNetCore.WebSockets.Services;
using Lib.AspNetCore.WebSocketsCompression;
using Lib.AspNetCore.WebSocketsCompression.Providers;

namespace Demo.AspNetCore.WebSockets.Middlewares
{
    internal class WebSocketConnectionsMiddleware
    {
        #region Fields
        private WebSocketConnectionsOptions _options;
        private IWebSocketConnectionsService _connectionsService;
        private IWebSocketCompressionService _compressionService;
        #endregion

        #region Constructor
        public WebSocketConnectionsMiddleware(RequestDelegate next, WebSocketConnectionsOptions options, IWebSocketConnectionsService connectionsService, IWebSocketCompressionService compressionService)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _connectionsService = connectionsService ?? throw new ArgumentNullException(nameof(connectionsService));
            _compressionService = compressionService ?? throw new ArgumentNullException(nameof(compressionService));
        }
        #endregion

        #region Methods
        public async Task Invoke(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                if (ValidateOrigin(context))
                {
                    ITextWebSocketSubprotocol subProtocol = NegotiateSubProtocol(context.WebSockets.WebSocketRequestedProtocols);

                    IWebSocketCompressionProvider webSocketCompressionProvider = _compressionService.NegotiateCompression(context);

                    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync(subProtocol?.SubProtocol);

                    WebSocketConnection webSocketConnection = new WebSocketConnection(webSocket, webSocketCompressionProvider, subProtocol ?? _options.DefaultSubProtocol);
                    webSocketConnection.Receive += async (sender, message) => { await webSocketConnection.SendAsync(message, CancellationToken.None); };
                    _connectionsService.AddConnection(webSocketConnection);

                    WebSocketReceiveResult webSocketCloseResult = await ReceiveMessagesAsync(webSocket, webSocketCompressionProvider, webSocketConnection);

                    await webSocket.CloseAsync(webSocketCloseResult.CloseStatus.Value, webSocketCloseResult.CloseStatusDescription, CancellationToken.None);

                    _connectionsService.RemoveConnection(webSocketConnection.Id);
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                }
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        private bool ValidateOrigin(HttpContext context)
        {
            return (_options.AllowedOrigins == null) || (_options.AllowedOrigins.Count == 0) || (_options.AllowedOrigins.Contains(context.Request.Headers["Origin"].ToString()));
        }

        private ITextWebSocketSubprotocol NegotiateSubProtocol(IList<string> requestedSubProtocols)
        {
            ITextWebSocketSubprotocol subProtocol = null;

            foreach (ITextWebSocketSubprotocol supportedSubProtocol in _options.SupportedSubProtocols)
            {
                if (requestedSubProtocols.Contains(supportedSubProtocol.SubProtocol))
                {
                    subProtocol = supportedSubProtocol;
                    break;
                }
            }

            return subProtocol;
        }

        private async Task<WebSocketReceiveResult> ReceiveMessagesAsync(WebSocket webSocket, IWebSocketCompressionProvider webSocketCompressionProvider, WebSocketConnection webSocketConnection)
        {
            byte[] receivePayloadBuffer = new byte[_options.ReceivePayloadBufferSize];
            WebSocketReceiveResult webSocketReceiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(receivePayloadBuffer), CancellationToken.None);
            while (webSocketReceiveResult.MessageType != WebSocketMessageType.Close)
            {
                if (webSocketReceiveResult.MessageType == WebSocketMessageType.Binary)
                {
                    await webSocketCompressionProvider.DecompressBinaryMessageAsync(webSocket, webSocketReceiveResult, receivePayloadBuffer);
                }
                else
                {
                    string webSocketMessage = await webSocketCompressionProvider.DecompressTextMessageAsync(webSocket, webSocketReceiveResult, receivePayloadBuffer);
                    webSocketConnection.OnReceive(webSocketMessage);
                }

                webSocketReceiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(receivePayloadBuffer), CancellationToken.None);
            }

            return webSocketReceiveResult;
        }
        #endregion
    }
}
