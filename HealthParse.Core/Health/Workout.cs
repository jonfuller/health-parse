using System;
using System.Xml.Linq;

namespace HealthParse.Core.Health
{
    public class Workout
    {
        public string WorkoutType { get; private set; }
        public string SourceName { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public DateTime? CreationDate { get; private set; }
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
            return new Workout()
            {
                WorkoutType = r.Attribute("workoutActivityType").Value,
                SourceName = r.Attribute("sourceName").Value,
                EndDate = r.Attribute("endDate").ValueDateTime(),
                StartDate = r.Attribute("startDate").ValueDateTime(),
                CreationDate = r.Attribute("creationDate").ValueDateTime(),
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
