using System;
using System.Threading.Tasks;

namespace ApplicationCore.Interfaces
{
    public interface IMessagingService
    {
        Task Send(string topic, object obj);
        Task RegisterHandler<T>(string topic, Action<T> handler);
    }
}