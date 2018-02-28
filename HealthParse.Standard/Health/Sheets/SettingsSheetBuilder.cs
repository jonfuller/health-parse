using System.Linq;

namespace HealthParse.Standard.Health.Sheets
{
    public class SettingsSheetBuilder : IRawSheetBuilder<unit>
    {
        private readonly Settings.Settings _settings;

        public SettingsSheetBuilder(Settings.Settings settings)
        {
            _settings = settings;
        }

        public Dataset<unit> BuildRawSheet()
        {
            var columns = _settings
                .Aggregate(new
                    {
                        name = new Column<unit> { Header = ColumnNames.Settings.Name() },
                        value = new Column<unit> { Header = ColumnNames.Settings.Value() },
                        defaultValue = new Column<unit> { Header = ColumnNames.Settings.DefaultValue() },
                        description = new Column<unit> { Header = ColumnNames.Settings.Description() },
                    },
                    (cols, s) =>
                    {
                        cols.name.Add(unit.v, s.Name);
                        cols.value.Add(unit.v, s.Value.ToString()); // TODO not tostring...
                        cols.defaultValue.Add(unit.v, s.DefaultValue);
                        cols.description.Add(unit.v, s.Description);
                        return cols;
                    });

            return new Dataset<unit>(columns.name, columns.value, columns.defaultValue, columns.description);
        }
    }
}