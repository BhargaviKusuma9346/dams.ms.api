using Dams.ms.auth.Models;
using Dams.ms.auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dams.ms.auth.Controllers.v1
{
    [EnableCors("AllowAllHeaders")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0", Deprecated = false)]
    [ApiExplorerSettings(GroupName = "v1")]
    [Authorize]
    [ApiController]
    public class RolesController : Controller
    {
        #region Private Variables
        public readonly IConfigurationRoot _objConfiguration;
        private readonly RolesService _rolesService;
        #endregion

        #region Constructor 
        public RolesController(IConfigurationRoot Configuration)
        {
            _objConfiguration = Configuration;
            _rolesService = new RolesService(_objConfiguration);
        }
        #endregion

        #region Get Role By Id
        [AllowAnonymous]
        [HttpGet("GetRoleById")]
        public RolePermission GetRoleById(int roleId)
        {
            return _rolesService.GetRoleById(new RoleBody() { RoleId = roleId });
        }
        #endregion

        #region Get Roles permissions
        [AllowAnonymous]
        [HttpGet("GetRolesPermissions")]
        public List<RolePermission> GetRolesPermissions(Nullable<Guid> accountId)
        {
            return _rolesService.GetRolesPermissions(new RolePermissionBody() { AccountId = accountId });
        }
        #endregion

        #region Get Permissions
        [AllowAnonymous]
        [HttpGet("GetPermissions")]
        public List<Permission> GetPermissions(string appIds)
        {
            return _rolesService.GetPermissions(new PermissionBody() { AppIds = appIds });
        }
        #endregion
    }
}
