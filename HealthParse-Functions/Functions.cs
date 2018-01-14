using HealthParse.Mail;
using HealthParse.Standard.Health;
using HealthParse.Standard.Mail;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using MimeKit;
using System;
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

            var reply = ProcessEmail(originalEmail);

            var filename = EmailStorage.SaveEmailToStorage(reply, outgoingContainer);
            outputQueue.Add(filename);
            log.Info($"extracted data, enqueueing reply - {reply.To.ToString()} - {filename}");
        }

        private static MimeMessage ProcessEmail(MimeMessage originalEmail)
        {
            var attachments = originalEmail.LoadAttachments();
            var exportAttachment = attachments.FirstOrDefault(a => a.Item1 == "export.zip");

            if (exportAttachment != null)
            {
                var attachment = ExcelReport.CreateReport(exportAttachment.Item2);
                var attachmentName = $"export.{originalEmail.Date.Date.ToString("yyyy-mm-dd")}.xlsx";
                return ConstructExcelReportMessage(originalEmail, attachment, attachmentName);
            }

            // TODO: what if no export.zip?
            // TODO: other scenarios

            return ConstructErrorMessage(originalEmail);
        }

        private static MimeMessage ConstructErrorMessage(MimeMessage originalEmail)
        {
            return ConstructReply(originalEmail, builder =>
            {
                builder.TextBody = @"Something went wrong... sorry about that!";
            });
        }
        private static MimeMessage ConstructExcelReportMessage(MimeMessage message, byte[] attachment, string attachmentName)
        {
            return ConstructReply(message, builder =>
            {
                builder.TextBody = @"Hey there, I saw your health data... good work!";
                builder.Attachments.Add(attachmentName, attachment);
            });
        }

        private static MimeMessage ConstructReply(MimeMessage message, Action<BodyBuilder> builderAction)
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
            builderAction(builder);

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
