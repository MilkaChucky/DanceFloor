using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DanceFloor.Api.Extensions;
using DanceFloor.Api.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace DanceFloor.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    // [AutoValidateAntiforgeryToken]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AuthController(
            ILogger<AuthController> logger,
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet("/login/social")]
        public async Task<ActionResult> ExternalLoginCallback()
        {
            try
            {
                var info = await _signInManager.GetExternalLoginInfoAsync();

                if (info == null)
                    return Unauthorized(new {Reason = ""});

                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(email);
                    return Ok(user);
                }
                else
                {
                    var user = new User
                    {
                        Email = email,
                        UserName = email.GetUntilOrEmpty('@')
                    };

                    var identityResult = await _userManager.CreateAsync(user);

                    if (identityResult.Succeeded)
                    {
                        identityResult = await _userManager.AddLoginAsync(user, info);

                        if (identityResult.Succeeded)
                        {
                            await _signInManager.SignInAsync(user, false);
                            return Created("/", user);
                        }
                    }
                
                    return Unauthorized(new {Reason = ""});
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal Server Error");
                throw;
            }
        }
        
        [HttpPost("/register")]
        public async Task<ActionResult> Register([FromBody] UserCredentials credentials)
        {
            try
            {
                var existing = await _userManager.FindByEmailAsync(credentials.Email);

                if (existing != null)
                    return BadRequest(new {Reason = "User is already registered!"});
                
                var user = new User
                {
                    Email = credentials.Email,
                    UserName = credentials.Email.GetUntilOrEmpty('@')
                };
                
                var result = await _userManager.CreateAsync(user, credentials.Password);

                if (!result.Succeeded)
                    return Ok(string.Join(",", result.Errors?.Select(error => error.Description) ?? Array.Empty<string>()));
                
                return Created("/", credentials);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal Server Error");
                throw;
            }
        }

        [HttpPost("/login/social")]
        // [ValidateAntiForgeryToken]
        public async Task<ActionResult> LoginSocial([Required] string provider)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(provider))
                    return BadRequest(new {Reason = "The provider cannot be empty!"});
            
                var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();

                if (schemes.All(scheme => scheme.Name != provider))
                    return BadRequest(new {Reason = $"{provider} is not a supported external login provider!"});

                var uri = Url.Action("ExternalLoginCallback");
                var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, uri);
            
                return Challenge(properties, provider);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal Server Error");
                throw;
            }
        }

        [HttpPost("/login")]
        public async Task<ActionResult> Login([FromBody] UserCredentials credentials)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(credentials.Email);
            
                if (user == null)
                    return Unauthorized(new {Reason="This email hasn't been registered!"});

                var result = await _signInManager.PasswordSignInAsync(user, credentials.Password, false, false);

                if (!result.Succeeded)
                    return Unauthorized(new {Reason = "Invalid credentials!"});
            
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal Server Error");
                throw;
            }
        }

        [HttpPost("/logout")]
        public async Task<ActionResult> Logout()
        {
            try
            {
                await _signInManager.SignOutAsync();
                return RedirectToRoute("/login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal Server Error");
                throw;
            }
        }
    }
}