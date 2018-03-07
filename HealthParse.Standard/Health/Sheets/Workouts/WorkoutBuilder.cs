using System.Collections.Generic;
using System.Linq;
using NodaTime;

namespace HealthParse.Standard.Health.Sheets.Workouts
{
    public abstract class WorkoutBuilder : IRawSheetBuilder<unit>, IMonthlySummaryBuilder<LocalDate>, ISummarySheetBuilder<LocalDate>
    {
        private readonly IEnumerable<Workout> _workouts;
        private readonly DateTimeZone _zone;
        private readonly string _workoutColumnName;
        private readonly Settings.Settings _settings;

        protected WorkoutBuilder(IEnumerable<Workout> workouts, string workoutKey, string workoutColumnName, DateTimeZone zone, Settings.Settings settings)
        {
            _workouts = workouts.Where(w => w.WorkoutType == workoutKey);
            _workoutColumnName = workoutColumnName;
            _settings = settings;
            _zone = zone;
        }

        public Dataset<unit> BuildRawSheet()
        {
            var columns = _workouts
                .OrderByDescending(r => r.StartDate)
                .Aggregate(new
                    {
                        date = new Column<unit> { Header = ColumnNames.Date() },
                        distance = new Column<unit> { Header = ColumnNames.Distance(_settings.DistanceUnit) },
                        energy = new Column<unit> { Header = ColumnNames.EnergyBurned(_settings.EnergyUnit) },
                        duration = new Column<unit> { Header = ColumnNames.Duration(_settings.DurationUnit) },
                    },
                    (cols, r) =>
                    {
                        cols.date.Add(unit.v, r.StartDate.InZone(_zone));
                        cols.distance.Add(unit.v, r.Distance.As(_settings.DistanceUnit));
                        cols.energy.Add(unit.v, r.Energy.As(_settings.EnergyUnit));
                        cols.duration.Add(unit.v, r.Duration.As(_settings.DurationUnit));

                        return cols;
                    });

            return new Dataset<unit>(columns.date, columns.distance, columns.energy, columns.duration);
        }

        public IEnumerable<Column<LocalDate>> BuildSummary()
        {
            var columns = _workouts
                .GroupBy(r => new { r.StartDate.InZone(_zone).Date.Year, r.StartDate.InZone(_zone).Date.Month})
                .Aggregate(new
                    {
                        distance = new Column<LocalDate> { Header = $"{_workoutColumnName} - {ColumnNames.Distance(_settings.DistanceUnit)}", RangeName = $"total_{_workoutColumnName}_distance"},
                        energy = new Column<LocalDate> { Header = $"{_workoutColumnName} - {ColumnNames.EnergyBurned(_settings.EnergyUnit)}", RangeName = $"total_{_workoutColumnName}_energy_burned"},
                        duration = new Column<LocalDate> { Header = $"{_workoutColumnName} - {ColumnNames.Duration(_settings.DurationUnit)}", RangeName = $"total_{_workoutColumnName}_duration"},
                    },
                    (cols, r) =>
                    {
                        var date = new LocalDate(r.Key.Year, r.Key.Month, 1);

                        cols.distance.Add(date, r.Sum(c => c.Distance).As(_settings.DistanceUnit));
                        cols.energy.Add(date, r.Sum(c => c.Energy).As(_settings.EnergyUnit));
                        cols.duration.Add(date, r.Sum(c => c.Duration).As(_settings.DurationUnit));

                        return cols;
                    });

            yield return columns.distance;
            yield return columns.energy;
            yield return columns.duration;
        }

        public IEnumerable<Column<LocalDate>> BuildSummaryForDateRange(IRange<ZonedDateTime> dateRange)
        {
            var columns = _workouts
                .Where(x => dateRange.Includes(x.StartDate.InZone(_zone), Clusivity.Inclusive))
                .GroupBy(r => r.StartDate.InZone(_zone).Date)
                .Aggregate(new
                    {
                        distance = new Column<LocalDate> { Header = $"{_workoutColumnName} - {ColumnNames.Distance(_settings.DistanceUnit)}", RangeName = $"{_workoutColumnName}_distance"},
                        energy = new Column<LocalDate> { Header = $"{_workoutColumnName} - {ColumnNames.EnergyBurned(_settings.EnergyUnit)}", RangeName = $"{_workoutColumnName}_energy_burned"},
                        duration = new Column<LocalDate> { Header = $"{_workoutColumnName} - {ColumnNames.Duration(_settings.DurationUnit)}", RangeName = $"{_workoutColumnName}_duration"},
                    },
                    (cols, r) =>
                    {
                        cols.distance.Add(r.Key, r.Sum(c => c.Distance).As(_settings.DistanceUnit));
                        cols.energy.Add(r.Key, r.Sum(c => c.Energy).As(_settings.EnergyUnit));
                        cols.duration.Add(r.Key, r.Sum(c => c.Duration).As(_settings.DurationUnit));

                        return cols;
                    });

            yield return columns.distance;
            yield return columns.energy;
            yield return columns.duration;
        }
    }
}
