using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Concurrent;

namespace ValheimServerTools
{
    [BepInPlugin(MOD_ID, MOD_NAME, MOD_VERSION)]
    [BepInProcess("valheim_server.exe")]
    class Mod : BaseUnityPlugin
    {
        public const string MOD_ID = "ValheimServerTools";
        public const string MOD_NAME = "Valheim Server Tools";
        public const string MOD_VERSION = "0.1.0";
        private const int DEFAULT_GAME_PORT = 2456;

        private void Awake()
        {
            try
            {
                var args = ValheimCommandLineArgs.Parse(Environment.GetCommandLineArgs());
                var configuration = Config.Load();

                var harmony = new Harmony(MOD_ID);
                harmony.PatchAll();

                var webServerPort = args.WebServerPort ?? (args.GamePort != null ? args.GamePort + configuration.PortOffset.Value : DEFAULT_GAME_PORT + configuration.PortOffset.Value);
                RunWebServer(webServerPort.Value);
            }
            catch (Exception e)
            {
                ModLog.LogError(e.Message);
            }
        }

        private void RunWebServer(int port)
        {
            _server = new WebServer(port);

            Terminal_Patch.MessageAdded += (message) => _server.TerminalHub.BroadcastMessage(message);
            _server.TerminalHub.MessageReceived += (message) => Terminal_Patch.Write(message);

            ZNet_Patch.ServerStarted += () => _server.EventHub.BroadcastEvent(new ServerStarted());
            ZDOMan_Patch.PeerConnected += (userName) => _server.EventHub.BroadcastEvent(new PeerConnected(userName));
            ZDOMan_Patch.PeerDisconnected += (userName) => _server.EventHub.BroadcastEvent(new PeerDisconnected(userName));

            _server.Start();
            ModLog.Log($"web server started on port {port}");
        }

        private void OnDestroy()
        {
            try
            {
                if (_server != null)
                {
                    _server.Stop();
                    ModLog.Log($"web server stopped");
                }
            }
            catch (Exception e)
            {
                ModLog.LogError(e.Message);
            }
        }

        private WebServer? _server;
    }


    static class ModLog
    {
        public static void Log(string message) => ZLog.Log($"[{Mod.MOD_ID}]: {message}");
        public static void LogError(string message) => ZLog.LogError($"[{Mod.MOD_ID}]: {message}");
    }


    [HarmonyPatch(typeof(Terminal))]
    static class Terminal_Patch
    {
        public static event Action<string>? MessageAdded;

        public static void Write(string message)
        {
            try
            {
                _messages.Enqueue(message);
            }
            catch (Exception e)
            {
                ModLog.LogError(e.Message);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("AddString", typeof(string))]
        private static void AddString(string text)
        {
            try
            {
                MessageAdded?.Invoke(text);
            }
            catch (Exception e)
            {
                ModLog.LogError(e.Message);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        private static void Update_Postfix(Terminal __instance)
        {
            try
            {
                while (_messages.TryDequeue(out var message))
                {
                    if (!string.IsNullOrEmpty(message))
                    {
                        __instance.AddString(message);
                        __instance.TryRunCommand(message);
                    }

                }
            }
            catch (Exception e)
            {
                ModLog.LogError(e.Message);
            }
        }

        private static readonly ConcurrentQueue<string> _messages = new ConcurrentQueue<string>();
    }


    [HarmonyPatch(typeof(ZNet))]
    static class ZNet_Patch
    {
        public static event Action? ServerStarted;

        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        private static void Start_Postfix()
        {
            try
            {
                ServerStarted?.Invoke();
            }
            catch (Exception e)
            {
                ModLog.LogError(e.Message);
            }
        }
    }


    [HarmonyPatch(typeof(ZDOMan))]
    static class ZDOMan_Patch
    {
        public static event Action<string>? PeerConnected;
        public static event Action<string>? PeerDisconnected;

        [HarmonyPostfix]
        [HarmonyPatch("AddPeer", typeof(ZNetPeer))]
        private static void AddPeer_Postfix(ZNetPeer netPeer)
        {
            try
            {
                if (netPeer != null && !string.IsNullOrEmpty(netPeer.m_playerName))
                    PeerConnected?.Invoke(netPeer.m_playerName);
            }
            catch (Exception e)
            {
                ModLog.LogError(e.Message);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("RemovePeer", typeof(ZNetPeer))]
        private static void RemovePeer_Postfix(ZNetPeer netPeer)
        {
            try
            {
                if (netPeer != null && !string.IsNullOrEmpty(netPeer.m_playerName))
                    PeerDisconnected?.Invoke(netPeer.m_playerName);
            }
            catch (Exception e)
            {
                ModLog.LogError(e.Message);
            }
        }
    }
}
