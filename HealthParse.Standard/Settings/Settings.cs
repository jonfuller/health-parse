using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HealthParse.Standard.Settings
{
    public class Settings : IEnumerable<Setting>
    {
        public static Settings Default
        {
            get
            {
                var settings = new Settings();
                var emptyLookup = settings.ToDictionary(s => s.Name, s => s);
                typeof(Settings)
                    .GetProperties()
                    .Where(prop => prop.Name != nameof(Default))
                    .ToList()
                    .ForEach(prop =>
                    {
                        var settingAttribute = prop.GetCustomAttributes(typeof(SettingsAttribute), true).FirstOrDefault();
                        var settingName = ((SettingsAttribute)settingAttribute).Name;
                        var defaultValue = emptyLookup[settingName].DefaultValue;

                        prop.SetValue(settings, defaultValue);
                    });

                return settings;
            }
        }

        public void SetValue(string settingName, object value)
        {
            var settingProp = GetType()
                .GetProperties()
                .Select(prop => new {prop, attr = (SettingsAttribute)prop.GetCustomAttributes(typeof(SettingsAttribute), true).FirstOrDefault() })
                .FirstOrDefault(x => x.attr.Name == settingName);

            settingProp?.prop.SetValue(this, value);
        }

        [Settings(Name = "OmitEmptySheets", Description = "Omits a sheet if there is no data for it.", DefaultValue = true)]
        public bool OmitEmptySheets { get; set; }

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
            return GetType()
                .GetProperties()
                .Where(prop => prop.Name != nameof(Default))
                .Select(prop => new{prop, attr = (SettingsAttribute)prop.GetCustomAttributes(typeof(SettingsAttribute), true).FirstOrDefault()})
                .Select(x => new Setting
                {
                    Name = x.attr.Name,
                    Value = x.prop.GetValue(this),
                    DefaultValue = x.attr.DefaultValue,
                    Description = x.attr.Description
                });
        }
    }
}