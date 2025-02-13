using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace StoryEmails.Emails
{
    internal class EmailHandler
    {
        //Built-in emails:
        //Game mechanics explanations
        private static EmailData[] tutorialEmails = new EmailData[0];
        //Story-related messages
        private static EmailData[] storyEmails = new EmailData[0];
        //Messages regarding removed beta elements.
        private static EmailData[] betaEmails = new EmailData[0];

        internal static EmailData[] GetSortedEmails()
        {

            List<EmailData> emailTemp = new List<EmailData>();
            //First defaul value that will not be rendered but should exist regardless.
            emailTemp.Add(new EmailData { from = "kubo@catbat.rocks", fromName = "Kubo", body = "Hi!\nYou <i>really</i> should not be seeing this e-mail!\nSomething broke! Bad!\nFill a bug report maybe?", status = EmailType.Timed, subject = "How.", hasAttachment = false });

            //Append built-ins
            emailTemp.AddRange(tutorialEmails);
            emailTemp.AddRange(storyEmails);
            emailTemp.AddRange(betaEmails);

            //TESTING
            emailTemp.Add(new EmailData
            {
                from = "magister@shangtu.gov",
                fromName = "Magister",
                body = "<s=1.5>Greetings <PLAYER></s>,\r\nWe hope you find the facilities in the palace to your liking. Your center of operations is the map screen, which will allow you to select your next mission. \r\n\r\nAdditionally, there is a training room run by Gong, as well as a lab area for the creation of items useful to your missions. The rest of the palace is yours to explore.\r\n\r\nSafe winds,\r\n<s=1.5>The Magister</s>",
                status = EmailType.Story,
                subject = "Welcome!",
                hasAttachment = true,
                attachmentFileName = "Test.png",
                attachmentImage = Plugin.moddedBundle.LoadAsset<Sprite>("MenuIconMail0")
            });
            emailTemp.Add(new EmailData { from = "magister@shangtu.gov", fromName = "Magister", body = "Lol. Lmao.", status = EmailType.Story, subject = "Great Apologies for Spam.", hasAttachment = false });
            emailTemp.Add(new EmailData { from = "magister@shangtu.gov", fromName = "Magister", body = "Lol. Lmao.", status = EmailType.Story, subject = "Great Apologies.", hasAttachment = false });
            emailTemp.Add(new EmailData { from = "magister@shangtu.gov", fromName = "Magister", body = "Lol. Lmao.", status = EmailType.Story, subject = "Spam.", hasAttachment = false });
            emailTemp.Add(new EmailData { from = "magister@shangtu.gov", fromName = "Magister", body = "Lol. Lmao.", status = EmailType.Story, subject = "Great.", hasAttachment = false });
            emailTemp.Add(new EmailData { from = "magister@shangtu.gov", fromName = "Magister", body = "Lol. Lmao.", status = EmailType.Story, subject = "Please", hasAttachment = false });
            emailTemp.Add(new EmailData { from = "magister@shangtu.gov", fromName = "Magister", body = "Lol. Lmao.", status = EmailType.Story, subject = "Help.", hasAttachment = false });
            emailTemp.Add(new EmailData { from = "magister@shangtu.gov", fromName = "Magister", body = "Lol. Lmao.", status = EmailType.Story, subject = "Computer broke.", hasAttachment = false });
            emailTemp.Add(new EmailData { from = "magister@shangtu.gov", fromName = "Magister", body = "Lol. Lmao.", status = EmailType.Story, subject = "Aaaaaaa.", hasAttachment = false });
            emailTemp.Add(new EmailData { from = "magister@shangtu.gov", fromName = "Magister", body = "Lol. Lmao.", status = EmailType.Story, subject = "Spam. Apologies.", hasAttachment = false });
            emailTemp.Add(new EmailData { from = "magister@shangtu.gov", fromName = "Magister", body = "Lol. Lmao.", status = EmailType.Story, subject = "Great Spam.", hasAttachment = false });
            emailTemp.Add(new EmailData { from = "magister@shangtu.gov", fromName = "Magister", body = "Lol. Lmao.", status = EmailType.Story, subject = "I give up.", hasAttachment = false });

            return emailTemp.ToArray();
        }
    }
}
