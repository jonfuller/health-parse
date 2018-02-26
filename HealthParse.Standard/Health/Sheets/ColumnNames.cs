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
        public static string CyclingDistance(LengthUnit unit) => $"Cycling {ColumnNames.Distance(unit)}";
        public static string StandHours() => "Stand Hours";

        public static class Settings
        {
            public static string Name() => "Name";
            public static string Value() => "Value";
            public static string DefaultValue() => "DefaultValue";
            public static string Description() => "Description";
        }

        public static class Workout
        {
            public static class Cycling
            {
                public static string Distance(LengthUnit unit) => $"Cycling {ColumnNames.Distance(unit)}";
                public static string Duration(DurationUnit unit) => $"Cycling {ColumnNames.Duration(unit)}";
            }
            public static class Running
            {
                public static string Distance(LengthUnit unit) => $"Running {ColumnNames.Distance(unit)}";
                public static string Duration(DurationUnit unit) => $"Running {ColumnNames.Duration(unit)}";
            }
            public static class Walking
            {
                public static string Distance(LengthUnit unit) => $"Walking {ColumnNames.Distance(unit)}";
                public static string Duration(DurationUnit unit) => $"Walking {ColumnNames.Duration(unit)}";
            }

            public static class Hiit
            {
                public static string Duration(DurationUnit unit) => $"HIIT {ColumnNames.Duration(unit)}";
            }

            public static class Play
            {
                public static string Duration(DurationUnit unit) => $"Play {ColumnNames.Duration(unit)}";
            }

            public static class Elliptical
            {
                public static string Duration(DurationUnit unit) => $"Elliptical {ColumnNames.Duration(unit)}";
            }

            public static class StrengthTraining
            {
                public static string Duration(DurationUnit unit) => $"Strength {ColumnNames.Duration(unit)}";
            }
        }
    }
}