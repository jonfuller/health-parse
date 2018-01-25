using System;

namespace HealthParse.Standard.Settings
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SettingsAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public object DefaultValue { get; set; }

    }
}