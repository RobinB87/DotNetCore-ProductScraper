using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Mail;

namespace MailAutomation
{
    /// <summary>
    /// Class with methods for email sending
    /// </summary>
    public class MailService : IMailService
    {
        private readonly ILogger<MailService> _logger;

        public MailService(ILogger<MailService> logger)
        {
            _logger = logger;
        }

        public string CreateEmailBody(List<string> discounts)
        {
            var body = string.Empty;
            foreach (var discount in discounts)
            {
                body += discount + Environment.NewLine;
            }

            return body;
        }

        public void SendEmail(string body, string subject)
        {
            var emailFrom = ConfigurationManager.AppSettings["EmailAddress"];
            var smtp = new SmtpClient
            {
                Host = "smtp.live.com",
                Port = 587,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential(emailFrom, ConfigurationManager.AppSettings["EmailPassword"]),
                EnableSsl = true
            };

            Execute(smtp, emailFrom, ConfigurationManager.AppSettings["EmailTo"], subject, body);
        }

        /// <summary>
        /// Send an email.
        /// </summary>
        /// <param name="smtpClient">The smtp client</param>
        /// <param name="emailFrom">The email sender</param>
        /// <param name="emailTo">The email recipients</param>
        /// <param name="emailSubject">The subject</param>
        /// <param name="htmlBody">The body</param>
        private void Execute(SmtpClient smtpClient, string emailFrom, string emailTo, string emailSubject, string htmlBody)
        {
            var from = new MailAddress(emailFrom);
            var message = new MailMessage
            {
                Subject = emailSubject,
                Body = htmlBody,
                IsBodyHtml = true,
                From = @from
            };

            if (emailTo.Contains(";"))
            {
                foreach (var address in emailTo.Split(';'))
                {
                    message.To.Add(address);
                }
            }
            else
            {
                message.To.Add(emailTo);
            }

            try
            {
                smtpClient.Send(message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Something went wrong while sending email.", ex);
                throw;
            }

            // Clean up.
            message.Dispose();
        }
    }
}