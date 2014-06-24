using SendGridMail;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace RosterIt.Notifications
{
    public class MailClient
    {
        public async Task SendMessage(string title, string message)
        {
            // Setup the email properties.
            var from = new MailAddress("notifications@rosterit.appcloud.net");
            
            // Create an email, passing in the the eight properties as arguments.
            SendGrid myMessage = SendGrid.GetInstance();

            var toAddresses = ConfigurationManager.AppSettings["to"];
            if (string.IsNullOrWhiteSpace(toAddresses))
                throw new ConfigurationErrorsException("Required application setting 'to' is missing from the Application Config (Report email receiptants).");

            var addresses = toAddresses.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            myMessage.AddTo(addresses);
            myMessage.From = from;
            myMessage.Html = message;
            myMessage.Subject = title;

            var credentials = new NetworkCredential("azure_53e539a5d10f195e59c16210c19e3d46@azure.com","4gDwYF2hbrC78k0");

            var webClient = SendGridMail.Web.GetInstance(credentials);
            
            await webClient.DeliverAsync(myMessage);
        }
    }
}
