using ApplicationCore.Chat.Domain;
using FluentValidation;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationCore.Bot.Commands
{
    public class ProcessCommandMessageCommand : IRequest<ProcessCommandMessageCommandResult>
    {
        public CommandMessage Command { get; set; }
    }

    public class ProcessCommandMessageCommandResult
    {

    }

    public class ProcessCommandMessageCommandValidator : AbstractValidator<ProcessCommandMessageCommand>
    {
        public ProcessCommandMessageCommandValidator()
        {
            this.RuleFor(x => x);
        }
    }

    public class ProcessCommandMessageCommandHandler : IRequestHandler<ProcessCommandMessageCommand, ProcessCommandMessageCommandResult>
    {

        public ProcessCommandMessageCommandHandler()
        {
        }

        public async Task<ProcessCommandMessageCommandResult> Handle(ProcessCommandMessageCommand request, CancellationToken cancellationToken)
        {
            return null;
        }
    }
}
