using ApplicationCore.Chat.Commands;
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
        private readonly IMessagingService messagingService;

        public ProcessCommandMessageCommandHandler(
            IStockService stockService,
            IMessagingService messagingService)
        {
            this.stockService = stockService;
            this.messagingService = messagingService;
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
            var msg = $"{stock.Symbol} quote is {stock.Open} per share";
            await this.messagingService.Send(
                ApplicationCore.Chat.Domain.Constants.MessagingQueue,
                new HandleQueuedMessageCommand
                {
                    Content = msg,
                    UserEmail = "bot@chat.com"
                }
            );

            return new ProcessCommandMessageCommandResult();
        }
    }
}
