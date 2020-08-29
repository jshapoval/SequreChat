using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Messenger.Common.DTOs;
using Messenger.EF;
using Messenger.Entities;
using Messenger.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Messenger.Controllers
{
    [ApiController]
    public class AuthController : MessengerBaseController
    {
        public AuthController(DialogService dialogService, FileService fileService, AccountService accountService,
            UserManager<MessengerUser> userManager, IMapper mapper, MessengerDbContext context,
            IConfiguration configuration) : base(dialogService, fileService, accountService, userManager, mapper,
            context, configuration)
        {
        }

        [Route("token")]
        [HttpPost]
        public async Task Token(LoginDTO model)
        {
            AuthObject response = null;

            var ss = ControllerContext;

            var user = await GetUser(model.Email, model.Password);

            if (user == null)
            {
                response = new AuthObject("Email or Login is incorrect.");
            }
            else
            {
                var token = GetToken(user);

                response = new AuthObject(user.Id, user.Email, token);
            }

            Response.ContentType = "application/json";
            await Response.WriteAsync(JsonConvert.SerializeObject(response, new JsonSerializerSettings { Formatting = Formatting.Indented }));
        }

        private async Task<MessengerUser> GetUser(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (await _userManager.CheckPasswordAsync(user, password))
            {
                return user;
            }

            return null;
        }

        private string GetToken(MessengerUser user)
        {
            var utcNow = DateTime.UtcNow;

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, utcNow.ToString())
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Tokens:Secret"]));
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            var jwt = new JwtSecurityToken(
                signingCredentials: signingCredentials,
                claims: claims,
                notBefore: utcNow,
                expires: utcNow.AddDays(1),
                audience: _configuration["Tokens:Issuer"],
                issuer: _configuration["Tokens:Issuer"]
            );

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}
