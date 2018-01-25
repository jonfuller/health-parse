using System;
using System.Collections.Generic;
using MimeKit;

namespace HealthParse.Standard.Mail.Processors
{
    public class HelpMailProcessor : IMailProcessor
    {
        private readonly string _from;

        public HelpMailProcessor(string from)
        {
            _from = @from;
        }
        public bool CanHandle(MimeMessage message, IEnumerable<Tuple<string, byte[]>> attachments)
        {
            return true;
        }

        public Result<MimeMessage> Process(MimeMessage message, IEnumerable<Tuple<string, byte[]>> attachments)
        {
            return Result.Success(MailUtility.ConstructReply(message, new MailboxAddress(_from), builder =>
            {
                builder.TextBody = $@"It looks like you might not know what you're doing.

Here's a link to some help: {MailUtility.HelpDocUrl}
";
            }));
        }
    }
}