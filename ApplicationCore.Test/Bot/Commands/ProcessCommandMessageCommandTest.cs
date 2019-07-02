using System.Threading;
using System.Threading.Tasks;
using ApplicationCore.Bot.Commands;
using ApplicationCore.Bot.Domain.Stock;
using ApplicationCore.Chat.Commands;
using ApplicationCore.Chat.Domain;
using ApplicationCore.Interfaces;
using FakeItEasy;
using Xunit;

namespace ApplicationCore.Test.Bot.Commands
{
    public class ProcessCommandMessageCommandTest
    {
        [Fact]
        public async Task Handler_PublishesTheCorrectMessage()
        {
            var (stockService, messagingService, sut) = GetSut();
            var command = new ProcessCommandMessageCommand
            {
                Command = new CommandMessage("/stock=appl.us"),
            };
            var companyStock = new CompanyStock{ Symbol = "APPL.US", Open = 15.5m };
            A
                .CallTo(() => stockService.GetStock(A<string>._, A<CancellationToken>._))
                .Returns(Task.FromResult(companyStock));

            var actual = await sut.Handle(command, new CancellationToken());

            A.CallTo(() => stockService.GetStock("appl.us", A<CancellationToken>._)).MustHaveHappened();
            A
                .CallTo(() => messagingService.Send(
                    ApplicationCore.Chat.Domain.Constants.MessagingQueue,
                    A<object>.That.Matches(obj =>
                        ((HandleQueuedMessageCommand)obj).Content == $"{companyStock.Symbol} quote is {companyStock.Open} per share" &&
                        ((HandleQueuedMessageCommand)obj).UserEmail == "bot@chat.com"
                    )
                ))
                .MustHaveHappened();
        }

        public (IStockService, IMessagingService, ProcessCommandMessageCommandHandler) GetSut()
        {
            var stockService = A.Fake<IStockService>();
            var messagingService = A.Fake<IMessagingService>();
            var handler = new ProcessCommandMessageCommandHandler(stockService, messagingService);
            return (stockService, messagingService, handler);
        }
    }
}