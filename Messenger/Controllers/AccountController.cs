using System.IO;
using System.Threading.Tasks;
using Messenger.Common.DTOs;
using Messenger.Models;
using Messenger.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Messenger.Controllers
{
    public class AccountController : Controller
    {
        private readonly AccountService _accountService;

        public AccountController(AccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        public async Task<IActionResult> Apps()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([FromServices] IHostingEnvironment env, RegisterModel model, IFormFile upload)
        {

            if (ModelState.IsValid)
            {
                RegisterModelDTO rm = new RegisterModelDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Password = model.Password,
                    Role = "user"
                };

                if (upload != null && upload.Length > 0)
                {
                    using (var reader = new BinaryReader(upload.OpenReadStream()))
                    {
                        rm.HasAvatar = true;
                        rm.Avatar = new FileDTO()
                        {
                            FileName = Path.GetFileName(upload.FileName),
                            ContentType = upload.ContentType,
                            Content = reader.ReadBytes((int)upload.Length)
                        };
                    }
                }
                else
                {
                    var webRoot = env.WebRootPath;
                    var p = Path.Combine(webRoot, "Images/pf.jpg");
                    rm.Avatar = new FileDTO()
                    {
                        FileName = "pf.jpg",
                        ContentType = "image/jpeg",
                        Content = System.IO.File.ReadAllBytes(p)
                    };
                }

                var operationDetails = await _accountService.Create(rm);

                if (operationDetails.Succedeed)
                    return RedirectToAction("Apps");
                else
                    ModelState.AddModelError(operationDetails.Property, operationDetails.Message);
            }

            return View(model);
        }
    }
}