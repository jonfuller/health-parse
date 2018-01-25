using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using MimeKit;
using OfficeOpenXml;

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
            var excelAttachment = attachments.First(a => a.Item1.Contains("xlsx")).Item2;

            using (var stream = new MemoryStream(excelAttachment))
            using (var excelDoc = new ExcelPackage(stream))
            {
                var settingsSheet = excelDoc.Workbook
                    .Worksheets
                    .FirstOrDefault(sheet => sheet.Name.StartsWith("settings", StringComparison.CurrentCultureIgnoreCase));

                if (settingsSheet == null)
                {
                    return Result.Success(ConstructSettingsUpdateErrorMessage(originalEmail));
                }

                return Result.Success(MailUtility.ConstructReply(originalEmail, new MailboxAddress(_from), builder =>
                {
                    builder.TextBody = @"I got your settings update! I'll take those into account next time around.

See you next time!";
                }));
            }
        }

        public bool CanHandle(MimeMessage message, IEnumerable<Tuple<string, byte[]>> attachments)
        {
            return message.Subject.ToLower(CultureInfo.CurrentCulture).Contains("settings")
                && attachments.Any(a => a.Item1.ToLower(CultureInfo.CurrentCulture).Contains("xlsx"));
        }

        private MimeMessage ConstructSettingsUpdateErrorMessage(MimeMessage original)
        {
            return MailUtility.ConstructReply(original, new MailboxAddress(_from), builder =>
            {
                builder.TextBody = $@"To update your settings, include the word 'settings' in your subject, and attach an Excel document with at least the 'Settings' sheet from your latest report (you can include the rest of the document too, I'll just ignore it).

For more information, take a look at the help, here: {MailUtility.HelpDocUrl}
";
            });
        }
    }
}