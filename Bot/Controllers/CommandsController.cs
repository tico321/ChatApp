using ApplicationCore.Bot.Commands;
using ApplicationCore.Chat.Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Bot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommandsController : ControllerBase
    {
        private readonly IMediator mediator;

        public CommandsController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CommandMessageDto command)
        {
            await this.mediator.Send(
                new ProcessCommandMessageCommand { Command = new CommandMessage(command) });
            return this.NoContent();
        }
    }
}
