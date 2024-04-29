using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Dams.ms.auth.Data
{
    public partial class RolesEntity
    {
        [Column("role_id")]
        public int RoleId { get; set; }

        [Column("account_id")]
        public Guid AccountId { get; set; }
        [Column("description")]
        public string Description { get; set; }
        [Column("rolename")]
        public string RoleName { get; set; }
        [Column("roles")]
        public string Roles { get; set; }
        [Column("status")]
        public string Status { get; set; }
        [Column("type")]
        public string Type { get; set; }
        [Column("visibility")]
        public string Visibilty { get; set; }
        [Column("category")]
        public string Category { get; set; }

    }
}
