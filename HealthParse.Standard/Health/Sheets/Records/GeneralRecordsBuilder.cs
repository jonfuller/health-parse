using System;
using System.Collections.Generic;
using System.Linq;
using NodaTime;
using UnitsNet;

namespace HealthParse.Standard.Health.Sheets.Records
{
    public class GeneralRecordsBuilder : IRawSheetBuilder<LocalDate>, ISummarySheetBuilder<(int Year, int Month)>, IMonthlySummaryBuilder<LocalDate>
    {
        private readonly DateTimeZone _zone;
        private readonly Settings.Settings _settings;
        private readonly List<Tuple<LocalDate, int>> _standing;
        private readonly List<Tuple<LocalDate, int>> _flightsClimbed;
        private readonly List<Tuple<LocalDate, double>> _exerciseTime;
        private readonly List<Tuple<LocalDate, double>> _basalEnergy;
        private readonly List<Tuple<LocalDate, double>> _activeEnergy;

        public GeneralRecordsBuilder(IEnumerable<Record> records, DateTimeZone zone, Settings.Settings settings)
        {
            _zone = zone;
            _settings = settings;

            var categorized = records.Aggregate(new
            {
                standing = new List<Record>(),
                exercise = new List<Record>(),
                flights = new List<Record>(),
                basalEnergy = new List<Record>(),
                activeEnergy = new List<Record>(),
            }, (accum, record) =>
            {
                if (record.Type == HKConstants.Records.Standing.StandType)
                    accum.standing.Add(record);
                if (record.Type == HKConstants.Records.ExerciseTime)
                    accum.exercise.Add(record);
                if (record.Type == HKConstants.Records.FlightsClimbed)
                    accum.flights.Add(record);
                if (record.Type == HKConstants.Records.BasalEnergyBurned)
                    accum.basalEnergy.Add(record);
                if (record.Type == HKConstants.Records.ActiveEnergyBurned)
                    accum.activeEnergy.Add(record);

                return accum;
            });

            _standing =  categorized.standing
                .Where(r => r.Value == HKConstants.Records.Standing.Stood)
                .GroupBy(r => r.StartDate.InZone(_zone).Date)
                .Select(r => Tuple.Create(r.Key, r.Count()))
                .ToList();

            _exerciseTime = categorized.exercise
                .GroupBy(r => r.StartDate.InZone(_zone).Date)
                .Select(r => Tuple.Create(r.Key, new UnitsNet.Duration(r.Sum(c => (c.EndDate - c.StartDate).TotalSeconds)).As(settings.DurationUnit)))
                .ToList();

            _flightsClimbed = categorized.flights
                .GroupBy(r => r.StartDate.InZone(_zone).Date)
                .Select(r => Tuple.Create(r.Key, r.Sum(c => (int)c.Value.SafeParse(0))))
                .ToList();

            _basalEnergy = categorized.basalEnergy
                .GroupBy(r => r.StartDate.InZone(_zone).Date)
                .Select(r => Tuple.Create(r.Key, r
                    .Select(c => Energy.From(c.Value.SafeParse(0), Energy.ParseUnit(c.Unit)))
                    .Sum(c => c)
                    .As(settings.EnergyUnit)))
                .ToList();

            _activeEnergy = categorized.activeEnergy
                .GroupBy(r => r.StartDate.InZone(_zone).Date)
                .Select(r => Tuple.Create(r.Key, r
                    .Select(c => Energy.From(c.Value.SafeParse(0), Energy.ParseUnit(c.Unit)))
                    .Sum(c => c)
                    .As(settings.EnergyUnit)))
                .ToList();
        }

        public Dataset<LocalDate> BuildRawSheet()
        {
            var dates = _standing.Select(s => s.Item1)
                .Concat(_exerciseTime.Select(s => s.Item1))
                .Concat(_flightsClimbed.Select(s => s.Item1))
                .Distinct();

            return new Dataset<LocalDate>(
                new KeyColumn<LocalDate>(dates),
                _standing.MakeColumn(ColumnNames.StandHours()),
                _flightsClimbed.MakeColumn(ColumnNames.FlightsClimbed()),
                _exerciseTime.MakeColumn(ColumnNames.ExerciseDuration(_settings.DurationUnit)),
                _basalEnergy.MakeColumn(ColumnNames.BasalEnergy(_settings.EnergyUnit)),
                _activeEnergy.MakeColumn(ColumnNames.ActiveEnergy(_settings.EnergyUnit)));
        }

        public IEnumerable<Column<(int Year, int Month)>> BuildSummary()
        {
            yield return _standing
                .GroupBy(s => (s.Item1.Year, s.Item1.Month))
                .Select(r => Tuple.Create(r.Key, r.Average(c => c.Item2)))
                .MakeColumn(ColumnNames.AverageStandHours(), "avg_stand_hours");

            yield return _flightsClimbed
                .GroupBy(s => (s.Item1.Year, s.Item1.Month))
                .Select(r => Tuple.Create(r.Key, r.Sum(c => c.Item2)))
                .MakeColumn(ColumnNames.TotalFlightsClimbed(), "total_flights");

            yield return _exerciseTime
                .GroupBy(s => (s.Item1.Year, s.Item1.Month))
                .Select(r => Tuple.Create(r.Key, r.Sum(c => c.Item2)))
                .MakeColumn(ColumnNames.TotalExerciseDuration(_settings.DurationUnit), "total_exercise");

            yield return _basalEnergy
                .GroupBy(s => (s.Item1.Year, s.Item1.Month))
                .Select(r => Tuple.Create(r.Key, r.Average(c => c.Item2)))
                .MakeColumn(ColumnNames.AverageBasalEnergy(_settings.EnergyUnit), "avg_basal_energy");

            yield return _activeEnergy
                .GroupBy(s => (s.Item1.Year, s.Item1.Month))
                .Select(r => Tuple.Create(r.Key, r.Average(c => c.Item2)))
                .MakeColumn(ColumnNames.AverageActiveEnergy(_settings.EnergyUnit), "avg_active_energy");
        }

        public IEnumerable<Column<LocalDate>> BuildSummaryForDateRange(IRange<ZonedDateTime> dateRange)
        {
            yield return _standing
                .Where(r => dateRange.Includes(r.Item1.AtStartOfDayInZone(_zone), Clusivity.Inclusive))
                .MakeColumn(ColumnNames.StandHours(), "stand_hours");

            yield return _flightsClimbed
                .Where(r => dateRange.Includes(r.Item1.AtStartOfDayInZone(_zone), Clusivity.Inclusive))
                .MakeColumn(ColumnNames.FlightsClimbed(), "flights");

            yield return _exerciseTime
                .Where(r => dateRange.Includes(r.Item1.AtStartOfDayInZone(_zone), Clusivity.Inclusive))
                .MakeColumn(ColumnNames.ExerciseDuration(_settings.DurationUnit), "exercise_time");

            yield return _basalEnergy
                .Where(r => dateRange.Includes(r.Item1.AtStartOfDayInZone(_zone), Clusivity.Inclusive))
                .MakeColumn(ColumnNames.BasalEnergy(_settings.EnergyUnit), "basal_energy");

            yield return _activeEnergy
                .Where(r => dateRange.Includes(r.Item1.AtStartOfDayInZone(_zone), Clusivity.Inclusive))
                .MakeColumn(ColumnNames.ActiveEnergy(_settings.EnergyUnit), "active_energy");
        }
    }
}