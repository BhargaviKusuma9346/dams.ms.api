using Dams.ms.auth.Models;
using Dams.ms.auth.Reflections;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dams.ms.auth.Services
{
    public class RolesService
    {
        #region Private Variables
        private readonly IConfigurationRoot _objConfiguration;
        #endregion

        #region Constructor
        public RolesService(IConfigurationRoot objConfiguration)
        {
            _objConfiguration = objConfiguration;
        }
        #endregion

        #region Get permissions
        public List<Permission> GetPermissions(PermissionBody body)
        {
            Database<Permission, PermissionBody> obj = new Database<Permission, PermissionBody>(_objConfiguration);
            List<Permission> result = obj.Execute("[portal].[get_permissions_by_apps]", body);
            return result ?? new List<Permission>();
        }
        #endregion

        #region Get Roles permissions
        public List<RolePermission> GetRolesPermissions(RolePermissionBody body)
        {
            Database<RolePermission, RolePermissionBody> obj = new Database<RolePermission, RolePermissionBody>(_objConfiguration);
            List<RolePermission> result = obj.Execute("[account].[get_role_permissions]", body);
            return result ?? new List<RolePermission>();
        }
        #endregion

        #region Get Roles  
        public List<Role> GetRoles(RoleBody body)
        {
            Database<Role, RoleBody> obj = new Database<Role, RoleBody>(_objConfiguration);
            List<Role> result = obj.Execute("[account].[get_roles]", body);
            return result ?? new List<Role>();
        }
        #endregion

        #region Create/Update Role
        public int SaveRole(RolePermission body)
        {
            Database<RolePermission, RolePermission> obj = new Database<RolePermission, RolePermission>(_objConfiguration);
            List<RolePermission> result = obj.Execute("[account].[save_role_permissons]", body);
            return result?.Count > 0 ? 1 : 0;
        }
        #endregion

        #region Update User Role
        public int UpdateUserRole(AppUser body)
        {
            Database<RolePermission, AppUser> obj = new Database<RolePermission, AppUser>(_objConfiguration);
            List<RolePermission> result = obj.Execute("[dbo].[update_user_role]", body);
            return result?.Count > 0 ? 1 : 0;
        }
        #endregion

        #region Get Role By Id
        public RolePermission GetRoleById(RoleBody body)
        {
            Database<RolePermission, RoleBody> obj = new Database<RolePermission, RoleBody>(_objConfiguration);
            List<RolePermission> result = obj.Execute("[account].[get_role_by_id]", body);
            return result.FirstOrDefault();
        }
        #endregion
    }
}
