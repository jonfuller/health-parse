using UnitsNet.Units;

namespace HealthParse.Standard.Health.Sheets
{
    public static class ColumnNames
    {
        public static string Date() => "Date";
        public static string Month() => "Month";
        public static string Distance(LengthUnit unit) => $"Distance ({unit})";
        public static string Duration(DurationUnit unit) => $"Duration ({unit})";
        public static string EnergyBurned(EnergyUnit unit) => $"Energy Burned ({unit})";
        public static string Weight(MassUnit unit) => $"Weight ({unit})";
        public static string AverageWeight(MassUnit unit) => $"Weight ({unit}, avg)";

        public static string Steps() => "Steps";
        public static string BodyFatPercentage() => "Body Fat (%)";
        public static string AverageBodyFatPercentage() => "Body Fat (%, avg)";
        public static string CyclingDistance(LengthUnit unit) => $"Cycling {Distance(unit)}";
        public static string StandHours() => "Stand Hours";
        public static string AverageStandHours() => "Average Stand Hours";
        public static string TotalFlightsClimbed() => "Total Flights Climbed";
        public static string FlightsClimbed() => "Flights Climbed";
        public static string TotalExerciseDuration(DurationUnit unit) => $"Total Exercise Time ({unit})";
        public static string ExerciseDuration(DurationUnit unit) => $"Exercise Time ({unit})";
        public static string BasalEnergy(EnergyUnit unit) => $"Basal Energy ({unit})";
        public static string ActiveEnergy(EnergyUnit unit) => $"Active Energy ({unit})";
        public static string AverageBasalEnergy(EnergyUnit unit) => $"Average {BasalEnergy(unit)}";
        public static string AverageActiveEnergy(EnergyUnit unit) => $"Average {ActiveEnergy(unit)}";

        public static class Markers
        {
            public const string RestingHeartRate = "Resting HR";
            public const string Vo2Max = "VO2 Max";
            public const string WalkingHeartRateAverage = "Walking HR (Avg)";

            public const string RestingHeartRateAverage = "Resting HR (Avg)";
            public const string Vo2MaxAverage = "VO2 Max (Avg)";
        }
        public static class Settings
        {
            public static string Name() => "Name";
            public static string Value() => "Value";
            public static string DefaultValue() => "DefaultValue";
            public static string Description() => "Description";
        }

        public static class Workout
        {
            public static string Cycling() => "Cycling";
            public static string Running() => "Running";
            public static string Walking() => "Walking";
            public static string HIIT() => "HIIT";
            public static string Play() => "Play";
            public static string Elliptical() => "Elliptical";
            public static string StrengthTraining() => "Strength";
        }
    }
}