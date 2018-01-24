using HealthParse.Mail;
using HealthParse.Standard.Mail;
using HealthParseFunctions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using MimeKit;

namespace HealthParse
{
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
}