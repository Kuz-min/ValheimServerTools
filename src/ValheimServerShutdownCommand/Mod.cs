using BepInEx;
using HarmonyLib;
using System;
using UnityEngine;

namespace ValheimServerShutdownCommand
{
    [BepInPlugin(MOD_ID, MOD_NAME, MOD_VERSION)]
    [BepInProcess("valheim_server.exe")]
    class Mod : BaseUnityPlugin
    {
        public const string MOD_ID = "ValheimServerShutdownCommand";
        public const string MOD_NAME = "Valheim Server Shutdown Command";
        public const string MOD_VERSION = "0.1.0";

        private void ShutdownCommandHandler()
        {
            Application.Quit(0);
        }

        private void Awake()
        {
            var harmony = new Harmony(MOD_ID);
            harmony.PatchAll();

            Terminal_Patch.ShutdownCommandExecuted += ShutdownCommandHandler;
        }
    }


    static class ModLog
    {
        public static void Log(string message) => ZLog.Log($"[{Mod.MOD_ID}]: {message}");
        public static void LogError(string message) => ZLog.LogError($"[{Mod.MOD_ID}]: {message}");
    }


    [HarmonyPatch(typeof(Terminal))]
    static class Terminal_Patch
    {
        public static event Action? ShutdownCommandExecuted;

        [HarmonyPrefix]
        [HarmonyPatch("InitTerminal")]
        private static void InitTerminal_Prefix()
        {
            try
            {
                new Terminal.ConsoleCommand("shutdown", "save the world and shut down the server immediately", args =>
                {
                    ShutdownCommandExecuted?.Invoke();
                }, onlyAdmin: true);
            }
            catch (Exception e)
            {
                ModLog.LogError(e.Message);
            }
        }
    }
}