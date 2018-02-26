using System.Collections.Generic;
using System.Linq;
using NodaTime;
using OfficeOpenXml;

namespace HealthParse.Standard.Health.Sheets.Records
{
    public class StandBuilder : IRawSheetBuilder, ISummarySheetBuilder<StandBuilder.StandItem>, IMonthlySummaryBuilder<StandBuilder.StandItem>
    {
        private readonly DateTimeZone _zone;
        private readonly List<StandDay> _records;

        public StandBuilder(IEnumerable<Record> records, DateTimeZone zone)
        {
            _zone = zone;
            _records = records
                .Where(r => r.Type == HKConstants.Records.Standing.StandType)
                .Where(r => r.Value == HKConstants.Records.Standing.Stood)
                .GroupBy(r => r.StartDate.InZone(_zone).Date)
                .Select(g => new StandDay{Date = g.Key,StandHours  = g.Count()})
                .OrderByDescending(r => r.Date)
                .ToList();
        }

        public IEnumerable<object> BuildRawSheet()
        {
            return _records;
        }

        public IEnumerable<string> Headers => new[]
        {
            ColumnNames.Date(),
            ColumnNames.StandHours(),
        };

        public void Customize(ExcelWorksheet worksheet, ExcelWorkbook workbook)
        {
        }

        public IEnumerable<StandItem> BuildSummary()
        {
            return _records
                .GroupBy(s => new { s.Date.Year, s.Date.Month })
                .Select(x => new StandItem(x.Key.Year, x.Key.Month, x.Average(c => c.StandHours)));
        }

        public IEnumerable<StandItem> BuildSummaryForDateRange(IRange<ZonedDateTime> dateRange)
        {
            return _records
                .Where(r => dateRange.Includes(r.Date.AtStartOfDayInZone(_zone), Clusivity.Inclusive))
                .Select(x => new StandItem(x.Date, x.StandHours));
        }

        private class StandDay
        {
            public LocalDate Date { get; set; }
            public int StandHours { get; set; }
        }
        public class StandItem : DatedItem
        {
            public double AverageStandHours { get; }

            public StandItem(int year, int month, double averageStandHours) : base(year, month)
            {
                AverageStandHours = averageStandHours;
            }

            public StandItem(LocalDate date, double averageStandHours) : base(date)
            {
                AverageStandHours = averageStandHours;
            }
        }
    }
}