using HealthParse.Mail;
using HealthParseFunctions;
using MailKit.Net.Smtp;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace HealthParse
{
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