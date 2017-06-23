namespace Demo.AspNetCore.WebSockets.Infrastructure
{
    public class PlainTextWebSocketSubprotocol : TextWebSocketSubprotocolBase, ITextWebSocketSubprotocol
    {
        public string SubProtocol => "aspnetcore-ws.plaintext";
    }
}
