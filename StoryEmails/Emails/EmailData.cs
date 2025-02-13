using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace StoryEmails.Emails
{
    public class EmailData
    {
        //Contents
        public string subject;
        public string body;
        public string from;
        public string fromName;
        public EmailType status;

        //Attachments
        public bool hasAttachment;
        public string attachmentFileName;
        public Sprite attachmentImage;

        //Unlock requirements
        public int storyflag = 0;       
    }
}
