using OfficeOpenXml;
using System.Collections.Generic;
using System.Linq;

namespace HealthParse.Standard.Health.Sheets
{
    public class StepBuilder : ISheetBuilder<StepBuilder.MonthlyStep>
    {
        private Dictionary<string, IEnumerable<Record>> _records;

        public StepBuilder(Dictionary<string, IEnumerable<Record>> records)
        {
            _records = records;
        }

        void ISheetBuilder.Build(ExcelWorksheet sheet)
        {
            var steps = StepHelper.PrioritizeSteps(_records[HKConstants.Records.StepCount])
                .GroupBy(s => s.StartDate.Date)
                .Select(x => new
                {
                    date = x.Key,
                    steps = x.Sum(r => r.Value.SafeParse(0))
                })
                .OrderByDescending(s => s.date);

            sheet.WriteData(steps);
        }

        IEnumerable<MonthlyStep> ISheetBuilder<MonthlyStep>.BuildSummary()
        {
            return StepHelper.PrioritizeSteps(_records[HKConstants.Records.StepCount])
                .GroupBy(s => new { s.StartDate.Date.Year, s.StartDate.Date.Month })
                .Select(x => new MonthlyStep(x.Key.Year, x.Key.Month, (int)x.Sum(r => r.Value.SafeParse(0))));
        }

        public class MonthlyStep : MonthlyItem
        {
            public MonthlyStep(int year, int month, int steps) : base(year, month)
            {
                Steps = steps;
            }

            public int Steps { get; }
        }
    }
}
