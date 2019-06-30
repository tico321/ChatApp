using ApplicationCore.Interfaces;
using FluentValidation;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
namespace ApplicationCore.Chat.Commands
{
    public class SendMessageCommand : IRequest<bool>
    {
        public string Content { get; set; }
    }

    public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
    {
        public SendMessageCommandValidator()
        {
            this.RuleFor(m => m.Content).NotNull().NotEmpty().MaximumLength(500);
        }
    }

    public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, bool>
    {
        private readonly IAppDbContext dbContext;

        public SendMessageCommandHandler(IAppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<bool> Handle(SendMessageCommand request, CancellationToken cancellationToken)
        {
            return false;
        }
    }
}
