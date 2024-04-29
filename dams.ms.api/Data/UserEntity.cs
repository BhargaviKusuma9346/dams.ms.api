using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Dams.ms.auth.Data
{
    public partial class UserEntity : IdentityUser<int>
    {
        public Guid UserGuid { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Pincode { get; set; }
        public int UserTypeId { get; set; } = 1;
        public int LoginTypeId { get; set; } = 1;
        public string CountryCode { get; set; }
        public string ForgotPasswordOtp { get; set; } = null;
        public bool IsDeleted { get; set; } = false;
        public DateTime? CreatedDt { get; set; } = DateTime.Now;
        public DateTime? UpdatedDt { get; set; }
        public string Otp { get; set; }
    }
}
