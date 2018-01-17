using OfficeOpenXml;
using System.Collections.Generic;
using System.Linq;

namespace HealthParse.Standard.Health.Sheets
{
    public class SummaryBuilder : ISheetBuilder
    {
        private Dictionary<string, IEnumerable<Record>> _records;
        private Dictionary<string, IEnumerable<Workout>> _workouts;
        private readonly ISheetBuilder<StepBuilder.MonthlyStep> _stepBuilder;
        private readonly ISheetBuilder<WorkoutBuilder.MonthlyWorkout> _cyclingBuilder;
        private readonly ISheetBuilder<WorkoutBuilder.MonthlyWorkout> _runningBuilder;
        private readonly ISheetBuilder<WorkoutBuilder.MonthlyWorkout> _walkingBuilder;
        private readonly ISheetBuilder<WorkoutBuilder.MonthlyWorkout> _strengthBuilder;
        private readonly ISheetBuilder<DistanceCyclingBuilder.MonthlyCycling> _distanceCyclingBuilder;

        public SummaryBuilder(Dictionary<string, IEnumerable<Record>> records, Dictionary<string, IEnumerable<Workout>> workouts,
            ISheetBuilder<StepBuilder.MonthlyStep> stepBuilder,
            ISheetBuilder<WorkoutBuilder.MonthlyWorkout> cyclingBuilder,
            ISheetBuilder<WorkoutBuilder.MonthlyWorkout> runningBuilder,
            ISheetBuilder<WorkoutBuilder.MonthlyWorkout> walkingBuilder,
            ISheetBuilder<WorkoutBuilder.MonthlyWorkout> strengthBuilder,
            ISheetBuilder<DistanceCyclingBuilder.MonthlyCycling> distanceCyclingBuilder
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
                .Select(m => new MonthlyItem(m.Year, m.Month).Date);

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
