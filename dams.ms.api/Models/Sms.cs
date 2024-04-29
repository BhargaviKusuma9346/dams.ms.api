using System;
using System.Collections.Generic;
using System.Text;

namespace Dams.ms.auth.Models
{
    public class Sms
    {
        public int Id { get; set; }
    }
    public class SmsBody
    {
        public string PhoneNumber { get; set; }
        public string CountryCode { get; set; } = "+91";  
        public Guid UserGuid { get; set; }
      
     
    }
    public class SmsResponseBody
    {
        public string Otp { get; set; }
        public Guid UserGuid { get; set; }
    }
    public class LoginOtpCommand
    {
        public string Otp { get; set; }
        public string PhoneNumber { get; set; }
        public string CountryCode { get; set; } = "+91";
    }

    #region Text Local Response Object
    public class Message
    {
        public int num_parts { get; set; }
        public string sender { get; set; }
        public string content { get; set; }
    }

    public class Messages
    {
        public string id { get; set; }
        public long recipient { get; set; }
    }

    public class TextLocalResponse
    {
        public int balance { get; set; }
        public int batch_id { get; set; }
        public int cost { get; set; }
        public int num_messages { get; set; }
        public Message message { get; set; }
        public string receipt_url { get; set; }
        public string custom { get; set; }
        public List<Messages> messages { get; set; }
        public string status { get; set; }
    }
    #endregion
}
