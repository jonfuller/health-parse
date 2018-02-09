using System;
using System.Xml.Linq;
using NodaTime;
using NodaTime.Text;

namespace HealthParse.Standard.Health
{
    public class Workout
    {
        public string WorkoutType { get; private set; }
        public string SourceName { get; private set; }
        public Instant StartDate { get; private set; }
        public Instant EndDate { get; private set; }
        public double? Duration { get; private set; }
        public string DurationUnit { get; private set; }
        public double? TotalDistance { get; private set; }
        public string TotalDistanceUnit { get; private set; }
        public double? TotalEnergyBurned { get; private set; }
        public string TotalEnergyBurnedUnit { get; private set; }
        public string Device { get; private set; }
        public XElement Raw { get; private set; }

        public static Workout FromXElement(XElement r)
        {
            var pattern = OffsetDateTimePattern.CreateWithInvariantCulture("yyyy-MM-dd HH:mm:ss o<M>");

            var startDate = pattern.Parse(r.Attribute("startDate").Value).Value.ToInstant();
            var endDate = pattern.Parse(r.Attribute("endDate").Value).Value.ToInstant();

            return new Workout()
            {
                WorkoutType = r.Attribute("workoutActivityType").Value,
                SourceName = r.Attribute("sourceName").Value,
                EndDate = endDate,
                StartDate = startDate,
                Duration = r.Attribute("duration").ValueDouble(),
                DurationUnit = r.Attribute("durationUnit")?.Value,
                TotalDistance = r.Attribute("totalDistance").ValueDouble(),
                TotalDistanceUnit = r.Attribute("totalDistanceUnit")?.Value,
                TotalEnergyBurned = r.Attribute("totalEnergyBurned").ValueDouble(),
                TotalEnergyBurnedUnit = r.Attribute("totalEnergyBurnedUnit")?.Value,
                Device = r.Attribute("device")?.Value,
                Raw = r
            };
        }
    }
}
