using System.Threading.Tasks;
using ApplicationCore.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Web.Hubs
{
    public class BroadcastService : IBroadcastService
    {
        private readonly IHubContext<ChatHub> chatHub;

        public BroadcastService(IHubContext<ChatHub> chatHub)
        {
            this.chatHub = chatHub;
        }
        public async Task NotifyNewMessage()
        {
            await chatHub.Clients.All.SendAsync("NewMessage");
        }
    }
}