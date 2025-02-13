using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StoryEmails.Emails
{
    public class EmailData
    {
        public bool received;

        public string subject;
        public string body;
        public string from;
        public string fromName;
        public EmailStatus status;

    }
}
