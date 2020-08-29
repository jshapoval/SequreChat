using System;
using Messenger.EF;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Messenger.Common.DTOs;
using Messenger.Models;
using Microsoft.AspNetCore.Identity;
using Messenger.Entities;
using Messenger.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;

namespace Messenger.Repositories
{
    public class AccountService
    {
        private readonly MessengerDbContext db;
        private readonly UserManager<MessengerUser> _userManager;
        private readonly MessengerDbContext _context;

        public AccountService(MessengerDbContext db, UserManager<MessengerUser> userManager, MessengerDbContext context)
        {
            this.db = db;
            this._userManager = userManager;
            this._context = context;
        }

        public async Task<OperationDetails> Create(RegisterModelDTO rm)
        {
            var user = await _userManager.FindByEmailAsync(rm.Email);

            if (user == null)
            {
                IImageFormat format;

                var originalImage = Image.Load(rm.Avatar.Content, out format);

                var minDim = Math.Min(originalImage.Width, originalImage.Height);

                var original = new File
                {
                    FileName = rm.Avatar.FileName,
                    ContentType = rm.Avatar.ContentType,
                    Content = originalImage.ToBytes(format),
                };

                var square = new File
                {
                    FileName = rm.Avatar.FileName,
                    ContentType = rm.Avatar.ContentType,
                    Content = originalImage.Clone(x => x.Resize(new ResizeOptions
                    {
                        Size = new SixLabors.Primitives.Size(minDim),
                        Mode = ResizeMode.Crop
                    })).ToBytes(format)
                };

                var size100 = Math.Min(minDim, 100);

                var square100 = new File
                {
                    FileName = rm.Avatar.FileName,
                    ContentType = rm.Avatar.ContentType,
                    Content = originalImage.Clone(x => x.Resize(new ResizeOptions
                    {
                        Size = new SixLabors.Primitives.Size(size100),
                        Mode = ResizeMode.Crop
                    })).ToBytes(format)
                };

                var size300 = Math.Min(minDim, 300);

                var square300 = new File
                {
                    FileName = rm.Avatar.FileName,
                    ContentType = rm.Avatar.ContentType,
                    Content = originalImage.Clone(x => x.Resize(new ResizeOptions
                    {
                        Size = new SixLabors.Primitives.Size(size300),
                        Mode = ResizeMode.Crop
                    })).ToBytes(format)
                };

                var size600 = Math.Min(minDim, 600);

                var square600 = new File
                {
                    FileName = rm.Avatar.FileName,
                    ContentType = rm.Avatar.ContentType,
                    Content = originalImage.Clone(x => x.Resize(new ResizeOptions
                    {
                        Size = new SixLabors.Primitives.Size(size600),
                        Mode = ResizeMode.Crop
                    })).ToBytes(format)
                };

                user = new MessengerUser
                {
                    Email = rm.Email,
                    UserName = rm.Email,
                    FName = rm.FirstName,
                    LName = rm.LastName,
                    Files = new List<File>
                    {
                        original,
                        square,
                        square100,
                        square300,
                        square600
                    }
                };

                var result = await _userManager.CreateAsync(user, rm.Password);

                user.Avatar = new Avatar
                {
                    Default = !rm.HasAvatar,
                    Original = original,
                    Square = square,
                    Square_100 = square100,
                    Square_300 = square300,
                    Square_600 = square600
                };

                _context.Update(user);
                await _context.SaveChangesAsync();

                if (result.Errors.Any())
                    return new OperationDetails(false, result.Errors.FirstOrDefault().Description, "");

                return new OperationDetails(true, "Регистрация успешно пройдена", "");
            }
            else
            {
                return new OperationDetails(false, "Пользователь с таким логином уже существует", "Email");
            }
        }
    }
}
