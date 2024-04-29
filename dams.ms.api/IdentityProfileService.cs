

using IdentityServer4.Services;
using System;
using System.Threading.Tasks;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Identity;
using IdentityServer4.Extensions;
using System.Linq;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Data;
using Dams.ms.auth.Data;
using System.Data.SqlClient;
using Dams.ms.auth.Utility;

namespace Dams.ms.auth
{
    public class IdentityProfileService : IProfileService
    {

        #region Private Variables
        private readonly IUserClaimsPrincipalFactory<UserEntity> _claimsFactory;
        private readonly UserManager<UserEntity> _userManager;
        public readonly IConfigurationRoot _objConfiguration;
        public readonly string SQLConnectionString;
        #endregion

        #region Constructor
        public IdentityProfileService(IUserClaimsPrincipalFactory<UserEntity> claimsFactory, UserManager<UserEntity> userManager, IConfigurationRoot Configuration)
        {
            _claimsFactory = claimsFactory;
            _userManager = userManager;
            _objConfiguration = Configuration;

            SQLConnectionString = _objConfiguration.GetConnectionString("DefaultConnection");
        }
        #endregion

        #region Get Profile Data Async
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            if (user == null)
            {
                throw new ArgumentException("");
            }

            context.IssuedClaims.Add(new Claim("Email", user.Email.ToString()));
            context.IssuedClaims.Add(new Claim("FirstName", user.FirstName ?? ""));
            context.IssuedClaims.Add(new Claim("LastName", user.LastName ?? ""));
            context.IssuedClaims.Add(new Claim("EmailConfirmed", user.EmailConfirmed.ToString()));
            context.IssuedClaims.Add(new Claim("UserGuid", user.UserGuid.ToString()));
            context.IssuedClaims.Add(new Claim("UserId", user.Id.ToString()));
            if (user.UpdatedDt == null) { context.IssuedClaims.Add(new Claim("UpdatedDt",user.CreatedDt.ToString())); } else
            {
                context.IssuedClaims.Add(new Claim("UpdatedDt", user.UpdatedDt.ToString()));
            }
            context.IssuedClaims.AddRange(GetUserClaims(user.Id));
        }
        #endregion 

        #region Is Active Async
        public async Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            context.IsActive = user != null;
        }
        #endregion

        #region Get User Claims
        public List<Claim> GetUserClaims(int userId)
        {
            //string SQLConnectionString = ConnectionString.CName;
            SqlConnection con = new SqlConnection(SQLConnectionString);
            List<Claim> claims = new List<Claim>();
            try
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[spGetUserClaims]";
                    cmd.Parameters.AddWithValue("@userid", userId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Claim claim = new Claim(reader["claim_name"].ToString(), reader["claim_value"].ToString());
                                claims.Add(claim);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
            }
            finally
            {
                con.Close();
            }
            return claims;
        }
        #endregion

        #region Convert Into Claims
        //private IEnumerable<Claim> ConvertIntoClaims(UserClaims userClaims)
        //{
        //    try
        //    {
        //        List<Claim> claims = new List<Claim>();
        //        PropertyInfo[] properties = Reflector<UserClaims>.GetPropertyInfos();

        //        foreach (PropertyInfo property in properties)
        //        {
        //            string value = property.GetValue(userClaims, null)?.ToString() ?? ""; // Get value
        //            Claim claim = new Claim(property.Name, value);
        //            claims.Add(claim);
        //        }
        //        return claims;
        //    }
        //    catch { return new List<Claim>(); }
        //}
        #endregion
    }
}
