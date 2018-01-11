using MailKit.Net.Imap;
using MailKit.Net.Pop3;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using System;
using System.Configuration;
using System.Linq;

namespace Mailin
{
    class Program
    {
        static void Main(string[] args)
        {
            // attachments: https://github.com/jstedfast/MailKit/blob/master/FAQ.md#q-how-can-i-create-a-message-with-attachments

            var gmailUsername = ConfigurationManager.AppSettings["GmailUsername"];
            var gmailPassword = ConfigurationManager.AppSettings["GmailPassword"];

            using (var client = new ImapClient())
            {
                // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                client.Connect("imap.gmail.com", 993, SecureSocketOptions.SslOnConnect);
                client.Authenticate(gmailUsername, gmailPassword);

                var inbox = client.Inbox;
                inbox.Open(MailKit.FolderAccess.ReadWrite);

                Enumerable.Range(0, inbox.Count)
                    .Select(i => inbox.GetMessage(i))
                    .Select(m => $"{m.Subject}: {m.Attachments.Count()}")
                    .ToList().ForEach(Console.WriteLine);

                var query = SearchQuery.NotSeen;
                inbox.Search(query)
                    .Select(i => inbox.GetMessage(i))
                    .Select(m => $"{m.Subject}: {m.Attachments.Count()}")
                    .ToList().ForEach(Console.WriteLine);
                Console.WriteLine("done");
                client.Disconnect(true);
            }
            //using (var client = new Pop3Client())
            //{
            //    // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
            //    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            //    client.Connect("pop.gmail.com", 995, true);
            //    client.Authenticate(gmailUsername, gmailPassword);

            //    for (int i = 0; i < client.Count; i++)
            //    {
            //        var message = client.GetMessage(i);
            //        ;
            //        Console.WriteLine($"Subject: {message.Subject} - {message.Attachments.Count()}");
            //    }

            //    client.Disconnect(true);
            //}
            Console.ReadKey();
        }
    }
}
