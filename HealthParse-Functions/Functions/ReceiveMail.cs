using System.Linq;
using HealthParse.Mail;
using HealthParseFunctions;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;

namespace HealthParse
{
    public static class ReceiveMail
    {
        [FunctionName("ReceiveMail")]
        public static void Run(
            [TimerTrigger("0 */1 * * * *")]TimerInfo timer,
            [Queue(queueName: Fn.Qs.IncomingMail, Connection = Fn.ConnectionKeyName)]ICollector<string> outputQueue,
            TraceWriter log)
        {
            using (var client = new ImapClient())
            {
                var emailConfig = Fn.EmailConfig.Load();
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                client.Connect(emailConfig.ImapServer, emailConfig.ImapPort, SecureSocketOptions.SslOnConnect);
                client.Authenticate(emailConfig.Username, emailConfig.Password);

                var inbox = client.Inbox;
                inbox.Open(FolderAccess.ReadWrite);

                var storageConfig = Fn.StorageConfig.Load();
                var storageAccount = CloudStorageAccount.Parse(storageConfig.ConnectionString);
                var blobClient = storageAccount.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference(storageConfig.IncomingMailContainerName);
                var telemetry = new TelemetryClient();

                inbox.Search(SearchQuery.NotSeen)
                    .Select(uid => new { uid, message = inbox.GetMessage(uid) })
                    .ToList()
                    .ForEach(x => {
                        var filename = EmailStorage.SaveEmailToStorage(x.message, container);

                        outputQueue.Add(filename);
                        inbox.AddFlags(x.uid, MessageFlags.Seen, false);

                        log.Info($"Queued email - {x.message.From.ToString()} - {x.message.Subject} - {filename}");
                        telemetry.TrackEvent(Events.ReceivedMail);
                    });

                client.Disconnect(true);
            }
        }
    }
}