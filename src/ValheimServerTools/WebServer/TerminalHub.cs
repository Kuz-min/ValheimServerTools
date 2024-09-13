using System;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace ValheimServerTools
{
    interface ITerminalHub
    {
        event Action<string>? MessageReceived;
        void BroadcastMessage(string message);
    }


    class TerminalHub : ITerminalHub
    {
        public const string PATH = "/terminal";

        public event Action<string>? MessageReceived;

        private readonly HttpServer _server;

        public TerminalHub(HttpServer server) => _server = server;

        public void BroadcastMessage(string message) => Broadcast(message);

        public WebSocketBehavior CreateWSBehavior()
        {
            var behavior = new TerminalWSBehaviour();
            behavior.MessageReceived += MessageReceived;
            return behavior;
        }

        private void Broadcast(string text)
        {
            if (_server.IsListening)
                _server.WebSocketServices[PATH].Sessions.Broadcast(text);
        }


        private class TerminalWSBehaviour : WebSocketBehavior
        {
            public event Action<string>? MessageReceived;

            protected override void OnMessage(MessageEventArgs e)
            {
                if (e.IsText)
                {
                    var message = e.Data;
                    MessageReceived?.Invoke(message);
                }
            }
        }
    }
}
