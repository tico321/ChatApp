using ApplicationCore.Chat.Domain;
using ApplicationCore.Chat.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Web.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IMediator mediator;

        public IEnumerable<Message> Messages { get; set; }

        public IndexModel(IMediator mediator)
        {
            this.mediator = mediator;
        }

        public async Task OnGetAsync()
        {
            var result = await this.mediator.Send(new Top50MessagesQuery());
            this.Messages = result.Messages;
        }
    }
}
