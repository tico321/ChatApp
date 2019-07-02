using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Infrastructure.Stock;
using Xunit;

namespace ApplicationCore.Test.Infrastructure.Stock
{
    public class StockServiceTest
    {
        [Fact(Skip = "Test helper for manual integration testing")]
        public async Task GetStock()
        {
            var sut = this.GetSut();

            var result = await sut.GetStock("aapl.us", new CancellationToken());

            Assert.NotNull(result);
        }

        public StockService GetSut()
        {
            var config = new StockConfiguration
            {
                BaseUrl = "https://stooq.com/q/l"
            };
            var httpClientFactory = A.Fake<IHttpClientFactory>();
            var client = HttpClientFactory.Create();
            A.CallTo(() => httpClientFactory.CreateClient(A<string>._)).Returns(client);
            return new StockService(httpClientFactory, config);
        }
    }
}