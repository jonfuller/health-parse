using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using HealthParse.Standard.Health;
using MimeKit;
using OfficeOpenXml;

namespace HealthParse.Standard.Mail.Processors
{
    public class AppleHealthAttachmentMailProcessor : AppleHealthMailProcessor
    {
        public AppleHealthAttachmentMailProcessor(string from, Settings.Settings settings, IEnumerable<ExcelWorksheet> customSheets) : base(@from, settings, customSheets)
        {
        }

        protected override byte[] GetExportZip(MimeMessage message, IEnumerable<Tuple<string, byte[]>> attachments)
        {
            return attachments.Single(a => a.Item1 == "export.zip").Item2;
        }
        public override bool CanHandle(MimeMessage message, IEnumerable<Tuple<string, byte[]>> attachments)
        {
            return attachments.Any(a => a.Item1 == "export.zip");
        }
    }

    public class AppleHealthGoogleDriveMailProcessor : AppleHealthMailProcessor
    {
        public AppleHealthGoogleDriveMailProcessor(string from, Settings.Settings settings, IEnumerable<ExcelWorksheet> customSheets) : base(@from, settings, customSheets)
        {
        }

        public override bool CanHandle(MimeMessage message, IEnumerable<Tuple<string, byte[]>> attachments)
        {
            return GetGoogleDriveLinkInEmail(message) != null;
        }

        protected override byte[] GetExportZip(MimeMessage message, IEnumerable<Tuple<string, byte[]>> attachments)
        {
            var link = GetGoogleDriveLinkInEmail(message);
            var fileId = new Uri(link).Segments.OrderByDescending(s => s.Length).First().Replace("/", "");
            using (var client = new WebClient())
            {
                return client.DownloadData($"https://drive.google.com/uc?id={fileId}");
            }
        }

        private static string GetGoogleDriveLinkInEmail(MimeMessage message)
        {
            return message.BodyParts
                .Where(p => p is TextPart)
                .Cast<TextPart>()
                .Where(r => r.Text.Contains("https://drive.google.com"))
                .Where(r => r.ContentType.MimeType == "text/plain")
                .Select(p => GetLinkFromTextPart(p.Text))
                .FirstOrDefault(p => p != null);
        }

        private static string GetLinkFromTextPart(string text)
        {
            return text
                .Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Skip(2)
                .Take(1)
                .Select(s => s.Substring(1, s.Length - 2))
                .FirstOrDefault();
        }
    }

    public abstract class AppleHealthMailProcessor : IMailProcessor
    {
        private readonly string _from;
        private readonly Settings.Settings _settings;
        private readonly IEnumerable<ExcelWorksheet> _customSheets;

        protected AppleHealthMailProcessor(string from, Settings.Settings settings, IEnumerable<ExcelWorksheet> customSheets)
        {
            _from = @from;
            _settings = settings;
            _customSheets = customSheets;
        }

        public abstract bool CanHandle(MimeMessage message, IEnumerable<Tuple<string, byte[]>> attachments);

        public Result<MimeMessage> Process(MimeMessage originalEmail, IEnumerable<Tuple<string, byte[]>> attachments)
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

        protected abstract byte[] GetExportZip(MimeMessage message, IEnumerable<Tuple<string, byte[]>> attachments);
    }
}