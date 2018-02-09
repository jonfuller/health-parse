using System;
using NodaTime;

namespace HealthParse.Standard
{
    public class InstantRange : IRange<Instant>
    {
        public Instant Start { get; }
        public Instant End { get; }

        public bool Includes(Instant value, Clusivity clusivity = Clusivity.Exclusive)
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

        public bool Includes(IRange<Instant> range, Clusivity clusivity = Clusivity.Exclusive)
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

        public InstantRange(Instant start, Instant end)
        {
            Start = start;
            End = end;
        }
    }
}