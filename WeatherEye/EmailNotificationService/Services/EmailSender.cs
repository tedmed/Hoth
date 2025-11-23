using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace EmailNotificationService.Services
{
    public interface IEmailSender
    {
        void SendEmail(string recipientEmail, string subject, string body, bool isBodyHtml = false);
    }
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;
        private readonly IConfiguration _configuration;
        private readonly string SmtpServer;
        private readonly int SmtpPort;
        private readonly bool UseSsl;

        private readonly string SenderEmail;
        private readonly string SenderPassword;

        private readonly string SenderUserName;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _logger = logger;
            _configuration = configuration;

            SmtpServer = _configuration["Smtp:Host"]
                ?? throw new ArgumentNullException("Smtp server is not configured");

            SmtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");

            SenderEmail = _configuration["Smtp:FromAddress"]
                ?? throw new ArgumentNullException("Sender email is not configured");

            SenderUserName = _configuration["Smtp:Username"]
                ?? throw new ArgumentNullException("Sender username is not configured");

            SenderPassword = _configuration["Smtp:Password"]
                ?? throw new ArgumentNullException("Sender password is not configured");

            UseSsl = bool.Parse(_configuration["Smtp:UseSsl"] ?? "true");
        }

        public void SendEmail(string recipientEmail, string subject, string body, bool isBodyHtml = false)
        {

            try
            {
                using (MailMessage mail = new MailMessage(SenderEmail, recipientEmail, subject, body))
                {
                    mail.IsBodyHtml = isBodyHtml;

                    using (SmtpClient smtp = new SmtpClient(SmtpServer, SmtpPort))
                    {
                        smtp.EnableSsl = UseSsl;
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtp.Credentials = new NetworkCredential(SenderUserName, SenderPassword);

                        smtp.Send(mail);
                        _logger.LogInformation("Email sent to {recipientEmail} with subject {subject}", recipientEmail, subject);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {recipientEmail} with subject {subject}", recipientEmail, subject);

                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, "Inner exception details:");
                }
            }
        }
    }
}
