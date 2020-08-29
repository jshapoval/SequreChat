using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Messenger.Common.DTOs;
using Messenger.EF;
using Messenger.Entities;
using Messenger.Entities.DTOs;
using Messenger.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Messenger.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessageController : MessengerBaseController
    {
        public MessageController(DialogService dialogService, FileService fileService, AccountService accountService,
            UserManager<MessengerUser> userManager, IMapper mapper, MessengerDbContext context,
            IConfiguration configuration) : base(dialogService, fileService, accountService, userManager, mapper,
            context, configuration)
        {
        }

        [HttpGet("list/{dialogId}")]
        public async Task<ApiCallResult<ICollection<MessageDTO>>> GetList(int dialogId)
        {
            var dialog = await _context.Dialogs.FindAsync(dialogId);

            var messages = await _dialogService.GetMessages(dialog);

            var dto = _mapper.Map<ICollection<Message>, ICollection<MessageDTO>>(messages);

            return new ApiCallResult<ICollection<MessageDTO>>(dto);
        }   
    }
}
