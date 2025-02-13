using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using StoryEmails.Patches;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace StoryEmails
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;
        internal static AssetBundle moddedBundle;

        private void Awake()
        {
            Logger = base.Logger;
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

            string assetPath = Path.Combine(Path.GetFullPath("."), "mod_overrides");
            moddedBundle = AssetBundle.LoadFromFile(Path.Combine(assetPath, "email_menu.assets"));
            if (moddedBundle == null)
            {
                Logger.LogError("Failed to load AssetBundle! Mod cannot work without it, exiting. Please reinstall it.");
                //return;
            }

            Harmony.CreateAndPatchAll(typeof(PatchMenuGlobalPause));
        }
    }
}
