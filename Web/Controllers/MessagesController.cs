using ApplicationCore.Chat.Commands;
using ApplicationCore.Chat.Domain;
using ApplicationCore.Chat.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly IMediator mediator;

        public MessagesController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        public async Task<IActionResult> SendMessage(SendMessageCommand command)
        {
            var messageId = await this.mediator.Send(command);
            return this.Ok(messageId);
        }

        public async Task<IEnumerable<Message>> GetMessages()
        {
            var result = await this.mediator.Send(new Top50MessagesQuery());
            return result.Messages;
        }
    }
}
