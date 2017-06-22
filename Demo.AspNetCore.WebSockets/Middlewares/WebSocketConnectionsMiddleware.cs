using System.Net.WebSockets;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Demo.AspNetCore.WebSockets.Infrastructure;
using Demo.AspNetCore.WebSockets.Services;
using System.Threading;

namespace Demo.AspNetCore.WebSockets.Middlewares
{
    public class WebSocketConnectionsMiddleware
    {
        #region Fields
        private WebSocketConnectionsOptions _options;
        private IWebSocketConnectionsService _connectionsService;
        #endregion

        #region Constructor
        public WebSocketConnectionsMiddleware(RequestDelegate next, WebSocketConnectionsOptions options, IWebSocketConnectionsService connectionsService)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _connectionsService = connectionsService ?? throw new ArgumentNullException(nameof(connectionsService));
        }
        #endregion

        #region Methods
        public async Task Invoke(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                ITextWebSocketSubprotocol subProtocol = NegotiateSubProtocol(context.WebSockets.WebSocketRequestedProtocols);
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync(subProtocol?.SubProtocol);
                WebSocketConnection webSocketConnection = new WebSocketConnection(webSocket, subProtocol ?? _options.DefaultSubProtocol);

                _connectionsService.AddConnection(webSocketConnection);

                byte[] webSocketBuffer = new byte[1024 * 4];
                WebSocketReceiveResult webSocketReceiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(webSocketBuffer), CancellationToken.None);
                if (webSocketReceiveResult.MessageType != WebSocketMessageType.Close)
                {
                    throw new NotSupportedException("This demo doesn't support receiving data.");
                }
                await webSocket.CloseAsync(webSocketReceiveResult.CloseStatus.Value, webSocketReceiveResult.CloseStatusDescription, CancellationToken.None);

                _connectionsService.RemoveConnection(webSocketConnection.Id);
            }
            else
            {
                context.Response.StatusCode = 400;
            }
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
