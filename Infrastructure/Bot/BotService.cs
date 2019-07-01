using ApplicationCore.Chat.Domain;
using ApplicationCore.Exceptions;
using ApplicationCore.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Bot
{
    public class BotService : IBotService
    {
        public static readonly string ClientName = nameof(BotService);

        private readonly IHttpClientFactory factory;
        private readonly BotConfiguration config;

        public BotService(IHttpClientFactory factory, BotConfiguration config)
        {
            this.factory = factory;
            this.config = config;
        }

        public async Task<bool> SendCommand(
            CommandMessage commandMessage,
            CancellationToken cancellationToken)
        {
            var client = this.factory.CreateClient(ClientName);
            var body = new StringContent(
                JsonConvert.SerializeObject(commandMessage.ToDto()),
                Encoding.UTF8,
                "application/json");
            var response = await client.PostAsync(
                $"{this.config.BaseUrl}/api/Commands",
                body);

            if (response.StatusCode != HttpStatusCode.NoContent)
            {
                throw new BadRequestException(
                    new List<(string, string)>
                    {
                        ("BotSendCommand", response.ReasonPhrase)
                    });
            }

            return true;
        }
    }
}
