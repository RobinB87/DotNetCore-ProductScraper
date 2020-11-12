using System.Collections.Generic;

namespace MailAutomation
{
    public interface IMailService
    {
        string CreateEmailBody(List<string> discounts);
        void SendEmail(string body, string subject);
    }
}