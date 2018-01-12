using System.Configuration;
using System.Linq;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Queue;
using HealthParse.Standard;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using System.IO;

namespace HealthParseFunctions
{
    public static class Fn
    {
        public const string ConnectionKeyName = "QueueStorage";

        public static class Qs
        {
            public const string IncomingMail = "health-parse-incoming-mail";
            public const string OutgoingMail = "health-parse-outgoing-mail";
        }

        public class StorageConfig
        {
            public string IncomingMailContainerName { get; private set; }
            public string OutgoingMailContainerName { get; private set; }
            public string ConnectionString { get; private set; }

            public static StorageConfig Load()
            {
                return new StorageConfig
                {
                    IncomingMailContainerName = Environment.GetEnvironmentVariable("StorageBlob_IncomingMail"),
                    OutgoingMailContainerName = Environment.GetEnvironmentVariable("StorageBlob_OutgoingMail"),
                    ConnectionString = Environment.GetEnvironmentVariable("StorageBlob_Connection"),
                };
            }
        }

        public class EmailConfig
        {
            public string Username;
            public string Password;
            public string ImapServer;
            public int ImapPort;
            public string SmtpServer;
            public int SmtpPort;

            public static EmailConfig Load()
            {
                return new EmailConfig
                {
                    Username = Environment.GetEnvironmentVariable("EmailUsername"),
                    Password = Environment.GetEnvironmentVariable("EmailPassword"),
                    ImapServer = Environment.GetEnvironmentVariable("EmailImapServer"),
                    ImapPort = int.Parse(Environment.GetEnvironmentVariable("EmailImapPort")),
                    SmtpServer = Environment.GetEnvironmentVariable("EmailSmtpServer"),
                    SmtpPort = int.Parse(Environment.GetEnvironmentVariable("EmailSmtpPort")),
                };
            }
        }
    }

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

    public static class EmailStorage
    {
        public static string SaveEmailToStorage(MimeMessage email, CloudBlobContainer container)
        {
            var filename = Path.GetFileName(Path.GetTempFileName());
            var blockBlob = container.GetBlockBlobReference(filename);
            var messageBytes = email.ToBytes();
            blockBlob.UploadFromByteArrayAsync(messageBytes, 0, messageBytes.Length);

            return filename;
        }

        public static MimeMessage LoadEmailFromStorage(string filename, CloudBlobContainer container)
        {
            var blockBlob = container.GetBlockBlobReference(filename);
            using (var stream = new MemoryStream())
            {
                blockBlob.DownloadToStreamAsync(stream).Wait();

                stream.Position = 0;
                return MimeMessage.Load(stream);
            }
        }
    }

    public static class DataExtraction
    {
        [FunctionName("ExtractData")]
        public static void Run(
            [QueueTrigger(queueName: Fn.Qs.IncomingMail, Connection = Fn.ConnectionKeyName)]CloudQueueMessage message,
            [Queue(queueName: Fn.Qs.OutgoingMail, Connection = Fn.ConnectionKeyName)]ICollector<string> outputQueue,
            TraceWriter log)
        {
            var storageConfig = Fn.StorageConfig.Load();
            var storageAccount = CloudStorageAccount.Parse(storageConfig.ConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var incomingContainer = blobClient.GetContainerReference(storageConfig.IncomingMailContainerName);
            var outgoingContainer = blobClient.GetContainerReference(storageConfig.OutgoingMailContainerName);
            var originalEmail = EmailStorage.LoadEmailFromStorage(message.AsString, incomingContainer);

            // parse data
            // build excel
            var attachment = System.Text.Encoding.Default.GetBytes("hello world");
            var reply = ConstructMessage(originalEmail, attachment);

            var filename = EmailStorage.SaveEmailToStorage(reply, outgoingContainer);
            outputQueue.Add(filename);
            log.Info($"extracted data, enqueueing reply - {reply.To.ToString()} - {filename}");
        }

        private static MimeMessage ConstructMessage(MimeMessage message, byte[] attachment)
        {
            var reply = new MimeMessage();

            reply.From.Add(new MailboxAddress("applehealthreport@gmail.com"));

            // reply to the sender of the message
            if (message.ReplyTo.Count > 0)
            {
                reply.To.AddRange(message.ReplyTo);
            }
            else if (message.From.Count > 0)
            {
                reply.To.AddRange(message.From);
            }
            else if (message.Sender != null)
            {
                reply.To.Add(message.Sender);
            }

            if (!message.Subject.StartsWith("Re:", StringComparison.OrdinalIgnoreCase))
                reply.Subject = "Re: " + message.Subject;
            else
                reply.Subject = message.Subject;

            // construct the In-Reply-To and References headers
            if (!string.IsNullOrEmpty(message.MessageId))
            {
                reply.InReplyTo = message.MessageId;
                foreach (var id in message.References)
                    reply.References.Add(id);
                reply.References.Add(message.MessageId);
            }

            var builder = new BodyBuilder();

            builder.TextBody = @"Hey there, I saw your health data... good work!";
            builder.Attachments.Add($"export.{message.Date.Date.ToString("yyyy-mm-dd")}.xlsx", attachment);
            reply.Body = builder.ToMessageBody();

            return reply;
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
                var originalEmail = EmailStorage.LoadEmailFromStorage(message.AsString, outgoingContainer);
                var emailConfig = Fn.EmailConfig.Load();

                var email = EmailStorage.LoadEmailFromStorage(message.AsString, outgoingContainer);
                client.Connect(emailConfig.SmtpServer, emailConfig.SmtpPort);
                client.Authenticate(emailConfig.Username, emailConfig.Password);
                client.Send(email);
                client.Disconnect(true);
                log.Info($"sent mail to {email.To.ToString()} - {email.Subject}");
            }
        }
    }
}
