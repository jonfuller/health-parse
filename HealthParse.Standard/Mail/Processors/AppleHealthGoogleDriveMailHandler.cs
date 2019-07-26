using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using MimeKit;
using OfficeOpenXml;

namespace HealthParse.Standard.Mail.Processors
{
    public class AppleHealthGoogleDriveMailHandler : AppleHealthMailHandler
    {
        public AppleHealthGoogleDriveMailHandler(string from, Settings.Settings settings, IEnumerable<ExcelWorksheet> customSheets) : base(@from, settings, customSheets)
        {
        }

        public override bool CanHandle(MimeMessage message, IEnumerable<(string name, byte[] data)> attachments)
        {
            return GetGoogleDriveLinkInEmail(message) != null;
        }

        protected override byte[] GetExportZip(MimeMessage message, IEnumerable<(string name, byte[] data)> attachments)
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
}