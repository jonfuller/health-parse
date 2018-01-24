using System;
using System.Collections.Generic;
using MimeKit;

namespace HealthParse.Standard.Mail.Processors
{
    public interface IMailProcessor
    {
        bool CanHandle(MimeMessage message, IEnumerable<Tuple<string, byte[]>> attachments);
        Result<MimeMessage> Process(MimeMessage message, IEnumerable<Tuple<string, byte[]>> attachments);
    }
}
