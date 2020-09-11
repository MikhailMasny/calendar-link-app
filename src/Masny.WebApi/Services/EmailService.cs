using MailKit.Net.Smtp;
using MailKit.Security;
using Masny.WebApi.Helpers;
using Masny.WebApi.Interfaces;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using System;
using System.Threading.Tasks;

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

        public async Task SendAsync(string to, string subject, string html, string from = null)
        {
            var email = new MimeMessage();

            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html) { Text = html };
            email.Sender = MailboxAddress.Parse(from ?? _appSettings.EmailFrom);

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_appSettings.SmtpHost, _appSettings.SmtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_appSettings.SmtpUser, _appSettings.SmtpPass);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
