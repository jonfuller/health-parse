using System;
using System.Xml.Linq;
using NodaTime;
using NodaTime.Text;

namespace HealthParse.Standard.Health
{
    public class Record
    {
        public string Type { get; private set; }
        public Instant StartDate { get; private set; }
        public Instant EndDate { get; private set; }
        public XElement Raw { get; private set; }
        public IRange<Instant> DateRange { get; private set; }
        public string Value { get; private set; }
        public string Unit { get; private set; }
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
                Raw = r
            };
        }
    }
}
