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
                    ITextWebSocketSubprotocol textSubProtocol = NegotiateSubProtocol(context.WebSockets.WebSocketRequestedProtocols);

                    IWebSocketCompressionProvider webSocketCompressionProvider = _compressionService.NegotiateCompression(context);

                    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync(textSubProtocol?.SubProtocol);

                    WebSocketConnection webSocketConnection = new WebSocketConnection(webSocket, webSocketCompressionProvider, textSubProtocol ?? _options.DefaultSubProtocol, _options.ReceivePayloadBufferSize);
                    webSocketConnection.ReceiveText += async (sender, message) => { await webSocketConnection.SendAsync(message, CancellationToken.None); };

                    _connectionsService.AddConnection(webSocketConnection);

                    await webSocketConnection.ReceiveMessagesUntilCloseAsync();

                    if (webSocketConnection.CloseStatus.HasValue)
                    {
                        await webSocket.CloseAsync(webSocketConnection.CloseStatus.Value, webSocketConnection.CloseStatusDescription, CancellationToken.None);
                    }

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
        #endregion
    }
}
