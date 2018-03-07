using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using OfficeOpenXml;

namespace HealthParse.Standard.Settings
{
    public class SettingsStore : ISettingsStore
    {
        private readonly IStorage _storage;

        public SettingsStore(IStorage storage)
        {
            _storage = storage;
        }

        public void UpdateSettings(ExcelWorksheet settingsSheet, string userId)
        {
            WriteCurrentSettings(userId, ParseSettingsFromSheet(settingsSheet));
        }

        public Func<ExcelWorksheet, bool> IsCustomWorksheet => worksheet =>
            worksheet.Name.StartsWith("custom", StringComparison.CurrentCultureIgnoreCase);

        public IEnumerable<ExcelWorksheet> GetCustomSheets(string userId)
        {
            var customSheets = _storage.GetCustomSheets(userId);

            return customSheets == null
                ? Enumerable.Empty<ExcelWorksheet>()
                : customSheets.Workbook.Worksheets.Where(IsCustomWorksheet);
        }
        public void UpdateCustomSheets(IEnumerable<ExcelWorksheet> customSheets, string userId)
        {
            var sheetList = customSheets.ToList();
            if (sheetList.IsEmpty()) return;

            using (var package = new ExcelPackage())
            {
                foreach (var sheet in sheetList)
                {
                    package.Workbook.Worksheets.Add(sheet.Name, sheet);
                }

                _storage.WriteCustomSheets(package, userId);
            }
        }

        public Settings GetCurrentSettings(string userId)
        {
            var settingsJson = _storage.GetSettingsJson(userId);

            if (settingsJson == null) return Settings.Default;

            var type = new[] {new {name = "", value = new object()}};
            var deserialized = JsonConvert.DeserializeAnonymousType(settingsJson, type);

            var settings = Settings.Default;
            foreach (var item in deserialized)
            {
                settings.SetValue(item.name, item.value);
            }
            return settings;
        }

        private void WriteCurrentSettings(string userId, Settings settings)
        {
            var toSerialize = settings
                .Select(s => new {name = s.Name, value = s.JsonSerialization == SerializationBehavior.Nothing
                    ? s.Value
                    : s.Value.ToString()})
                .ToArray();

            _storage.WriteSettingsFile(JsonConvert.SerializeObject(toSerialize), userId);
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