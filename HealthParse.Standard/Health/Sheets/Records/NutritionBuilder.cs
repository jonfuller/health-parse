using System;
using System.Collections.Generic;
using System.Linq;
using NodaTime;
using UnitsNet;

namespace HealthParse.Standard.Health.Sheets.Records
{
    public class NutritionBuilder : IRawSheetBuilder<LocalDate>, ISummarySheetBuilder<LocalDate>, IMonthlySummaryBuilder<LocalDate>
    {
        private readonly DateTimeZone _zone;
        private readonly Settings.Settings _settings;
        private readonly List<Tuple<LocalDate, double>> _fat;
        private readonly List<Tuple<LocalDate, double>> _energyConsumed;
        private readonly List<Tuple<LocalDate, double>> _carbs;
        private readonly List<Tuple<LocalDate, double>> _protein;

        public NutritionBuilder(IEnumerable<Record> records, DateTimeZone zone, Settings.Settings settings)
        {
            _zone = zone;
            _settings = settings;

            var categorized = records.Aggregate(new
            {
                energyConsumed = new List<Record>(),
                fat = new List<Record>(),
                carbs = new List<Record>(),
                protein = new List<Record>(),
            }, (accum, record) =>
            {
                if (record.Type == HKConstants.Records.Nutrition.EnergyConsumed)
                    accum.energyConsumed.Add(record);
                if (record.Type == HKConstants.Records.Nutrition.Fat)
                    accum.fat.Add(record);
                if (record.Type == HKConstants.Records.Nutrition.Carbs)
                    accum.carbs.Add(record);
                if (record.Type == HKConstants.Records.Nutrition.Protein)
                    accum.protein.Add(record);
                return accum;
            });

            _energyConsumed = categorized.energyConsumed
                .GroupBy(r => r.StartDate.InZone(_zone).Date)
                .Select(r => Tuple.Create(r.Key, r
                    .Select(c => Energy.From(c.Value.SafeParse(0), Energy.ParseUnit(c.Unit)))
                    .Sum(c => c)
                    .As(settings.EnergyUnit)))
                .ToList();

            _fat = categorized.fat
                .GroupBy(r => r.StartDate.InZone(_zone).Date)
                .Select(r => Tuple.Create(r.Key, r
                    .Select(c => Mass.From(c.Value.SafeParse(0), Mass.ParseUnit(c.Unit)))
                    .Sum(c => c)
                    .Grams))
                .ToList();

            _carbs = categorized.carbs
                .GroupBy(r => r.StartDate.InZone(_zone).Date)
                .Select(r => Tuple.Create(r.Key, r
                    .Select(c => Mass.From(c.Value.SafeParse(0), Mass.ParseUnit(c.Unit)))
                    .Sum(c => c)
                    .Grams))
                .ToList();

            _protein = categorized.protein
                .GroupBy(r => r.StartDate.InZone(_zone).Date)
                .Select(r => Tuple.Create(r.Key, r
                    .Select(c => Mass.From(c.Value.SafeParse(0), Mass.ParseUnit(c.Unit)))
                    .Sum(c => c)
                    .Grams))
                .ToList();

        }
        public Dataset<LocalDate> BuildRawSheet()
        {
            var dates = _energyConsumed.Select(s => s.Item1)
                .Concat(_fat.Select(s => s.Item1))
                .Concat(_carbs.Select(s => s.Item1))
                .Concat(_protein.Select(s => s.Item1))
                .Distinct();

            return new Dataset<LocalDate>(
                new KeyColumn<LocalDate>(dates),
                _energyConsumed.MakeColumn(ColumnNames.Nutrition.EnergyConsumed(_settings.EnergyUnit)),
                _fat.MakeColumn(ColumnNames.Nutrition.Fat()),
                _carbs.MakeColumn(ColumnNames.Nutrition.Carbs()),
                _protein.MakeColumn(ColumnNames.Nutrition.Protein()));
        }

        public IEnumerable<Column<LocalDate>> BuildSummary()
        {
            yield return _energyConsumed
                .GroupBy(s => new { s.Item1.Year, s.Item1.Month })
                .Select(r => Tuple.Create(new LocalDate(r.Key.Year, r.Key.Month, 1), r.Average(c => c.Item2)))
                .MakeColumn(ColumnNames.Nutrition.AverageConsumed(_settings.EnergyUnit), "avg_consumed");
            yield return _fat
                .GroupBy(s => new { s.Item1.Year, s.Item1.Month })
                .Select(r => Tuple.Create(new LocalDate(r.Key.Year, r.Key.Month, 1), r.Average(c => c.Item2)))
                .MakeColumn(ColumnNames.Nutrition.AverageFat(), "avg_fat");
            yield return _carbs
                .GroupBy(s => new { s.Item1.Year, s.Item1.Month })
                .Select(r => Tuple.Create(new LocalDate(r.Key.Year, r.Key.Month, 1), r.Average(c => c.Item2)))
                .MakeColumn(ColumnNames.Nutrition.AverageCarbs(), "avg_carbs");
            yield return _protein
                .GroupBy(s => new { s.Item1.Year, s.Item1.Month })
                .Select(r => Tuple.Create(new LocalDate(r.Key.Year, r.Key.Month, 1), r.Average(c => c.Item2)))
                .MakeColumn(ColumnNames.Nutrition.AverageProtein(), "avg_protein");
        }

        public IEnumerable<Column<LocalDate>> BuildSummaryForDateRange(IRange<ZonedDateTime> dateRange)
        {
            yield return _energyConsumed
                .Where(r => dateRange.Includes(r.Item1.AtStartOfDayInZone(_zone), Clusivity.Inclusive))
                .MakeColumn(ColumnNames.Nutrition.EnergyConsumed(_settings.EnergyUnit), "consumed");
            yield return _fat
                .Where(r => dateRange.Includes(r.Item1.AtStartOfDayInZone(_zone), Clusivity.Inclusive))
                .MakeColumn(ColumnNames.Nutrition.Fat(), "fat");
            yield return _carbs
                .Where(r => dateRange.Includes(r.Item1.AtStartOfDayInZone(_zone), Clusivity.Inclusive))
                .MakeColumn(ColumnNames.Nutrition.Fat(), "carbs");
            yield return _protein
                .Where(r => dateRange.Includes(r.Item1.AtStartOfDayInZone(_zone), Clusivity.Inclusive))
                .MakeColumn(ColumnNames.Nutrition.Fat(), "protein");
        }
    }
}