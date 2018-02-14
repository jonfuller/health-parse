using System;
using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml;

namespace HealthParse.Standard.Health.Sheets
{
    public class SettingsSheetBuilder : ISheetBuilder
    {
        private readonly Settings.Settings _settings;

        public SettingsSheetBuilder(Settings.Settings settings)
        {
            _settings = settings;
        }

        public IEnumerable<object> BuildRawSheet()
        {
            return _settings
                .Select((setting, i) => new
                {
                    setting.Name,
                    setting.Value,
                    setting.DefaultValue,
                    setting.Description
                });
        }

        void ISheetBuilder.Customize(ExcelWorksheet worksheet, ExcelWorkbook workbook)
        {
        }

        bool ISheetBuilder.HasHeaders => false;

        IEnumerable<string> ISheetBuilder.Headers => throw new NotImplementedException();
    }
}