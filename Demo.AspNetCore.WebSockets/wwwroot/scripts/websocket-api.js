var WebSocketApi = (function() {
    var locationInput, messageInput;
    var plainTextSubprotocolCheckbox, jsonSubprotocolCheckbox;
    var connectButton, disconnectButton, sendButton;
    var consoleOutput;

    var webSocket;

    var openWebSocket = function() {
        var subprotocols = new Array();

        if (plainTextSubprotocolCheckbox.checked) {
            subprotocols.push('aspnetcore-ws.plaintext');
        }

        if (jsonSubprotocolCheckbox.checked) {
            subprotocols.push('aspnetcore-ws.json');
        }

        webSocket = (subprotocols.length == 0) ? new WebSocket(locationInput.value) : new WebSocket(locationInput.value, subprotocols);
        webSocket.onopen = webSocketOnOpen;
        webSocket.onclose = webSocketOnClose;
        webSocket.onerror = webSocketOnError;
        webSocket.onmessage = webSocketOnMessage;
    };

    var closeWebSocket = function() {
        webSocket.close();
    }

    function sendToWebSocket() {
        var text = messageInput.value;

        writeToConsole('[-- SEND --]: ' + text);
        webSocket.send(text);
    }

    var webSocketOnOpen = function () {
        if (webSocket.protocol) {
            writeToConsole('[-- CONNECTION ESTABLISHED (' + webSocket.protocol + ') --]');
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

    var webSocketOnMessage = function(message) {
        if (webSocket.protocol == 'aspnetcore-ws.json') {
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
            connectButton = document.getElementById('connect');
            disconnectButton = document.getElementById('disconnect');
            sendButton = document.getElementById('send');
            consoleOutput = document.getElementById('console');

            connectButton.addEventListener('click', openWebSocket);
            disconnectButton.addEventListener('click', closeWebSocket);
            sendButton.addEventListener('click', sendToWebSocket);
            document.getElementById('clear').addEventListener('click', clearConsole);
        }
    };
})();

WebSocketApi.initialize();