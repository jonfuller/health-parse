using System;
using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml;

namespace HealthParse.Standard.Health.Sheets
{
    public abstract class WorkoutBuilder : ISheetBuilder
    {
        private Dictionary<string, IEnumerable<Workout>> _workouts;
        private Func<Workout, object> _selector;
        private string _workoutKey;

        public WorkoutBuilder(Dictionary<string, IEnumerable<Workout>> workouts, string workoutKey, Func<Workout, object> selector)
        {
            _workouts = workouts;
            _selector = selector;
            _workoutKey = workoutKey;
        }

        void ISheetBuilder.Build(ExcelWorksheet sheet)
        {
            var workouts = _workouts[_workoutKey]
                 .OrderByDescending(r => r.StartDate)
                 .Select(_selector);

            sheet.WriteData(workouts);
        }
    }
}
