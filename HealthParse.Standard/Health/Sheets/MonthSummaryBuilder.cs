using System;
using System.Collections.Generic;
using System.Linq;
using NodaTime;
using OfficeOpenXml;

namespace HealthParse.Standard.Health.Sheets
{
    public class MonthSummaryBuilder : ISheetBuilder
    {
        private readonly int _targetYear;
        private readonly int _targetMonth;
        private readonly DateTimeZone _zone;
        private readonly Settings.Settings _settings;
        private readonly ISheetBuilder<StepBuilder.StepItem> _stepBuilder;
        private readonly ISheetBuilder<WorkoutBuilder.WorkoutItem> _cyclingBuilder;
        private readonly ISheetBuilder<WorkoutBuilder.WorkoutItem> _playBuilder;
        private readonly ISheetBuilder<WorkoutBuilder.WorkoutItem> _ellipticalBuilder;
        private readonly ISheetBuilder<WorkoutBuilder.WorkoutItem> _runningBuilder;
        private readonly ISheetBuilder<WorkoutBuilder.WorkoutItem> _walkingBuilder;
        private readonly ISheetBuilder<WorkoutBuilder.WorkoutItem> _strengthBuilder;
        private readonly ISheetBuilder<WorkoutBuilder.WorkoutItem> _hiitBuilder;
        private readonly ISheetBuilder<DistanceCyclingBuilder.CyclingItem> _distanceCyclingBuilder;
        private readonly ISheetBuilder<MassBuilder.MassItem> _massBuilder;
        private readonly ISheetBuilder<BodyFatPercentageBuilder.BodyFatItem> _bodyFatBuilder;

        public MonthSummaryBuilder(int targetYear, int targetMonth, DateTimeZone zone, Settings.Settings settings,
            ISheetBuilder<StepBuilder.StepItem> stepBuilder,
            ISheetBuilder<WorkoutBuilder.WorkoutItem> cyclingBuilder,
            ISheetBuilder<WorkoutBuilder.WorkoutItem> playBuilder,
            ISheetBuilder<WorkoutBuilder.WorkoutItem> ellipticalBuilder,
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
            _settings = settings;

            _stepBuilder = stepBuilder;
            _cyclingBuilder = cyclingBuilder;
            _playBuilder = playBuilder;
            _ellipticalBuilder = ellipticalBuilder;
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
            var playWorkouts = _playBuilder.BuildSummaryForDateRange(range);
            var ellipticalWorkouts = _ellipticalBuilder.BuildSummaryForDateRange(range);
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
                join play in playWorkouts on day equals play.Date into tmpPlay
                join elliptical in ellipticalWorkouts on day equals elliptical.Date into tmpElliptical
                join rCycling in cyclingDistances on day equals rCycling.Date into tmpRCycling
                join strength in stregthTrainings on day equals strength.Date into tmpStrength
                join hiit in hiits on day equals hiit.Date into tmpHiit
                join running in runnings on day equals running.Date into tmpRunning
                join walking in walkings on day equals walking.Date into tmpWalking
                join mass in masses on day equals mass.Date into tmpMasses
                join bodyFat in bodyFats on day equals bodyFat.Date into tmpBodyFats
                from step in tmpSteps.DefaultIfEmpty()
                from wCycling in tmpWCycling.DefaultIfEmpty()
                from play in tmpPlay.DefaultIfEmpty()
                from elliptical in tmpElliptical.DefaultIfEmpty()
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
                    mass = mass?.Mass.As(_settings.WeightUnit),
                    bodyFat?.BodyFatPercentage,
                    cyclingWorkoutDistance = wCycling?.Distance.As(_settings.DistanceUnit),
                    cyclingWorkoutMinutes = wCycling?.Duration.As(_settings.DurationUnit),
                    distanceCyclingDistance = rCycling?.Distance.As(_settings.DistanceUnit),
                    strengthMinutes = strength?.Duration.As(_settings.DurationUnit),
                    hiitMinutes = hiit?.Duration.As(_settings.DurationUnit),
                    runningDistance = running?.Distance.As(_settings.DistanceUnit),
                    runningDuration = running?.Duration.As(_settings.DurationUnit),
                    walkingDistance = walking?.Distance.As(_settings.DistanceUnit),
                    walkingDuration = walking?.Duration.As(_settings.DurationUnit),
                    playDuration = play?.Duration.As(_settings.DurationUnit),
                    elliptical = elliptical?.Duration.As(_settings.DurationUnit),
                };

            return data;
        }

        void ISheetBuilder.Customize(ExcelWorksheet sheet, ExcelWorkbook workbook)
        {
            workbook.Names.Add($"{sheet.Name.Rangify()}_steps", sheet.Cells["B:B"]);
            workbook.Names.Add($"{sheet.Name.Rangify()}_weight", sheet.Cells["C:C"]);
            workbook.Names.Add($"{sheet.Name.Rangify()}_bodyfatpct", sheet.Cells["D:D"]);
            workbook.Names.Add($"{sheet.Name.Rangify()}_cyclingdistance", sheet.Cells["E:E"]);
            workbook.Names.Add($"{sheet.Name.Rangify()}_cyclingduration", sheet.Cells["F:F"]);
            workbook.Names.Add($"{sheet.Name.Rangify()}_distancecyclingdistance", sheet.Cells["G:G"]);
            workbook.Names.Add($"{sheet.Name.Rangify()}_strengthtrainingduration", sheet.Cells["H:H"]);
            workbook.Names.Add($"{sheet.Name.Rangify()}_hittduration", sheet.Cells["I:I"]);
            workbook.Names.Add($"{sheet.Name.Rangify()}_runningdistance", sheet.Cells["J:J"]);
            workbook.Names.Add($"{sheet.Name.Rangify()}_runningduration", sheet.Cells["K:K"]);
            workbook.Names.Add($"{sheet.Name.Rangify()}_walkingdistance", sheet.Cells["L:L"]);
            workbook.Names.Add($"{sheet.Name.Rangify()}_walkingduration", sheet.Cells["M:M"]);
            workbook.Names.Add($"{sheet.Name.Rangify()}_playduration", sheet.Cells["N:N"]);
            workbook.Names.Add($"{sheet.Name.Rangify()}_ellipticalduration", sheet.Cells["O:O"]);
        }

        bool ISheetBuilder.HasHeaders => true;

        IEnumerable<string> ISheetBuilder.Headers => new []
        {
            ColumnNames.Date(),
            ColumnNames.Steps(),
            ColumnNames.Weight(_settings.WeightUnit),
            ColumnNames.BodyFatPercentage(),
            ColumnNames.Workout.Cycling.Distance(_settings.DistanceUnit),
            ColumnNames.Workout.Cycling.Duration(_settings.DurationUnit),
            ColumnNames.CyclingDistance(_settings.DistanceUnit),
            ColumnNames.Workout.StrengthTraining.Duration(_settings.DurationUnit),
            ColumnNames.Workout.Hiit.Duration(_settings.DurationUnit),
            ColumnNames.Workout.Running.Distance(_settings.DistanceUnit),
            ColumnNames.Workout.Running.Duration(_settings.DurationUnit),
            ColumnNames.Workout.Walking.Distance(_settings.DistanceUnit),
            ColumnNames.Workout.Walking.Duration(_settings.DurationUnit),
            ColumnNames.Workout.Play.Duration(_settings.DurationUnit),
            ColumnNames.Workout.Elliptical.Duration(_settings.DurationUnit),
        };
    }
}