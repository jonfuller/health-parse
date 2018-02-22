using System.Collections.Generic;
using System.Xml.Linq;
using NodaTime.Text;
using UnitsNet;

namespace HealthParse.Standard.Health.Sheets
{
    public static class RecordParser
    {
        public static Record ParseRecord(Dictionary<string, string> pairs)
        {
            var pattern = OffsetDateTimePattern.CreateWithInvariantCulture("yyyy-MM-dd HH:mm:ss o<M>");
            var startDate = pattern.Parse(pairs["startDate"]).Value.ToInstant();
            var endDate = pattern.Parse(pairs["endDate"]).Value.ToInstant();

            pairs.TryGetValue("value", out var value);
            pairs.TryGetValue("unit", out var unit);
            pairs.TryGetValue("sourceName", out var sourceName);

            return new Record
            {
                Type = pairs["type"],
                EndDate = endDate,
                StartDate = startDate,
                DateRange = new InstantRange(startDate, endDate),
                Value = value,
                Unit = unit,
                SourceName = sourceName,
            };
        }
        public static Record FromXElement(XElement r)
        {
            var pattern = OffsetDateTimePattern.CreateWithInvariantCulture("yyyy-MM-dd HH:mm:ss o<M>");
            var startDate = pattern.Parse(r.Attribute("startDate").Value).Value.ToInstant();
            var endDate = pattern.Parse(r.Attribute("endDate").Value).Value.ToInstant();

            return new Record
            {
                Type = r.Attribute("type").Value,
                EndDate = endDate,
                StartDate = startDate,
                DateRange = new InstantRange(startDate, endDate),
                Value = r.Attribute("value")?.Value ?? "<null>",
                Unit = r.Attribute("unit")?.Value ?? "<null>",
                SourceName = r.Attribute("sourceName")?.Value ?? "<null>",
            };
        }

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