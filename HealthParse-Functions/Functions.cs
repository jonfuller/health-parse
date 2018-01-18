using HealthParse.Mail;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using MimeKit;
using System.Linq;

namespace HealthParseFunctions
{
    public static class ReceiveMail
    {
        [FunctionName("ReceiveMail")]
        public static void Run(
            [TimerTrigger("0 */5 * * * *")]TimerInfo timer,
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

                inbox.Search(SearchQuery.NotSeen)
                    .Select(uid => new { uid, message = inbox.GetMessage(uid) })
                    .ToList()
                    .ForEach(x => {
                        var filename = EmailStorage.SaveEmailToStorage(x.message, container);

                        outputQueue.Add(filename);
                        inbox.AddFlags(x.uid, MessageFlags.Seen, false);

                        log.Info($"Queued email - {x.message.From.ToString()} - {x.message.Subject} - {filename}");
                    });

                client.Disconnect(true);
            }
        }
    }

    public static class ErrorNotification
    {
        [FunctionName("ErrorNotification")]
        public static void Run(
            [QueueTrigger(queueName: Fn.Qs.ErrorNotification, Connection = Fn.ConnectionKeyName)]CloudQueueMessage message,
            [Queue(queueName: Fn.Qs.OutgoingMail, Connection = Fn.ConnectionKeyName)]ICollector<string> outgoingMail,
            TraceWriter log)
        {
            var storageConfig = Fn.StorageConfig.Load();
            var storageAccount = CloudStorageAccount.Parse(storageConfig.ConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var outgoingContainer = blobClient.GetContainerReference(storageConfig.OutgoingMailContainerName);
            var errorContainer = blobClient.GetContainerReference(storageConfig.ErrorMailContainerName);

            var originalEmail = EmailStorage.LoadEmailFromStorage(message.AsString, errorContainer);
            log.Info($"Got error {originalEmail.Subject}... notifying... ");

            NotifyByEmail(originalEmail, outgoingContainer, outgoingMail, log);

            EmailStorage.DeleteEmailFromStorage(message.AsString, errorContainer);
        }

        private static void NotifyByEmail(MimeMessage originalMessage, CloudBlobContainer outgoingContainer, ICollector<string> outgoingMail, TraceWriter log)
        {
            var emailConfig = Fn.EmailConfig.Load();
            log.Info($"... Notifying admin of error via email ({emailConfig.AdminEmailAddress})");

            var forwarded = MailUtility.ForwardMessage(
                originalMessage,
                "Here's an error email.",
                emailConfig.AdminEmailAddress,
                emailConfig.FromEmailAddress);
            outgoingMail.Add(EmailStorage.SaveEmailToStorage(forwarded, outgoingContainer));
        }
    }

    public static class DataExtraction
    {
        [FunctionName("ExtractData")]
        public static void Run(
            [QueueTrigger(queueName: Fn.Qs.IncomingMail, Connection = Fn.ConnectionKeyName)]CloudQueueMessage message,
            [Queue(queueName: Fn.Qs.OutgoingMail, Connection = Fn.ConnectionKeyName)]ICollector<string> outputQueue,
            [Queue(queueName: Fn.Qs.ErrorNotification, Connection = Fn.ConnectionKeyName)]ICollector<string> errorQueue,
            TraceWriter log)
        {
            var storageConfig = Fn.StorageConfig.Load();
            var storageAccount = CloudStorageAccount.Parse(storageConfig.ConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var incomingContainer = blobClient.GetContainerReference(storageConfig.IncomingMailContainerName);
            var outgoingContainer = blobClient.GetContainerReference(storageConfig.OutgoingMailContainerName);
            var errorContainer = blobClient.GetContainerReference(storageConfig.ErrorMailContainerName);
            var originalEmail = EmailStorage.LoadEmailFromStorage(message.AsString, incomingContainer);

            var reply = MailUtility.ProcessEmail(originalEmail, Fn.EmailConfig.Load().FromEmailAddress);

            if (!reply.WasSuccessful)
            {
                var erroredFile = EmailStorage.SaveEmailToStorage(originalEmail, errorContainer);
                errorQueue.Add(erroredFile);
                log.Info($"enqueueing error - {erroredFile}");
            }
            EmailStorage.DeleteEmailFromStorage(message.AsString, incomingContainer);

            var filename = EmailStorage.SaveEmailToStorage(reply.Value, outgoingContainer);
            outputQueue.Add(filename);
            log.Info($"extracted data, enqueueing reply - {reply.Value.To} - {filename}");
        }

    }

    public static class SendMail
    {
        [FunctionName("SendMail")]
        public static void Run(
            [QueueTrigger(queueName: Fn.Qs.OutgoingMail, Connection = Fn.ConnectionKeyName)]CloudQueueMessage message,
            TraceWriter log)
        {
            using (var client = new SmtpClient())
            {
                var storageConfig = Fn.StorageConfig.Load();
                var storageAccount = CloudStorageAccount.Parse(storageConfig.ConnectionString);
                var blobClient = storageAccount.CreateCloudBlobClient();
                var outgoingContainer = blobClient.GetContainerReference(storageConfig.OutgoingMailContainerName);
                var emailConfig = Fn.EmailConfig.Load();

                var email = EmailStorage.LoadEmailFromStorage(message.AsString, outgoingContainer);
                client.Connect(emailConfig.SmtpServer, emailConfig.SmtpPort);
                client.Authenticate(emailConfig.Username, emailConfig.Password);
                client.Send(email);
                client.Disconnect(true);

                EmailStorage.DeleteEmailFromStorage(message.AsString, outgoingContainer);

                log.Info($"sent mail to {email.To} - {email.Subject}");
            }
        }
    }
}
