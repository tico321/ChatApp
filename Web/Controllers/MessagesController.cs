using ApplicationCore.Chat.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            await this.mediator.Send(command);
            return this.Ok();
        }
    }
}
