﻿using System.Collections.Generic;
using System.Linq;
using NodaTime;
using OfficeOpenXml;
using UnitsNet;

namespace HealthParse.Standard.Health.Sheets.Records
{
    public class DistanceCyclingBuilder : IRawSheetBuilder, IMonthlySummaryBuilder<DistanceCyclingBuilder.CyclingItem>, ISummarySheetBuilder<DistanceCyclingBuilder.CyclingItem>
    {
        private readonly DateTimeZone _zone;
        private readonly Settings.Settings _settings;
        private readonly IEnumerable<DistanceCycling> _records;

        public DistanceCyclingBuilder(IEnumerable<Record> records, DateTimeZone zone, Settings.Settings settings)
        {
            _zone = zone;
            _settings = settings;
            _records = records
                .Where(r => r.Type == HKConstants.Records.DistanceCycling)
                .Select(DistanceCycling.FromRecord)
                .ToList();
        }
        public IEnumerable<object> BuildRawSheet()
        {
            return _records
                .OrderByDescending(r => r.StartDate)
                .Select(r => new
                {
                    Date = r.StartDate.InZone(_zone),
                    Distance = r.Distance.As(_settings.DistanceUnit),
                });
        }

        public void Customize(ExcelWorksheet _, ExcelWorkbook workbook)
        {
        }

        public IEnumerable<string> Headers => new[]
        {
            ColumnNames.Date(),
            ColumnNames.Distance(_settings.DistanceUnit),
        };

        public IEnumerable<CyclingItem> BuildSummary()
        {
            return _records
                .Select(record => new {zoned = record.StartDate.InZone(_zone), record})
                .GroupBy(s => new { s.zoned.Year, s.zoned.Month })
                .Select(x => new CyclingItem(x.Key.Year, x.Key.Month, x.Sum(c => c.record.Distance)));
        }

        public IEnumerable<CyclingItem> BuildSummaryForDateRange(IRange<ZonedDateTime> dateRange)
        {
            return _records
                .Select(record => new { zoned = record.StartDate.InZone(_zone), record })
                .Where(x => dateRange.Includes(x.zoned, Clusivity.Inclusive))
                .GroupBy(x => x.zoned.Date)
                .Select(x => new CyclingItem(x.Key, x.Sum(c => c.record.Distance)))
                .OrderBy(x => x.Date);
        }

        private class DistanceCycling
        {
            public Instant StartDate { get; private set; }
            public Length Distance { get; private set; }
            public static DistanceCycling FromRecord(Record record)
            {
                return new DistanceCycling
                {
                    StartDate = record.StartDate,
                    Distance = RecordParser.Distance(record)
                };
            }
        }

        public class CyclingItem : DatedItem
        {
            public CyclingItem(LocalDate date, Length distance) : base(date)
            {
                Distance = distance;
            }

            public CyclingItem(int year, int month, Length distance) : base(year, month)
            {
                Distance = distance;
            }

            public Length Distance { get; }
        }
    }
}
