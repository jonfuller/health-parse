using System;
using System.Collections.Generic;
using OfficeOpenXml;

namespace HealthParse.Standard.Settings
{
    public interface ISettingsStore
    {
        void UpdateSettings(ExcelWorksheet settingsSheet, string userId);
        void UpdateCustomSheets(IEnumerable<ExcelWorksheet> settingsSheet, string userId);
        Func<ExcelWorksheet, bool> IsCustomWorksheet { get; }
        Settings GetCurrentSettings(string userId);
    }
}