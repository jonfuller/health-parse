using System;
using System.Collections.Generic;
using System.Linq;
using MimeKit;

namespace HealthParse.Standard.Mail.Processors
{
    public class SettingsUpdateMailProcessor : IMailProcessor
    {
        private readonly string _from;

        public SettingsUpdateMailProcessor(string from)
        {
            _from = @from;
        }

        public Result<MimeMessage> Process(MimeMessage originalEmail, IEnumerable<Tuple<string, byte[]>> attachments)
        {
            return Result.Success(MailUtility.ConstructReply(originalEmail, new MailboxAddress(_from), builder =>
            {
                builder.TextBody = "We're still working on this... sit tight!";
            }));
        }

        public bool CanHandle(MimeMessage message, IEnumerable<Tuple<string, byte[]>> attachments)
        {
            return message.Subject.Contains("SETTINGS") && attachments.Any(a => a.Item1.Contains("SETTINGS"));
        }
    }
}