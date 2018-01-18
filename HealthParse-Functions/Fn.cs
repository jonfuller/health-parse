using System;

namespace HealthParseFunctions
{
    public static class Fn
    {
        public const string ConnectionKeyName = "QueueStorage";

        public static class Qs
        {
            public const string IncomingMail = "health-parse-incoming-mail";
            public const string OutgoingMail = "health-parse-outgoing-mail";
            public const string ErrorNotification = "health-parse-error-notification";
        }

        public class StorageConfig
        {
            public string IncomingMailContainerName { get; private set; }
            public string OutgoingMailContainerName { get; private set; }
            public string ErrorMailContainerName { get; private set; }
            public string ConnectionString { get; private set; }

            public static StorageConfig Load()
            {
                return new StorageConfig
                {
                    IncomingMailContainerName = Environment.GetEnvironmentVariable("StorageBlob_IncomingMail"),
                    OutgoingMailContainerName = Environment.GetEnvironmentVariable("StorageBlob_OutgoingMail"),
                    ErrorMailContainerName = Environment.GetEnvironmentVariable("StorageBlob_ErrorMail"),
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

            public string AdminEmailAddress;
            public string FromEmailAddress;

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
                    AdminEmailAddress = Environment.GetEnvironmentVariable("AdminEmailAddress"),
                    FromEmailAddress = Environment.GetEnvironmentVariable("FromEmailAddress"),
                };
            }
        }
    }
}
