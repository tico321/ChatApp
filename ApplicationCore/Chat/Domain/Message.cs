using ApplicationCore.Users.Domain;

namespace ApplicationCore.Chat.Domain
{
    public class Message
    {
        public string Content { get; private set; }
        public ApplicationUser User { get; private set; }

        public Message(string content, ApplicationUser user)
        {
            this.Content = content;
            this.User = user;
        }
    }
}
