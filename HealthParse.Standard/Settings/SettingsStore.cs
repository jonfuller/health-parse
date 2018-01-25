using System.Linq;
using OfficeOpenXml;

namespace HealthParse.Standard.Settings
{
    public class SettingsStore : ISettingsStore
    {
        public void UpdateSettings(ExcelWorksheet settingsSheet, string userId)
        {
            var defaultSettings = Settings.Default;
            var currentSettings = GetCurrentSettings(userId);
            var updatedSettings = ParseSettingsFromSheet(settingsSheet);
        }

        public void PopulateSettingsSheet(ExcelWorksheet settingsSheet, Settings settings)
        {
            settings
                .Select((setting, i) => new {setting, i})
                .ToList()
                .ForEach(s =>
                {
                    settingsSheet.Cells[s.i + 2, 1].Value = s.setting.Name;
                    settingsSheet.Cells[s.i + 2, 2].Value = s.setting.Value;
                    settingsSheet.Cells[s.i + 2, 3].Value = s.setting.DefaultValue;
                    settingsSheet.Cells[s.i + 2, 4].Value = s.setting.Description;
                });
            settingsSheet.Cells[1, 1].Value = "Name";
            settingsSheet.Cells[1, 1].Value = "Value";
            settingsSheet.Cells[1, 1].Value = "DefaultValue";
            settingsSheet.Cells[1, 1].Value = "Description";
        }

        public Settings GetCurrentSettings(string userId)
        {
            // TODO
            return Settings.Default;
        }

        private Settings ParseSettingsFromSheet(ExcelWorksheet sheet)
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