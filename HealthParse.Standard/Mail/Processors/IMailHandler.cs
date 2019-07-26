using System;
using System.Collections.Generic;
using MimeKit;

namespace HealthParse.Standard.Mail.Processors
{
    public interface IMailHandler
    {
        bool CanHandle(MimeMessage message, IEnumerable<(string name, byte[] data)> attachments);
        Result<MimeMessage> Process(MimeMessage message, IEnumerable<(string name, byte[] data)> attachments);
    }
}
