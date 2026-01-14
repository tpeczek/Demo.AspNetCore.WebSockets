var WebSocketApi = (function() {
    var locationInput, messageInput;
    var plainTextSubprotocolCheckbox, jsonSubprotocolCheckbox;
    var webSocketStreamCheckbox;
    var connectButton, disconnectButton, sendButton;
    var consoleOutput;

    var webSocket;
    var webSocketStream;
    var webSocketStreamConnection;
    var webSocketStreamReader;
    var webSocketStreamWriter;

    var openWebSocket = function() {
        var subprotocols = new Array();

        if (plainTextSubprotocolCheckbox.checked) {
            subprotocols.push('aspnetcore-ws.plaintext');
        }

        if (jsonSubprotocolCheckbox.checked) {
            subprotocols.push('aspnetcore-ws.json');
        }

        webSocket = null;
        webSocketStream = null;
        webSocketStreamConnection = null;
        webSocketStreamReader = null;

        if (webSocketStreamCheckbox.checked) {
            webSocketStream = new WebSocketStream(locationInput.value, { protocols: subprotocols });

            webSocketStream.opened.then(webSocketStreamOnOpen, webSocketOnError);
            webSocketStream.closed.then(webSocketOnClose);
        } else {
            webSocket = (subprotocols.length == 0) ? new WebSocket(locationInput.value) : new WebSocket(locationInput.value, subprotocols);

            webSocket.onopen = webSocketOnOpen;
            webSocket.onclose = webSocketOnClose;
            webSocket.onerror = webSocketOnError;
            webSocket.onmessage = webSocketOnMessage;
        }
    };

    var closeWebSocket = function () {
        (webSocket || webSocketStream).close();
    }

    function sendToWebSocket() {
        var text = messageInput.value;

        writeToConsole('[-- SEND --]: ' + text);

        if (webSocket) {
            webSocket.send(text);
        } else if (webSocketStreamWriter) {
            webSocketStreamWriter.write(text);
        }
    }

    var webSocketStreamOnOpen = function (connection) {
        webSocketStreamConnection = connection
        webSocketOnOpen();

        webSocketStreamReader = webSocketStreamConnection.readable.getReader();
        webSocketStreamWriter = webSocketStreamConnection.writable.getWriter();

        webSocketStreamDrain();
    };

    var webSocketOnOpen = function () {
        if ((webSocket || webSocketStreamConnection).protocol) {
            writeToConsole('[-- CONNECTION ESTABLISHED (' + (webSocket || webSocketStreamConnection).protocol + ') --]');
        } else {
            writeToConsole('[-- CONNECTION ESTABLISHED --]');
        }
        changeUIState(true);
    };

    var webSocketOnClose = function() {
        writeToConsole('[-- CONNECTION CLOSED --]');
        changeUIState(false);
    }

    var webSocketOnError = function () {
        writeToConsole('[-- ERROR OCCURRED --]');
        changeUIState(false);
    }

    var webSocketStreamDrain = function () {
        webSocketStreamReader.read().then(function (readResult) {
            if (readResult.value) {
                webSocketOnMessage({ data: readResult.value });
            }

            if (!readResult.done) {
                webSocketStreamDrain();
            }
        });
    };

    var webSocketOnMessage = function(message) {
        if ((webSocket || webSocketStreamConnection).protocol == 'aspnetcore-ws.json') {
            var parsedData = JSON.parse(message.data);
            writeToConsole('[-- RECEIVED --]: ' + parsedData.message + ' {SERVER TIMESTAMP: ' + parsedData.timestamp + '}');
        } else {
            writeToConsole('[-- RECEIVED --]: ' + message.data);
        }
    };

    var clearConsole = function() {
        while (consoleOutput.childNodes.length > 0) {
            consoleOutput.removeChild(consoleOutput.lastChild);
        }
    };

    var writeToConsole = function(text) {
        var paragraph = document.createElement('p');
        paragraph.style.wordWrap = 'break-word';
        paragraph.appendChild(document.createTextNode(text));

        consoleOutput.appendChild(paragraph);
    };

    var changeUIState = function(isConnected) {
        locationInput.disabled = isConnected;
        messageInput.disabled = !isConnected;
        connectButton.disabled = isConnected;
        disconnectButton.disabled = !isConnected;
        sendButton.disabled = !isConnected;
    };

    return {
        initialize: function () {
            locationInput = document.getElementById('location');
            messageInput = document.getElementById('message');
            plainTextSubprotocolCheckbox = document.getElementById('plainTextSubprotocol');
            jsonSubprotocolCheckbox = document.getElementById('jsonSubprotocol');
            webSocketStreamCheckbox = document.getElementById('webSocketStream');
            connectButton = document.getElementById('connect');
            disconnectButton = document.getElementById('disconnect');
            sendButton = document.getElementById('send');
            consoleOutput = document.getElementById('console');

            webSocketStreamCheckbox.disabled = !("WebSocketStream" in self);

            connectButton.addEventListener('click', openWebSocket);
            disconnectButton.addEventListener('click', closeWebSocket);
            sendButton.addEventListener('click', sendToWebSocket);
            document.getElementById('clear').addEventListener('click', clearConsole);
        }
    };
})();

WebSocketApi.initialize();