using HarmonyLib;
using MonoMod.Utils;
using StoryEmails.Emails;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace StoryEmails.Patches
{
    class PatchMenuGlobalPause
    {
        //Importante variables
        static int currentEmailID = 0;
        static int currentEmailOffset = 0;
        static int emailListLength = 0;

        static EmailData[] emailList;
        static GameObject[] inboxFields;

        //References to objects
        static GameObject window;
        static GameObject inbox;
        static GameObject mail;

        //Sprites
        static Sprite[] statusIcons;
        static Sprite slotOn;
        static Sprite slotOff;

        //Things we need to 'borrow'
        internal static MenuGlobalPause menuPause;
        internal static byte currentWindow;

        private static readonly MethodInfo m_State_FolderSelect = typeof(MenuGlobalPause).GetMethod("State_FolderSelect", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(MenuGlobalPause), "State_FolderSelect", MethodType.Normal)]
        static IEnumerable<CodeInstruction> PatchMenuGlobalPauseFolderSelect(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (var i = 1; i < codes.Count; i++)
            {
                //Edit the forced menu exit icon to be 5th, not 4th one.
                if (codes[i].opcode == OpCodes.Ldc_I4_4 && codes[i - 1].opcode == OpCodes.Ldfld)
                {
                    codes[i].opcode = OpCodes.Ldc_I4_5;
                }
            }

            return codes;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MenuGlobalPause), "State_FolderSelect", MethodType.Normal)]
        static void PatchMenuGlobalPauseFolder(byte ___currentWindow)
        {
            if ((FPStage.menuInput.right || FPStage.menuInput.confirm) && ___currentWindow == 4)
            {
                menuPause.folder[currentWindow].icon.sprite = menuPause.folder[currentWindow].iconOff;
                if (menuPause.folder[currentWindow].currentFolder == 0)
                {
                    menuPause.folder[currentWindow].currentFolder = 1;
                }
                menuPause.state = new FPObjectState(State_Emails);
                menuPause.cursor.gameObject.SetActive(true);
                FPAudio.PlaySfx(2);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MenuGlobalPause), "Start", MethodType.Normal)]
        static void PatchMenuGlobalPauseStart(MenuGlobalPause __instance, ref MenuFolder[] ___folder, ref byte ___currentWindow)
        {
            menuPause = __instance;
            currentWindow = ___currentWindow;

            //Move 'Exit' to the 5th slot and move it's icon.
            ___folder = ___folder.AddToArray(___folder[4]);
            ___folder[5].icon.transform.position = new Vector3(64, -368, 0);

            //Load our stuff
            GameObject mapButtonEmail = UnityEngine.Object.Instantiate(Plugin.moddedBundle.LoadAsset<GameObject>("map_buttons_email"));
            window = UnityEngine.Object.Instantiate(Plugin.moddedBundle.LoadAsset<GameObject>("Window_Email"));

            SpriteRenderer tabInbox = window.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
            SpriteRenderer tabMail = window.transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>();
            inbox = window.transform.GetChild(2).gameObject;
            mail = window.transform.GetChild(3).gameObject;

            slotOn = Plugin.moddedBundle.LoadAsset<Sprite>("slot_on");
            slotOff = Plugin.moddedBundle.LoadAsset<Sprite>("slot_off");

            //Set parent and positions
            window.transform.SetParent(__instance.windows.transform);
            window.transform.localPosition = new Vector3(168, -1456, 0);
            mapButtonEmail.transform.SetParent(__instance.transform);
            //Create folder
            MenuFolder emailFolder = new MenuFolder
            {
                icon = mapButtonEmail.GetComponent<SpriteRenderer>(),
                iconOff = Plugin.moddedBundle.LoadAsset<Sprite>("MenuIconMail0"),
                iconOn = Plugin.moddedBundle.LoadAsset<Sprite>("MenuIconMail1"),
                folderTabs = [tabInbox, tabMail],
                content = [inbox, mail]
            };
            //Add our folder!
            ___folder[4] = emailFolder;

            //Now build the arrays used by our other code
            inboxFields = new GameObject[10];
            for (int i = 0; i < 10; i++)
            {
                inboxFields[i] = inbox.transform.GetChild(3).GetChild(i).gameObject;
                //Clean up the debug content used in the object
                //I kinda like seeing things in editor :3
                inboxFields[i].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = null;
                inboxFields[i].transform.GetChild(2).GetComponent<TextMesh>().text = string.Empty;
                inboxFields[i].transform.GetChild(3).GetComponent<TextMesh>().text = string.Empty;
            }

            //Mail status icons
            statusIcons = new Sprite[4];
            statusIcons[0] = Plugin.moddedBundle.LoadAsset<Sprite>("MailUnreadIcon");
            statusIcons[1] = Plugin.moddedBundle.LoadAsset<Sprite>("MailReadIcon");
            statusIcons[2] = Plugin.moddedBundle.LoadAsset<Sprite>("MailSpamIcon");
            statusIcons[3] = Plugin.moddedBundle.LoadAsset<Sprite>("MailTimedIcon");




            //FOR TESTING
            emailList = emailList.AddToArray(new EmailData { from = "magister@shangtu.gov", fromName = "Magister", body = "Lol. Lmao.", received = true, status = EmailStatus.Read, subject = "Welcome!" });
            emailList = emailList.AddToArray(new EmailData { from = "magister@shangtu.gov", fromName = "Magister", body = "Lol. Lmao.", received = true, status = EmailStatus.Unread, subject = "Great Apologies for Spam." });


            //Update email list
            UpdateEmailList(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MenuGlobalPause), "Update", MethodType.Normal)]
        static void PatchMenuGlobalPauseUpdate(MenuGlobalPause __instance, ref byte ___currentWindow)
        {
            menuPause = __instance;
            currentWindow = ___currentWindow;
        }

        [HarmonyReversePatch(HarmonyReversePatchType.Snapshot)]
        [HarmonyPatch(typeof(MenuGlobalPause), "EnhanceScrolling")]
        public static void EnhanceScrolling(object instance) =>
            throw new NotImplementedException("It's a stub");

        private static void State_Emails()
        {
            State_Emails(menuPause);
        }

        private static void State_Emails(MenuGlobalPause instance)
        {
            if (FPStage.menuInput.left)
            {
                MenuFolder menuFolder = instance.folder[currentWindow];
                menuFolder.currentFolder -= 1;
                FPAudio.PlaySfx(1);
            }
            else if (FPStage.menuInput.right && instance.folder[currentWindow].currentFolder < instance.folder[currentWindow].folderTabs.Length)
            {
                MenuFolder menuFolder2 = instance.folder[currentWindow];
                menuFolder2.currentFolder += 1;
                FPAudio.PlaySfx(1);
            }
            EnhanceScrolling(instance);
            for (int i = 0; i < instance.folder[currentWindow].folderTabs.Length; i++)
            {
                if (i == instance.folder[currentWindow].currentFolder - 1)
                {
                    instance.folder[currentWindow].folderTabs[i].sprite = instance.tabOn;
                    instance.cursor.transform.position = new Vector3(-999f, 0f, 0f);
                    if (!instance.folder[currentWindow].content[i].activeSelf)
                    {
                        instance.folder[currentWindow].content[i].SetActive(true);
                    }
                }
                else if (instance.folder[currentWindow].currentFolder > 0)
                {
                    instance.folder[currentWindow].folderTabs[i].sprite = instance.tabOff;
                    if (instance.folder[currentWindow].content[i].activeSelf)
                    {
                        instance.folder[currentWindow].content[i].SetActive(false);
                    }
                }
            }
            if (instance.folder[currentWindow].currentFolder == 1)
            {
                if (FPStage.menuInput.down)
                {
                    if (currentEmailID < emailList.Length - 1)
                    {
                        currentEmailID++;
                        FPAudio.PlaySfx(11);
                    }
                    else if (currentEmailID + currentEmailOffset < emailList.Length - 1)
                    {
                        currentEmailOffset++;
                        FPAudio.PlaySfx(11);
                    }
                }
                if (FPStage.menuInput.up)
                {
                    if (currentEmailID > 0)
                    {
                        currentEmailID--;
                        FPAudio.PlaySfx(11);
                    }
                    else if (currentEmailOffset > 0)
                    {
                        currentEmailOffset--;
                        FPAudio.PlaySfx(11);
                    }
                }
                UpdateEmailList(instance);
            }
            else if (instance.folder[currentWindow].currentFolder == 2)
            {

                //RenderEmail(instance);
            }
            if (instance.folder[currentWindow].currentFolder == 0 || FPStage.menuInput.cancel)
            {
                instance.state = (FPObjectState)Delegate.CreateDelegate(typeof(FPObjectState),instance, m_State_FolderSelect);
                instance.cursor.transform.position = new Vector3(-999f, 0f, 0f);
            }
        }

        static void UpdateEmailList(MenuGlobalPause instance)
        {

            GameObject emailScrollbar = inbox.transform.GetChild(0).gameObject;
            GameObject emailScrollbarTop = inbox.transform.GetChild(2).gameObject;

            int[] messageRows = new int[emailList.Length];
            emailListLength = 0;
            for (int i = 1; i < emailList.Length; i++)
            {
                if (i == 1 || emailList[i].received)
                {
                    messageRows[emailListLength] = i;
                    emailListLength++;
                }
            }
            for (int j = 0; j < emailList.Length; j++)
            {
                SpriteRenderer slot = inboxFields[j].transform.GetChild(0).GetComponent<SpriteRenderer>();
                SpriteRenderer unread = inboxFields[j].transform.GetChild(1).GetComponent<SpriteRenderer>();
                TextMesh subject = inboxFields[j].transform.GetChild(2).GetComponent<TextMesh>();
                TextMesh sender = inboxFields[j].transform.GetChild(3).GetComponent<TextMesh>();

                EmailData email = emailList[messageRows[j + currentEmailOffset]];

                if (messageRows[j + currentEmailOffset] == 1 || email.received)
                {
                    //Fill contents of row
                    subject.text = email.subject;
                    sender.text = email.fromName;
                    unread.sprite = statusIcons[(int)email.status];
                }
                else
                {
                    //Empty the row
                    subject.text = string.Empty;
                    sender.text = string.Empty;
                    unread.sprite = null;
                }
                if (currentEmailID == j)
                {   
                    //Mark as selected
                    if (slot != null)
                    {
                        slot.sprite = slotOn;
                    }
                }
                else
                {   
                    //Mark unselected
                    if (slot != null)
                    {
                        slot.sprite = slotOff;
                    }
                }
            }
            emailScrollbar.transform.localPosition = emailScrollbarTop.transform.position - new Vector3(0f, (float)(currentEmailID + currentEmailOffset) / (float)Mathf.Max(emailList.Length - 1, emailListLength - 1) * 72f, 0f);
        }

        static void RenderEmail(MenuGlobalPause instance, EmailData email)
        {

        }

    }
}
