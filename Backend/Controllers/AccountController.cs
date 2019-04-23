using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Backend.DAL.Entities;
using Backend.Helpers;
using Backend.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly EFContext _context;
        private readonly UserManager<DbUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;
        public AccountController(EFContext context,
           UserManager<DbUser> userManager,
           IConfiguration configuration,
           IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _emailSender = emailSender;
        }

        public List<UserViewModel> Get()
        {
            return _context.Users.Select(u => new UserViewModel
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.UserProfile.FirstName,
                LastName = u.UserProfile.LastName,
                Age = u.UserProfile.Age,
                Salary = u.UserProfile.Salary
            }).ToList();
        }

        [HttpPost("users")]
        public async Task<IActionResult> Post([FromBody]UserAddViewModel model)
        {
            string id = null;
            if (!ModelState.IsValid)
            {
                var errors = CustomValidator.GetErrorsByModel(ModelState);
                return BadRequest(errors);
            }
            var userProfile = new UserProfile
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Salary = model.Salary,
                Age = model.Age
            };
            var user = new DbUser()
            {
                UserName = model.Email,
                Email = model.Email,
                UserProfile = userProfile
            };
            IdentityResult result = await _userManager
                .CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = CustomValidator.GetErrorsByIdentityResult(result);
                return BadRequest(errors);
            }
            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var frontEndURL = _configuration.GetValue<string>("FrontEndURL");
            var callbackUrl =
                $"{frontEndURL}/confirmemail?userId={user.Id}&" +
                $"code={WebUtility.UrlEncode(code)}";

            await _emailSender.SendEmailAsync(model.Email, "Confirm Email",
               $"Please confirm your email by clicking here: " +
               $"<a href='{callbackUrl}'>link</a>");

            return Ok();
        }

        [HttpPut("users/confirmemail/{userid}")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userid, [FromBody]ConfirmEmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errrors = CustomValidator.GetErrorsByModel(ModelState);
                return BadRequest(errrors);
            }
            var user = await _userManager.FindByIdAsync(userid);
            if (user == null)
            {
                return BadRequest(new { invalid = "User is not found" });
            }
            var result = await _userManager.ConfirmEmailAsync(user, model.Code);
            if (!result.Succeeded)
            {
                var errrors = CustomValidator.GetErrorsByIdentityResult(result);
                return BadRequest(errrors);
            }
            return Ok();
        }
    }
}