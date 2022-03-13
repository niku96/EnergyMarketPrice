using EMP_API.Authentification;
using EMP_API.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EMP_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration _configuration;
        public AuthenticateController(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await userManager.FindByNameAsync(model.Username);
            if (user.IsDeleted == false)
            {
                if (user != null && await userManager.CheckPasswordAsync(user, model.Password))
                {
                    var userRoles = await userManager.GetRolesAsync(user);

                    var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                    foreach (var userRole in userRoles)
                    {
                        authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                    }

                    var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

                    var token = new JwtSecurityToken(
                        issuer: _configuration["JWT:ValidIssuer"],
                        audience: _configuration["JWT:ValidAudience"],
                        expires: DateTime.Now.AddMinutes(2),
                        claims: authClaims,
                        signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                        );

                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        expiration = token.ValidTo
                    });
                }
            }
            else
            {

            }
            return Unauthorized();
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var userExists = await userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });

            ApplicationUser user = new ApplicationUser()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username,
                LastName = model.LastName,
                FirstName = model.FirstName,
                BirthDate = model.BirthDate,
                IsDeleted = false

            };


            var result = await userManager.CreateAsync(user, model.Password);
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action(nameof(ConfirmEmail), "Authentificate", new { token, email = model.Email }, Request.Scheme);
            SendEmail(model.Email, "Confirmation email link", @"Please confirm your account by <a href='https://localhost:44315/Authentificate/ConfirmEmail?userId="+token+"'>clicking here</a>.");
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });

            return Ok(new Response { Status = "Success", Message = "User created successfully!" });
        }


        //[HttpGet]
        //[ApiExplorerSettings(IgnoreApi = true)]
        //[Route("ConfirmEmail")]
        //public async Task<ActionResult<List<UserList>>>  ConfirmEmail() {
        //    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });
        //}

        [HttpPost]
        [Authorize]        
        [Route("changeUserStatus")]
        public async Task<IActionResult> ChangeUserStatus([FromBody] ChangeUserStatus model)
        {
            var userExists = await userManager.FindByNameAsync(model.Username);
            if (userExists == null)
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Inexistent username!" });
            userExists.IsDeleted = model.IsDeleted;

            
            var result = await userManager.UpdateAsync(userExists);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User change status failed! Please check user details and try again." });
            return Ok(new Response { Status = "Success", Message = "User change status successfully!" });
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public void SendEmail(string email, string subject, string htmlMessage)
        {
            MailAddress from = new MailAddress(_configuration["EmailSender:UserName"]);
            MailAddress to = new MailAddress(email);
            MailMessage m = new MailMessage(from, to);
            m.Subject = subject;
            m.Body = htmlMessage;
            m.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient(_configuration["EmailSender:Host"], Int32.Parse( _configuration["EmailSender:Port"]));
            smtp.Credentials = new NetworkCredential(_configuration["EmailSender:UserName"], _configuration["EmailSender:Password"]);
            smtp.EnableSsl = true;
            smtp.Send(m);
        }


        [HttpGet]
        [Authorize]
        [Route("getuserlist")]
        public async Task<ActionResult <List<UserList>>>  GetUserList()
        {
            List<UserList> UserList = new List<UserList>();
            var userList =  userManager.Users.ToList();
            if (userList.Count != 0)
            {
                foreach (var item in userList)
                {
                    UserList.Add(new UserList
                    {
                        BirthDate = item.BirthDate,
                        LastName = item.LastName,
                        FirstName = item.FirstName,
                        Username = item.UserName,
                        IsDeleted = item.IsDeleted
                    });
                    
                }
                return UserList;
            }
            else
            {
                return StatusCode(StatusCodes.Status204NoContent, new Response { Status = "Error", Message = "Users are not registered! Please check user details and try again." });
            }
                  }
        }

    }