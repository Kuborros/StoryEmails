using BepInEx;
using BepInEx.Logging;
using FP2Lib.Player;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace StoryEmails.Emails
{
    internal class EmailHandler
    {
        //Built-in emails:
        //Game mechanics explanations
        private static List<EmailData> tutorialEmails = [];
        //Story-related messages
        private static List<EmailData> storyEmails = [];
        //Messages regarding removed beta elements.
        private static List<EmailData> betaEmails = [];
        //Extra e-mails added by mods
        public static List<EmailData> modEmails = [];

        private static string customCharUid;


        private static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Email Loader");


        static EmailHandler()
        {
            //Load built-ins.
            LoadBuiltInEmails();
            //Load modded emails
            LoadFromModFiles();
            //Load Attachments
            LoadAttachments();
        }


        internal static EmailData[] GetSortedEmails()
        {
            List<EmailData> emailTemp = new List<EmailData>();
            FPCharacterID currentCharacter = FPSaveManager.character;
            customCharUid = string.Empty;
            if (currentCharacter > FPCharacterID.NEERA)
            {
                PlayableChara character = PlayerHandler.currentCharacter;
                if (character != null)
                {
                    customCharUid = character.uid;
                }
            }
            //First default value that will not be rendered but should exist regardless.
            emailTemp.Add(new EmailData { from = "kubo@catbat.rocks", fromName = "Kubo", body = "Hi!\nYou <i>really</i> should not be seeing this e-mail!\nSomething broke! Bad!\nFill a bug report maybe?", status = EmailType.Timed, subject = "How.", hasAttachment = false, ModdedChars = true, BaseChars = true });

            //Append built-ins
            if (Plugin.configAddTutorialEmail.Value)
            {
                foreach (EmailData emailData in tutorialEmails)
                {
                    if (EmailShouldBeShown(emailData)) emailTemp.Add(emailData);
                }
            }
            if (Plugin.configAddStoryEmail.Value)
            {
                foreach (EmailData emailData in storyEmails)
                {
                    if (EmailShouldBeShown(emailData)) emailTemp.Add(emailData);
                }
            }
            if (Plugin.configAddBetaEmail.Value)
            {
                foreach (EmailData emailData in betaEmails)
                {
                    if (EmailShouldBeShown(emailData)) emailTemp.Add(emailData);
                }
            }

            //E-mails added by mods
            foreach (EmailData emailData in modEmails)
            {
                if (EmailShouldBeShown(emailData)) emailTemp.Add(emailData);
            }

            //Sort by story flags (and such the unlock order)
            List<EmailData> emailSorted = emailTemp.OrderBy(o => o.storyFlag).ToList();

            return emailSorted.ToArray();
        }

        static bool EmailShouldBeShown(EmailData emailData)
        {
            switch (FPSaveManager.character)
            {
                case FPCharacterID.LILAC:
                    if ((emailData.Lilac || emailData.BaseChars) && (emailData.storyFlag == 0 || GetStoryFlagValue(emailData.storyFlag) > 0))
                    {
                        if (emailData.senderIsNPC && FPSaveManager.gameMode != FPGameMode.CLASSIC)
                        {
                            return (FPSaveManager.npcFlag[FPSaveManager.GetNPCNumber(emailData.fromName)] > 0);
                        }
                        else return true;
                    }  
                    break;
                case FPCharacterID.CAROL:
                case FPCharacterID.BIKECAROL:
                    if ((emailData.Carol || emailData.BaseChars) && (emailData.storyFlag == 0 || GetStoryFlagValue(emailData.storyFlag) > 0))
                    {
                        if (emailData.senderIsNPC && FPSaveManager.gameMode != FPGameMode.CLASSIC)
                        {
                            return (FPSaveManager.npcFlag[FPSaveManager.GetNPCNumber(emailData.fromName)] > 0);
                        }
                        else return true;
                    }
                    break;
                case FPCharacterID.MILLA:
                    if ((emailData.Milla || emailData.BaseChars) && (emailData.storyFlag == 0 || GetStoryFlagValue(emailData.storyFlag) > 0))
                    {
                        if (emailData.senderIsNPC && FPSaveManager.gameMode != FPGameMode.CLASSIC)
                        {
                            return (FPSaveManager.npcFlag[FPSaveManager.GetNPCNumber(emailData.fromName)] > 0);
                        }
                        else return true;
                    }
                    break;
                case FPCharacterID.NEERA:
                    if ((emailData.Neera || emailData.BaseChars) && (emailData.storyFlag == 0 || GetStoryFlagValue(emailData.storyFlag) > 0))
                    {
                        if (emailData.senderIsNPC && FPSaveManager.gameMode != FPGameMode.CLASSIC)
                        {
                            return (FPSaveManager.npcFlag[FPSaveManager.GetNPCNumber(emailData.fromName)] > 0);
                        }
                        else return true;
                    }
                    break;
                default:
                    if ((emailData.modRecipients.Contains(customCharUid) || (emailData.ModdedChars && !emailData.excludedModRecipients.Contains(customCharUid))) && (emailData.storyFlag == 0 || GetStoryFlagValue(emailData.storyFlag) > 0))
                    {
                        if (emailData.senderIsNPC && FPSaveManager.gameMode != FPGameMode.CLASSIC)
                        {
                            return (FPSaveManager.npcFlag[FPSaveManager.GetNPCNumber(emailData.fromName)] > 0);
                        }
                        else return true;
                    }
                    break;
            }
            return false;
        }


        //Safer method to get the story flag, in case some e-mail decides to have flag 999999
        static int GetStoryFlagValue(int storyflag)
        {
            if (storyflag < FPSaveManager.storyFlag.Length)
            {
                return FPSaveManager.storyFlag[storyflag];
            }
            else return 0;
        }

        private static void LoadAttachments()
        {
            //Concat my beloved
            foreach (EmailData email in tutorialEmails.Concat(storyEmails).Concat(betaEmails).Concat(modEmails))
            {
                //If we don't want any attachments then we skip loading them here, and set every mail to no attachment mode
                if (Plugin.configNoAttachments.Value)
                {
                    email.hasAttachment = false;
                }
                else
                {
                    //Check if e-mail has attachment and said attachment has a path set
                    if (email.hasAttachment && !email.attachmentRealActualPath.IsNullOrWhiteSpace())
                    {
                        string attachmentPath = Path.Combine(email.jsonPath, email.attachmentRealActualPath);
                        Logger.LogDebug("Will now attempt to load attachmment .png from: " + attachmentPath);
                        //Is it a thing that actually exists?
                        if (File.Exists(attachmentPath))
                        {
                            try
                            {
                                byte[] array = File.ReadAllBytes(attachmentPath);
                                Texture2D texture2D = new Texture2D(2, 2, TextureFormat.RGB24, false);
                                texture2D.filterMode = FilterMode.Point;
                                texture2D.LoadImage(array,false);
                                if (texture2D != null)
                                {
                                    //This also forces proper filtering and scaling settings
                                    //Brutally compresses anything too big to 240p. Aspect ratio? Who's that?
                                    Resize(texture2D, Math.Min(320,texture2D.width), Math.Min(240,texture2D.height));
                                    email.attachmentImage = Sprite.Create(texture2D, new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), new Vector2(0.5f, 0.5f), 1f);
                                }
                            }
                            catch (Exception ex) 
                            {
                                //If we failed to load it or something equally bad has happened, set it to not render the attachment at all
                                email.hasAttachment = false;
                                Logger.LogError(ex);
                            }
                        }
                        else email.hasAttachment = false;
                    }
                }
            }
        }


        private static void LoadBuiltInEmails()
        {
            string pluginDir = Paths.PluginPath + "/StoryEmails";
            try
            {
                if (File.Exists(pluginDir + "/Tutorials.json"))
                {
                    string json = File.ReadAllText(pluginDir + "/Tutorials.json");
                    tutorialEmails.AddRange(ParseBootlegJsonList(json));
                    tutorialEmails.ForEach(o => o.jsonPath = pluginDir);
                }
                if (File.Exists(pluginDir + "/Story.json"))
                {
                    string json = File.ReadAllText(pluginDir + "/Story.json");
                    storyEmails.AddRange(ParseBootlegJsonList(json));
                    storyEmails.ForEach(o => o.jsonPath = pluginDir);
                }
                if (File.Exists(pluginDir + "/Beta.json"))
                {
                    string json = File.ReadAllText(pluginDir + "/Beta.json");
                    betaEmails.AddRange(ParseBootlegJsonList(json));
                    betaEmails.ForEach(o => o.jsonPath = pluginDir);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }

        internal static void LoadFromModFiles()
        {
            string[] plugins = Directory.GetDirectories(Paths.PluginPath);
            foreach (string plugin in plugins)
            {
                try
                {
                    if (File.Exists(plugin + "/ExtraEmails.json"))
                    {
                        Logger.LogDebug("Found ExtraEmails at: " +  plugin);
                        string json = File.ReadAllText(plugin + "/ExtraEmails.json");
                        List<EmailData> modMailsTemp = ParseBootlegJsonList(json);
                        modMailsTemp.ForEach(o => o.jsonPath = plugin);
                        modEmails.AddRange(modMailsTemp);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                }
            }
        }


        //I ❤️ UNITY, BUT WHY DOES IT NOT LOVE ME BACK
        //https://stackoverflow.com/questions/36239705/serialize-and-deserialize-json-and-json-array-in-unity
        //I promise, i tested every option there. It does not work. No clue why, as it should. But it doesn't. So we have this instead. 
        private static List<EmailData> ParseBootlegJsonList(string json)
        {
            List<EmailData> result = [];
            //If there is nothing to work with, return nothing right away
            if (json.IsNullOrWhiteSpace()) return result;

            //I LOVE regex, i hope you do so too!!!
            //But seriously, this splits the JSON array into just single objects
            //More in-depth explanation at: https://regex101.com/r/P7TTB3/2
            //Genuinely cursed, but what else can i do outside of adding external libraries...
            Regex jsonRegex = new(@"\{\s+[^\}]+\}(?=(,|\s+\]))", RegexOptions.Multiline | RegexOptions.Compiled);

            MatchCollection elements = jsonRegex.Matches(json);

            foreach (Match element in elements)
            {
                EmailData data = JsonUtility.FromJson<EmailData>(element.Value);
                if (data != null)
                {
                    Logger.LogDebug("Loaded e-mail: " + data.subject);
                    result.Add(data);
                }
            }
            return result;
        }


        //Hacky resize code, there does not seem to be an easier and faster way to do so in Unity 5.6
        //Every GPU should be able to handle it relatively painlessly, unless someone *slaps in* a 4K image or something.
        private static void Resize(Texture2D texture, int newWidth, int newHeight)
        {
            RenderTexture tmp = RenderTexture.GetTemporary(newWidth, newHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            RenderTexture.active = tmp;
            Graphics.Blit(texture, tmp);
            texture.Resize(newWidth, newHeight, texture.format, false);
            texture.filterMode = FilterMode.Point;
            texture.ReadPixels(new Rect(Vector2.zero, new Vector2(newWidth, newHeight)), 0, 0);
            texture.Apply();
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(tmp);
        }
    }
}
