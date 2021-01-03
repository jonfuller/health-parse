using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using HealthParse.Standard.Health;
using HealthParse.Standard.Settings;
using OfficeOpenXml;
using UnitsNet.Units;

namespace HealthParse
{
    class Program
    {
        private const string fileLocation = @"REPLACE_THIS_WITH_YOUR_IMPORT_ZIP";// e.g. @"c:\users\jcfuller\Downloads\export.zip";
        private const string outputLocation = @"REPLACE_THIS_WITH_YOUR_EXPORT_LOCATION";// e.g. @"c:\users\jcfuller\Desktop\";

        static async Task Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var settings = Settings.Default;
            settings.UseConstantNameForMostRecentMonthlySummarySheet = true;
            settings.UseConstantNameForPreviousMonthlySummarySheet = true;

            var fileData = await File.ReadAllBytesAsync(fileLocation);

            var attachment = ExcelReport.CreateReport(fileData, settings, Enumerable.Empty<ExcelWorksheet>());

            await File.WriteAllBytesAsync(outputLocation + "test-edt.xlsx", attachment);

            Console.WriteLine("done, press a key");
            Console.ReadKey();
        }
    }
}
