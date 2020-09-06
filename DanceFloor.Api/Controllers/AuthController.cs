using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DanceFloor.Api.Extensions;
using DanceFloor.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

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
        private readonly IConfiguration _configuration;

        public AuthController(
            ILogger<AuthController> logger,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration configuration)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }
        
        [HttpPost("/login/external")]
        public async Task<ActionResult> LoginExternal([FromHeader(Name = "ExternalLoginScheme")] string scheme)
        {
            try
            {
                var info = await _signInManager.GetExternalLoginInfoAsync(scheme, null);

                if (info == null)
                    return Unauthorized(new { Reason = "" });
                
                var expiration = info.AuthenticationProperties.GetParameter<int?>("Expiration") ?? 500;
                var issuedAt = info.AuthenticationProperties.GetParameter<int?>("IssuedAt");
                var notBefore = info.AuthenticationProperties.GetParameter<int?>("NotBefore");
                var emailVerified = info.AuthenticationProperties.GetParameter<bool>("EmailVerified");
                
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var name = info.Principal.FindFirstValue(ClaimTypes.Name);
                var givenName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
                var surname = info.Principal.FindFirstValue(ClaimTypes.Surname);

                var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

                    user.Email = email;
                    user.UserName = email.GetUntilOrEmpty('@');
                    user.Name = name;
                    user.GivenName = givenName;
                    user.Surname = surname;
                    user.EmailConfirmed = emailVerified;

                    var identityResult = await _userManager.UpdateAsync(user);

                    if (!identityResult.Succeeded)
                        return Unauthorized(new { Reason = "" });
                    
                    var token = GenerateToken(user, expiration, issuedAt, notBefore);
                    
                    return Ok(new { Token = token });
                }
                else
                {
                    var user = new User
                    {
                        Email = email,
                        UserName = email.GetUntilOrEmpty('@'),
                        Name = name,
                        GivenName = givenName,
                        Surname = surname,
                        EmailConfirmed = emailVerified
                    };

                    var identityResult = await _userManager.CreateAsync(user);

                    if (!identityResult.Succeeded)
                        return Unauthorized(new { Reason = "" });
                    
                    identityResult = await _userManager.AddLoginAsync(user, info);

                    if (!identityResult.Succeeded)
                        return Unauthorized(new { Reason = "" });
                    
                    await _signInManager.SignInAsync(user, false);
                            
                    var token = GenerateToken(user, expiration, issuedAt, notBefore);
                    
                    return Created("/", new { Token = token });
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
                    return BadRequest(new { Reason = "User is already registered!" });

                var userName = credentials.Email.GetUntilOrEmpty('@');
                var nameParts = userName.Split(' ');
                var user = new User
                {
                    Name = userName,
                    Email = credentials.Email,
                    UserName = userName,
                    Surname = nameParts.Skip(1).LastOrDefault() ?? "",
                    GivenName = nameParts.SkipLast(1).Any() ?
                        string.Join(' ', nameParts.SkipLast(1)) : userName
                };
                
                var result = await _userManager.CreateAsync(user, credentials.Password);

                if (!result.Succeeded)
                    return BadRequest(new
                    {
                        Reason = string.Join(",", result.Errors?.Select(error => error.Description) ?? Array.Empty<string>())
                    });

                await _signInManager.SignInAsync(user, false);
                
                var token = GenerateToken(user, 5000, 0, 0);
                
                return Created("/", new { Token = token });
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
                    return Unauthorized(new { Reason="This email hasn't been registered!" });

                var result = await _signInManager.PasswordSignInAsync(user, credentials.Password, false, false);

                if (!result.Succeeded)
                    return Unauthorized(new { Reason = "Invalid credentials!" });

                var token = GenerateToken(user, 5000, 0, 0);
                
                return Ok(new { Token = token });
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
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal Server Error");
                throw;
            }
        }

        private string GenerateToken(User user, int? expiration = null, int? issuedAt = null, int? notBefore = null)
        {
            var jwtSection = _configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSection["Key"]);
            var securityKey = new SymmetricSecurityKey(key);
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                
                Subject = new ClaimsIdentity(new []
                {
                    // new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), 
                    // new Claim(ClaimTypes.Email, user.Email),
                    // new Claim(ClaimTypes.Name, user.Name),
                    // new Claim(ClaimTypes.GivenName, user.GivenName),
                    // new Claim(ClaimTypes.Surname, user.Surname),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), 
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.NameId, user.Name),
                    new Claim(JwtRegisteredClaimNames.GivenName, user.GivenName),
                    new Claim(JwtRegisteredClaimNames.FamilyName, user.Surname),
                }),
                Claims = new Dictionary<string, object>
                {
                    [JwtRegisteredClaimNames.Iat] = issuedAt,
                    [JwtRegisteredClaimNames.Exp] = expiration,
                    [JwtRegisteredClaimNames.Nbf] = notBefore
                },
                Expires = expiration.HasValue ? DateTime.UtcNow.AddSeconds(expiration.Value) : (DateTime?)null,
                IssuedAt = issuedAt.HasValue ? DateTime.UtcNow.AddSeconds(issuedAt.Value) : (DateTime?)null,
                NotBefore = notBefore.HasValue ? DateTime.UtcNow.AddSeconds(notBefore.Value) : (DateTime?)null,
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256),
                Audience = jwtSection["Audience"],
                Issuer = jwtSection["Issuer"]
            };
         
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            
            return tokenHandler.WriteToken(token);
        }
    }
}