using FP2Lib.Player;
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

        static EmailData currentEmail;
        private static EmailData lastDrawnEmail;

        //References to objects
        static GameObject window;
        static GameObject inbox;
        static GameObject mail;
        static GameObject attachments;



        static SpriteRenderer tabInbox;
        static SpriteRenderer tabMail;
        static SpriteRenderer tabAttachment;

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
        static IEnumerable<CodeInstruction> PatchMenuGlobalPauseFolderSelect(IEnumerable<CodeInstruction> instructions)
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

            emailList = new EmailData[1];

            //Move 'Exit' to the 5th slot and move it's icon.
            ___folder = ___folder.AddToArray(___folder[4]);
            ___folder[5].icon.transform.position = new Vector3(64, -368, 0);

            //Load our stuff
            GameObject mapButtonEmail = UnityEngine.Object.Instantiate(Plugin.moddedBundle.LoadAsset<GameObject>("map_buttons_email"));
            window = UnityEngine.Object.Instantiate(Plugin.moddedBundle.LoadAsset<GameObject>("Window_Email"));

            tabInbox = window.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
            tabMail = window.transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>();
            tabAttachment = window.transform.GetChild(2).gameObject.GetComponent<SpriteRenderer>();
            inbox = window.transform.GetChild(3).gameObject;
            mail = window.transform.GetChild(4).gameObject;
            attachments = window.transform.GetChild(5).gameObject;

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
                folderTabs = [tabInbox, tabMail, tabAttachment],
                content = [inbox, mail, attachments]
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
            emailList[0] = new EmailData { from = "magister@shangtu.gov", fromName = "Magister", 
                body = "<s=1.5>Greetings <PLAYER></s>,\r\nWe hope you find the facilities in the palace to your liking. Your center of operations is the map screen, which will allow you to select your next mission. \r\n\r\nAdditionally, there is a training room run by Gong, as well as a lab area for the creation of items useful to your missions. The rest of the palace is yours to explore.\r\n\r\nSafe winds,\r\n<s=1.5>The Magister</s>", 
                received = true, status = EmailType.Story, subject = "Welcome!", hasAttachment = true, attachmentFileName = "Test.png", attachmentImage = Plugin.moddedBundle.LoadAsset<Sprite>("MenuIconMail0")};
            emailList = emailList.AddToArray(new EmailData { from = "magister@shangtu.gov", fromName = "Magister", body = "Lol. Lmao.", received = true, status = EmailType.Story, subject = "Great Apologies for Spam.", hasAttachment = false });


            //Update email list
            UpdateEmailList(__instance);
            RenderEmail(__instance, currentEmail);
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
            else if (FPStage.menuInput.right || FPStage.menuInput.confirm)
            {
                if (instance.folder[currentWindow].currentFolder <= 2)
                {
                    if (instance.folder[currentWindow].currentFolder < 2 || currentEmail.hasAttachment)
                    {
                        MenuFolder menuFolder2 = instance.folder[currentWindow];
                        menuFolder2.currentFolder += 1;
                        FPAudio.PlaySfx(1);
                    }
                }
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
                if (FPStage.menuInput.down)
                {

                }
            }
            else if (instance.folder[currentWindow].currentFolder == 3)
            {
                if (FPStage.menuInput.down)
                {

                }
            }
            if (instance.folder[currentWindow].currentFolder == 0 || FPStage.menuInput.cancel)
            {
                //Delegate instead of reverse patch as we *really want* the completely patched version of the method, including patches from other mods
                //Reverse Patch's Snapshot mode can and will miss some patches, as even if we set lowest priority another mod can always request to be loaded *after* us.
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
            currentEmail = emailList[messageRows[currentEmailID + currentEmailOffset]];
            if (emailList[messageRows[currentEmailID + currentEmailOffset]].hasAttachment)
            {
                tabAttachment.gameObject.SetActive(true);
            }
            else
            {
                tabAttachment.gameObject.SetActive(false);
            }
            //Update the mail only when needed.
            //It do be expensive.
            if (lastDrawnEmail != currentEmail)
            {
                RenderEmail(instance, currentEmail);
            }
            emailScrollbar.transform.localPosition = emailScrollbarTop.transform.position - new Vector3(0f, (float)(currentEmailID + currentEmailOffset) / (float)Mathf.Max(emailList.Length - 1, emailListLength - 1) * 72f, 0f);
        }

        static void RenderEmail(MenuGlobalPause instance, EmailData email)
        {
            //Prep things

            //Subject + Sender name
            GameObject infoRow1 = mail.transform.GetChild(0).gameObject;
            TextMesh subject = infoRow1.GetComponent<TextMesh>();
            TextMesh sender = infoRow1.transform.GetChild(0).gameObject.GetComponent<TextMesh>();
            //Free Space + e-mail adress
            GameObject infoRow2 = mail.transform.GetChild(1).gameObject;
            TextMesh attachmentInfo = infoRow2.GetComponent<TextMesh>();
            TextMesh emailAdress = infoRow2.transform.GetChild(0).gameObject.GetComponent<TextMesh>();
            //Child 2 is the separator
            //Message Body
            GameObject message = mail.transform.GetChild(3).gameObject;
            SuperTextMesh messageBody = message.GetComponent<SuperTextMesh>();

            //Attachment window
            GameObject attachmentInfoRow1 = attachments.transform.GetChild(0).gameObject;
            TextMesh attachmentFileName = attachmentInfoRow1.GetComponent<TextMesh>();
            SpriteRenderer attachmentImage = attachments.transform.GetChild(2).gameObject.GetComponent<SpriteRenderer>();

            //Set content
            subject.text = email.subject;
            sender.text = email.fromName;
            emailAdress.text = "<" + email.from + ">";
            messageBody.text = ParseEmailBody(email.body);

            //Attachment logic           
            if (email.hasAttachment)
            {
                attachmentFileName.text =  "Filename: " + email.attachmentFileName;
                attachmentImage.sprite = email.attachmentImage;
            }
            else
            {
                //Clean-up any previous content
                attachmentInfo.text = "";
                attachmentFileName.text = "Filename: None";
                attachmentImage.sprite = null;
            }
            //Not used
            attachmentInfo.text = "";

            lastDrawnEmail = email;
        }

        private static string ParseEmailBody(string body)
        {
            //Casual Name
            string playerName = "Player";
            //Official Name
            string playerNameOfficial = "Player";

            //Replace "<PLAYER>" with character name, and <PLAYERFULL> with official variant
            //Vanila characters logic
            if (FPSaveManager.character <= FPCharacterID.NEERA)
            {
                switch (FPSaveManager.character)
                {
                    case FPCharacterID.LILAC:
                        playerName = "Lilac";
                        playerNameOfficial = "Sash Lilac";
                        break;
                    case FPCharacterID.CAROL:
                    case FPCharacterID.BIKECAROL:
                        playerName = "Carol";
                        playerNameOfficial = "Carol Tea";
                        break;
                    case FPCharacterID.MILLA:
                        playerName = "Milla";
                        playerNameOfficial = "Milla Basset";
                        break;
                    case FPCharacterID.NEERA:
                        playerName = "Neera";
                        playerNameOfficial = "Neera Li";
                        break;
                }
            }
            //Modded characters!
            else
            {
                PlayableChara character = PlayerHandler.GetPlayableCharaByRuntimeId((int)FPSaveManager.character);
                //Just in case we somehow got ID of spooky ghost
                if (character != null)
                {
                    playerName = character.Name;
                    //For now. If needed, this can be expanded later to include honorifics, or possibly allow modders to set their own.
                    playerNameOfficial = character.Name;
                }
            }
            //Replace names.
            body = body.Replace("<PLAYER>", playerName);
            body = body.Replace("<PLAYERFULL>", playerNameOfficial);

            return body;
        }

    }
}
