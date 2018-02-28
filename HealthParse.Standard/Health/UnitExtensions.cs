using System;
using System.Collections.Generic;
using System.Linq;
using UnitsNet;

namespace HealthParse.Standard.Health
{
    public static class UnitExtensions
    {
        public static Length Sum<T>(this IEnumerable<T> target, Func<T, Length> selector)
        {
            return target.Aggregate(Length.Zero, (current, item) => current + selector(item));
        }
        public static Duration Sum<T>(this IEnumerable<T> target, Func<T, Duration> selector)
        {
            return target.Aggregate(Duration.Zero, (current, item) => current + selector(item));
        }
        public static Mass Sum<T>(this IEnumerable<T> target, Func<T, Mass> selector)
        {
            return target.Aggregate(Mass.Zero, (current, item) => current + selector(item));
        }
        public static Energy Sum<T>(this IEnumerable<T> target, Func<T, Energy> selector)
        {
            return target.Aggregate(Energy.Zero, (current, item) => current + selector(item));
        }

        public static Mass Average<T>(this IEnumerable<T> target, Func<T, Mass> selector)
        {
            var listed = target.ToList();

            return listed.Sum(selector) / listed.Count;
        }
    }
}