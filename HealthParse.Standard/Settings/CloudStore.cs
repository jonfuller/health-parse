using System.IO;
using Microsoft.WindowsAzure.Storage.Blob;
using OfficeOpenXml;

namespace HealthParse.Standard.Settings
{
    public class CloudStore : IStorage
    {
        private readonly CloudBlobContainer _container;

        public CloudStore(CloudBlobContainer container)
        {
            _container = container;
        }

        public ExcelPackage GetCustomSheets(string userId)
        {
            var customSheetsRef = _container.GetBlobReference(Path.Combine(userId, "custom_sheets.xlsx"));
            if (!customSheetsRef.ExistsAsync().Result) return null;

            using (var stream = customSheetsRef.OpenReadAsync().Result)
            {
                var package = new ExcelPackage();
                package.Load(stream);
                return package;
            }
        }

        public void WriteCustomSheets(ExcelPackage package, string userId)
        {
            var customSheetsRef = _container.GetBlockBlobReference(Path.Combine(userId, "custom_sheets.xlsx"));
            using (var stream = customSheetsRef.OpenWriteAsync().Result)
            {
                package.SaveAs(stream);
            }
        }

        public void WriteSettingsFile(string serializeObject, string userId)
        {
            var settingsRef = _container.GetBlockBlobReference(Path.Combine(userId, "settings.json"));

            settingsRef.WriteBlob(serializeObject);
        }

        public string GetSettingsJson(string userId)
        {
            var settingsRef = _container.GetBlobReference(Path.Combine(userId, "settings.json"));
            return settingsRef.ExistsAsync().Result
                ? settingsRef.ReadBlob()
                : null;
        }
    }
}