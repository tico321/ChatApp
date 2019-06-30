using ApplicationCore.Chat.Commands;
using ApplicationCore.Chat.Domain;
using ApplicationCore.Chat.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Web.Controllers.Api
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

        [HttpPost]
        public async Task<IActionResult> PostMessage(SendMessageCommand command)
        {
            command.User = HttpContext.User;
            var messageId = await this.mediator.Send(command);
            return this.Ok(messageId);
        }

        [HttpGet]
        public async Task<IEnumerable<Message>> GetMessages()
        {
            var result = await this.mediator.Send(new Top50MessagesQuery());
            return result.Messages;
        }
    }
}
