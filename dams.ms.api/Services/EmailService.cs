using Dams.ms.auth.Models;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Dams.ms.auth.Services
{
    public class EmailService
    {
        public async Task<EmailResponse> SendEmail(EmailCommands command)
        {
            var response = new EmailResponse();
            
            try
            {
            MailMessage message = new MailMessage();
            SmtpClient smtp = new SmtpClient();
            message.From = new MailAddress("outbound.quanbytech@gmail.com");
            message.To.Add(command.To);
            message.Subject = command.Subject;
            message.IsBodyHtml = command.IsHtml; 
            message.Body = command.Body;
            smtp.Port = 587;
            smtp.Host = "smtp.gmail.com"; 
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("outbound.quanbytech@gmail.com", "yvppzwardrtdelvy");
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.Send(message);
            response.Status = "Success";
            }
            catch (Exception)
            {
                response.Status = "Failure";
            }
            return response;
        }
    }
}

