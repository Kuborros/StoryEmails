using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using StoryEmails.Emails;
using StoryEmails.Patches;
using System.IO;
using UnityEngine;

namespace StoryEmails
{
    [BepInPlugin("com.kuborro.plugins.fp2.storyemails", "Story E-Mails", "1.0.0")]
    [BepInDependency("000.kuborro.libraries.fp2.fp2lib")]
    public class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;
        internal static AssetBundle moddedBundle;

        internal static ConfigEntry<bool> configAddTutorialEmail;
        internal static ConfigEntry<bool> configAddStoryEmail;
        internal static ConfigEntry<bool> configAddBetaEmail;

        internal static ConfigEntry<bool> configNoAttachments;

#pragma warning disable IDE0051 // Remove unused private members
        private void Awake()
#pragma warning restore IDE0051 // Remove unused private members
        {
            string assetPath = Path.Combine(Path.GetFullPath("."), "mod_overrides");
            moddedBundle = AssetBundle.LoadFromFile(Path.Combine(assetPath, "email_menu.assets"));
            if (moddedBundle == null)
            {
                Logger.LogError("Failed to load AssetBundle! Mod cannot work without it, exiting. Please reinstall it.");
                return;
            }

            configAddTutorialEmail = Config.Bind("Built-in E-Mail",
                                     "Tutorial E-Mail",
                                     true,
                                     "Should the tutorial e-mails be shown.");
            configAddStoryEmail = Config.Bind("Built-in E-Mail",
                                    "Story E-Mail",
                                    true,
                                    "Should the story-related e-mails be shown.");

            configAddBetaEmail = Config.Bind("Built-in E-Mail",
                                    "Beta E-Mail",
                                    false,
                                    "Should the e-mails referencing beta or scrapped content be shown.");

            configNoAttachments = Config.Bind("Performance",
                        "Disable Attachments",
                        false,
                        "Disables loading attachments and attachment tab. Useful if your game slows down loading all the .png files.");

            Harmony.CreateAndPatchAll(typeof(PatchMenuGlobalPause));

            //Load the e-mails.
            new EmailHandler();
        }
    }
}
