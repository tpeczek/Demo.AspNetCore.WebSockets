using System.Collections.Generic;
using Demo.AspNetCore.WebSockets.Infrastructure;

namespace Demo.AspNetCore.WebSockets.Middlewares
{
    public class WebSocketConnectionsOptions
    {
        public IList<ITextWebSocketSubprotocol> SupportedSubProtocols { get; set; }

        public ITextWebSocketSubprotocol DefaultSubProtocol { get; set; }
    }
}
