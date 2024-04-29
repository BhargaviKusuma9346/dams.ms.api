using Dams.ms.auth.Reflections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dams.ms.auth.Models
{
    public class Role : BaseEntity
    {
        public int RoleId { get; set; }
        public int _Id { get; set; }
        public Guid AccountId { get; set; }
        public string Description { get; set; }
        public string RoleName { get; set; }
        public string Roles { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string Visibility { get; set; }
        public string Category { get; set; }
    }
    public class Permission : BaseEntity
    {
        public string Module { get; set; }
        public string Name { get; set; }
        public string Namecd { get; set; }
        public int Level { get; set; }
        public int Sort { get; set; }
        public Guid _Id { get; set; }
        public string Env { get; set; }
        public string Group { get; set; }
        public Boolean Selected { get; set; } = false;
        public int PermissionId { get; set; }
    }
    public class RolePermission : BaseEntity
    {
        public int RolePermissionId { get; set; }
        public string Type { get; set; }
        public int RoleId { get; set; }
        public Guid _Id { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
        public string Visibility { get; set; }
        public string Env { get; set; }
        public string Status { get; set; }
        public string Permissions { get; set; }
        public string Cretedtm { get; set; }
        public string Updatedtm { get; set; }
        public string AccountId { get; set; }
        public string Default { get; set; }
        public string Module { get; set; }
        public string Updatedtm1 { get; set; }
    }
    public class RoleBody : BaseEntity
    {
        public Nullable<Guid> AccountId { get; set; }
        public int RoleId { get; set; }
    }
    public class PermissionBody : BaseEntity
    {
        public string AppIds { get; set; } = "";
    }
    public class RolePermissionBody : BaseEntity
    {
        public Nullable<Guid> AccountId { get; set; }
    }
}
