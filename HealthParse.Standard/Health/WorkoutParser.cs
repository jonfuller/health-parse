using System.Xml.Linq;
using NodaTime.Text;
using UnitsNet;

namespace HealthParse.Standard.Health
{
    public static class WorkoutParser
    {
        public static Workout FromXElement(XElement r)
        {
            var pattern = OffsetDateTimePattern.CreateWithInvariantCulture("yyyy-MM-dd HH:mm:ss o<M>");

            var startDate = pattern.Parse(r.Attribute("startDate").Value).Value.ToInstant();
            var endDate = pattern.Parse(r.Attribute("endDate").Value).Value.ToInstant();

            return new Workout
            {
                WorkoutType = r.Attribute("workoutActivityType").Value,
                SourceName = r.Attribute("sourceName").Value,
                EndDate = endDate,
                StartDate = startDate,
                Duration = WorkoutParser.WorkoutDuration(r),
                Distance = WorkoutParser.Distance(r),
                Energy = WorkoutParser.EnergyBurned(r),
                Device = r.Attribute("device")?.Value,
                Raw = r
            };
        }
        private static Length Distance(XElement element)
        {
            var valueAttr = element.Attribute("totalDistance");
            var unitAttr = element.Attribute("totalDistanceUnit");

            if (valueAttr == null)
            {
                return Length.Zero;
            }

            var value = valueAttr.Value.SafeParse(0);
            var unit = Length.ParseUnit(unitAttr.Value);

            return Length.From(value, unit);
        }

        private static Energy EnergyBurned(XElement element)
        {
            var valueAttr = element.Attribute("totalEnergyBurned");
            var unitAttr = element.Attribute("totalEnergyBurnedUnit");

            if (valueAttr == null)
            {
                return Energy.Zero;
            }

            var value = valueAttr.Value.SafeParse(0);
            var unit = Energy.ParseUnit(unitAttr.Value);

            return Energy.From(value, unit);
        }

        private static Duration WorkoutDuration(XElement element)
        {
            var valueAttr = element.Attribute("duration");
            var unitAttr = element.Attribute("durationUnit");

            if (valueAttr == null)
            {
                return Duration.Zero;
            }

            var value = valueAttr.Value.SafeParse(0);
            var unit = Duration.ParseUnit(unitAttr.Value);

            return Duration.From(value, unit);
        }
    }
}