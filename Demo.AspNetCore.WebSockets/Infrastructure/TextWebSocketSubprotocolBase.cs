using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Demo.AspNetCore.WebSockets.Infrastructure
{
    internal abstract class TextWebSocketSubprotocolBase
    {
        public virtual Task SendAsync(string message, Stream webSocketMessageStream, CancellationToken cancellationToken)
        {
            using StreamWriter webSocketMessageStreamWriter = new StreamWriter(webSocketMessageStream);
            return webSocketMessageStreamWriter.WriteAsync(message);
        }

        public virtual string Read(Stream webSocketMessageStream)
        {
            using StreamReader webSocketMessageStreamReader = new StreamReader(webSocketMessageStream);
            return webSocketMessageStreamReader.ReadToEnd();
        }
    }
}
