namespace Demo.AspNetCore.WebSockets.Infrastructure
{
    internal class PlainTextWebSocketSubprotocol : TextWebSocketSubprotocolBase, ITextWebSocketSubprotocol
    {
        public string SubProtocol => "aspnetcore-ws.plaintext";
    }
}
