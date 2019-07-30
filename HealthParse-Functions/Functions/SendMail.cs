using System.Linq;
using HealthParse.Mail;
using HealthParse.Standard;
using HealthParseFunctions;
using MailKit.Net.Smtp;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace HealthParse
{
    public static class SendMail
    {
        [FunctionName("SendMail")]
        public static void Run(
            [QueueTrigger(queueName: Fn.Qs.OutgoingMail, Connection = Fn.ConnectionKeyName)]CloudQueueMessage message,
            ILogger log)
        {
            using (var client = new SmtpClient())
            {
                var storageConfig = Fn.StorageConfig.Load();
                var storageAccount = CloudStorageAccount.Parse(storageConfig.ConnectionString);
                var blobClient = storageAccount.CreateCloudBlobClient();
                var outgoingContainer = blobClient.GetContainerReference(storageConfig.OutgoingMailContainerName);
                var emailConfig = Fn.EmailConfig.Load();
                var telemetry = new TelemetryClient(new TelemetryConfiguration(Fn.InstrumentationKey()));

                var email = EmailStorage.LoadEmailFromStorage(message.AsString, outgoingContainer);
                client.Connect(emailConfig.SmtpServer, emailConfig.SmtpPort);
                client.Authenticate(emailConfig.Username, emailConfig.Password);
                client.Send(email);
                client.Disconnect(true);
                telemetry.TrackEvent(
                    Events.SentMail,
                    Events.Properties.Init()
                        .Then(Events.Properties.EmailAddress, email.To.Mailboxes.First().Address));

                EmailStorage.DeleteEmailFromStorage(message.AsString, outgoingContainer);

                log.LogInformation($"sent mail to {email.To} - {email.Subject}");
            }
        }
    }
}