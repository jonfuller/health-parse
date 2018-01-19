using System;
using OfficeOpenXml;
using System.Collections.Generic;
using System.Linq;

namespace HealthParse.Standard.Health
{
    public static class ExcelExtensions
    {
        private static readonly Dictionary<Type, Action<ExcelRange>> Formatters = new Dictionary<Type, Action<ExcelRange>>()
        {
            {typeof(DateTime), range => range.Style.Numberformat.Format = "yyyy-mm-dd" },
        };
        public static void WriteData(this ExcelWorksheet target, IEnumerable<object> rows)
        {
            GetLines(rows)
                .SelectMany((row, rowNum) => row.Select((value, columnNum) => new { value, rowNum, columnNum }))
                .ToList()
                .ForEach(item =>
                {
                    var cell = target.Cells[item.rowNum + 1, item.columnNum + 1];
                    var formatter = item.value != null && Formatters.ContainsKey(item.value.GetType())
                        ? Formatters[item.value.GetType()]
                        : range => { };

                    cell.Value = item.value;
                    formatter(cell);
                });
        }

        private static IEnumerable<IEnumerable<object>> GetLines(IEnumerable<object> rows)
        {
            var rowsList = rows.ToList();
            if (!rowsList.Any())
            {
                yield break;
            }

            var first = rowsList.First();
            var props = first.GetType().GetProperties();

            yield return props.Select(prop => prop.Name);
            foreach (var row in rowsList)
            {
                yield return props.Select(prop => prop.GetValue(row));
            }
        }

    }
}
