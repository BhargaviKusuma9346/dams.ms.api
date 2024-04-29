using Dams.ms.auth.Adapters;
using Dams.ms.auth.Data;
using Dams.ms.auth.Repositories;
using Dams.ms.auth.Services;
using Dams.ms.auth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Twilio.TwiML.Messaging;
using System.Xml.Linq;
using Twilio.Types;
using System.Net.Http;
using System.Numerics;
using static System.Net.WebRequestMethods;
using System.Text;
using System.Threading;

namespace Dams.ms.auth.Controllers.v1
{
    [EnableCors("AllowAllHeaders")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0", Deprecated = false)]
    [ApiExplorerSettings(GroupName = "v1")]
    [Authorize]
    [ApiController]
    public class AuthController : Controller
    {
        #region Private Variables
        public readonly IConfigurationRoot _objConfiguration;
        private readonly AuthService _authService;
        private readonly UserManager<UserEntity> _userManager;
        private readonly UserService userService;
        private readonly IDMDbContext _context;
        private readonly int _accountid = 0;
        private readonly EmailService _emailService;
        private readonly EmailTemplateService _emailTemplateService;
        private readonly string _getDamsUrl;
        private readonly string _getdoctorUrl;
        private readonly string _getAdminUrl;
        private readonly string _whatsappApiUrl;
        private readonly string _whatsappApiKey;
        private readonly string _smsApiUrl;
        private readonly string _smsApiKey;
        private readonly string _tinyapiUrl;
        private readonly string _tinyurlApiKey;
        private static readonly HttpClient client = new HttpClient();

        #endregion

        #region Constructor 
        public AuthController(IConfigurationRoot Configuration, UserManager<UserEntity> userManager, IDMDbContext idmDbContext, IRequestContext context)
        {
            _objConfiguration = Configuration;
            _authService = new AuthService(_objConfiguration);
            _userManager = userManager;
            _context = idmDbContext;
            _accountid = context.UserId;
            userService = new UserService(_objConfiguration);
            _emailService = new EmailService();
            _emailTemplateService = new EmailTemplateService(_objConfiguration);
            _getAdminUrl = _objConfiguration.GetSection("EmailTemplateUrls").GetSection("AdminUrl").Value;
            _getDamsUrl = _objConfiguration.GetSection("EmailTemplateUrls").GetSection("DamsUrl").Value;
            _getdoctorUrl = _objConfiguration.GetSection("EmailTemplateUrls").GetSection("DoctorUrl").Value;
            _whatsappApiUrl = _objConfiguration.GetSection("WhatsappAPI").GetSection("apiUrl").Value;
            _whatsappApiKey = _objConfiguration.GetSection("WhatsappAPI").GetSection("apiKey").Value;
            _smsApiUrl = _objConfiguration.GetSection("SMSMeetingAPI").GetSection("apiUrl").Value;
            _smsApiKey = _objConfiguration.GetSection("SMSMeetingAPI").GetSection("apiKey").Value;
            _tinyapiUrl = _objConfiguration.GetSection("TinyUrlAPI").GetSection("apiUrl").Value;
            _tinyurlApiKey = _objConfiguration.GetSection("TinyUrlAPI").GetSection("apiKey").Value;

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

        #region User Signup
        [AllowAnonymous]
        [HttpPost("Signup")]
        public async Task<JsonResult> Signup([FromBody] RegisterUserBody mUser)
        {
            var exUsers = await _userManager.Users.Where(e => (e.UserName.Trim() == mUser.Email.Trim() && !e.IsDeleted)).ToListAsync();
            exUsers = exUsers == null ? new List<UserEntity>() : exUsers;
            EmailTemplatesQuery emailTemplatesQuery = new EmailTemplatesQuery();
            emailTemplatesQuery.Name = "WelcomeAdmin"; var emailTemplates = await _emailTemplateService.GetEmailTemplates(emailTemplatesQuery);
            if (exUsers.Count == 0)
            {
                var cUser = new UserEntity() { Email = mUser.Email, UserTypeId = 1, LoginTypeId = 1, UserName = mUser.Email, UserGuid = Guid.NewGuid(), FirstName = mUser.FirstName, LastName = mUser.LastName, PhoneNumber = mUser.PhoneNumber, CountryCode = mUser.CountryCode, EmailConfirmed=true,CreatedDt = DateTime.Now, UpdatedDt = DateTime.Now };
                var vResult = await _userManager.CreateAsync(cUser, mUser.Password);
                var pHistory = new PasswordHistory() { UserGuid = cUser.UserGuid, IsDeleted = false, PasswordHash1 = mUser.Password, PasswordHash2 = mUser.Password, PasswordHash3 = mUser.Password, PasswordHash4 = mUser.Password, PasswordHash5 = mUser.Password, CreatedDt = DateTime.Now, UpdatedDt = DateTime.Now };
                _context.Add(pHistory);
                _context.SaveChanges();

                emailTemplates.TemplateHtml = emailTemplates.TemplateHtml.Replace("%%user%%", mUser.Email);
                emailTemplates.TemplateHtml = emailTemplates.TemplateHtml.Replace("%%UserType%%", "Admin");
                emailTemplates.TemplateHtml = emailTemplates.TemplateHtml.Replace("%%UserName%%", mUser.Email);
                emailTemplates.TemplateHtml = emailTemplates.TemplateHtml.Replace("%%Password%%", mUser.Password);
                var verifyLink = _getAdminUrl + "/auth/login/" + cUser.UserGuid;
                emailTemplates.TemplateHtml = emailTemplates.TemplateHtml.Replace("%%VERIFYLINK%%", verifyLink);


                if (vResult.Succeeded == true)
                {
                    EmailCommands oEmailCommand = new EmailCommands();

                    oEmailCommand.To = mUser.Email;
                    oEmailCommand.Subject = "Welcome Mail";
                    oEmailCommand.IsHtml = true;
                    oEmailCommand.Body = emailTemplates.TemplateHtml;

                    await _emailService.SendEmail(oEmailCommand);
                    //save user profile
                    return new SuccessResponse(new List<string>(), "User Registered Successfully", HttpStatusCode.OK);

                }

                else
                {
                    return new ErrorResponse(vResult.Errors, "User Registration Failed", HttpStatusCode.BadRequest);
                }
            }
            else
            {
                return new ErrorResponse(new List<string>(), "User Already Exists", HttpStatusCode.BadRequest);
            }
        }
        #endregion



        #region User Signup
        [AllowAnonymous]
        [HttpPost("MobileSignup")]
        public async Task<JsonResult> MobileSignup([FromBody] RegisterMobileBody mUser)
        {
            var exUsers = await _userManager.Users.Where(e => (e.PhoneNumber.Trim() == mUser.PhoneNumber.Trim() && e.UserTypeId==5 && !e.IsDeleted)).ToListAsync();
            exUsers = exUsers == null ? new List<UserEntity>() : exUsers;
            EmailTemplatesQuery emailTemplatesQuery = new EmailTemplatesQuery();
            emailTemplatesQuery.Name = "WelcomePatient"; 
            var emailTemplates = await _emailTemplateService.GetEmailTemplates(emailTemplatesQuery);
            if (exUsers.Count == 0)
            {
                var cUser = new UserEntity() { Email = mUser.Email, UserTypeId = 5, LoginTypeId = 5, UserName = mUser.Email, UserGuid = Guid.NewGuid(), FirstName = mUser.FirstName, LastName = mUser.LastName, PhoneNumber = mUser.PhoneNumber, CountryCode = mUser.CountryCode, CreatedDt = DateTime.Now, UpdatedDt = DateTime.Now };
                var vResult = await _userManager.CreateAsync(cUser);
                var pHistory = new PasswordHistory() { UserGuid = cUser.UserGuid, IsDeleted = false, CreatedDt = DateTime.Now, UpdatedDt = DateTime.Now };
                _context.Add(pHistory);
                _context.SaveChanges();

                emailTemplates.TemplateHtml = emailTemplates.TemplateHtml.Replace("%%user%%", mUser.FirstName);

                if (vResult.Succeeded == true)
                {
                    EmailCommands oEmailCommand = new EmailCommands();

                    oEmailCommand.To = mUser.Email;
                    oEmailCommand.Subject = "Welcome Mail";
                    oEmailCommand.IsHtml = true;
                    oEmailCommand.Body = emailTemplates.TemplateHtml;

                    await _emailService.SendEmail(oEmailCommand);
                    //save user profile
                    return new SuccessResponse(new List<string>(), "User Registered Successfully", HttpStatusCode.OK);

                }

                else
                {
                    return new ErrorResponse(vResult.Errors, "User Registration Failed", HttpStatusCode.BadRequest);
                }
            }
            else
            {
                return new ErrorResponse(new List<string>(), "User Already Exists", HttpStatusCode.BadRequest);
            }
        }
        #endregion

        #region User Signup
        [AllowAnonymous]
        [HttpPost("CoordinatorSignup")]
        public async Task<JsonResult> CoordinatorSignup([FromBody] RegisterUserBody mUser)
        {
            var exUsers = await _userManager.Users.Where(e => (e.UserName.Trim() == mUser.Email.Trim() && !e.IsDeleted)).ToListAsync();
            exUsers = exUsers == null ? new List<UserEntity>() : exUsers;
            EmailTemplatesQuery emailTemplatesQuery = new EmailTemplatesQuery();
            emailTemplatesQuery.Name = "WelcomeUser"; var emailTemplates = await _emailTemplateService.GetEmailTemplates(emailTemplatesQuery);
            if (exUsers.Count == 0)
            {
                var cUser = new UserEntity() { Email = mUser.Email, UserTypeId = 2, LoginTypeId = 2, UserName = mUser.Email, UserGuid = Guid.NewGuid(), FirstName = mUser.FirstName, LastName = mUser.LastName, PhoneNumber = mUser.PhoneNumber, CountryCode = mUser.CountryCode, CreatedDt = DateTime.Now, UpdatedDt = DateTime.Now };
                var vResult = await _userManager.CreateAsync(cUser, mUser.Password);
                var pHistory = new PasswordHistory() { UserGuid = cUser.UserGuid, IsDeleted = false, PasswordHash1 = mUser.Password, PasswordHash2 = mUser.Password, PasswordHash3 = mUser.Password, PasswordHash4 = mUser.Password, PasswordHash5 = mUser.Password, CreatedDt = DateTime.Now, UpdatedDt = DateTime.Now };
                _context.Add(pHistory);
                _context.SaveChanges();

                emailTemplates.TemplateHtml = emailTemplates.TemplateHtml.Replace("%%user%%", mUser.FirstName);
                emailTemplates.TemplateHtml = emailTemplates.TemplateHtml.Replace("%%UserType%%", "Coordinator");
                var verifyLink = _getDamsUrl + "/auth/reset-password/" + cUser.UserGuid;
                emailTemplates.TemplateHtml = emailTemplates.TemplateHtml.Replace("%%VERIFYLINK%%", verifyLink);


                if (vResult.Succeeded == true)
                {
                    EmailCommands oEmailCommand = new EmailCommands();

                    oEmailCommand.To = mUser.Email;
                    oEmailCommand.Subject = "Welcome Mail";
                    oEmailCommand.IsHtml = true;
                    oEmailCommand.Body = emailTemplates.TemplateHtml;

                    await _emailService.SendEmail(oEmailCommand);
                    //save user profile
                    return new SuccessResponse(new List<string>(), "User Registered Successfully", HttpStatusCode.OK);

                }

                else
                {
                    return new ErrorResponse(vResult.Errors, "User Registration Failed", HttpStatusCode.BadRequest);
                }
            }
            else
            {
                return new ErrorResponse(new List<string>(), "User Already Exists", HttpStatusCode.BadRequest);
            }
        }
        #endregion

        #region User Signup
        [AllowAnonymous]
        [HttpPost("DoctorSignup")]
        public async Task<JsonResult> DoctorSignup([FromBody] RegisterUserBody mUser)
        {
            var exUsers = await _userManager.Users.Where(e => (e.UserName.Trim() == mUser.Email.Trim() && !e.IsDeleted)).ToListAsync();
            exUsers = exUsers == null ? new List<UserEntity>() : exUsers;
            EmailTemplatesQuery emailTemplatesQuery = new EmailTemplatesQuery();
            emailTemplatesQuery.Name = "WelcomeUser"; var emailTemplates = await _emailTemplateService.GetEmailTemplates(emailTemplatesQuery);
            if (exUsers.Count == 0)
            {
                var cUser = new UserEntity() { Email = mUser.Email, UserTypeId = 3, LoginTypeId = 3, UserName = mUser.Email, UserGuid = Guid.NewGuid(), FirstName = mUser.FirstName, LastName = mUser.LastName, PhoneNumber = mUser.PhoneNumber, CountryCode = mUser.CountryCode, CreatedDt = DateTime.Now, UpdatedDt = DateTime.Now };
                var vResult = await _userManager.CreateAsync(cUser, mUser.Password);
                var pHistory = new PasswordHistory() { UserGuid = cUser.UserGuid, IsDeleted = false, PasswordHash1 = mUser.Password, PasswordHash2 = mUser.Password, PasswordHash3 = mUser.Password, PasswordHash4 = mUser.Password, PasswordHash5 = mUser.Password, CreatedDt = DateTime.Now, UpdatedDt = DateTime.Now };
                _context.Add(pHistory);
                _context.SaveChanges();

                emailTemplates.TemplateHtml = emailTemplates.TemplateHtml.Replace("%%user%%", mUser.FirstName);
                emailTemplates.TemplateHtml = emailTemplates.TemplateHtml.Replace("%%UserType%%", "Doctor");
                var verifyLink = _getdoctorUrl + "/auth/reset-password/" + cUser.UserGuid;
                emailTemplates.TemplateHtml = emailTemplates.TemplateHtml.Replace("%%VERIFYLINK%%", verifyLink);


                if (vResult.Succeeded == true)
                {
                    EmailCommands oEmailCommand = new EmailCommands();

                    oEmailCommand.To = mUser.Email;
                    oEmailCommand.Subject = "Welcome Mail";
                    oEmailCommand.IsHtml = true;
                    oEmailCommand.Body = emailTemplates.TemplateHtml;

                    await _emailService.SendEmail(oEmailCommand);
                    //save user profile
                    return new SuccessResponse(new List<string>(), "User Registered Successfully", HttpStatusCode.OK);

                }

                else
                {
                    return new ErrorResponse(vResult.Errors, "User Registration Failed", HttpStatusCode.BadRequest);
                }
            }
            else
            {
                return new ErrorResponse(new List<string>(), "User Already Exists", HttpStatusCode.BadRequest);
            }
        }
        #endregion

        #region Duplicate Email Check
        [AllowAnonymous]
        [HttpPost("DuplicateEmailCheck")]
        public async Task<JsonResult> DuplicateEmailCheck([FromBody] EmailUserBody mUser)
        {
            Boolean valid = false;
            var exUsers = await _userManager.Users.Where(e => (e.UserName.Trim() == mUser.Email.Trim() && !e.IsDeleted)).ToListAsync();
            exUsers = exUsers == null ? new List<UserEntity>() : exUsers;
            if (exUsers.Count > 0)
            {
                valid = false;
                return new ErrorResponse(valid, "User already exists!", HttpStatusCode.OK);
            }
            else
            {
                valid = true;
                return new ErrorResponse(valid, "Valid email", HttpStatusCode.OK);
            }
        }
        #endregion

        #region Duplicate Mobile Check
        [AllowAnonymous]
        [HttpPost("DuplicateMobilecheck")]
        public async Task<JsonResult> DuplicateMobilecheck([FromBody] MobileNumberUserBody mUser)
        {
            Boolean valid = false;
            var exUsers = await _userManager.Users.Where(e => (e.PhoneNumber == mUser.Mobile.Trim() && !e.IsDeleted && e.UserTypeId == mUser.UserTypeId)).ToListAsync();
            exUsers = exUsers == null ? new List<UserEntity>() : exUsers;
            if (exUsers.Count > 0)
            {

                valid = false;
                return new ErrorResponse(valid, "Mobile number  already exists!", HttpStatusCode.OK);
            }
            else
            {
                valid = true;
                return new ErrorResponse(valid, "Valid mobile number", HttpStatusCode.OK);
            }
        }
        #endregion

        #region Branch Signup
        [AllowAnonymous]
        [HttpPost("BranchSignup")]
        public async Task<JsonResult> BranchSignup([FromBody] BranchUserBody mUser)
        {
            EmailTemplatesQuery emailTemplatesQuery = new EmailTemplatesQuery();
            emailTemplatesQuery.Name = "BranchCreation";
            var emailTemplates = await _emailTemplateService.GetEmailTemplates(emailTemplatesQuery);
            emailTemplates.TemplateHtml = emailTemplates.TemplateHtml.Replace("%user%", mUser.FirstName);
            emailTemplates.TemplateHtml = emailTemplates.TemplateHtml.Replace("%%BranchName%%", mUser.BranchName);
            emailTemplates.TemplateHtml = emailTemplates.TemplateHtml.Replace("%%ContactPersonName%%", mUser.FirstName);
            emailTemplates.TemplateHtml = emailTemplates.TemplateHtml.Replace("%%ContactPersonEmail%%", mUser.Email);
            emailTemplates.TemplateHtml = emailTemplates.TemplateHtml.Replace("%%ContactPersonMobileNumber%%", mUser.PhoneNumber);

            EmailCommands oEmailCommand = new EmailCommands();

            oEmailCommand.To = mUser.Email;
            oEmailCommand.Subject = "Welcome Mail";
            oEmailCommand.IsHtml = true;
            oEmailCommand.Body = emailTemplates.TemplateHtml;

            await _emailService.SendEmail(oEmailCommand);
            return new SuccessResponse(new List<string>(), "Mail Sent Successfully", HttpStatusCode.OK);
        }
        #endregion
        #region WhatsappLink
        [AllowAnonymous]
        [HttpPost("WhatsappLink")]
        public async Task<IActionResult> WhatsappLink([FromBody] WhatsappUserBody mUser)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, _whatsappApiUrl);
            request.Headers.Add("Authorization", _whatsappApiKey);
            string shortUrl = await ShortenUrl(longUrl: mUser.MeetingLink, _tinyurlApiKey);

            var templateParameters = new
            {
                name = mUser.PatientName,
                datetime = mUser.AppointmentDate.ToString("dd MMM yyyy hh:mm tt", System.Globalization.CultureInfo.InvariantCulture),
                doctor_name = mUser.DoctorName,
                link = shortUrl
            };
            var content = new StringContent(
                Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    template_id = mUser.template_link,
                    template_parameters = templateParameters,
                    phone_number = mUser.PhoneNumber
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
        #region WhatsappAppointmentMeetingLink
        [AllowAnonymous]
        [HttpPost("SMSLink")]
        public async Task<IActionResult> SMSLink([FromBody] WhatsappUserBody mUser)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, _smsApiUrl);
            request.Headers.Add("Authorization", _smsApiKey);
            string shortUrl = await ShortenUrl(longUrl: mUser.MeetingLink, _tinyurlApiKey);

            var templateParameters = new
            {
                name = mUser.PatientName,
                datetime = mUser.AppointmentDate.ToString("dd MMM yyyy hh:mm tt", System.Globalization.CultureInfo.InvariantCulture),
                link = shortUrl
            };
            var content = new StringContent(
                Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    template_id = mUser.template_link,
                    template_parameters = templateParameters,
                    phone_number = mUser.PhoneNumber
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
        #endregion 
        #region ShortenUrl
        [AllowAnonymous]
        [HttpPost("ShortenUrl")]
        public async Task<string> ShortenUrl(string longUrl, string apiKey)
        {
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, _tinyapiUrl);
            request.Headers.Add("Authorization", $"Bearer {apiKey}");

            var content = new StringContent(
               Newtonsoft.Json.JsonConvert.SerializeObject(new
               {
                   url = longUrl
               }),
               Encoding.UTF8,
               "application/json");
            request.Content = content;

            try
            {
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                string responseContent = await response.Content.ReadAsStringAsync();

                var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<ShortenUrlResponse>(responseContent);

                if (responseObject != null)
                {
                    return responseObject.data.tiny_url;
                }
                else
                {
                    return "Failed to retrieve shortened URL";
                }
            }
            catch (Exception ex)
            {
                return "An error occurred: " + ex.Message;
            }

        }
        #endregion
    }
    #endregion
}