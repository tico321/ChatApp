using ApplicationCore.Users.Domain;
using System;

namespace ApplicationCore.Chat.Domain
{
    public class Message
    {
        public int MessageId { get; private set; }
        public string Content { get; private set; }
        public ApplicationUser User { get; private set; }
        public DateTime CreationDate { get; private set; }

        protected Message() { }

        public Message(string content, ApplicationUser user)
        {
            this.Content = content;
            this.User = user;
            this.CreationDate = DateTime.Now;
        }
    }
}
