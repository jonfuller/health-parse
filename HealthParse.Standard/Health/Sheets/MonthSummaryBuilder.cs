﻿using System;
using System.Linq;
using OfficeOpenXml;

namespace HealthParse.Standard.Health.Sheets
{
    public class MonthSummaryBuilder : ISheetBuilder
    {
        private readonly int _targetYear;
        private readonly int _targetMonth;
        private readonly ISheetBuilder<StepBuilder.StepItem> _stepBuilder;
        private readonly ISheetBuilder<WorkoutBuilder.WorkoutItem> _cyclingBuilder;
        private readonly ISheetBuilder<WorkoutBuilder.WorkoutItem> _runningBuilder;
        private readonly ISheetBuilder<WorkoutBuilder.WorkoutItem> _walkingBuilder;
        private readonly ISheetBuilder<WorkoutBuilder.WorkoutItem> _strengthBuilder;
        private readonly ISheetBuilder<DistanceCyclingBuilder.CyclingItem> _distanceCyclingBuilder;

        public MonthSummaryBuilder(int targetYear, int targetMonth,
            ISheetBuilder<StepBuilder.StepItem> stepBuilder,
            ISheetBuilder<WorkoutBuilder.WorkoutItem> cyclingBuilder,
            ISheetBuilder<WorkoutBuilder.WorkoutItem> runningBuilder,
            ISheetBuilder<WorkoutBuilder.WorkoutItem> walkingBuilder,
            ISheetBuilder<WorkoutBuilder.WorkoutItem> strengthBuilder,
            ISheetBuilder<DistanceCyclingBuilder.CyclingItem> distanceCyclingBuilder
        )
        {
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
}