using Dams.ms.auth.Data;
using Dams.ms.auth.Reflections;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Dams.ms.auth.Models
{
    public class User : BaseEntity
    {
        [Column("account_id")]
        public Guid AccountId { get; set; }
        [Column("_id")]
        public Guid _Id { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public string Password { get; set; }
        public string NewPassword { get; set; } = "";
        public string UserType { get; set; } = "User";
    }

    public class AppUser : BaseEntity
    {
        public int UserId { get; set; }
        public Guid UserGuid { get; set; }
        public string AccountName { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string FullName { get; set; } = "";
        public bool IsAdmin { get; set; } = false;
        public string LastName { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public Nullable<Guid> AccountId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
    }

    public class UserBody
    {
        public int UserId { get; set; }
        public string Email { get; set; } = "";
        public string UserName { get; set; } = "";
    }

    public class Apps : BaseEntity
    {
        public int AppId { get; set; }
        public string AppName { get; set; } = "";
        public bool IsSelected { get; set; } = false;
    }

    public class UpdatePasswordBody
    {
        public Nullable<Guid> UserId { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public class AccountUserBody
    {
        public Nullable<Guid> AccountId { get; set; } = null;
    }

    public class RegisterUserBody : BaseEntity
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string ConfirmPassword { get; set; } = "";
        public string CountryCode { get; set; } = "+91";
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime UpdateDate { get; set; } = DateTime.Now;
        public string UserType { get; set; } 
    }

    public class RegisterMobileBody : BaseEntity
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Email { get; set; } = "";
        public string CountryCode { get; set; } = "+91";
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime UpdateDate { get; set; } = DateTime.Now;
        public string UserType { get; set; }
    }

    public class ChatUserBody
    {
        public Nullable<Guid> UserGuid { get; set; } = null;
        public int UserTypeId { get; set; } = 0;
    }

    public class ChatUser : BaseEntity
    {
        public Nullable<Guid> id { get; set; } = null;
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string userName { get; set; }
        public bool isOnline { get; set; }
        public bool isActive { get; set; }
    }
    public class EmailUserBody : BaseEntity
    {
        public string Email { get; set; } = "";
    }

    public class MobileNumberUserBody : BaseEntity
    {
        public string Mobile { get; set; } = "";
        public int UserTypeId { get; set; }
    }
    public class UsersBody
    {
        public Nullable<Guid> UserGuid { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Email { get; set; } = "";
        public string CountryCode { get; set; } = "+91";
    }
    public class BranchUserBody : BaseEntity
    {
        public string Email { get; set; } = "";
        public string BranchName { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
    }
    public class WhatsappUserBody : BaseEntity
    {
        public string template_link { get; set; } = "";
        public string PatientName { get; set; } = "";
        public string DoctorName { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string MeetingLink { get; set; } = "";
        public DateTime AppointmentDate { get; set; } 
    }
    public class ShortenUrlResponse : BaseEntity
    {
        public Data data { get; set; }

        public class Data
        {
            public string tiny_url { get; set; }
        }
    }

}
