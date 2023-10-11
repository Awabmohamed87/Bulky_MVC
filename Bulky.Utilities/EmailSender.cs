
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Bulky.Utility
{
    public class EmailSender : IEmailSender
    {
        public string SendGridSecretkey {  get; set; }  
        public EmailSender(IConfiguration config)
        {
            SendGridSecretkey = config.GetValue<string>("SendGrid:SecretKey");
        }
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SendGridClient(SendGridSecretkey);
            var  from = new EmailAddress("sender@gmail.com", "Bulky Web");
            var  to = new EmailAddress(email);
            var message = MailHelper.CreateSingleEmail(from, to, subject, " ", htmlMessage);
            return client.SendEmailAsync(message);
        }
    }
}
