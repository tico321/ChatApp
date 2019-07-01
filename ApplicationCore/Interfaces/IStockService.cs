using ApplicationCore.Bot.Domain.Stock;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationCore.Interfaces
{
    public interface IStockService
    {
        Task<CompanyStock> GetStock(string company, CancellationToken cancellationToken);
    }
}