using System;
using System.Collections.Generic;
using System.Linq;
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
}