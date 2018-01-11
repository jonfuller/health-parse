using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Queue;

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
    }

    public static class ReceiveMail
    {
        [FunctionName("ReceiveMail")]
        public static void Run(
            [TimerTrigger("0 */5 * * * *")]TimerInfo timer,
            [Queue(queueName: Fn.Qs.IncomingMail, Connection = Fn.ConnectionKeyName)]ICollector<byte[]> outputQueue,
            TraceWriter log)
        {
            // TODO:
            // get emails
            // write an email into Queues.IncomingMail
            // mark email as seen
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");
            outputQueue.Add(System.Text.Encoding.Default.GetBytes(DateTime.Now.ToLongTimeString()));
        }
    }


    public static class DataExtraction
    {
        [FunctionName("ExtractData")]
        public static void Run(
            [QueueTrigger(queueName: Fn.Qs.IncomingMail, Connection = Fn.ConnectionKeyName)]CloudQueueMessage message,
            [Queue(queueName: Fn.Qs.OutgoingMail, Connection = Fn.ConnectionKeyName)]ICollector<byte[]> outputQueue,
            TraceWriter log)
        {
            // TODO:
            // open email
            // parse data
            // build excel
            // construct email
            // write email to Queues.OutgoingMail
            var fromQueue = System.Text.Encoding.Default.GetString(message.AsBytes);
            var newMessage = "in data extract: " + fromQueue;
            outputQueue.Add(System.Text.Encoding.Default.GetBytes(newMessage));
            log.Info($"extract data - {newMessage}");
        }
    }

    public static class SendMail
    {
        [FunctionName("SendMail")]
        public static void Run(
            [QueueTrigger(queueName: Fn.Qs.OutgoingMail, Connection = Fn.ConnectionKeyName)]CloudQueueMessage message,
            TraceWriter log)
        {
            // TODO:
            // open email
            // parse data
            // build excel
            // construct email
            // write email to Queues.OutgoingMail
            log.Info($"send mail {System.Text.Encoding.Default.GetString(message.AsBytes)}");
        }
    }
}
