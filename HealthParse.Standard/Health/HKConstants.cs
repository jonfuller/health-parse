namespace HealthParse.Standard.Health
{
    public static class HKConstants
    {
        public static class Records
        {
            public const string BodyMass = "HKQuantityTypeIdentifierBodyMass";
            public const string BodyFatPercentage = "HKQuantityTypeIdentifierBodyFatPercentage";
            public const string StepCount = "HKQuantityTypeIdentifierStepCount";
            public const string DistanceCycling = "HKQuantityTypeIdentifierDistanceCycling";

            public const string ExerciseTime = "HKQuantityTypeIdentifierAppleExerciseTime";
            public const string FlightsClimbed = "HKQuantityTypeIdentifierFlightsClimbed";

            public static class Standing
            {
                public const string StandType = "HKCategoryTypeIdentifierAppleStandHour";
                public const string Stood = "HKCategoryValueAppleStandHourStood";
            }
        }

        public static class Workouts
        {
            public const string Strength = "HKWorkoutActivityTypeTraditionalStrengthTraining";
            public const string Cycling = "HKWorkoutActivityTypeCycling";
            public const string Running = "HKWorkoutActivityTypeRunning";
            public const string Walking = "HKWorkoutActivityTypeWalking";
            public const string Hiit = "HKWorkoutActivityTypeHighIntensityIntervalTraining";
            public const string Elliptical = "HKWorkoutActivityTypeElliptical";
            public const string Play = "HKWorkoutActivityTypePlay";
        }
    }
}
