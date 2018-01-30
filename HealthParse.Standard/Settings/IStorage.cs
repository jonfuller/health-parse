using OfficeOpenXml;

namespace HealthParse.Standard.Settings
{
    public interface IStorage
    {
        ExcelPackage GetCustomSheets(string userId);
        void WriteCustomSheets(ExcelPackage package, string userId);
        void WriteSettingsFile(string serializeObject, string userId);
        string GetSettingsJson(string userId);
    }
}