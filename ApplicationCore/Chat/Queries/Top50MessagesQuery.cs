using ApplicationCore.Chat.Domain;
using ApplicationCore.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace ApplicationCore.Chat.Queries
{
    public class Top50MessagesQuery : IRequest<Top50MessagesQueryResult>
    {
    }

    public class Top50MessagesQueryResult
    {
        public IEnumerable<Message> Messages { get; set; }
    }

    public class Top50MessagesQueryHandler
        : IRequestHandler<Top50MessagesQuery, Top50MessagesQueryResult>
    {
        private readonly IAppDbContext dbContext;

        public Top50MessagesQueryHandler(IAppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Top50MessagesQueryResult> Handle(
            Top50MessagesQuery request,
            CancellationToken cancellationToken)
        {
            var messages = await this.dbContext.Messages
                .AsNoTracking()
                .OrderByDescending(m => m.CreationDate)
                .Take(50)
                .ToListAsync(cancellationToken);

            return new Top50MessagesQueryResult
            {
                Messages = messages
            };
        }
    }
}
