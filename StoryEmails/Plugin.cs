using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
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
            moddedBundle = AssetBundle.LoadFromFile(Path.Combine(assetPath, "story_emails.assets"));
            if (moddedBundle == null)
            {
                Logger.LogError("Failed to load AssetBundle! Mod cannot work without it, exiting. Please reinstall it.");
                //return;
            }

        }
    }

    class PatchMenuGlobalPause
    {
        //Things we need to 'borrow'
        internal static MenuGlobalPause menuPause;
        internal static byte currentWindow;

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(MenuGlobalPause), "State_FolderSelect", MethodType.Normal)]
        static IEnumerable<CodeInstruction> PatchMenuGlobalPauseFolderSelect(IEnumerable<CodeInstruction> instructions) 
        {
            //Edit the forced menu exit icon to be 5th, not 4th one.
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (var i = 1; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4_4 && codes[i - 1].opcode == OpCodes.Ldfld)
                {
                    codes[i].opcode = OpCodes.Ldc_I4_5;
                }
            }




            return codes;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MenuGlobalPause),"Start",MethodType.Normal)]
        static void PatchMenuGlobalPauseStart(MenuGlobalPause __instance, ref MenuFolder[] ___folder, ref byte ___currentWindow)
        {
            menuPause = __instance;
            currentWindow = ___currentWindow;

            //Move 'Exit' to the 5th slot and move it's icon.
            ___folder = ___folder.AddToArray(___folder[4]);
            ___folder[5].icon.transform.position = new Vector3(64, -368, 0);

            MenuFolder emailFolder = new MenuFolder
            {
                icon = Plugin.moddedBundle.LoadAsset<SpriteRenderer>("map_buttons_email"),
                iconOff = Plugin.moddedBundle.LoadAsset<Sprite>("MenuIconMail0"),
                iconOn = Plugin.moddedBundle.LoadAsset<Sprite>("MenuIconMail1"),
                folderTabs = [Plugin.moddedBundle.LoadAsset<SpriteRenderer>("Tab1"), Plugin.moddedBundle.LoadAsset<SpriteRenderer>("Tab2")],
                content = [Plugin.moddedBundle.LoadAsset<GameObject>("Tab_Inbox"),Plugin.moddedBundle.LoadAsset<GameObject>("Tab_Emails")]
            };
            //Add our folder!
            ___folder[4] = emailFolder;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MenuGlobalPause), "Update", MethodType.Normal)]
        static void PatchMenuGlobalPauseUpdate(MenuGlobalPause __instance, ref byte ___currentWindow)
        {
            menuPause = __instance;
            currentWindow = ___currentWindow;
        }

            [HarmonyReversePatch]
        [HarmonyPatch(typeof(MenuGlobalPause), "EnhanceScrolling")]
        public static void EnhanceScrolling(object instance) =>
            throw new NotImplementedException("It's a stub");

        //Obtain means to get State_FolderSelect
        internal static void State_FolderSelect()
        {
            State_FolderSelect(menuPause);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(MenuGlobalPause), "State_FolderSelect")]
        internal static void State_FolderSelect(object instance) =>
            throw new NotImplementedException("It's a stub");


        private void State_Emails(MenuGlobalPause instance)
        {
            if (FPStage.menuInput.left)
            {
                MenuFolder menuFolder = instance.folder[(int)currentWindow];
                menuFolder.currentFolder -= 1;
                FPAudio.PlaySfx(1);
            }
            else if (FPSaveManager.gameMode == FPGameMode.ADVENTURE && FPStage.menuInput.right && (int)instance.folder[(int)currentWindow].currentFolder < instance.folder[(int)currentWindow].folderTabs.Length)
            {
                MenuFolder menuFolder2 = instance.folder[(int)currentWindow];
                menuFolder2.currentFolder += 1;
                FPAudio.PlaySfx(1);
            }
            EnhanceScrolling(instance);
            for (int i = 0; i < instance.folder[(int)currentWindow].folderTabs.Length; i++)
            {
                if (i == (int)(instance.folder[(int)currentWindow].currentFolder - 1))
                {
                    instance.folder[(int)currentWindow].folderTabs[i].sprite = instance.tabOn;
                    instance.cursor.transform.position = new Vector3(-999f, 0f, 0f);
                    if (!instance.folder[(int)currentWindow].content[i].activeSelf)
                    {
                        instance.folder[(int)currentWindow].content[i].SetActive(true);
                    }
                }
                else if (instance.folder[(int)currentWindow].currentFolder > 0)
                {
                    instance.folder[(int)currentWindow].folderTabs[i].sprite = instance.tabOff;
                    if (instance.folder[(int)currentWindow].content[i].activeSelf)
                    {
                        instance.folder[(int)currentWindow].content[i].SetActive(false);
                    }
                }
            }
            if (instance.folder[(int)currentWindow].currentFolder == 1)
            {
                if (FPStage.menuInput.down)
                {
                    
                }
                if (FPStage.menuInput.up)
                {
                   
                }
                //instance.UpdateRecordsList();
            }
            else if (instance.folder[(int)currentWindow].currentFolder == 2)
            {
                if (FPStage.menuInput.down)
                {
                    
                }
                if (FPStage.menuInput.up)
                {
                    
                }
                //instance.UpdateNPCList();
            }
            if (instance.folder[(int)currentWindow].currentFolder == 0 || FPStage.menuInput.cancel)
            {
                instance.state = new FPObjectState(State_FolderSelect);
                instance.cursor.transform.position = new Vector3(-999f, 0f, 0f);
            }
        }

    }
}
