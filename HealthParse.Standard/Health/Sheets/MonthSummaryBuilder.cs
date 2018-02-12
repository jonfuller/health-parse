using System;
using System.Collections.Generic;
using System.Linq;
using NodaTime;

namespace HealthParse.Standard.Health.Sheets
{
    public class MonthSummaryBuilder : ISheetBuilder
    {
        private readonly int _targetYear;
        private readonly int _targetMonth;
        private readonly DateTimeZone _zone;
        private readonly ISheetBuilder<StepBuilder.StepItem> _stepBuilder;
        private readonly ISheetBuilder<WorkoutBuilder.WorkoutItem> _cyclingBuilder;
        private readonly ISheetBuilder<WorkoutBuilder.WorkoutItem> _runningBuilder;
        private readonly ISheetBuilder<WorkoutBuilder.WorkoutItem> _walkingBuilder;
        private readonly ISheetBuilder<WorkoutBuilder.WorkoutItem> _strengthBuilder;
        private readonly ISheetBuilder<WorkoutBuilder.WorkoutItem> _hiitBuilder;
        private readonly ISheetBuilder<DistanceCyclingBuilder.CyclingItem> _distanceCyclingBuilder;
        private readonly ISheetBuilder<MassBuilder.MassItem> _massBuilder;
        private readonly ISheetBuilder<BodyFatPercentageBuilder.BodyFatItem> _bodyFatBuilder;

        public MonthSummaryBuilder(int targetYear, int targetMonth, DateTimeZone zone,
            ISheetBuilder<StepBuilder.StepItem> stepBuilder,
            ISheetBuilder<WorkoutBuilder.WorkoutItem> cyclingBuilder,
            ISheetBuilder<WorkoutBuilder.WorkoutItem> runningBuilder,
            ISheetBuilder<WorkoutBuilder.WorkoutItem> walkingBuilder,
            ISheetBuilder<WorkoutBuilder.WorkoutItem> strengthBuilder,
            ISheetBuilder<WorkoutBuilder.WorkoutItem> hiitBuilder,
            ISheetBuilder<DistanceCyclingBuilder.CyclingItem> distanceCyclingBuilder,
            ISheetBuilder<MassBuilder.MassItem> massBuilder,
            ISheetBuilder<BodyFatPercentageBuilder.BodyFatItem> bodyFatBuilder)
        {
            _targetYear = targetYear;
            _targetMonth = targetMonth;
            _zone = zone;

            _stepBuilder = stepBuilder;
            _cyclingBuilder = cyclingBuilder;
            _runningBuilder = runningBuilder;
            _walkingBuilder = walkingBuilder;
            _strengthBuilder = strengthBuilder;
            _hiitBuilder = hiitBuilder;
            _distanceCyclingBuilder = distanceCyclingBuilder;
            _massBuilder = massBuilder;
            _bodyFatBuilder = bodyFatBuilder;
        }

        IEnumerable<object> ISheetBuilder.BuildRawSheet()
        {
            var monthDays = Enumerable.Range(1, DateTime.DaysInMonth(_targetYear, _targetMonth))
                .Select(d => new LocalDate(_targetYear, _targetMonth, d))
                .ToList();

            var range = new DateRange(monthDays.First().AtStartOfDayInZone(_zone), monthDays.Last().AtStartOfDayInZone(_zone));

            var stepsData = _stepBuilder.BuildSummaryForDateRange(range);
            var cyclingWorkouts = _cyclingBuilder.BuildSummaryForDateRange(range);
            var cyclingDistances = _distanceCyclingBuilder.BuildSummaryForDateRange(range);
            var stregthTrainings = _strengthBuilder.BuildSummaryForDateRange(range);
            var hiits = _hiitBuilder.BuildSummaryForDateRange(range);
            var runnings = _runningBuilder.BuildSummaryForDateRange(range);
            var walkings = _walkingBuilder.BuildSummaryForDateRange(range);
            var masses = _massBuilder.BuildSummaryForDateRange(range);
            var bodyFats = _bodyFatBuilder.BuildSummaryForDateRange(range);

            var data = from day in monthDays
                join step in stepsData on day equals step.Date into tmpSteps
                join wCycling in cyclingWorkouts on day equals wCycling.Date into tmpWCycling
                join rCycling in cyclingDistances on day equals rCycling.Date into tmpRCycling
                join strength in stregthTrainings on day equals strength.Date into tmpStrength
                join hiit in hiits on day equals hiit.Date into tmpHiit
                join running in runnings on day equals running.Date into tmpRunning
                join walking in walkings on day equals walking.Date into tmpWalking
                join mass in masses on day equals mass.Date into tmpMasses
                join bodyFat in bodyFats on day equals bodyFat.Date into tmpBodyFats
                from step in tmpSteps.DefaultIfEmpty()
                from wCycling in tmpWCycling.DefaultIfEmpty()
                from rCycling in tmpRCycling.DefaultIfEmpty()
                from strength in tmpStrength.DefaultIfEmpty()
                from hiit in tmpHiit.DefaultIfEmpty()
                from running in tmpRunning.DefaultIfEmpty()
                from walking in tmpWalking.DefaultIfEmpty()
                from mass in tmpMasses.DefaultIfEmpty()
                from bodyFat in tmpBodyFats.DefaultIfEmpty()
                orderby day descending
                select new
                {
                    day = day.ToDateTimeUnspecified(),
                    step?.Steps,
                    mass?.Mass,
                    bodyFat?.BodyFatPercentage,
                    cyclingWorkoutDistance = wCycling?.Distance,
                    cyclingWorkoutMinutes = wCycling?.Duration,
                    distanceCyclingDistance = rCycling?.Distance,
                    strengthMinutes = strength?.Duration,
                    hiitMinutes = hiit?.Duration,
                    runningDistance = running?.Distance,
                    runningDuration = running?.Duration,
                    walkingDistance = walking?.Distance,
                    walkingDuration = walking?.Duration,
                };

            return data;
        }

        bool ISheetBuilder.HasHeaders => false;

        IEnumerable<string> ISheetBuilder.Headers => throw new NotImplementedException();
    }
}