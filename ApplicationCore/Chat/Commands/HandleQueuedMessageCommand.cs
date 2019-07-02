using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationCore.Chat.Domain;
using ApplicationCore.Exceptions;
using ApplicationCore.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApplicationCore.Chat.Commands
{
    public class HandleQueuedMessageCommand : IRequest<HandleQueuedMessageCommandResult>
    {
        public string Content { get; set; }
        public string UserEmail { get; set; }
    }

    public class HandleQueuedMessageCommandResult
    {
        public int Id { get; set; }
    }

    public class HandleQueuedMessageCommandValidator : AbstractValidator<HandleQueuedMessageCommand>
    {
        public HandleQueuedMessageCommandValidator()
        {
            this.RuleFor(m => m.Content).NotNull().NotEmpty();
            this.RuleFor(m => m.UserEmail).NotNull().NotEmpty().EmailAddress();
        }
    }

    public class HandleQueuedMessageCommandHandler
        : IRequestHandler<HandleQueuedMessageCommand, HandleQueuedMessageCommandResult>
    {
        private readonly IAppDbContext dbContext;
        private readonly IBroadcastService broadcastService;

        public HandleQueuedMessageCommandHandler(
            IAppDbContext dbContext,
            IBroadcastService broadcastService)
        {
            this.dbContext = dbContext;
            this.broadcastService = broadcastService;
        }

        public async Task<HandleQueuedMessageCommandResult> Handle(
            HandleQueuedMessageCommand request,
            CancellationToken cancellationToken)
        {
            var applicationUser = await this.dbContext.Users
                .Where(u => u.Email == request.UserEmail)
                .FirstOrDefaultAsync(cancellationToken);

            if (applicationUser == null)
            {
                throw new UnAuthorizedException();
            }

            var msg = new Message(request.Content, applicationUser);
            this.dbContext.Messages.Add(msg);
            await this.dbContext.SaveChangesAsync(cancellationToken);

            await this.broadcastService.NotifyNewMessage();

            return new HandleQueuedMessageCommandResult
            {
                Id = msg.MessageId
            };
        }
    }
}