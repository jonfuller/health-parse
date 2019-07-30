using NodaTime;

namespace HealthParse.Standard
{
    public class DateRange : InstantRange, IRange<ZonedDateTime>
    {
        public new ZonedDateTime Start { get; }
        public new ZonedDateTime End { get; }

        public DateRange(ZonedDateTime start, ZonedDateTime end) : base(start.ToInstant(), end.ToInstant())
        {
            Start = start;
            End = end;
        }

        public bool Includes(ZonedDateTime value, Clusivity clusivity = Clusivity.Exclusive)
        {
            return Includes(value.ToInstant(), clusivity);
        }

        public bool Includes(IRange<ZonedDateTime> range, Clusivity clusivity = Clusivity.Exclusive)
        {
            return Includes(new InstantRange(range.Start.ToInstant(), range.End.ToInstant()), clusivity);
        }
    }
}
