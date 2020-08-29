using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Messenger.Common.DTOs;
using Messenger.Entities.DTOs;
using Messenger.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Messenger.EF;
using Messenger.Entities;
using Messenger.Entities.Models;
using Microsoft.Extensions.Configuration;

namespace Messenger.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DialogController : MessengerBaseController
    {
        public DialogController(DialogService dialogService, FileService fileService, AccountService accountService,
            UserManager<MessengerUser> userManager, IMapper mapper, MessengerDbContext context,
            IConfiguration configuration) : base(dialogService, fileService, accountService, userManager, mapper,
            context, configuration)
        {
        }

        [HttpGet("list/{userId}")]
        public async Task<ApiCallResult<List<DialogDTO>>> GetList(string userId)
        {
            var data = await _dialogService.GetByUserId(userId, int.MaxValue, 0, false);

            var dto = _mapper.Map<List<Dialog>, List<DialogDTO>>(data);

            return new ApiCallResult<List<DialogDTO>>(dto);
        }   

        [HttpGet("get/{email}")]
        public async Task<ApiCallResult<DialogDTO>> GetDialogBetweenUsers(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return new ApiCallResult<DialogDTO>(HttpStatusCode.BadRequest, "No such user");

            if (CurrentUserId == user.Id)
                return new ApiCallResult<DialogDTO>(HttpStatusCode.BadRequest, HttpStatusCode.BadRequest.ToString());

            var data = await _dialogService.GetDialogBetweenUsers(new[] {user.Id, _userManager.GetUserId(User)});

            var dto = _mapper.Map<Dialog, DialogDTO>(data);

            return new ApiCallResult<DialogDTO>(dto);
        }

        [HttpPost]
        public async Task<ApiCallResult<object>> Cancel(int dialogId)
        {
            ApiCallResult<object> result;

            try
            {
                var dialog = await _context.Dialogs.FindAsync(dialogId);

                if (dialog == null)
                    throw new ApplicationException("There is no such dialog.");

                await _context.Entry(dialog).Collection(r => r.Participants).LoadAsync();

                if (!dialog.Participants.Any(p => p.MessengerUserId.Equals(CurrentUserId)))
                    throw new ApplicationException("Requested dialog does not belong to current user.");

                if (dialog.Status.Equals(DialogStatus.Canceled))
                    throw new ApplicationException("Requested dialog has been already canceled.");

                dialog.Status = DialogStatus.Canceled;

                _context.Dialogs.Update(dialog);

                await _context.SaveChangesAsync();

                result = new ApiCallResult<object>();
            }
            catch (ApplicationException ae)
            {
                result = new ApiCallResult<object>(HttpStatusCode.BadRequest, ae.Message);
            }
            catch (Exception e)
            {
                result = new ApiCallResult<object>(HttpStatusCode.InternalServerError, "Something went wrong.");
            }

            return result;
        }
    }
}
