﻿using System;
using System.Collections.Generic;
using System.Linq;
using NodaTime;

namespace HealthParse.Standard.Health.Sheets
{
    public class StepBuilder : ISheetBuilder<StepBuilder.StepItem>
    {
        private readonly DateTimeZone _zone;
        private readonly IEnumerable<Record> _records;

        public StepBuilder(IReadOnlyDictionary<string, IEnumerable<Record>> records, DateTimeZone zone)
        {
            _zone = zone;
            _records = records.ContainsKey(HKConstants.Records.StepCount)
                ? records[HKConstants.Records.StepCount]
                : Enumerable.Empty<Record>();
        }

        IEnumerable<object> ISheetBuilder.BuildRawSheet()
        {
            return GetStepsByDay()
                .Select(s => new{Date = s.Date.ToDateTimeUnspecified(), s.Steps});
        }

        bool ISheetBuilder.HasHeaders => true;

        IEnumerable<string> ISheetBuilder.Headers => new[]
        {
            ColumnNames.Date(),
            ColumnNames.Steps(),
        };

        IEnumerable<StepItem> ISheetBuilder<StepItem>.BuildSummary()
        {
            return GetStepsByDay()
                .GroupBy(s => new { s.Date.Year, s.Date.Month })
                .Select(x => new StepItem(x.Key.Year, x.Key.Month, x.Sum(r => r.Steps)));
        }

        IEnumerable<StepItem> ISheetBuilder<StepItem>.BuildSummaryForDateRange(IRange<ZonedDateTime> dateRange)
        {
            return GetStepsByDay().Where(x => dateRange.Includes(x.Date.AtStartOfDayInZone(_zone), Clusivity.Inclusive));
        }

        private IEnumerable<StepItem> GetStepsByDay()
        {
            return StepHelper.PrioritizeSteps(_records)
                .Select(r => new { zoned = r.StartDate.InZone(_zone), r })
                .GroupBy(s => s.zoned.Date)
                .Select(x => new StepItem(x.Key, (int)x.Sum(r => r.r.Value.SafeParse(0))))
                .OrderByDescending(s => s.Date);
        }

        public class StepItem : DatedItem
        {
            public StepItem(LocalDate date, int steps) : base(date)
            {
                Steps = steps;
            }

            public StepItem(int year, int month, int steps) : base(year, month)
            {
                Steps = steps;
            }

            public int Steps { get; }

        }
    }
}
