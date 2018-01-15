using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml;

namespace HealthParse.Standard.Health.Sheets
{
    public class CyclingWorkoutBuilder : ISheetBuilder
    {
        private Dictionary<string, IEnumerable<Workout>> _workouts;

        public CyclingWorkoutBuilder(Dictionary<string, IEnumerable<Workout>> workouts)
        {
            _workouts = workouts;
        }

        void ISheetBuilder.Build(ExcelWorksheet sheet)
        {
            var cycling = _workouts[HKConstants.Workouts.Cycling]
                 .OrderBy(r => r.StartDate)
                 .Select(r => new
                 {
                     date = r.StartDate,
                     duration = r.Duration,
                     durationUnit = r.DurationUnit,
                     distance = r.TotalDistance,
                     unit = r.TotalDistanceUnit,
                 });

            sheet.WriteData(cycling);
        }
    }
}
