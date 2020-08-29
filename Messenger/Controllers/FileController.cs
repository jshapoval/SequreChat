using System;
using System.Threading.Tasks;
using AutoMapper;
using Messenger.EF;
using Messenger.Entities;
using Messenger.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Messenger.Controllers
{
    public class FileController : MessengerBaseController
    {
        public FileController(DialogService dialogService, FileService fileService, AccountService accountService,
            UserManager<MessengerUser> userManager, IMapper mapper, MessengerDbContext context,
            IConfiguration configuration) : base(dialogService, fileService, accountService, userManager, mapper,
            context, configuration)
        {
        }

        [ResponseCache(Location = ResponseCacheLocation.Client, Duration = int.MaxValue)]
        public async Task<ActionResult> Get(Guid id)
        {
            try
            {
                var fileToRetrieve = await _fileService.GetById(id);

                return File(fileToRetrieve.Content, fileToRetrieve.ContentType);
            }
            catch (ApplicationException ae)
            {
                return BadRequest(ae.Message);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}