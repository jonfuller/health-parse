using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using HealthParse.Standard.Health.Sheets;
using Microsoft.WindowsAzure.Storage.Blob;
using MimeKit;

namespace HealthParse.Standard
{
    public static class Extensions
    {
        public static Column<TKey> MakeColumn<TKey, TVal>(this IEnumerable<Tuple<TKey, TVal>> data, string header = null, string range = null)
        {
            return data.Aggregate(
                new Column<TKey> { Header = header, RangeName = range },
                (col, r) => { col.Add(r.Item1, r.Item2); return col; });
        }

        public static TV GetValue<TK, TV>(this IDictionary<TK, TV> dict, TK key, TV defaultValue = default(TV))
        {
            return dict.TryGetValue(key, out var value) ? value : defaultValue;
        }

        public static bool IsEmpty<T>(this IEnumerable<T> target)
        {
            return !target.Any();
        }

        public static string ReadBlob(this CloudBlob blob)
        {
            using (var stream = blob.OpenReadAsync().Result)
            using (var streamReader = new StreamReader(stream))
            {
                return streamReader.ReadToEnd();
            }
        }

        public static void WriteBlob(this CloudBlockBlob blob, string content)
        {
            using (var stream = blob.OpenWriteAsync().Result)
            using (var writer = new StreamWriter(stream))
            {
                writer.WriteAsync(content).Wait();
            }
        }

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
