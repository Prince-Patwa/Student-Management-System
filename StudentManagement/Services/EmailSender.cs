using System.Net;
using System.Net.Mail;

namespace StudentManagement.Services
{
    public class EmailSender
    {
        private SmtpClient _smtpClient;
        private string username = "student@programmerkashyap.com";
        private string password = "Jmv8euXg$75A";
        private int port = 587; // 465

        public EmailSender()
        {
            _smtpClient = new SmtpClient("mail.programmerkashyap.com")
            {
                Port = port,
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true,
            };
        }

        public async Task SendEmail(string sendto, string subject, string mailbody)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(username),
                Subject = subject,
                Body = mailbody,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(sendto);

            await _smtpClient.SendMailAsync(mailMessage);
        }
    }
}
