using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthParse.Standard.Health.Sheets
{
    public class SummaryBuilder : ISheetBuilder
    {
        private Dictionary<string, IEnumerable<Record>> _records;
        private Dictionary<string, IEnumerable<Workout>> _workouts;

        public SummaryBuilder(Dictionary<string, IEnumerable<Record>> records, Dictionary<string, IEnumerable<Workout>> workouts)
        {
            _records = records;
            _workouts = workouts;
        }

        void ISheetBuilder.Build(ExcelWorksheet sheet)
        {
            var recordMonths = _records.Values
                .SelectMany(r => r)
                .GroupBy(s => new { s.StartDate.Date.Year, s.StartDate.Date.Month })
                .Select(g => new DateTime(g.Key.Year, g.Key.Month, 1));

            var workoutMonths = _workouts.Values
                .SelectMany(r => r)
                .GroupBy(s => new { s.StartDate.Date.Year, s.StartDate.Date.Month })
                .Select(g => new DateTime(g.Key.Year, g.Key.Month, 1));

            var stepsByMonth = StepHelper.PrioritizeSteps(_records[HKConstants.Records.StepCount])
                .OrderBy(w => w.StartDate)
                .GroupBy(s => new { s.StartDate.Date.Year, s.StartDate.Date.Month })
                .Select(x => new
                {
                    date = new DateTime(x.Key.Year, x.Key.Month, 1),
                    steps = x.Sum(r => r.Value.SafeParse(0))
                })
                .OrderByDescending(s => s.date);

            var cyclingWorkouts = _workouts[HKConstants.Workouts.Cycling]
                .OrderBy(w => w.StartDate)
                .GroupBy(s => new { s.StartDate.Date.Year, s.StartDate.Date.Month })
                .Select(x => new
                {
                    date = new DateTime(x.Key.Year, x.Key.Month, 1),
                    distance = x.Sum(c => c.TotalDistance),
                    minutes = x.Sum(c => c.Duration ?? 0)
                });

            var cyclingDistances = _records[HKConstants.Records.DistanceCycling]
                .OrderBy(w => w.StartDate)
                .GroupBy(s => new { s.StartDate.Date.Year, s.StartDate.Date.Month })
                .Select(x => new
                {
                    date = new DateTime(x.Key.Year, x.Key.Month, 1),
                    distance = x.Sum(c => c.Raw.Attribute("value").ValueDouble(0)),
                });

            var stregthTrainings = _workouts[HKConstants.Workouts.Strength]
                .OrderBy(w => w.StartDate)
                .GroupBy(s => new { s.StartDate.Date.Year, s.StartDate.Date.Month })
                .Select(x => new
                {
                    date = new DateTime(x.Key.Year, x.Key.Month, 1),
                    duration = x.Sum(c => c.Raw.Attribute("duration").ValueDouble(0)),
                });

            var healthMonths = recordMonths.Concat(workoutMonths).Distinct();

            var dataByMonth = from month in healthMonths
                      join steps in stepsByMonth on month equals steps.date into tmpSteps
                      join wCycling in cyclingWorkouts on month equals wCycling.date into tmpWCycling
                      join rCycling in cyclingDistances on month equals rCycling.date into tmpRCycling
                      join strength in stregthTrainings on month equals strength.date into tmpStrength
                      from steps in tmpSteps.DefaultIfEmpty()
                      from wCycling in tmpWCycling.DefaultIfEmpty()
                      from rCycling in tmpRCycling.DefaultIfEmpty()
                      from strength in tmpStrength.DefaultIfEmpty()
                      orderby month descending
                      select new
                      {
                          month,
                          steps?.steps,
                          cyclingWorkoutDistance = wCycling?.distance,
                          cyclingWorkoutMinutes = wCycling?.minutes,
                          distanceCyclingDistance = rCycling?.distance,
                          strengthMinutes = strength?.duration
                      };

            sheet.WriteData(dataByMonth);
        }
    }
}
