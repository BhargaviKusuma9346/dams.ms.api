using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Dams.ms.auth.Data;
using Dams.ms.auth.Models;
using Dams.ms.auth.Services;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.Net;

namespace Dams.ms.auth
{
    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        #region Private Variables
        //repository to get user from db
        private readonly IDMDbContext _dbContext;
        private readonly UserManager<UserEntity> _userManager;
        private readonly UserProfileData _userProfileData;
        #endregion

        #region Constructor
        public ResourceOwnerPasswordValidator(UserManager<UserEntity> userManager, IConfigurationRoot Configuration)
        {
            _userManager = userManager;
            _dbContext = new IDMDbContext(Configuration);
            _userProfileData = new UserProfileData(Configuration);
        }
        #endregion

        #region Validate Async
        //this is used to validate your user account with provided grant at /connect/token
        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {

            var UserTypeId = context.Request.Raw.Get("user_type_id")?.ToLower()?.Trim();
            var auth_type = context.Request.Raw.Get("auth_type")?.ToLower()?.Trim();
            var verification_code = context.Request.Raw.Get("verification_code")?.ToLower()?.Trim();
            var phone_num = context.Request.Raw.Get("phone_num")?.ToLower()?.Trim();
            var country_code = context.Request.Raw.Get("country_code")?.ToLower()?.Trim();
            var social_token = context.Request.Raw.Get("social_token")?.Trim();
            UserProfileData tokenMethod = new UserProfileData();
            try
            {
                var users = await _userManager.Users.Where(e => e.UserName.Trim() == context.UserName.Trim() && e.IsDeleted == false && e.UserTypeId== Convert.ToInt32(UserTypeId)).ToListAsync();
                users = (users == null) ? new List<UserEntity>() : users;

                if (users.Count() == 1)
                {
                    if ((auth_type != "custom" && !string.IsNullOrEmpty(auth_type)) || await _userManager.CheckPasswordAsync(users.FirstOrDefault(), context.Password.Trim()))
                    {
                        if (auth_type == "google")
                        {
                            if ((tokenMethod.WebRequest(string.Format("https://www.googleapis.com/oauth2/v3/tokeninfo?id_token={0}", social_token), null, null) != null))
                            {
                                context.Result = new GrantValidationResult(
                                         subject: users.FirstOrDefault().Id.ToString(),
                                         authenticationMethod: auth_type ?? "custom");
                                return;
                            }
                            else
                            {
                                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Google token is invalid");
                                return;
                            }
                        }
                    }
                    if (string.Equals(auth_type, "password", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Password = context.Password == null ? "" : context.Password;
                        bool isSuccess = await _userManager.CheckPasswordAsync(users.FirstOrDefault(), context.Password.Trim());
                        if (isSuccess)
                        {
                            context.Result = new GrantValidationResult(
                                subject: users.FirstOrDefault().Id.ToString(),
                                authenticationMethod: auth_type ?? "custom");
                            return;
                        }
                        else
                        {
                            context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Incorrect password");
                            return;
                        }
                    }
                    else
                    {
                        context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Invalid auth type.");
                        return;
                    }
                }
                else if (users.Count == 0)
                {
                    if (auth_type == "google")
                    {
                        var profileData = tokenMethod.WebRequest(string.Format("https://www.googleapis.com/oauth2/v3/tokeninfo?id_token={0}", social_token), null, null);
                        var cUser = new UserEntity() { Email = context.UserName, UserTypeId = 1,EmailConfirmed = true, LoginTypeId = 1, UserName = context.UserName, PhoneNumber = "", CountryCode = "", UserGuid = Guid.NewGuid(), FirstName = "", LastName = "", CreatedDt = DateTime.Now, UpdatedDt = DateTime.Now };
                        var vResult = await _userManager.CreateAsync(cUser, context.Password);
                        if (vResult.Succeeded == true)
                        {                           
                            context.Result = new GrantValidationResult(
                                subject: cUser.Id.ToString(),
                                authenticationMethod: auth_type ?? "google");
                            return;
                        }
                        else
                        {
                            context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "User Registered Failed");
                            return;
                        }
                    }
                    if (auth_type == "otplogin")
                    {
                        var phoneUsers = await _userManager.Users.Where(e => (e.PhoneNumber == phone_num && e.CountryCode == country_code && e.IsDeleted==false && e.UserTypeId==5)).ToListAsync();
                        phoneUsers = (phoneUsers == null) ? new List<UserEntity>() : phoneUsers;
                        if (phoneUsers.FirstOrDefault().Otp == verification_code)
                        {
                            context.Result = new GrantValidationResult(
                               subject: phoneUsers.FirstOrDefault().Id.ToString(),
                               authenticationMethod: auth_type ?? "custom");
                            return;
                        }
                        else
                        {
                            context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Please enter valid OTP");
                            return;
                        }
                    }
                    else
                    {
                        context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Invalid auth type.");
                        return;
                    }
                }
                else if (users.Count() > 1)
                {
                    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Multiple users found.");
                    return;
                }
                else
                {
                    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "User does not exists.");
                    return;
                }

            }
            catch (Exception ex)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Invalid credentials");
                throw ex;
            }
        }
        #endregion

        #region Get Users
        public IList GetUsers(int userTypeId)
        {
            if (userTypeId == 1)
            {
                return new List<UserEntity>();
            }
            else if (userTypeId == 2)
            {
                return new List<UserEntity>();
            }
            else if (userTypeId == 3)
            {
                return new List<UserEntity>();
            }
            else
            {
                return new List<UserEntity>();
            }
        }

        #endregion
    }
}
