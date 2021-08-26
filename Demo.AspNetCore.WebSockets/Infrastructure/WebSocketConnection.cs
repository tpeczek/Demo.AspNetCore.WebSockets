using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;

namespace Demo.AspNetCore.WebSockets.Infrastructure
{
    internal class WebSocketConnection
    {
        #region Fields
        private readonly int _receivePayloadBufferSize;
        private readonly int? _sendSegmentSize;

        private readonly WebSocket _webSocket;
        private readonly ITextWebSocketSubprotocol _textSubProtocol;
        #endregion

        #region Properties
        public Guid Id { get; } = Guid.NewGuid();

        public WebSocketCloseStatus? CloseStatus { get; private set; } = null;

        public string CloseStatusDescription { get; private set; } = null;
        #endregion

        #region Events
        public event EventHandler<string> ReceiveText;

        public event EventHandler<byte[]> ReceiveBinary;
        #endregion

        #region Constructor
        public WebSocketConnection(WebSocket webSocket, ITextWebSocketSubprotocol textSubProtocol, int? sendSegmentSize, int receivePayloadBufferSize)
        {
            _webSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
            _textSubProtocol = textSubProtocol ?? throw new ArgumentNullException(nameof(textSubProtocol));
            _sendSegmentSize = sendSegmentSize;
            _receivePayloadBufferSize = receivePayloadBufferSize;
        }
        #endregion

        #region Methods
        public Task SendAsync(string message, CancellationToken cancellationToken)
        {
            return _textSubProtocol.SendAsync(message, SendTextMessageBytesAsync, cancellationToken);
        }

        public Task SendAsync(byte[] message, CancellationToken cancellationToken)
        {
            return SendMessageBytesAsync(message, WebSocketMessageType.Binary, cancellationToken);
        }        

        public async Task ReceiveMessagesUntilCloseAsync()
        {
            try
            {
                byte[] receivePayloadBuffer = new byte[_receivePayloadBufferSize];
                WebSocketReceiveResult webSocketReceiveResult = await _webSocket.ReceiveAsync(new ArraySegment<byte>(receivePayloadBuffer), CancellationToken.None);
                while (webSocketReceiveResult.MessageType != WebSocketMessageType.Close)
                {
                    byte[] webSocketMessage = await ReceiveMessagePayloadAsync(webSocketReceiveResult, receivePayloadBuffer);
                    if (webSocketReceiveResult.MessageType == WebSocketMessageType.Binary)
                    {
                        OnReceiveBinary(webSocketMessage);
                    }
                    else
                    {
                        OnReceiveText(Encoding.UTF8.GetString(webSocketMessage));
                    }

                    webSocketReceiveResult = await _webSocket.ReceiveAsync(new ArraySegment<byte>(receivePayloadBuffer), CancellationToken.None);
                }

                CloseStatus = webSocketReceiveResult.CloseStatus.Value;
                CloseStatusDescription = webSocketReceiveResult.CloseStatusDescription;
            }
            catch (WebSocketException wsex) when (wsex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
            { }
        }

        private Task SendTextMessageBytesAsync(byte[] messageBytes, CancellationToken cancellationToken)
        {
            return SendMessageBytesAsync(messageBytes, WebSocketMessageType.Text, cancellationToken);
        }

        private async Task SendMessageBytesAsync(byte[] messageBytes, WebSocketMessageType messageType, CancellationToken cancellationToken)
        {
            if (_webSocket.State == WebSocketState.Open)
            {
                if (_sendSegmentSize.HasValue && (_sendSegmentSize.Value < messageBytes.Length))
                {
                    int messageOffset = 0;
                    int messageBytesToSend = messageBytes.Length;

                    while (messageBytesToSend > 0)
                    {
                        int messageSegmentSize = Math.Min(_sendSegmentSize.Value, messageBytesToSend);
                        ArraySegment<byte> messageSegment = new ArraySegment<byte>(messageBytes, messageOffset, messageSegmentSize);

                        messageOffset += messageSegmentSize;
                        messageBytesToSend -= messageSegmentSize;

                        await _webSocket.SendAsync(messageSegment, WebSocketMessageType.Text, (messageBytesToSend == 0), cancellationToken);
                    }
                }
                else
                {
                    ArraySegment<byte> messageSegment = new ArraySegment<byte>(messageBytes, 0, messageBytes.Length);

                    await _webSocket.SendAsync(messageSegment, WebSocketMessageType.Text, true, cancellationToken);
                }
            }
        }

        private async Task<byte[]> ReceiveMessagePayloadAsync(WebSocketReceiveResult webSocketReceiveResult, byte[] receivePayloadBuffer)
        {
            byte[] messagePayload = null;

            if (webSocketReceiveResult.EndOfMessage)
            {
                messagePayload = new byte[webSocketReceiveResult.Count];
                Array.Copy(receivePayloadBuffer, messagePayload, webSocketReceiveResult.Count);
            }
            else
            {
                using (MemoryStream messagePayloadStream = new MemoryStream())
                {
                    messagePayloadStream.Write(receivePayloadBuffer, 0, webSocketReceiveResult.Count);
                    while (!webSocketReceiveResult.EndOfMessage)
                    {
                        webSocketReceiveResult = await _webSocket.ReceiveAsync(new ArraySegment<byte>(receivePayloadBuffer), CancellationToken.None);
                        messagePayloadStream.Write(receivePayloadBuffer, 0, webSocketReceiveResult.Count);
                    }

                    messagePayload = messagePayloadStream.ToArray();
                }
            }

            return messagePayload;
        }

        private void OnReceiveText(string webSocketMessage)
        {
            string message = _textSubProtocol.Read(webSocketMessage);

            ReceiveText?.Invoke(this, message);
        }

        private void OnReceiveBinary(byte[] webSocketMessage)
        {
            ReceiveBinary?.Invoke(this, webSocketMessage);
        }
        #endregion
    }
}
