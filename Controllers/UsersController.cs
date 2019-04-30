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
            var user = _userService.Authenticate(userDto.Username, userDto.Password);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

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
                Username = user.Username,
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
            if (user.Email != null)
            {
                var code = _userManager.GenerateEmailConfirmationTokenAsync(user);

                // var callbackUrl = Url.Page(
                //     "/Account/ConfirmEmail",
                //     pageHandler: null,
                //     values: new { userId = user.Id, code = code },
                //     protocol: Request.Scheme);

                // _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                //    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                string url = "https://localhost:5001/users/confirm/" + user.Id + "/" + code.Result;
                _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                            $"Please confirm your account by <a href='{url}'>clicking here</a>.");
            }
            else
            {
                //TODO
            }
            try
            {
                // save 
                user.IsConfirmed = false;
                _userService.Create(user, userDto.Password);
                return Ok();
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPost("confirm/{id}/{code}")]
        [AllowAnonymous]
        public IActionResult Confirm(string id, string code)
        {
            if (code == null || id == null)
            {
                return BadRequest("Błędny kod aktywacyjny");
            }
            var user = _userService.GetById(int.Parse(id));
            if (user == null)
            {
                return BadRequest("Uzytkownik nie istnieje");
            }
            var result = _userManager.ConfirmEmailAsync(user, code);
            return Ok(result);

        }




        [HttpGet]
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