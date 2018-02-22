using NodaTime;

namespace HealthParse.Standard.Health
{
    public class Record
    {
        public string Type { get; set; }
        public Instant StartDate { get; set; }
        public Instant EndDate { get; set; }
        public string SourceName { get; set; }
        public IRange<Instant> DateRange { get; set; }
        public string Value { get; set; }
        public string Unit { get; set; }
    }
}
