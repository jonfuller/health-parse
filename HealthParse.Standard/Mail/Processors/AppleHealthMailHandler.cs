using System;
using System.Collections.Generic;
using HealthParse.Standard.Health;
using MimeKit;
using OfficeOpenXml;

namespace HealthParse.Standard.Mail.Processors
{
    public abstract class AppleHealthMailHandler : IMailHandler
    {
        private readonly string _from;
        private readonly Settings.Settings _settings;
        private readonly IEnumerable<ExcelWorksheet> _customSheets;

        protected AppleHealthMailHandler(string from, Settings.Settings settings, IEnumerable<ExcelWorksheet> customSheets)
        {
            _from = @from;
            _settings = settings;
            _customSheets = customSheets;
        }

        public abstract bool CanHandle(MimeMessage message, IEnumerable<(string name, byte[] data)> attachments);

        public Result<MimeMessage> Process(MimeMessage originalEmail, IEnumerable<(string name, byte[] data)> attachments)
        {
            var theExportZip = GetExportZip(originalEmail, attachments);
            var attachment = ExcelReport.CreateReport(theExportZip, _settings, _customSheets);
            var attachmentName = $"export.{originalEmail.Date.Date:yyyy-MM-dd}.xlsx";

            var reply = MailUtility.ConstructReply(originalEmail, new MailboxAddress(_from), builder =>
            {
                builder.TextBody = @"Hey there, I saw your health data... good work!";
                builder.Attachments.Add(attachmentName, attachment);
            });

            return Result.Success(reply);
        }

        protected abstract byte[] GetExportZip(MimeMessage message, IEnumerable<(string name, byte[] data)> attachments);
    }
}