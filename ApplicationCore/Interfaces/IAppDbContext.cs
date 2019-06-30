using ApplicationCore.Chat.Domain;
using ApplicationCore.Users.Domain;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationCore.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<ApplicationUser> Users { get; set; }
        DbSet<Message> Messages { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
