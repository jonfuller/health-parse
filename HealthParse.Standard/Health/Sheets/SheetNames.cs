namespace HealthParse.Standard.Health.Sheets
{
    public static class SheetNames
    {
        public const string Summary = "Overall Summary";
        public const string Steps = "Steps";
        public const string Mass = "Mass (Weight)";
        public const string BodyFat = "Body Fat %";
        public const string GeneralRecords = "General Records";
        public const string HealthMarkers = "Health Markers";
        public const string Nutrition = "Nutrition";
        public const string CyclingDistance = "Cycling (Distance)";
        public const string Settings = "Settings";

        public static string For(string hkWorkout)
        {
            var shortName = hkWorkout.Replace("HKWorkoutActivityType", string.Empty);

            switch (shortName)
            {
                case "TraditionalStrengthTraining":
                    return "Strength Training";
                case "HighIntensityIntervalTraining":
                    return "HIIT";
                default:
                    return shortName.SplitCamelCase();
            }
        }

        public static class MonthSummary
        {
            public const string Current = "Month Summary - Current";
            public const string Previous = "Month Summary - Previous";
            public static string Name(int year, int month) => $"Month Summary - {year} - {month}";
        }
    }
}