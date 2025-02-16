using System;
using System.Collections.Generic;
using UnityEngine;

namespace StoryEmails.Emails
{
    [Serializable]
    public class EmailData
    {
        //Contents
        /// <summary>
        /// Subject of the message.
        /// </summary>
        public string subject = string.Empty;
        /// <summary>
        /// Contents of the message.
        /// Takes control characters like \n for new line, as well as all supported Super Text Mesh html tags.
        /// Special placeholders will be replaced at runtime:
        /// "<PLAYER>" is replaced with the character's name.
        /// "<PLAYERFULL>" is replaced with their name and surname.
        /// </summary>
        public string body = string.Empty;
        /// <summary>
        /// E-Mail adress of the sender. Will be surrounded by <> at runtime.
        /// </summary>
        public string from = string.Empty;
        /// <summary>
        /// Name of the sender.
        /// </summary>
        public string fromName = string.Empty;
        /// <summary>
        /// What icon should be applied to the e-mail.
        /// </summary>
        public EmailType status = EmailType.Normal;

        //Recipients
        /// <summary>
        /// Should the mail be available for Lilac.
        /// </summary>
        public bool Lilac = false;
        /// <summary>
        /// Should the mail be available for Carol.
        /// </summary>
        public bool Carol = false;
        /// <summary>
        /// Should the mail be available for Milla.
        /// </summary>
        public bool Milla = false;
        /// <summary>
        /// Should the mail be available for Neera.
        /// </summary>
        public bool Neera = false;
        //Generic Recipients
        /// <summary>
        /// Should the mail be available for every base game character.
        /// </summary>
        public bool BaseChars = false;
        /// <summary>
        /// Should the mail be available for every modded character.
        /// </summary>
        public bool ModdedChars = false;
        //Modded Recipients
        /// <summary>
        /// List of uids of custom characters which should get this mail.
        /// </summary>
        public List<string> modRecipients = [];
        /// <summary>
        /// List of uids of folks we *don't* want the e-mail to be delivered to. Used with <c>ModdedChars = true</c>.
        /// </summary>
        public List<string> excludedModRecipients = [];

        //Attachments
        /// <summary>
        /// Should we show the attachments tab.
        /// </summary>
        public bool hasAttachment = false;
        /// <summary>
        /// Decorational filename for the attachment.
        /// Does not need to be the actual name of the file!
        /// </summary>
        public string attachmentFileName = string.Empty;
        /// <summary>
        /// *Actual* path and filename for the attachment.
        /// Relative to the location of ExtraEmails.json when loading from file. 
        /// Can be empty if you are loading it by hand from your mod code.
        /// </summary>
        public string attachmentRealActualPath = string.Empty;
        /// <summary>
        /// Sprite to be used as attachment.
        /// </summary>
        [NonSerialized]
        public Sprite attachmentImage = null;
        /// <summary>
        /// Internal field holding full path to the .json file defining this e-mail.
        /// </summary>
        [NonSerialized]
        internal string jsonPath = string.Empty;

        //Unlock requirements
        /// <summary>
        /// What story flag needs to be checked to see if the mail should be shown.
        /// </summary>
        public int storyFlag = 0;       
        /// <summary>
        /// Is the sender an NPC in the game
        /// In that case, the e-mail will be received *only* if you met them at some point
        /// Identification is performed by name used in fromName
        /// </summary>
        public bool senderIsNPC = false;
    }
}
