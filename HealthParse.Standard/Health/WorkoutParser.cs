using System.Collections.Generic;
using NodaTime.Text;
using UnitsNet;

namespace HealthParse.Standard.Health
{
    public static class WorkoutParser
    {
        public static class Dictionary
        {
            public static Workout ParseWorkout(Dictionary<string, string> r)
            {
                var pattern = OffsetDateTimePattern.CreateWithInvariantCulture("yyyy-MM-dd HH:mm:ss o<M>");

                var startDate = pattern.Parse(r["startDate"]).Value.ToInstant();
                var endDate = pattern.Parse(r["endDate"]).Value.ToInstant();

                r.TryGetValue("sourceName", out var sourceName);
                r.TryGetValue("device", out var device);

                return new Workout
                {
                    WorkoutType = r["workoutActivityType"],
                    SourceName = sourceName,
                    EndDate = endDate,
                    StartDate = startDate,
                    Duration = WorkoutDuration(r),
                    Distance = Distance(r),
                    Energy = EnergyBurned(r),
                    Device = device,
                };
            }
            private static Length Distance(Dictionary<string, string> element)
            {
                var hasValue = element.TryGetValue("totalDistance", out var valueStr);
                element.TryGetValue("totalDistanceUnit", out var unitStr);

                if (!hasValue)
                {
                    return Length.Zero;
                }

                var value = valueStr.SafeParse(0);
                var unit = Length.ParseUnit(unitStr);

                return Length.From(value, unit);
            }
            private static Energy EnergyBurned(Dictionary<string, string> element)
            {
                var hasValue = element.TryGetValue("totalEnergyBurned", out var valueStr);
                element.TryGetValue("totalEnergyBurnedUnit", out var unitStr);

                if (!hasValue)
                {
                    return Energy.Zero;
                }

                var value = valueStr.SafeParse(0);
                var unit = Energy.ParseUnit(unitStr);

                return Energy.From(value, unit);
            }
            private static Duration WorkoutDuration(Dictionary<string, string> element)
            {
                var hasValue = element.TryGetValue("duration", out var valueStr);
                element.TryGetValue("durationUnit", out var unitStr);

                if (!hasValue)
                {
                    return Duration.Zero;
                }

                var value = valueStr.SafeParse(0);
                var unit = Duration.ParseUnit(unitStr);

                return Duration.From(value, unit);
            }
        }
    }
}