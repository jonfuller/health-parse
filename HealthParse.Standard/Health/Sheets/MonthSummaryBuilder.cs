using System;
using System.Collections.Generic;
using System.Linq;
using HealthParse.Standard.Health.Sheets.Records;
using HealthParse.Standard.Health.Sheets.Workouts;
using NodaTime;
using OfficeOpenXml;

namespace HealthParse.Standard.Health.Sheets
{
    public class MonthSummaryBuilder : IRawSheetBuilder
    {
        private readonly int _targetYear;
        private readonly int _targetMonth;
        private readonly DateTimeZone _zone;
        private readonly Settings.Settings _settings;
        private readonly StepBuilder _stepBuilder;
        private readonly StandBuilder _standBuilder;
        private readonly CyclingWorkoutBuilder _cyclingBuilder;
        private readonly PlayWorkoutBuilder _playBuilder;
        private readonly EllipticalWorkoutBuilder _ellipticalBuilder;
        private readonly RunningWorkoutBuilder _runningBuilder;
        private readonly WalkingWorkoutBuilder _walkingBuilder;
        private readonly StrengthTrainingBuilder _strengthBuilder;
        private readonly HiitBuilder _hiitBuilder;
        private readonly DistanceCyclingBuilder _distanceCyclingBuilder;
        private readonly MassBuilder _massBuilder;
        private readonly BodyFatPercentageBuilder _bodyFatBuilder;

        public MonthSummaryBuilder(int targetYear, int targetMonth, DateTimeZone zone, Settings.Settings settings,
            StepBuilder stepBuilder,
            StandBuilder standBuilder,
            CyclingWorkoutBuilder cyclingBuilder,
            PlayWorkoutBuilder playBuilder,
            EllipticalWorkoutBuilder ellipticalBuilder,
            RunningWorkoutBuilder runningBuilder,
            WalkingWorkoutBuilder walkingBuilder,
            StrengthTrainingBuilder strengthBuilder,
            HiitBuilder hiitBuilder,
            DistanceCyclingBuilder distanceCyclingBuilder,
            MassBuilder massBuilder,
            BodyFatPercentageBuilder bodyFatBuilder)
        {
            _targetYear = targetYear;
            _targetMonth = targetMonth;
            _zone = zone;
            _settings = settings;

            _stepBuilder = stepBuilder;
            _standBuilder = standBuilder;
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

        public IEnumerable<object> BuildRawSheet()
        {
            var monthDays = Enumerable.Range(1, DateTime.DaysInMonth(_targetYear, _targetMonth))
                .Select(d => new LocalDate(_targetYear, _targetMonth, d))
                .ToList();

            var range = new DateRange(monthDays.First().AtStartOfDayInZone(_zone), monthDays.Last().AtStartOfDayInZone(_zone));

            var stepsData = _stepBuilder.BuildSummaryForDateRange(range);
            var standHours = _standBuilder.BuildSummaryForDateRange(range);
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
                join stand in standHours on day equals stand.Date into tmpStand
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
                from stand in tmpStand.DefaultIfEmpty()
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
                    stand?.AverageStandHours,
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

        public void Customize(ExcelWorksheet sheet, ExcelWorkbook workbook)
        {
            workbook.Names.Add($"{sheet.Name.Rangify()}_steps", sheet.Cells["B:B"]);
            workbook.Names.Add($"{sheet.Name.Rangify()}_standhours", sheet.Cells["C:C"]);
            workbook.Names.Add($"{sheet.Name.Rangify()}_weight", sheet.Cells["D:D"]);
            workbook.Names.Add($"{sheet.Name.Rangify()}_bodyfatpct", sheet.Cells["E:E"]);
            workbook.Names.Add($"{sheet.Name.Rangify()}_cyclingdistance", sheet.Cells["F:F"]);
            workbook.Names.Add($"{sheet.Name.Rangify()}_cyclingduration", sheet.Cells["G:G"]);
            workbook.Names.Add($"{sheet.Name.Rangify()}_distancecyclingdistance", sheet.Cells["H:H"]);
            workbook.Names.Add($"{sheet.Name.Rangify()}_strengthtrainingduration", sheet.Cells["I:I"]);
            workbook.Names.Add($"{sheet.Name.Rangify()}_hiitduration", sheet.Cells["J:J"]);
            workbook.Names.Add($"{sheet.Name.Rangify()}_runningdistance", sheet.Cells["K:K"]);
            workbook.Names.Add($"{sheet.Name.Rangify()}_runningduration", sheet.Cells["L:L"]);
            workbook.Names.Add($"{sheet.Name.Rangify()}_walkingdistance", sheet.Cells["M:M"]);
            workbook.Names.Add($"{sheet.Name.Rangify()}_walkingduration", sheet.Cells["N:N"]);
            workbook.Names.Add($"{sheet.Name.Rangify()}_playduration", sheet.Cells["O:O"]);
            workbook.Names.Add($"{sheet.Name.Rangify()}_ellipticalduration", sheet.Cells["P:P"]);
        }

        public IEnumerable<string> Headers => new []
        {
            ColumnNames.Date(),
            ColumnNames.Steps(),
            ColumnNames.StandHours(),
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