using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;

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
            var stepsByMonth = StepHelper.PrioritizeSteps(_records[HKConstants.Records.StepCount])
                .GroupBy(s => new { s.StartDate.Date.Year, s.StartDate.Date.Month })
                .Select(x => new
                {
                    date = new DateTime(x.Key.Year, x.Key.Month, 1),
                    steps = x.Sum(r => r.Value.SafeParse(0))
                })
                .OrderByDescending(s => s.date);

            sheet.WriteData(stepsByMonth);
        }
    }
}
