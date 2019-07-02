using System.Threading.Tasks;

namespace ApplicationCore.Interfaces
{
    public interface IBroadcastService
    {
        Task NotifyNewMessage();
    }
}