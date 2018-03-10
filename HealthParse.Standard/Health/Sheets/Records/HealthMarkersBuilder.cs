using System;
using System.Collections.Generic;
using System.Linq;
using NodaTime;
using UnitsNet;

namespace HealthParse.Standard.Health.Sheets.Records
{
    public class HealthMarkersBuilder : IRawSheetBuilder<LocalDate>, ISummarySheetBuilder<LocalDate>, IMonthlySummaryBuilder<LocalDate>
    {
        private readonly DateTimeZone _zone;
        private readonly List<Tuple<LocalDate, double>> _restingHeartRate;
        private readonly List<Tuple<LocalDate, double>> _walkingHeartRateAvg;
        private readonly List<Tuple<LocalDate, double>> _vo2Max;

        public HealthMarkersBuilder(IEnumerable<Record> records, DateTimeZone zone)
        {
            _zone = zone;

            var categorized = records.Aggregate(new
            {
                restingHeartRate = new List<Record>(),
                vo2Max = new List<Record>(),
                walkingHeartRateAvg = new List<Record>(),
            }, (accum, record) =>
            {
                if (record.Type == HKConstants.Records.Markers.RestingHeartRate)
                    accum.restingHeartRate.Add(record);
                if (record.Type == HKConstants.Records.Markers.WalkingHeartRateAverage)
                    accum.walkingHeartRateAvg.Add(record);
                if (record.Type == HKConstants.Records.Markers.Vo2Max)
                    accum.vo2Max.Add(record);
                return accum;
            });

            // TODO: parse the unit (would heart rate ever be something other than count/min?)
            _restingHeartRate = categorized.restingHeartRate
                .GroupBy(r => r.StartDate.InZone(_zone).Date)
                .Select(r => new { date = r.Key, value = r.First().Value.SafeParse(0) })
                .Select(r => Tuple.Create(r.date, Frequency.FromCyclesPerMinute(r.value).CyclesPerMinute))
                .ToList();

            // TODO: parse the unit (would heart rate ever be something other than count/min?)
            _walkingHeartRateAvg = categorized.walkingHeartRateAvg
                .GroupBy(r => r.StartDate.InZone(_zone).Date)
                .Select(r => new { date = r.Key, value = r.First().Value.SafeParse(0) })
                .Select(r => Tuple.Create(r.date, Frequency.FromCyclesPerMinute(r.value).CyclesPerMinute))
                .ToList();

            _vo2Max = categorized.vo2Max
                .GroupBy(r => r.StartDate.InZone(_zone).Date)
                .Select(r => new { date = r.Key, value = r.First().Value.SafeParse(0) })
                .Select(r => Tuple.Create(r.date, r.value))
                .ToList();
        }
        public Dataset<LocalDate> BuildRawSheet()
        {
            var dates = _restingHeartRate.Select(s => s.Item1)
                .Concat(_vo2Max.Select(s => s.Item1))
                .Concat(_walkingHeartRateAvg.Select(s => s.Item1))
                .Distinct();

            return new Dataset<LocalDate>(
                new KeyColumn<LocalDate>(dates),
                _restingHeartRate.MakeColumn(ColumnNames.Markers.RestingHeartRate),
                _walkingHeartRateAvg.MakeColumn(ColumnNames.Markers.WalkingHeartRateAverage),
                _vo2Max.MakeColumn(ColumnNames.Markers.Vo2Max));
        }

        public IEnumerable<Column<LocalDate>> BuildSummary()
        {
            yield return _restingHeartRate
                .GroupBy(s => new { s.Item1.Year, s.Item1.Month })
                .Select(r => Tuple.Create(new LocalDate(r.Key.Year, r.Key.Month, 1), r.Average(c => c.Item2)))
                .MakeColumn(ColumnNames.Markers.RestingHeartRateAverage, "avg_resting_hr");

            yield return _vo2Max
                .GroupBy(s => new { s.Item1.Year, s.Item1.Month })
                .Select(r => Tuple.Create(new LocalDate(r.Key.Year, r.Key.Month, 1), r.Average(c => c.Item2)))
                .MakeColumn(ColumnNames.Markers.Vo2MaxAverage, "avg_vo2max");

            yield return _walkingHeartRateAvg
                .GroupBy(s => new { s.Item1.Year, s.Item1.Month })
                .Select(r => Tuple.Create(new LocalDate(r.Key.Year, r.Key.Month, 1), r.Average(c => c.Item2)))
                .MakeColumn(ColumnNames.Markers.WalkingHeartRateAverage, "avg_walking_hr_avg");
        }

        public IEnumerable<Column<LocalDate>> BuildSummaryForDateRange(IRange<ZonedDateTime> dateRange)
        {
            yield return _restingHeartRate
                .Where(r => dateRange.Includes(r.Item1.AtStartOfDayInZone(_zone), Clusivity.Inclusive))
                .MakeColumn(ColumnNames.Markers.RestingHeartRate, "resting_hr");

            yield return _walkingHeartRateAvg
                .Where(r => dateRange.Includes(r.Item1.AtStartOfDayInZone(_zone), Clusivity.Inclusive))
                .MakeColumn(ColumnNames.Markers.WalkingHeartRateAverage, "walking_hr_avg");

            yield return _vo2Max
                .Where(r => dateRange.Includes(r.Item1.AtStartOfDayInZone(_zone), Clusivity.Inclusive))
                .MakeColumn(ColumnNames.Markers.Vo2Max, "vo2max");
        }

    }
}