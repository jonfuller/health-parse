using System.Collections.Generic;

namespace HealthParse.Standard
{
    public static class Events
    {
        public const string ReceivedMail = "ReceivedMail";
        public const string SentMail = "SentMail";

        public static string SettingsUpdatedSettings = "SettingsUpdatedSettings";
        public static string SettingsUpdatedCustomSheets = "SettingsUpdatedCustomSheets";

        public static class Properties
        {
            public static Dictionary<string, string> Init() => new Dictionary<string, string>();
            public const string EmailAddress = "EmailAddress";
        }

        public static class Metrics
        {
            public static Dictionary<string, double> Init() => new Dictionary<string, double>();
            public const string Duration = "Duration";
        }
    }

    public static class EventExtensions
    {
        public static Dictionary<string, T> Then<T>(this Dictionary<string, T> x, string key, T value)
        {
            x.Add(key, value);
            return x;
        }
    }
}