using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using HealthParse.Standard.Health;
using HealthParse.Standard.Mail.Processors;
using HealthParse.Standard.Settings;
using OfficeOpenXml;

namespace HealthParse
{
    class Program
    {
        static void Main(string[] args)
        {
            var fileLocation = @"c:\users\jcfuller\Desktop\export.zip";
            XDocument export = null;
            using (var reader = new StreamReader(fileLocation))
            {
                export = ZipUtilities.ReadArchive(
                    reader.BaseStream,
                    entry => entry.FullName == "apple_health_export/export.xml",
                    entry => XDocument.Load(entry.Open()))
                .FirstOrDefault();
            }

            var settingsStore = new SettingsStore();
            var settings = settingsStore.GetCurrentSettings("abc");
            var filename = @"c:\users\jcfuller\Desktop\export.xlsx";
            using (var excelFile = new ExcelPackage())
            using (var filestream = new FileStream(filename, FileMode.Create))
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                ExcelReport.BuildReport(export, excelFile.Workbook, settings);
                excelFile.SaveAs(filestream);
            }
            //workouts[HKConstants.Workouts.Strength]
            //    .OrderBy(w => w.StartDate)
            //    .Select(w => $"{w.StartDate} - {w.SourceName} - {w.Duration}")
            //    .ToList().ForEach(Console.WriteLine);

            Console.WriteLine("done, press a key");
            Console.ReadKey();
        }
    }
}
