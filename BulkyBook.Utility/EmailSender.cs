using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Utility
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("c7381d410179f4", "763f0f41c9f519"),
                EnableSsl = true
            };
            var mail = new MailMessage();
            mail.IsBodyHtml = true;
            mail.From = new MailAddress("BulkyBook@gmail.com", "BulkyBook");
            mail.To.Add(email);
            mail.Subject = subject;
            mail.Body = htmlMessage;

            return client.SendMailAsync(mail);
        }
    }
}
