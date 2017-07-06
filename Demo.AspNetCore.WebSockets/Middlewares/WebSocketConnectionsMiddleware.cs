using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Demo.AspNetCore.WebSockets.Infrastructure;
using Demo.AspNetCore.WebSockets.Services;

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
                if (ValidateOrigin(context))
                {
                    ITextWebSocketSubprotocol subProtocol = NegotiateSubProtocol(context.WebSockets.WebSocketRequestedProtocols);

                    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync(subProtocol?.SubProtocol);

                    WebSocketConnection webSocketConnection = new WebSocketConnection(webSocket, subProtocol ?? _options.DefaultSubProtocol);
                    webSocketConnection.Receive += async (sender, message) => { await webSocketConnection.SendAsync(message, CancellationToken.None); };
                    _connectionsService.AddConnection(webSocketConnection);

                    WebSocketReceiveResult webSocketCloseResult = await ReceiveMessagesAsync(webSocket, webSocketConnection);

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

        private async Task<WebSocketReceiveResult> ReceiveMessagesAsync(WebSocket webSocket, WebSocketConnection webSocketConnection)
        {
            byte[] webSocketBuffer = new byte[1024 * 4];
            WebSocketReceiveResult webSocketReceiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(webSocketBuffer), CancellationToken.None);
            while (webSocketReceiveResult.MessageType != WebSocketMessageType.Close)
            {
                byte[] webSocketReceivedBytes = null;

                if (webSocketReceiveResult.EndOfMessage)
                {
                    webSocketReceivedBytes = new byte[webSocketReceiveResult.Count];
                    Array.Copy(webSocketBuffer, webSocketReceivedBytes, webSocketReceivedBytes.Length);
                }
                else
                {
                    IEnumerable<byte> webSocketReceivedBytesEnumerable = Enumerable.Empty<byte>();
                    webSocketReceivedBytesEnumerable = webSocketReceivedBytesEnumerable.Concat(webSocketBuffer);

                    while (!webSocketReceiveResult.EndOfMessage)
                    {
                        webSocketReceiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(webSocketBuffer), CancellationToken.None);
                        webSocketReceivedBytesEnumerable = webSocketReceivedBytesEnumerable.Concat(webSocketBuffer.Take(webSocketReceiveResult.Count));
                    }
                }

                webSocketConnection.OnReceive(webSocketReceivedBytes);

                webSocketReceiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(webSocketBuffer), CancellationToken.None);
            }

            return webSocketReceiveResult;
        }
        #endregion
    }
}
