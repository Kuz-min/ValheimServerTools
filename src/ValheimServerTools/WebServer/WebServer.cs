using WebSocketSharp;
using WebSocketSharp.Server;

namespace ValheimServerTools
{
    partial class WebServer
    {
        public IEventHub EventHub { get => _eventHub; }
        public ITerminalHub TerminalHub { get => _terminalHub; }

        private readonly HttpServer _server;
        private readonly EventHub _eventHub;
        private readonly TerminalHub _terminalHub;

        public WebServer(int port)
        {
            _server = new HttpServer(port, false);
            _server.Log.Level = LogLevel.Fatal;//Suppress unnecessary messages
            _server.Log.Output = (d, m) => { };//Suppress unnecessary messages

            _terminalHub = new TerminalHub(_server);
            _server.AddWebSocketService(ValheimServerTools.TerminalHub.PATH, () => _terminalHub.CreateWSBehavior());

            _eventHub = new EventHub(_server);
            _server.AddWebSocketService(ValheimServerTools.EventHub.PATH, () => _eventHub.CreateWSBehavior());
        }

        public void Start() => _server.Start();

        public void Stop() => _server.Stop();
    }
}
