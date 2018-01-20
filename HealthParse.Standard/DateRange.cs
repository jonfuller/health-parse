using System;

namespace HealthParse.Standard
{
    public class DateRange : IRange<DateTime>
    {
        public DateRange(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        public DateTime Start { get; }
        public DateTime End { get; }

        public bool Includes(DateTime value, Clusivity clusivity = Clusivity.Exclusive)
        {
            switch (clusivity)
            {
                case Clusivity.Inclusive:
                    return (Start <= value) && (value <= End);
                case Clusivity.Exclusive:
                    return (Start < value) && (value < End);
                case Clusivity.LowerInclusive:
                    return (Start <= value) && (value < End);
                case Clusivity.UpperInclusive:
                    return (Start < value) && (value <= End);
                default:
                    throw new ArgumentOutOfRangeException(nameof(clusivity), clusivity, null);
            }
        }

        public bool Includes(IRange<DateTime> range, Clusivity clusivity = Clusivity.Exclusive)
        {
            switch (clusivity)
            {
                case Clusivity.Inclusive:
                    return (Start <= range.Start) && (range.End <= End);
                case Clusivity.Exclusive:
                    return (Start < range.Start) && (range.End < End);
                case Clusivity.LowerInclusive:
                    return (Start <= range.Start) && (range.End < End);
                case Clusivity.UpperInclusive:
                    return (Start < range.Start) && (range.End <= End);
                default:
                    throw new ArgumentOutOfRangeException(nameof(clusivity), clusivity, null);
            }
        }
    }
}
