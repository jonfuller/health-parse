using HealthParse.Mail;
using HealthParse.Standard.Mail;
using HealthParse.Standard.Settings;
using HealthParseFunctions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace HealthParse
{
    public static class DataExtraction
    {
        [FunctionName("ExtractData")]
        public static void Run(
            [QueueTrigger(queueName: Fn.Qs.IncomingMail, Connection = Fn.ConnectionKeyName)]CloudQueueMessage message,
            [Queue(queueName: Fn.Qs.OutgoingMail, Connection = Fn.ConnectionKeyName)]ICollector<string> outputQueue,
            [Queue(queueName: Fn.Qs.ErrorNotification, Connection = Fn.ConnectionKeyName)]ICollector<string> errorQueue,
            ILogger log)
        {
            var storageConfig = Fn.StorageConfig.Load();
            var storageAccount = CloudStorageAccount.Parse(storageConfig.ConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var incomingContainer = blobClient.GetContainerReference(storageConfig.IncomingMailContainerName);
            var outgoingContainer = blobClient.GetContainerReference(storageConfig.OutgoingMailContainerName);
            var settingsContainer = blobClient.GetContainerReference(storageConfig.SettingsContainerName);
            var errorContainer = blobClient.GetContainerReference(storageConfig.ErrorMailContainerName);
            var originalEmail = EmailStorage.LoadEmailFromStorage(message.AsString, incomingContainer);
            var telemetry = new TelemetryClient(new TelemetryConfiguration(Fn.InstrumentationKey()));

            var settingsStore = new SettingsStore(new CloudStore(settingsContainer));
            var reply = MailProcessor.ProcessEmail(
                originalEmail,
                Fn.EmailConfig.Load().FromEmailAddress,
                settingsStore,
                telemetry);

            if (!reply.WasSuccessful)
            {
                var erroredFile = EmailStorage.SaveEmailToStorage(originalEmail, errorContainer);
                errorQueue.Add(erroredFile);
                log.LogInformation($"enqueueing error - {erroredFile}");
                log.LogError("Error processing email, ", reply.Exception);
            }
            EmailStorage.DeleteEmailFromStorage(message.AsString, incomingContainer);

            var filename = EmailStorage.SaveEmailToStorage(reply.Value, outgoingContainer);
            outputQueue.Add(filename);
            log.LogInformation($"extracted data, enqueueing reply - {reply.Value.To} - {filename}");
        }

    }
}