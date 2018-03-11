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
            public const string BasalEnergyBurned = "HKQuantityTypeIdentifierBasalEnergyBurned";
            public const string ActiveEnergyBurned = "HKQuantityTypeIdentifierActiveEnergyBurned";

            public static class Standing
            {
                public const string StandType = "HKCategoryTypeIdentifierAppleStandHour";
                public const string Stood = "HKCategoryValueAppleStandHourStood";
            }

            public static class Nutrition
            {
                public const string EnergyConsumed = "HKQuantityTypeIdentifierDietaryEnergyConsumed";
                public const string Fat = "HKQuantityTypeIdentifierDietaryFatTotal";
                public const string Carbs = "HKQuantityTypeIdentifierDietaryCarbohydrates";
                public const string Protein = "HKQuantityTypeIdentifierDietaryProtein";
            }

            public static class Markers
            {
                public const string RestingHeartRate = "HKQuantityTypeIdentifierRestingHeartRate";
                public const string Vo2Max = "HKQuantityTypeIdentifierVO2Max";
                public const string WalkingHeartRateAverage = "HKQuantityTypeIdentifierWalkingHeartRateAverage";
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
