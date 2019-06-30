using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Web.Hubs
{
    public class ChatHub : Hub
    {
        public async Task NewMessage()
        {
            await this.Clients.Others.SendAsync("NewMessage");
        }
    }
}
