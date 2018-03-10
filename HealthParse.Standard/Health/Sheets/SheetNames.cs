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
        public const string CyclingDistance = "Cycling (Distance)";
        public const string CyclingWorkouts = "Cycling (Workouts)";
        public const string StrengthTraining = "Strength Training";
        public const string Hiit = "HIIT";
        public const string Running = "Running";
        public const string Walking = "Walking";
        public const string Elliptical = "Elliptical";
        public const string Play = "Play";
        public const string Settings = "Settings";

        public static class MonthSummary
        {
            public const string Current = "Month Summary - Current";
            public const string Previous = "Month Summary - Previous";
            public static string Name(int year, int month) => $"Month Summary - {year} - {month}";
        }
    }
}