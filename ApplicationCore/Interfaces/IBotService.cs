using ApplicationCore.Chat.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationCore.Interfaces
{
    public interface IBotService
    {
        Task<bool> SendCommand(
            CommandMessage commandMessage,
            CancellationToken cancellationToken);
    }
}
