using OfficeOpenXml;
using System.Collections.Generic;

namespace HealthParse.Standard.Health
{
    public class SummaryBuilder : ISheetBuilder
    {
        private Dictionary<string, IEnumerable<Record>> _records;
        private Dictionary<string, IEnumerable<Workout>> _workouts;

        public SummaryBuilder(Dictionary<string, IEnumerable<Record>> records, Dictionary<string, IEnumerable<Workout>> workouts)
        {
            _records = records;
            _workouts = workouts;
        }

        void ISheetBuilder.Build(ExcelWorksheet sheet)
        {
            sheet.Cells["A1"].Value = "Hello World";
        }
    }
}
