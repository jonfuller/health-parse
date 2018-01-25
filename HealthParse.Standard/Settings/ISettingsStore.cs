using OfficeOpenXml;

namespace HealthParse.Standard.Settings
{
    public interface ISettingsStore
    {
        void UpdateSettings(ExcelWorksheet settingsSheet, string userId);
        Settings GetCurrentSettings(string userId);
    }
}