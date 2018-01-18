using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace HealthParse.Standard
{
    public static class Extensions
    {
        public static IEnumerable<T> Concat<T>(this IEnumerable<T> target, T item)
        {
            return target.Concat(new[] { item });
        }
        public static double SafeParse(this string target, double valueIfParseFail)
        {
            var parsed = double.TryParse(target, out var result);
            return parsed ? result : valueIfParseFail;
        }

        public static double? ValueDouble(this XAttribute target, double defaultValue = double.NaN)
        {
            return target?.Value.SafeParse(defaultValue);
        }

        public static DateTime ValueDateTime(this XAttribute target)
        {
            return target?.Value.ToDateTime() ?? DateTime.MinValue;
        }

        public static DateTime ToDateTime(this string target)
        {
            return DateTime.Parse(target);
        }

        public static byte[] ToBytes(this MimeMessage target)
        {
            using (var memoryStream = new MemoryStream())
            {
                target.WriteTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public static MimeMessage GetMessage(this byte[] target)
        {
            using (var memoryStream = new MemoryStream(target))
            {
                return MimeMessage.Load(memoryStream);
            }
        }
    }
}
