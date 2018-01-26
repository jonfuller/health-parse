using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using OfficeOpenXml;

namespace HealthParse.Standard.Settings
{
    public class SettingsStore : ISettingsStore
    {
        private readonly CloudBlobContainer _container;

        public SettingsStore(CloudBlobContainer container)
        {
            _container = container;
        }

        public void UpdateSettings(ExcelWorksheet settingsSheet, string userId)
        {
            WriteCurrentSettings(userId, ParseSettingsFromSheet(settingsSheet));
        }

        public Func<ExcelWorksheet, bool> IsCustomWorksheet => worksheet =>
            worksheet.Name.StartsWith("custom", StringComparison.CurrentCultureIgnoreCase);

        public IEnumerable<ExcelWorksheet> GetCustomSheets(string userId)
        {
            var customSheetsRef = _container.GetBlobReference(Path.Combine(userId, "custom_sheets.xlsx"));
            if (!customSheetsRef.ExistsAsync().Result) return Enumerable.Empty<ExcelWorksheet>();

            using (var stream = customSheetsRef.OpenReadAsync().Result)
            using (var package = new ExcelPackage())
            {
                package.Load(stream);

                return package.Workbook.Worksheets.Where(IsCustomWorksheet).ToList();
            }
        }
        public void UpdateCustomSheets(IEnumerable<ExcelWorksheet> customSheets, string userId)
        {
            var sheetList = customSheets.ToList();
            if (sheetList.IsEmpty()) return;

            using (var package = new ExcelPackage())
            {
                var customSheetsRef = _container.GetBlockBlobReference(Path.Combine(userId, "custom_sheets.xlsx"));

                foreach (var sheet in sheetList)
                {
                    package.Workbook.Worksheets.Add(sheet.Name, sheet);
                }

                using (var stream = customSheetsRef.OpenWriteAsync().Result)
                {
                    package.SaveAs(stream);
                }
            }
        }

        public Settings GetCurrentSettings(string userId)
        {
            var settingsRef = _container.GetBlobReference(Path.Combine(userId, "settings.json"));
            if (!settingsRef.ExistsAsync().Result) return Settings.Default;

            var type = new[] {new {name = "", value = new object()}};
            var deserialized = JsonConvert.DeserializeAnonymousType(settingsRef.ReadBlob(), type);

            var settings = Settings.Default;
            foreach (var item in deserialized)
            {
                settings.SetValue(item.name, item.value);
            }
            return settings;
        }

        private void WriteCurrentSettings(string userId, Settings settings)
        {
            var settingsRef = _container.GetBlockBlobReference(Path.Combine(userId, "settings.json"));

            var toSerialize = settings
                .Select(s => new {name = s.Name, value = s.Value})
                .ToArray();

            settingsRef.WriteBlob(JsonConvert.SerializeObject(toSerialize));
        }

        public static Settings ParseSettingsFromSheet(ExcelWorksheet sheet)
        {
            var settings = Settings.Default;

            var rows = sheet.Dimension.Rows;

            for (var i = 2; i <= rows; i++)
            {
                var name = sheet.Cells[i, 1].Value.ToString();
                var value = sheet.Cells[i, 2].Value;

                settings.SetValue(name, value);
            }

            return settings;
        }
    }
}