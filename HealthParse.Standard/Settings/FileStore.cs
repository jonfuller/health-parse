using System.IO;
using OfficeOpenXml;

namespace HealthParse.Standard.Settings
{
    public class FileStore : IStorage
    {
        private readonly string _root;

        public FileStore(string root)
        {
            _root = root;
        }

        private string RootedPathAndCreate(string userId, string filename)
        {
            var filenameAndPath = Path.Combine(_root, userId, filename);
            var filenameDir = Path.GetDirectoryName(filenameAndPath);

            if (!Directory.Exists(filenameDir))
                Directory.CreateDirectory(filenameDir);

            return filenameAndPath;
        }
        private string CustomSheetsPath(string userId)
        {
            return RootedPathAndCreate(userId, "custom_sheets.xlsx");
        }
        private string SettingsJsonPath(string userId)
        {
            return RootedPathAndCreate(userId, "settings.json");
        }

        public ExcelPackage GetCustomSheets(string userId)
        {
            if (!File.Exists(CustomSheetsPath(userId))) return null;

            using (var stream = File.Open(CustomSheetsPath(userId), FileMode.Open, FileAccess.Read))
            {
                var package = new ExcelPackage();
                package.Load(stream);
                return package;
            }
        }

        public void WriteCustomSheets(ExcelPackage package, string userId)
        {
            package.SaveAs(new FileInfo(CustomSheetsPath(userId)));
        }

        public void WriteSettingsFile(string serializeObject, string userId)
        {
            File.WriteAllText(SettingsJsonPath(userId), serializeObject);
        }

        public string GetSettingsJson(string userId)
        {
            return File.Exists(SettingsJsonPath(userId))
                ? File.ReadAllText(SettingsJsonPath(userId))
                : null;
        }
    }
}