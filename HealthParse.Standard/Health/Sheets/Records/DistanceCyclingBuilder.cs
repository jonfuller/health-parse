using System.Collections.Generic;
using System.Linq;
using NodaTime;
using UnitsNet;

namespace HealthParse.Standard.Health.Sheets.Records
{
    public class DistanceCyclingBuilder : IRawSheetBuilder<unit>, IMonthlySummaryBuilder<LocalDate>, ISummarySheetBuilder<LocalDate>
    {
        private readonly Settings.Settings _settings;
        private readonly IEnumerable<DistanceCycling> _records;

        public DistanceCyclingBuilder(IEnumerable<Record> records, DateTimeZone zone, Settings.Settings settings)
        {
            _settings = settings;
            _records = records
                .Where(r => r.Type == HKConstants.Records.DistanceCycling)
                .OrderByDescending(r => r.StartDate)
                .Select(r => DistanceCycling.FromRecord(r, zone))
                .ToList();
        }

        public Dataset<unit> BuildRawSheet()
        {
            var columns = _records
                .Aggregate(new
                    {
                        date = new Column<unit> { Header = ColumnNames.Date() },
                        distance = new Column<unit> { Header = ColumnNames.Distance(_settings.DistanceUnit) },
                    },
                    (cols, r) =>
                    {
                        cols.date.Add(unit.v, r.StartDate);
                        cols.distance.Add(unit.v, r.Distance.As(_settings.DistanceUnit));
                        return cols;
                    });

            return new Dataset<unit>(columns.date, columns.distance);
        }

        public IEnumerable<Column<LocalDate>> BuildSummary()
        {
            var distanceColumn = _records
                .GroupBy(s => new { s.StartDate.Year, s.StartDate.Month })
                .Aggregate(new Column<LocalDate> { Header = ColumnNames.CyclingDistance(_settings.DistanceUnit), RangeName = "total_cycling_distance" },
                    (col, r) =>
                    {
                        col.Add(new LocalDate(r.Key.Year, r.Key.Month, 1), r.Sum(c => c.Distance).As(_settings.DistanceUnit));
                        return col;
                    });

            yield return distanceColumn;
        }

        public IEnumerable<Column<LocalDate>> BuildSummaryForDateRange(IRange<ZonedDateTime> dateRange)
        {
            var distanceColumn = _records
                .Where(x => dateRange.Includes(x.StartDate, Clusivity.Inclusive))
                .GroupBy(x => x.StartDate.Date)
                .Aggregate(new Column<LocalDate> { Header = ColumnNames.CyclingDistance(_settings.DistanceUnit), RangeName = "total_cycling_distance"},
                    (col, r) =>
                    {
                        col.Add(r.Key, r.Sum(c => c.Distance).As(_settings.DistanceUnit));
                        return col;
                    });

            yield return distanceColumn;
        }

        private class DistanceCycling
        {
            public ZonedDateTime StartDate { get; private set; }
            public Length Distance { get; private set; }
            public static DistanceCycling FromRecord(Record record, DateTimeZone zone)
            {
                return new DistanceCycling
                {
                    StartDate = record.StartDate.InZone(zone),
                    Distance = RecordParser.Distance(record)
                };
            }
        }
    }
}
