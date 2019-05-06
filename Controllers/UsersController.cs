using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using System.IdentityModel.Tokens.Jwt;
using Podwoozka.Helpers;
using Microsoft.Extensions.Options;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Podwoozka.Services;
using Podwoozka.Dtos;
using Podwoozka.Entities;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;


namespace Podwoozka.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;
        private IMapper _mapper;
        private readonly AppSettings _appSettings;
        private readonly IAuthorizationService _authorizationService;
        private readonly UserManager<IdentityUser> _userManager;

        private readonly IEmailSender _emailSender;

        public UsersController(
            IUserService userService,
            IMapper mapper,
            IOptions<AppSettings> appSettings,
            IAuthorizationService authorizationService,
            UserManager<IdentityUser> userManager,
            IEmailSender emailSender)
        {
            _userService = userService;
            _mapper = mapper;
            _appSettings = appSettings.Value;
            _authorizationService = authorizationService;
            _userManager = userManager;
            _emailSender = emailSender;

        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]UserDto userDto)
        {
            var user = _userService.Authenticate(userDto.UserName, userDto.Password);

            if (user == null)
                return BadRequest(new { message = "UserName or password is incorrect" });

            if (user.IsConfirmed == false)
            {
                return BadRequest(new { message = "Your account is not acitaved. Check your email(TAK EMAIL, SERIO DZIALA) " });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // return basic user info (without password) and token to store client side
            return Ok(new
            {
                Id = user.Id,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = tokenString
            });
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody]UserDto userDto)
        {
            // map dto to entity
            var user = _mapper.Map<User>(userDto);

            try
            {
                // save 
                user.IsConfirmed = false;
                _userManager.UpdateSecurityStampAsync(user);
                _userService.Create(user, userDto.Password);
                if (user.Email != null)
                {
                    var userCreated = _userService.GetByUsername(user.UserName);
                    var code = _userManager.GenerateEmailConfirmationTokenAsync(userCreated);
                    var codeHtmlVersion = HttpUtility.UrlEncode(code.Result);
                    string url = "https://localhost:5001/users/confirm?name=" + user.UserName + "&code=" + codeHtmlVersion;
                    _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                                $"Please confirm your account by <a href='{url}'>clicking here</a>.");
                }
                else
                {
                    //TODO
                }


                return Ok(userDto);
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpGet("confirm")]
        [AllowAnonymous]
        public async Task<IActionResult> Confirm(string name, string code)
        {
            if (code == null || name == null)
            {
                return BadRequest("Błędny kod aktywacyjny");
            }
            var user = _userService.GetByUsername(name);
            if (user == null)
            {
                return BadRequest("Uzytkownik nie istnieje");
            }
            var codeHtmlDecoded = HttpUtility.UrlDecode(code);
            var codeWithoutSpaces = codeHtmlDecoded.Replace(' ', '+');
            var result = await _userManager.ConfirmEmailAsync(user, codeWithoutSpaces);
            return Ok(result);

        }




        [HttpGet]
        [AllowAnonymous]

        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            var userDtos = _mapper.Map<IList<UserDto>>(users);
            return Ok(userDtos);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var user = _userService.GetById(id);
            var userDto = _mapper.Map<UserDto>(user);
            return Ok(userDto);
        }


        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody]UserDto userDto)
        {

            // map dto to entity and set id
            var user = _mapper.Map<User>(userDto);
            user.Id = id;

            try
            {
                // save 
                _userService.Update(user, userDto.Password);
                return Ok();
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _userService.Delete(id);
            return Ok();
        }

    }
}