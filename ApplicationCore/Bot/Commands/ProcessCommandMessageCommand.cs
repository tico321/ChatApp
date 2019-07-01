using ApplicationCore.Chat.Domain;
using ApplicationCore.Interfaces;
using FluentValidation;
using MediatR;
using System;
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
            this.RuleFor(c => c.Command).NotNull();
            this.RuleFor(c => c.Command.Command).NotNull().NotEmpty();
        }
    }

    public class ProcessCommandMessageCommandHandler : IRequestHandler<ProcessCommandMessageCommand, ProcessCommandMessageCommandResult>
    {
        private readonly IStockService stockService;
        public ProcessCommandMessageCommandHandler(IStockService stockService)
        {
            this.stockService = stockService;
        }

        public Task<ProcessCommandMessageCommandResult> Handle(
            ProcessCommandMessageCommand request,
            CancellationToken cancellationToken)
        {
            switch(request.Command.Type)
            {
                case MessageCommandType.Stock:
                    return this.ProcessStockCommand(request.Command.Command, cancellationToken);
                default:
                    throw new NotSupportedException();
            }
        }

        private async Task<ProcessCommandMessageCommandResult> ProcessStockCommand(
            string company,
            CancellationToken cancellationToken)
        {
            var stock = await this.stockService.GetStock(company, cancellationToken);

            return new ProcessCommandMessageCommandResult();
        }
    }
}
