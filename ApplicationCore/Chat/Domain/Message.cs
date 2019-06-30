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

        public MessageView ToView()
        {
            return new MessageView
            {
                Content = this.Content,
                Email = this.User.Email,
                Date = this.CreationDate.ToString("MM/dd/yyyy HH:mm:ss")
            };
        }
    }

    public class MessageView
    {
        public string Content { get; set; }
        public string Email { get; set; }
        public string Date { get; set; }
    }
}
