using AutoMapper;
using Messenger.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Messenger.EF;
using Messenger.Entities;
using Microsoft.Extensions.Configuration;

namespace Messenger.Controllers
{
    public class MessengerBaseController : ControllerBase
    {
        protected readonly MessengerDbContext _context;
        protected readonly UserManager<MessengerUser> _userManager;
        protected readonly DialogService _dialogService;
        protected readonly AccountService _accountService;
        protected readonly FileService _fileService;
        protected readonly IMapper _mapper;
        protected readonly IConfiguration _configuration;
        private DialogService dialogService;
        private UserManager<MessengerUser> userManager;
        private IMapper mapper;
        private MessengerDbContext context;
        private IConfiguration configuration;

        public MessengerBaseController(DialogService dialogService, FileService fileService,
            AccountService accountService, UserManager<MessengerUser> userManager, IMapper mapper,
            MessengerDbContext context, IConfiguration configuration)
        {
            this._context = context;
            this._userManager = userManager;
            this._accountService = accountService;
            this._fileService = fileService;
            this._dialogService = dialogService;
            this._mapper = mapper;
            this._configuration = configuration;
        }

        protected string CurrentUserId
        {
            get { return _userManager.GetUserId(User); }
        }
    }
}
