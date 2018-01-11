using System;
using System.Xml.Linq;

namespace HealthParse.Core.Health
{
    public class Record
    {
        public string Type { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public DateTime? CreationDate { get; private set; }
        public XElement Raw { get; private set; }
        public DateRange DateRange { get; private set; }
        public string Value { get; private set; }
        public string Unit { get; private set; }
        public static Record FromXElement(XElement r)
        {
            var startDate = r.Attribute("startDate").ValueDateTime();
            var endDate = r.Attribute("endDate").ValueDateTime();
            return new Record
            {
                Type = r.Attribute("type").Value,
                EndDate = endDate,
                StartDate = startDate,
                DateRange = new DateRange(startDate, endDate),
                CreationDate = r.Attribute("creationDate")?.ValueDateTime(),
                Value = r.Attribute("value")?.Value ?? "<null>",
                Unit = r.Attribute("unit")?.Value ?? "<null>",
                Raw = r
            };
        }
    }
}
