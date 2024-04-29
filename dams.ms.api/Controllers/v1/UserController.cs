using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Dams.ms.auth.Services;
using Dams.ms.auth.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Dams.ms.auth.Data;
using System.Net;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using System.Text;
using System.Threading;
using System.Runtime.ConstrainedExecution;
using System.Net.Http;

namespace Dams.ms.auth.Controllers.v1
{

   
    [EnableCors("AllowAllHeaders")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0", Deprecated = false)]
    [ApiExplorerSettings(GroupName = "v1")]
    [Authorize]
    [ApiController]
    public class UserController : Controller
    {
        #region Private Variables
        public readonly IConfigurationRoot _objConfiguration;
        private readonly UserService _userService;
        private readonly UserManager<UserEntity> _userManager;
        private readonly IDMDbContext _context;
        private readonly EmailTemplateService _emailTemplateService;
#pragma warning disable CS0169 // The field 'UserController._getEmployeeUrl' is never used
        private readonly string _getEmployeeUrl;
#pragma warning restore CS0169 // The field 'UserController._getEmployeeUrl' is never used
        private readonly string _getEmployerUrl;
        private readonly EmailService _emailService;
        private readonly string _apiUrl;
        private readonly string _apiKey;
        private readonly string _twiliosettingsPhoneNumber;
        private readonly string _getDamsUrl;
        private readonly string _getdoctorUrl;
        private readonly string _getadminUrl;
        #endregion

        #region Constructor 
        public UserController(UserManager<UserEntity> userManager, IConfigurationRoot Configuration, IDMDbContext idmDbContext)
        {
            _objConfiguration = Configuration;
            _userService = new UserService(_objConfiguration);
            _userManager = userManager;
            _context = idmDbContext;
            _emailTemplateService = new EmailTemplateService(_objConfiguration);
            _emailService = new EmailService();
            _getDamsUrl = _objConfiguration.GetSection("EmailTemplateUrls").GetSection("DamsUrl").Value;
            _getdoctorUrl = _objConfiguration.GetSection("EmailTemplateUrls").GetSection("DoctorUrl").Value;
            _getadminUrl = _objConfiguration.GetSection("EmailTemplateUrls").GetSection("AdminUrl").Value;
            _apiUrl = _objConfiguration.GetSection("SMSAPI").GetSection("apiUrl").Value;
            _apiKey = _objConfiguration.GetSection("SMSAPI").GetSection("apiKey").Value;
            //_emailService = emailService;
        }
        #endregion

        #region Get UserName 
        [HttpGet("GetUserName")]
        [AllowAnonymous]
        public string GetUserName()
        {
            return User?.Claims?.FirstOrDefault(c => c.Type == "UserName")?.Value.ToString() ?? "Anonymous";
        }
        #endregion

        #region Hash Password

        [NonAction]
        public string HashPassword(string password)
        {
            byte[] salt;
            byte[] buffer2;
            if (password == null) throw new ArgumentNullException("password");
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, 0x10, 0x3e8))
            {
                salt = bytes.Salt;
                buffer2 = bytes.GetBytes(0x20);
            }
            byte[] dst = new byte[0x31];
            Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
            Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
            return Convert.ToBase64String(dst);
        }
        #endregion

        #region Get UserId
        [HttpGet("GetUserId")]
        [AllowAnonymous]
        public string GetUserId()
        {
            return User?.Claims?.FirstOrDefault(c => c.Type == "UserId")?.Value.ToString() ?? "0";
        }
        #endregion

        #region Get User By Id
        [AllowAnonymous]
        [HttpGet("GetUserById")]
        public async Task<UserEntity> GetUserById(int userId)
        {
            return await _userManager.FindByIdAsync(userId.ToString());
        }
        #endregion

        #region Check Duplicate User By Email
        [AllowAnonymous]
        [HttpGet("CheckDuplicateUserName")]
        public async Task<IEnumerable<UserEntity>> CheckDuplicateUserName(string userName,int UserTypeId)
        {
            List<UserEntity> users = await _userManager.Users.Where(e => e.UserName.ToLower().Trim() == userName.ToLower().Trim() && e.IsDeleted == false).ToListAsync();
            EmailService _emailService = new EmailService();

            EmailTemplatesQuery emailTemplatesQuery = new EmailTemplatesQuery();
            emailTemplatesQuery.Name = "ForgetPassword";

            var emailTemplates = await _emailTemplateService.GetEmailTemplates(emailTemplatesQuery);

            try
            {
                if (users.Count > 0)
                {

                    EmailCommands oEmailCommand = new EmailCommands();
                    if (users[0].UserTypeId == 2)

                    {

                        var resetLink = _getDamsUrl + "/auth/reset-password/" + users[0].UserGuid;
                        emailTemplates.TemplateHtml = emailTemplates.TemplateHtml.Replace("%%user%%", (users[0].FirstName == null || users[0].FirstName == "string") ? userName : users[0].FirstName);
                        emailTemplates.TemplateHtml = emailTemplates.TemplateHtml.Replace("%%ResetLink%%", resetLink);

                        oEmailCommand.To = users[0].Email;
                        oEmailCommand.Subject = "Forgot Password ";
                        oEmailCommand.IsHtml = true;

                        oEmailCommand.Body = emailTemplates.TemplateHtml;
                        await _emailService.SendEmail(oEmailCommand);
                    }
                    else if (users[0].UserTypeId == 3)
                    {

                        var resetLink = _getdoctorUrl + "/auth/reset-password/" + users[0].UserGuid;
                        emailTemplates.TemplateHtml = emailTemplates.TemplateHtml.Replace("%%user%%", (users[0].FirstName == null || users[0].FirstName == "string") ? userName : users[0].FirstName);
                        emailTemplates.TemplateHtml = emailTemplates.TemplateHtml.Replace("%%ResetLink%%", resetLink);

                        oEmailCommand.To = users[0].Email;
                        oEmailCommand.Subject = "Forgot Password ";
                        oEmailCommand.IsHtml = true;

                        oEmailCommand.Body = emailTemplates.TemplateHtml;
                        await _emailService.SendEmail(oEmailCommand);

                    }
                    else if (users[0].UserTypeId == 1)
                    {

                        var resetLink = _getadminUrl + "/auth/reset-password/" + users[0].UserGuid;
                        emailTemplates.TemplateHtml = emailTemplates.TemplateHtml.Replace("%%user%%", (users[0].FirstName == null || users[0].FirstName == "string") ? userName : users[0].FirstName);
                        emailTemplates.TemplateHtml = emailTemplates.TemplateHtml.Replace("%%ResetLink%%", resetLink);

                        oEmailCommand.To = users[0].Email;
                        oEmailCommand.Subject = "Forgot Password ";
                        oEmailCommand.IsHtml = true;

                        oEmailCommand.Body = emailTemplates.TemplateHtml;
                        await _emailService.SendEmail(oEmailCommand);

                    }
                    else
                    {
                        return users = null;
                    }



                }

            }

            catch (Exception ex)

            {
                var x = ex;
            }

            return users;

        }
        #endregion

        #region Update Password
        [AllowAnonymous]
        [HttpPost("UpdatePassword")]

        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordBody body)
        {
            if (body.Password == body.ConfirmPassword)
            {
                string _hashPassword = HashPassword(body.Password);
                var users = await _userManager.Users.Where(e => (e.UserGuid == body.UserId)).ToListAsync();
                users = (users == null) ? new List<UserEntity>() : users;
                users.FirstOrDefault().PasswordHash = _hashPassword;
                users.FirstOrDefault().EmailConfirmed = true;
                users.FirstOrDefault().SecurityStamp = Guid.NewGuid().ToString();

                var history = await _context.PasswordHistory.Where(h => (h.UserGuid == body.UserId && (h.PasswordHash1 == body.Password || h.PasswordHash2 == body.Password || h.PasswordHash3 == body.Password || h.PasswordHash4 == body.Password || h.PasswordHash5 == body.Password))).ToListAsync();
                if (history == null || history.Count <= 0)
                {
                    IdentityResult result = await _userManager.UpdateAsync(users.FirstOrDefault());
                    var query = "UPDATE [dbo].[PasswordHistory]  SET PasswordHash5 = PasswordHash4, PasswordHash4 = PasswordHash3, PasswordHash3 = PasswordHash2, PasswordHash2 = PasswordHash1, PasswordHash1 = '" + body.Password + "' where UserGuid = '" + body.UserId + "'";
                    _context.Database.ExecuteSqlCommand(query);
                    if (result.Succeeded)
                    {
                        return await Task.FromResult(new SuccessResponse(new List<string>(), "Password updated successfully", HttpStatusCode.OK));
                    }
                    else
                    {
                        return await Task.FromResult(new ErrorResponse(result, "Password update failed", HttpStatusCode.BadRequest));
                    }

                }
                else
                {
                    return await Task.FromResult(new ErrorResponse(new List<string>(), "Last 5 passwords are not accepted. Please create a new, unique password for security.", HttpStatusCode.BadRequest));
                }

            }
            else
            {
                return await Task.FromResult(new ErrorResponse(new List<string>(), "Password and Confirm Password Mismatch", HttpStatusCode.BadRequest));
            }

        }
        #endregion

        #region Get Users Except Id
        [AllowAnonymous]
        [HttpGet("GetUsersExceptId")]
        public List<ChatUser> GetUsersExceptId(Guid userGuid, int userTypeId)
        {
            return _userService.GetUsersExceptId(new ChatUserBody() { UserGuid = userGuid, UserTypeId = userTypeId });
        }
        #endregion

        #region Update EmailConfirm 
        [AllowAnonymous]
        [HttpPost("UpdateEmailConfirmed")]
        public async Task<IActionResult> UpdateEmailConfirmed([FromBody] EmailConfirmBody body)
        {


            var users = await _userManager.Users.Where(e => (e.UserGuid == body.UserGuid)).ToListAsync();
            users = (users == null) ? new List<UserEntity>() : users;


            users.FirstOrDefault().EmailConfirmed = true;

            IdentityResult result = await _userManager.UpdateAsync(users.FirstOrDefault());
            if (result.Succeeded)
            {
                return await Task.FromResult(new SuccessResponse(new List<string>(), "Updated Email Confirm successfully", HttpStatusCode.OK));
            }
            else
            {
                return await Task.FromResult(new ErrorResponse(result, "Updated Email Confirm Successfully ", HttpStatusCode.BadRequest));
            }
        }
        #endregion

        #region Update Resend Email 
        [AllowAnonymous]
        [HttpPost("ResendEmailVerification")]
        public async Task<IActionResult> ResendEmailVerification([FromBody] AppUser oAppUser)
        {
            var Users = await _userManager.Users.Where(e => (e.UserName.Trim() == oAppUser.Email.Trim() && !e.IsDeleted)).ToListAsync();
            Users = Users == null ? new List<UserEntity>() : Users;
            if (oAppUser.UpdateDate != null && Users.Count != 0)
            {
                Users.FirstOrDefault().UpdatedDt = oAppUser.UpdateDate;
                IdentityResult result = await _userManager.UpdateAsync(Users.FirstOrDefault());
                EmailTemplatesQuery emailTemplatesQuery = new EmailTemplatesQuery();
                emailTemplatesQuery.Name = "WelcomeUser"; 
                var emailTemplates = await _emailTemplateService.GetEmailTemplates(emailTemplatesQuery);
                if (Users.FirstOrDefault().UserTypeId == 3)
                {
                    emailTemplates.TemplateHtml = emailTemplates.TemplateHtml.Replace("%%user%%", oAppUser.FirstName);
                    emailTemplates.TemplateHtml = emailTemplates.TemplateHtml.Replace("%%UserType%%", "Doctor");
                    var verifyLink = _getdoctorUrl + "/auth/user-verification/" + oAppUser.UserGuid;
                    emailTemplates.TemplateHtml = emailTemplates.TemplateHtml.Replace("%%VERIFYLINK%%", verifyLink);
                }
                else if (Users.FirstOrDefault().UserTypeId == 2)
                {
                    emailTemplates.TemplateHtml = emailTemplates.TemplateHtml.Replace("%%user%%", oAppUser.FirstName);
                    emailTemplates.TemplateHtml = emailTemplates.TemplateHtml.Replace("%%UserType%%", "Coordinator");
                    var verifyLink = _getDamsUrl + "/auth/user-verification/" + oAppUser.UserGuid;
                    emailTemplates.TemplateHtml = emailTemplates.TemplateHtml.Replace("%%VERIFYLINK%%", verifyLink);
                }
                EmailCommands oEmailCommand = new EmailCommands();
                oEmailCommand.To = Users.FirstOrDefault().Email;
                oEmailCommand.Subject = "Welcome Mail";
                oEmailCommand.IsHtml = true;
                oEmailCommand.Body = emailTemplates.TemplateHtml;

                await _emailService.SendEmail(oEmailCommand);
                //save user profile
                return new SuccessResponse(new List<string>(), "Resend email successfully", HttpStatusCode.OK);
                // return await Task.FromResult(new SuccessResponse(new List<string>(), "Resend email successfully", HttpStatusCode.OK)); 
            }
            else
            {
                return await Task.FromResult(new ErrorResponse("Resend email failed", HttpStatusCode.BadRequest));
            }

        }
        #endregion
        #region Get User By Id
        [AllowAnonymous]
        [HttpGet("GetUserByGuid")]
        public async Task<UserEntity> GetUserByGuid(Guid userGuid)
        {
            // var users = await _userManager.Users.Where(e => (e.UserGuid == oAppUser.UserGuid)).ToListAsync();
            var users = await _userManager.Users.Where(e => (e.UserGuid == userGuid)).ToListAsync();
            return users.FirstOrDefault();
        }
        #endregion

        [AllowAnonymous]
        [HttpPost("SentVerificationCodeFromLogin")]

        public async Task<IActionResult> SentVerificationCodeFromLogin([FromBody] LoginOtpCommand body)
        {
            var users = await _userManager.Users.Where(e => (e.PhoneNumber == body.PhoneNumber && e.CountryCode == body.CountryCode && !e.IsDeleted && e.UserTypeId == 5)).ToListAsync();
            users = (users == null) ? new List<UserEntity>() : users;
            Random generator = new Random();
            String OTP = generator.Next(0, 1000000).ToString("D6");
            if (users.Count == 1)
            {
                try
                {
                    users.FirstOrDefault().PhoneNumber = body.PhoneNumber;
                    users.FirstOrDefault().CountryCode = body.CountryCode;
                    var CompletePhoneNumber = body.PhoneNumber;
                    users.FirstOrDefault().Otp = OTP;
                    await VerificationCodeSent(CompletePhoneNumber, OTP);
                }
                catch (Exception ex)
                {
                    return await Task.FromResult(new ErrorResponse(ex.Message));
                }
                IdentityResult result = await _userManager.UpdateAsync(users.FirstOrDefault());
                if (result.Succeeded)
                {
                    return await Task.FromResult(new SuccessResponse(new List<string>(), "Generated phone OTP successfully", HttpStatusCode.OK));
                }
                else
                {
                    return await Task.FromResult(new ErrorResponse(result, "Generated phone OTP unSuccessfully ", HttpStatusCode.BadRequest));
                }
            }
            else
            {
                return await Task.FromResult(new ErrorResponse("Please give valid phonenumber and countrycode ", HttpStatusCode.BadRequest));
            }

        }
        #region Update User Details
        [AllowAnonymous]
        [HttpPost("UpdateUserDetails")]

        public async Task<IActionResult> UpdateUserDetails([FromBody] UsersBody body)
        {
            var users = await _userManager.Users.Where(e => (e.UserGuid == body.UserGuid)).ToListAsync();
            users = (users == null) ? new List<UserEntity>() : users;
            users.FirstOrDefault().EmailConfirmed = true;
            users.FirstOrDefault().SecurityStamp = Guid.NewGuid().ToString();
            IdentityResult result = await _userManager.UpdateAsync(users.FirstOrDefault());
            var query = "UPDATE [dbo].[Users] SET FirstName = '" + body.FirstName + "', LastName = '" + body.LastName + "', PhoneNumber = '" + body.PhoneNumber + "' WHERE UserGuid = '" + body.UserGuid + "'";
            _context.Database.ExecuteSqlCommand(query);
            if (result.Succeeded)
            {
                return await Task.FromResult(new SuccessResponse(new List<string>(), "Users Details updated successfully", HttpStatusCode.OK));
            }
            else
            {
                return await Task.FromResult(new ErrorResponse(result, "User Details update failed", HttpStatusCode.BadRequest));
            }

        }
        #endregion

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> VerificationCodeSent(string CompletePhoneNumber, string OTP)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, _apiUrl);
            request.Headers.Add("Authorization", _apiKey);
            var content = new StringContent(
                Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    phone_number = CompletePhoneNumber,
                    otp = OTP
                }),
                Encoding.UTF8,
                "application/json");
            request.Content = content;

            try
            {
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                string responseContent = await response.Content.ReadAsStringAsync();
                return Ok(responseContent);
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred: " + ex.Message);
            }
        }

    }
}



