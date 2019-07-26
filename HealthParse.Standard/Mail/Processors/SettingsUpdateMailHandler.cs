using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using HealthParse.Standard.Settings;
using MimeKit;
using OfficeOpenXml;

namespace HealthParse.Standard.Mail.Processors
{
    public class SettingsUpdateMailHandler : IMailHandler
    {
        private readonly string _from;
        private readonly ISettingsStore _settingsStore;

        public SettingsUpdateMailHandler(string from, ISettingsStore settingsStore)
        {
            _from = @from;
            _settingsStore = settingsStore;
        }

        public Result<MimeMessage> Process(MimeMessage originalEmail, IEnumerable<(string name, byte[] data)> attachments)
        {
            var excelAttachment = attachments.First(a => a.name.Contains("xlsx")).data;

            using (var stream = new MemoryStream(excelAttachment))
            using (var excelDoc = new ExcelPackage(stream))
            {
                var settingsSheet = excelDoc.Workbook
                    .Worksheets
                    .FirstOrDefault(sheet => sheet.Name.StartsWith("settings", StringComparison.CurrentCultureIgnoreCase));

                var customSheets = excelDoc.Workbook
                    .Worksheets
                    .Where(_settingsStore.IsCustomWorksheet)
                    .ToList();

                if (settingsSheet == null && customSheets.IsEmpty())
                {
                    return Result.Success(ConstructSettingsUpdateErrorMessage(originalEmail));
                }

                var settingsUserId = originalEmail.From.Mailboxes.First().HashedEmail();
                _settingsStore.UpdateSettings(settingsSheet, settingsUserId);
                _settingsStore.UpdateCustomSheets(customSheets, settingsUserId);

                return Result.Success(MailUtility.ConstructReply(originalEmail, new MailboxAddress(_from), builder =>
                {
                    builder.TextBody = @"I got your settings update! I'll take those into account next time around.

See you next time!";
                }));
            }
        }

        public bool CanHandle(MimeMessage message, IEnumerable<(string name, byte[] data)> attachments)
        {
            return message.Subject.ToLower(CultureInfo.CurrentCulture).Contains("settings")
                && attachments.Any(a => a.name.ToLower(CultureInfo.CurrentCulture).Contains("xlsx"));
        }

        private MimeMessage ConstructSettingsUpdateErrorMessage(MimeMessage original)
        {
            return MailUtility.ConstructReply(original, new MailboxAddress(_from), builder =>
            {
                builder.TextBody = $@"To update your settings, include the word 'settings' in your subject, and attach an Excel document.

The Excel document should have either the 'Settings' sheet from your latest report and/or custom sheets you'd like included in your next report. Custom sheets should start with the word 'custom'.

You can include the rest of the document too, I'll just ignore the rest of it.

For more information, take a look at the help, here: {MailUtility.HelpDocUrl}
";
            });
        }
    }
}