using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HealthParse.Standard.Settings
{
    public class Settings : IEnumerable<Setting>
    {
        private static readonly List<Tuple<PropertyInfo, SettingsAttribute>> SettingProps;
        static Settings()
        {
            SettingProps = typeof(Settings)
                .GetProperties()
                .Select(prop => new {prop, attr = prop.GetCustomAttributes(typeof(SettingsAttribute), true).FirstOrDefault() })
                .Where(x => x.attr != null)
                .Select(x => Tuple.Create(x.prop, (SettingsAttribute)x.attr))
                .ToList();
        }
        public static Settings Default
        {
            get
            {
                var settings = new Settings();
                var settingsLookup = settings.ToDictionary(s => s.Name, s => s);
                SettingProps.ForEach(s =>
                {
                    var defaultValue = settingsLookup[s.Item2.Name].DefaultValue;

                    s.Item1.SetValue(settings, defaultValue);
                });

                return settings;
            }
        }

        [Settings(Name = "OmitEmptySheets", Description = "Omits a sheet if there is no data for it.", DefaultValue = true)]
        public bool OmitEmptySheets { get; set; }

        [Settings(Name = "OmitEmptyColumnsOnOverallSummary", Description = "Omits a column on the 'Overall Summary' sheet if that column is empty.", DefaultValue = true)]
        public bool OmitEmptyColumnsOnOverallSummary { get; set; }

        [Settings(Name = "OmitEmptyColumnsOnMonthlySummary", Description = "Omits a column on a Monthly Summary sheet if that column is empty.", DefaultValue = true)]
        public bool OmitEmptyColumnsOnMonthlySummary { get; set; }

        [Settings(Name = "NumberOfMonthlySummaries", Description = "The number of monthly summary sheets to include in the spreadsheet.", DefaultValue = 3)]
        public int NumberOfMonthlySummaries { get; set; }

        [Settings(Name = "UseConstantNameForMostRecentMonthlySummarySheet", Description = "The current month's sheet name will be 'Month Summary - Current' instead of 'Month Summary - yyyy - mm'. This enables advanced usage in coordination with custom sheets (e.g. custom reporting for the current month).", DefaultValue = false)]
        public bool UseConstantNameForMostRecentMonthlySummarySheet { get; set; }

        public void SetValue(string settingName, object value)
        {
            var settingProp = SettingProps.FirstOrDefault(x => x.Item2.Name == settingName);

            settingProp?.Item1.SetValue(this, Coercion.Coerce(value, settingProp.Item1.PropertyType));
        }

        public IEnumerator<Setting> GetEnumerator()
        {
            return CollectSettings().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return CollectSettings().GetEnumerator();
        }

        private IEnumerable<Setting> CollectSettings()
        {
            return SettingProps.Select(x => new Setting
            {
                Name = x.Item2.Name,
                Value = x.Item1.GetValue(this),
                DefaultValue = x.Item2.DefaultValue,
                Description = x.Item2.Description
            });
        }
    }
}