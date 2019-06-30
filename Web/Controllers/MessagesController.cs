using ApplicationCore.Chat.Domain;
using ApplicationCore.Chat.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Web.Controllers
{
    [Authorize]
    [Route("Messages")]
    public class MessagesController : Controller
    {
        private readonly IMediator mediator;

        public MessagesController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetMessages()
        {
            var result = await this.mediator.Send(new Top50MessagesQuery());

            return new PartialViewResult
            {
                ViewName = "~/Pages/_ChatList.cshtml",
                ViewData = new ViewDataDictionary<IEnumerable<Message>>(ViewData, result.Messages)
            };
        }
    }
}
