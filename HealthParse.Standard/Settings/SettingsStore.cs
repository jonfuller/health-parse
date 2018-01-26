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