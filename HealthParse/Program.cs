using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using HealthParse.Standard.Health;
using HealthParse.Standard.Settings;
using NodaTime.Text;
using OfficeOpenXml;

namespace HealthParse
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var fileLocation = @"c:\users\jcfuller\Downloads\export.zip";
            XDocument export = null;
            using (var reader = new StreamReader(fileLocation))
            {
                export = ZipUtilities.ReadArchive(
                    reader.BaseStream,
                    entry => entry.FullName == "apple_health_export/export.xml",
                    entry => XDocument.Load(entry.Open()))
                .FirstOrDefault();
            }

            var settings = Settings.Default;
            settings.UseConstantNameForMostRecentMonthlySummarySheet = true;
            settings.UseConstantNameForPreviousMonthlySummarySheet = true;

            using (var excelFile = new ExcelPackage())
            {
                ExcelReport.BuildReport(export, excelFile.Workbook, settings, Enumerable.Empty<ExcelWorksheet>());

                excelFile.SaveAs(new FileInfo(@"c:\users\jcfuller\Desktop\test-edt.xlsx"));
            }

            Console.WriteLine("done, press a key");
            Console.ReadKey();
        }
    }
}
