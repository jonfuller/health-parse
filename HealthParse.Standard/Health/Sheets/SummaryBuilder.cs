using System.Collections.Generic;
using System.Linq;
using HealthParse.Standard.Health.Sheets.Records;
using HealthParse.Standard.Health.Sheets.Workouts;
using NodaTime;
using OfficeOpenXml;

namespace HealthParse.Standard.Health.Sheets
{
    public class SummaryBuilder : IRawSheetBuilder
    {
        private readonly IEnumerable<Record> _records;
        private readonly IEnumerable<Workout> _workouts;
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

        public SummaryBuilder(IEnumerable<Record> records,
            IEnumerable<Workout> workouts,
            DateTimeZone zone,
            Settings.Settings settings,
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
            _records = records;
            _workouts = workouts;
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
            var recordMonths = _records
                .GroupBy(s => new { s.StartDate.InZone(_zone).Year, s.StartDate.InZone(_zone).Month })
                .Select(g => g.Key);

            var workoutMonths = _workouts
                .GroupBy(s => new { s.StartDate.InZone(_zone).Year, s.StartDate.InZone(_zone).Month })
                .Select(g => g.Key);

            var healthMonths = recordMonths.Concat(workoutMonths)
                .Distinct()
                .Select(m => new DatedItem(m.Year, m.Month).Date);

            var stepsByMonth = _stepBuilder.BuildSummary();
            var standingByMonth = _standBuilder.BuildSummary();
            var cyclingWorkouts = _cyclingBuilder.BuildSummary();
            var playWorkouts = _playBuilder.BuildSummary();
            var ellipticalWorkouts = _ellipticalBuilder.BuildSummary();
            var cyclingDistances = _distanceCyclingBuilder.BuildSummary();
            var stregthTrainings = _strengthBuilder.BuildSummary();
            var hiits = _hiitBuilder.BuildSummary();
            var runnings = _runningBuilder.BuildSummary();
            var walkings = _walkingBuilder.BuildSummary();
            var masses = _massBuilder.BuildSummary();
            var bodyFats = _bodyFatBuilder.BuildSummary();

            var dataByMonth = from month in healthMonths
                      join steps in stepsByMonth on month equals steps.Date into tmpSteps
                      join stand in standingByMonth on month equals stand.Date into tmpStand
                      join wCycling in cyclingWorkouts on month equals wCycling.Date into tmpWCycling
                      join play in playWorkouts on month equals play.Date into tmpPlay
                      join elliptical in ellipticalWorkouts on month equals elliptical.Date into tmpElliptical
                      join rCycling in cyclingDistances on month equals rCycling.Date into tmpRCycling
                      join strength in stregthTrainings on month equals strength.Date into tmpStrength
                      join hiit in hiits on month equals hiit.Date into tmpHiit
                      join running in runnings on month equals running.Date into tmpRunning
                      join walking in walkings on month equals walking.Date into tmpWalking
                      join mass in masses on month equals mass.Date into tmpMasses
                      join bodyFat in bodyFats on month equals bodyFat.Date into tmpBodyFats
                      from steps in tmpSteps.DefaultIfEmpty()
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
                      orderby month descending
                      select new
                      {
                          month = month.ToDateTimeUnspecified(),
                          steps?.Steps,
                          stand?.AverageStandHours,
                          AverageMass = mass?.Mass.As(_settings.WeightUnit),
                          AverageBodyFatPct = bodyFat?.BodyFatPercentage,
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

            return dataByMonth;
        }

        public void Customize(ExcelWorksheet sheet, ExcelWorkbook workbook)
        {
            workbook.Names.Add($"{sheet.Name.Rangify()}_steps", sheet.Cells["B:B"]);
            workbook.Names.Add($"{sheet.Name.Rangify()}_standhours", sheet.Cells["C:C"]);
            workbook.Names.Add($"{sheet.Name.Rangify()}_avgweight", sheet.Cells["D:D"]);
            workbook.Names.Add($"{sheet.Name.Rangify()}_avgbodyfatpct", sheet.Cells["E:E"]);
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

        public IEnumerable<string> Headers => new[]
        {
            ColumnNames.Month(),
            ColumnNames.Steps(),
            ColumnNames.AverageStandHours(),
            ColumnNames.AverageWeight(_settings.WeightUnit),
            ColumnNames.AverageBodyFatPercentage(),
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
