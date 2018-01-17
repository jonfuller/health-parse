using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthParse.Standard.Health.Sheets
{
    public class MonthSummaryBuilder : ISheetBuilder
    {
        private Dictionary<string, IEnumerable<Record>> _records;
        private Dictionary<string, IEnumerable<Workout>> _workouts;
        private readonly int _targetYear;
        private readonly int _targetMonth;
        private readonly ISheetBuilder<StepBuilder.StepItem> _stepBuilder;
        private readonly ISheetBuilder<WorkoutBuilder.WorkoutItem> _cyclingBuilder;
        private readonly ISheetBuilder<WorkoutBuilder.WorkoutItem> _runningBuilder;
        private readonly ISheetBuilder<WorkoutBuilder.WorkoutItem> _walkingBuilder;
        private readonly ISheetBuilder<WorkoutBuilder.WorkoutItem> _strengthBuilder;
        private readonly ISheetBuilder<DistanceCyclingBuilder.CyclingItem> _distanceCyclingBuilder;

        public MonthSummaryBuilder(Dictionary<string, IEnumerable<Record>> records, Dictionary<string, IEnumerable<Workout>> workouts,
            int targetYear, int targetMonth,
            ISheetBuilder<StepBuilder.StepItem> stepBuilder,
            ISheetBuilder<WorkoutBuilder.WorkoutItem> cyclingBuilder,
            ISheetBuilder<WorkoutBuilder.WorkoutItem> runningBuilder,
            ISheetBuilder<WorkoutBuilder.WorkoutItem> walkingBuilder,
            ISheetBuilder<WorkoutBuilder.WorkoutItem> strengthBuilder,
            ISheetBuilder<DistanceCyclingBuilder.CyclingItem> distanceCyclingBuilder
            )
        {
            _records = records;
            _workouts = workouts;

            _targetYear = targetYear;
            _targetMonth = targetMonth;

            _stepBuilder = stepBuilder;
            _cyclingBuilder = cyclingBuilder;
            _runningBuilder = runningBuilder;
            _walkingBuilder = walkingBuilder;
            _strengthBuilder = strengthBuilder;
            _distanceCyclingBuilder = distanceCyclingBuilder;
        }

        void ISheetBuilder.Build(ExcelWorksheet sheet)
        {
            var monthDays = Enumerable.Range(1, DateTime.DaysInMonth(_targetYear, _targetMonth))
                .Select(d => new DateTime(_targetYear, _targetMonth, d))
                .ToList();

            var range = new DateRange(monthDays.First(), monthDays.Last());

            var stepsData = _stepBuilder.BuildSummaryForDateRange(range);
            var cyclingWorkouts = _cyclingBuilder.BuildSummaryForDateRange(range);
            var cyclingDistances = _distanceCyclingBuilder.BuildSummaryForDateRange(range);
            var stregthTrainings = _strengthBuilder.BuildSummaryForDateRange(range);
            var runnings = _runningBuilder.BuildSummaryForDateRange(range);
            var walkings = _walkingBuilder.BuildSummaryForDateRange(range);

            var data = from day in monthDays
                join step in stepsData on day equals step.Date into tmpSteps
                join wCycling in cyclingWorkouts on day equals wCycling.Date into tmpWCycling
                join rCycling in cyclingDistances on day equals rCycling.Date into tmpRCycling
                join strength in stregthTrainings on day equals strength.Date into tmpStrength
                join running in runnings on day equals running.Date into tmpRunning
                join walking in walkings on day equals walking.Date into tmpWalking
                from step in tmpSteps.DefaultIfEmpty()
                from wCycling in tmpWCycling.DefaultIfEmpty()
                from rCycling in tmpRCycling.DefaultIfEmpty()
                from strength in tmpStrength.DefaultIfEmpty()
                from running in tmpRunning.DefaultIfEmpty()
                from walking in tmpWalking.DefaultIfEmpty()
                orderby day descending
                select new
                {
                    day,
                    step?.Steps,
                    cyclingWorkoutDistance = wCycling?.Distance,
                    cyclingWorkoutMinutes = wCycling?.Duration,
                    distanceCyclingDistance = rCycling?.Distance,
                    strengthMinutes = strength?.Duration,
                    runningDistance = running?.Distance,
                    runningDuration = running?.Duration,
                    walkingDistance = walking?.Distance,
                    walkingDuration = walking?.Duration,
                };

            sheet.WriteData(data);
        }
    }

    public class SummaryBuilder : ISheetBuilder
    {
        private Dictionary<string, IEnumerable<Record>> _records;
        private Dictionary<string, IEnumerable<Workout>> _workouts;
        private readonly ISheetBuilder<StepBuilder.StepItem> _stepBuilder;
        private readonly ISheetBuilder<WorkoutBuilder.WorkoutItem> _cyclingBuilder;
        private readonly ISheetBuilder<WorkoutBuilder.WorkoutItem> _runningBuilder;
        private readonly ISheetBuilder<WorkoutBuilder.WorkoutItem> _walkingBuilder;
        private readonly ISheetBuilder<WorkoutBuilder.WorkoutItem> _strengthBuilder;
        private readonly ISheetBuilder<DistanceCyclingBuilder.CyclingItem> _distanceCyclingBuilder;

        public SummaryBuilder(Dictionary<string, IEnumerable<Record>> records, Dictionary<string, IEnumerable<Workout>> workouts,
            ISheetBuilder<StepBuilder.StepItem> stepBuilder,
            ISheetBuilder<WorkoutBuilder.WorkoutItem> cyclingBuilder,
            ISheetBuilder<WorkoutBuilder.WorkoutItem> runningBuilder,
            ISheetBuilder<WorkoutBuilder.WorkoutItem> walkingBuilder,
            ISheetBuilder<WorkoutBuilder.WorkoutItem> strengthBuilder,
            ISheetBuilder<DistanceCyclingBuilder.CyclingItem> distanceCyclingBuilder
            )
        {
            _records = records;
            _workouts = workouts;

            _stepBuilder = stepBuilder;
            _cyclingBuilder = cyclingBuilder;
            _runningBuilder = runningBuilder;
            _walkingBuilder = walkingBuilder;
            _strengthBuilder = strengthBuilder;
            _distanceCyclingBuilder = distanceCyclingBuilder;
        }

        void ISheetBuilder.Build(ExcelWorksheet sheet)
        {
            var recordMonths = _records.Values
                .SelectMany(r => r)
                .GroupBy(s => new { s.StartDate.Date.Year, s.StartDate.Date.Month })
                .Select(g => g.Key);

            var workoutMonths = _workouts.Values
                .SelectMany(r => r)
                .GroupBy(s => new { s.StartDate.Date.Year, s.StartDate.Date.Month })
                .Select(g => g.Key);

            var healthMonths = recordMonths.Concat(workoutMonths)
                .Distinct()
                .Select(m => new DatedItem(m.Year, m.Month).Date);

            var stepsByMonth = _stepBuilder.BuildSummary();
            var cyclingWorkouts = _cyclingBuilder.BuildSummary();
            var cyclingDistances = _distanceCyclingBuilder.BuildSummary();
            var stregthTrainings = _strengthBuilder.BuildSummary();
            var runnings = _runningBuilder.BuildSummary();
            var walkings = _walkingBuilder.BuildSummary();

            var dataByMonth = from month in healthMonths
                      join steps in stepsByMonth on month equals steps.Date into tmpSteps
                      join wCycling in cyclingWorkouts on month equals wCycling.Date into tmpWCycling
                      join rCycling in cyclingDistances on month equals rCycling.Date into tmpRCycling
                      join strength in stregthTrainings on month equals strength.Date into tmpStrength
                      join running in runnings on month equals running.Date into tmpRunning
                      join walking in walkings on month equals walking.Date into tmpWalking
                      from steps in tmpSteps.DefaultIfEmpty()
                      from wCycling in tmpWCycling.DefaultIfEmpty()
                      from rCycling in tmpRCycling.DefaultIfEmpty()
                      from strength in tmpStrength.DefaultIfEmpty()
                      from running in tmpRunning.DefaultIfEmpty()
                      from walking in tmpWalking.DefaultIfEmpty()
                      orderby month descending
                      select new
                      {
                          month,
                          steps?.Steps,
                          cyclingWorkoutDistance = wCycling?.Distance,
                          cyclingWorkoutMinutes = wCycling?.Duration,
                          distanceCyclingDistance = rCycling?.Distance,
                          strengthMinutes = strength?.Duration,
                          runningDistance = running?.Distance,
                          runningDuration = running?.Duration,
                          walkingDistance = walking?.Distance,
                          walkingDuration = walking?.Duration,
                      };

            sheet.WriteData(dataByMonth);
        }
    }
}
