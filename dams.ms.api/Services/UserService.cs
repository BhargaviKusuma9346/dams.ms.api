using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Dams.ms.auth.Models;
using Dams.ms.auth.Reflections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dams.ms.auth.Services
{
    public class UserService
    {
        #region Private Variables
        private readonly string ConnectionString;
        private readonly IConfigurationRoot ObjConfiguration;
        Database<User, User> database;
        #endregion

        #region Constructor
        public UserService(IConfigurationRoot Configuration)
        {
            ObjConfiguration = Configuration;
            ConnectionString = ObjConfiguration.GetConnectionString("PostGreyConnection");
            database = new Database<User, User>(ObjConfiguration);

        }
        #endregion

        #region Get Users Except Id
        public List<ChatUser> GetUsersExceptId(ChatUserBody body)
        {
            Database<ChatUser, ChatUserBody> obj = new Database<ChatUser, ChatUserBody>(ObjConfiguration);
            List<ChatUser> result = obj.Execute("[dbo].[get_users_except_id]", body);
            return result ?? new List<ChatUser>();
        }
        #endregion
    }
}
