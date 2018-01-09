using System;
using System.Xml.Linq;

namespace HealthParse
{
    public static class Extensions
    {
        public static double SafeParse(this string target, double valueIfParseFail)
        {
            double result = 0;
            var parsed = double.TryParse(target, out result);
            return parsed ? result : valueIfParseFail;
        }

        public static double? ValueDouble(this XAttribute target)
        {
            return target?.Value.SafeParse(double.NaN);
        }

        public static DateTime ValueDateTime(this XAttribute target)
        {
            return target?.Value.ToDateTime() ?? DateTime.MinValue;
        }

        public static DateTime ToDateTime(this string target)
        {
            return DateTime.Parse(target);
        }
    }
}
