using System;
using System.Collections.Generic;
using System.Linq;
using MimeKit;
using OfficeOpenXml;

namespace HealthParse.Standard.Mail.Processors
{
    public class AppleHealthAttachmentMailHandler : AppleHealthMailHandler
    {
        public AppleHealthAttachmentMailHandler(string from, Settings.Settings settings, IEnumerable<ExcelWorksheet> customSheets) : base(@from, settings, customSheets)
        {
        }

        protected override byte[] GetExportZip(MimeMessage message, IEnumerable<(string name, byte[] data)> attachments)
        {
            return attachments.Single(a => a.Item1 == "export.zip").Item2;
        }
        public override bool CanHandle(MimeMessage message, IEnumerable<(string name, byte[] data)> attachments)
        {
            return attachments.Any(a => a.Item1 == "export.zip");
        }
    }
}