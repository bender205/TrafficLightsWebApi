using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using TrafficLights.Auth.Core.Services;
using TrafficLights.Auth.Model.Auth;
using TrafficLights.Core;
using TrafficLights.Model.Entities;

namespace TrafficLights.Auth.Controllers
{
 
    [Authorize]
    [ApiController]/*
    [Route("[controller]")]*/
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        //IPasswordHasher<UserIdentityEntity>, PasswordHasher<UserIdentityEntity>
        private readonly IUserService _userService;
        private readonly UserManager<UserIdentityEntity> _userManager;
        private readonly SignInManager<UserIdentityEntity> _signInManager;
        private readonly IPasswordHasher<RegisterRequest> _passwordHasher;

        public UsersController(IUserService userService, UserManager<UserIdentityEntity> userManager,
            SignInManager<UserIdentityEntity> signInManager, IPasswordHasher<RegisterRequest> passwordHasher)
        {
            _userService = userService;
            _userManager = userManager;
            _signInManager = signInManager;
            _passwordHasher = passwordHasher;

        }


        [AllowAnonymous]
        //[HttpPost("authenticate")]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthenticateRequest model, [FromServices] IUserStore<UserIdentityEntity> store)
        {
            //var b = new UserStore<UserIdentityEntity>();
           // var a = store.FindByNameAsync(model.UserName.ToUpper(), CancellationToken.None);
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null) return BadRequest(new { message = "Username or password is incorrect" });
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, true, false);
            if (result.Succeeded)
            {
                var response = await _userService.GetAuthenticateResponceTokensAsync(user, IpAddress());
                return Ok(response);
            }
            else
            {
                return BadRequest(new { message = "Username or password is incorrect" });
            }

        }



        [AllowAnonymous]
        [HttpPost("register")]
        //TODO check if this user already exist
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            if (ModelState.IsValid)
            {
                var passwordHash = _passwordHasher.HashPassword(model, model.Password);

                UserIdentityEntity user = new UserIdentityEntity()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    UserName = model.UserName,
                    PasswordHash = passwordHash
                };
                // добавляем пользователя
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    /*// cookie setting
                    await _signInManager.SignInAsync(user, false);
                    return RedirectToAction("Index", "Home");*/
                    return Ok("Registered successfully");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return BadRequest(result.Errors);
                }

            }
            else
            {
                return BadRequest();
            }
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RevokeTokenRequest token)
        {
            var response = await _userService.RefreshTokenAsync(token.Token, IpAddress());

            if (response == null)
                return Unauthorized(new { message = "Invalid token" });

            return Ok(response.RefreshToken);
        }

        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest model)
        {
            // accept token from request body or cookie
            var token = model.Token;

            if (string.IsNullOrEmpty(token))
                return BadRequest(new { message = "Token is required" });

            var response = await _userService.RevokeTokenAsync(token, IpAddress());

            if (!response)
                return NotFound(new { message = "Token not found" });

            return Ok(new { message = "Token revoked" });
        }


        private string IpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                return Request.Headers["X-Forwarded-For"];
            }
            else
            {
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
        }
    }
}

