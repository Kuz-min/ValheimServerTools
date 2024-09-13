using System;
using UnityEngine;
using WebSocketSharp.Server;

namespace ValheimServerTools
{
    [Serializable]
    class BaseEvent
    {
        public string Event;
        protected BaseEvent(string e) => Event = e;
    }
    [Serializable]
    class ServerStarted : BaseEvent
    {
        public ServerStarted() : base("server.started") { }
    }
    [Serializable]
    class PeerConnected : BaseEvent
    {
        public string UserName;
        public PeerConnected(string userName) : base("peer.connected") => UserName = userName;
    }
    [Serializable]
    class PeerDisconnected : BaseEvent
    {
        public string UserName;
        public PeerDisconnected(string userName) : base("peer.disconnected") => UserName = userName;
    }


    interface IEventHub
    {
        void BroadcastEvent(BaseEvent e);
    }


    class EventHub : IEventHub
    {
        public const string PATH = "/events";

        private readonly HttpServer _server;

        public EventHub(HttpServer server) => _server = server;

        public void BroadcastEvent(BaseEvent e)
        {
            var json = JsonUtility.ToJson(e);
            Broadcast(json);
        }

        public WebSocketBehavior CreateWSBehavior() => new EventWSBehaviour();

        private void Broadcast(string text)
        {
            if (_server.IsListening)
                _server.WebSocketServices[PATH].Sessions.Broadcast(text);
        }


        private class EventWSBehaviour : WebSocketBehavior
        {
        }
    }
}
