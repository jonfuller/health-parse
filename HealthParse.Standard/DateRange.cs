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

        public bool Includes(DateTime value)
        {
            return (Start < value) && (value < End);
        }

        public bool Includes(IRange<DateTime> range)
        {
            return (Start < range.Start) && (range.End < End);
        }
    }
}
