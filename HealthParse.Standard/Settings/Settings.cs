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
                .Where(prop => prop.Name != nameof(Default))
                .Select(prop => Tuple.Create(
                    prop,
                    (SettingsAttribute) prop.GetCustomAttributes(typeof(SettingsAttribute), true).Single()
                ))
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

        public void SetValue(string settingName, object value)
        {
            var settingProp = SettingProps.FirstOrDefault(x => x.Item2.Name == settingName);

            settingProp?.Item1.SetValue(this, value);
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