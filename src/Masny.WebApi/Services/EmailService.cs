using MailKit.Net.Smtp;
using MailKit.Security;
using Masny.WebApi.Helpers;
using Masny.WebApi.Interfaces;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using System;

namespace Masny.WebApi.Services
{
    public class EmailService : IEmailService
    {
        private readonly AppSettings _appSettings;

        public EmailService(IOptions<AppSettings> appSettings)
        {
            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            _appSettings = appSettings.Value;
        }

        // TODO: change it to async Task
        public void Send(string to, string subject, string html, string from = null)
        {
            var email = new MimeMessage();
            
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html) { Text = html };
            email.Sender = MailboxAddress.Parse(from ?? _appSettings.EmailFrom);

            using var smtp = new SmtpClient();
            smtp.Connect(_appSettings.SmtpHost, _appSettings.SmtpPort, SecureSocketOptions.StartTls);
            smtp.Authenticate(_appSettings.SmtpUser, _appSettings.SmtpPass);
            smtp.Send(email);
            smtp.Disconnect(true);
        }
    }
}
