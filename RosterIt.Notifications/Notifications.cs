using MongoDB.Driver;
using NLog;
using RosterIt.Data;
using RosterIt.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosterIt.Notifications
{
    public static class Notifications
    {
        public static async Task NotifyOfYesterdaysSubmissions()
        {
            Logger log = LogManager.GetCurrentClassLogger();
            try
            {   
                string serverUri = ConfigurationManager.AppSettings["mongoUri"];
                if (string.IsNullOrWhiteSpace(serverUri))
                    throw new ConfigurationErrorsException("'mongoUri' application setting not found.");

                string databaseUri = ConfigurationManager.AppSettings["databaseName"];
                if (string.IsNullOrWhiteSpace(databaseUri))
                    throw new ConfigurationErrorsException("'databaseName' application setting not found.");

                var client = new MongoClient(serverUri);
                var server = client.GetServer();
                var database = server.GetDatabase(databaseUri);
                var collection = database.GetCollection<Roster>("rosters");

                IRosterContext context = new RosterContext(collection);

                var yesterday = DateTime.Now.AddDays(-1).Date;
                var rostersSubmittedYesterday = await context.GetByDate(yesterday);

                StringBuilder emailContent = new StringBuilder();

                emailContent.AppendFormat("<h3>Rosters submitted on the {0}</h3>", yesterday.ToString("ddd MMMM, yyyy"));

                if (rostersSubmittedYesterday.Any())
                {
                    emailContent.Append("<ul>");
                    foreach (var roster in rostersSubmittedYesterday)
                    {
                        emailContent.AppendFormat("<li>{0} - {1}</li>", roster.Site.Name, roster.ShiftTime.ToString());
                    }

                    emailContent.Append("</ul>");
                }
                else
                {
                    emailContent.Append("<h5>No rosters were submitted</h5>");
                }
                
                await (new MailClient().SendMessage(string.Format("Roster submission report ({0})", yesterday.ToString("yyyy-MM-dd")), emailContent.ToString()));
            }
            catch(Exception exception)
            {
                StringBuilder messageBuilder = new StringBuilder();

                while (exception != null)
	            {
                    messageBuilder.AppendLine(exception.GetType().FullName);
                    messageBuilder.AppendLine(exception.Message);
                    messageBuilder.AppendLine(exception.StackTrace);
                    messageBuilder.AppendLine();

                    exception = exception.InnerException;
	            }

                log.Error(messageBuilder.ToString());

            }
        }
    }
}
