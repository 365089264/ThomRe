using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Net;
using log4net.Appender;

namespace VAV.Scheduler
{
    /// <summary>Sends an HTML email when logging event occurs</summary>
    public class HtmlSmtpAppender : SmtpAppender
    {
        /// <summary>Sends an email message</summary>
        protected override void SendEmail(string body)
        {
            CreateSmtpClient().Send(CreateMailMessage(body));
        }

        /// <summary>Creats new SMTP client based on the properties set in the configuration file</summary>
        private SmtpClient CreateSmtpClient()
        {
            var smtpClient = new SmtpClient
            {
                Host = SmtpHost,
                Port = Port,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            if (this.Authentication == SmtpAuthentication.Basic)
            {
                smtpClient.Credentials = new NetworkCredential(this.Username, this.Password);
            }
            else if (this.Authentication == SmtpAuthentication.Ntlm)
            {
                smtpClient.Credentials = CredentialCache.DefaultNetworkCredentials;
            }
            return smtpClient;
        }

        /// <summary>Creats new mail message based on the properties set in the configuration file</summary>
        private MailMessage CreateMailMessage(string body)
        {
            var mailMessage = new MailMessage { From = new MailAddress(From) };
            mailMessage.To.Add(To);
            mailMessage.Subject = Subject;
            mailMessage.Body = body;
            mailMessage.Priority = Priority;
            mailMessage.IsBodyHtml = true;
            return mailMessage;
        }
    }
}

