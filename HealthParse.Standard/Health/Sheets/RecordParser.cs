using UnitsNet;

namespace HealthParse.Standard.Health.Sheets
{
    public static class RecordParser
    {
        public static Length Distance(Record record)
        {
            var valueRaw = record.Value;
            var unitRaw = record.Unit;

            if (valueRaw == null)
            {
                return Length.Zero;
            }

            var value = valueRaw.SafeParse(0);
            var unit = Length.ParseUnit(unitRaw);

            return Length.From(value, unit);
        }
        public static Mass Weight(Record record)
        {
            var valueRaw = record.Value;
            var unitRaw = record.Unit;

            if (valueRaw == null)
            {
                return Mass.Zero;
            }

            var value = valueRaw.SafeParse(0);
            var unit = Mass.ParseUnit(unitRaw);

            return Mass.From(value, unit);
        }
    }
}