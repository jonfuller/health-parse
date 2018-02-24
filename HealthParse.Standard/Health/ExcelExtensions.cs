using System;
using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml;

namespace HealthParse.Standard.Health
{
    public static class ExcelExtensions
    {
        public static string Rangify(this string original)
        {
            return original
                    .Replace(" ", "_")
                    .Replace("-", "_")
                    .Replace("__", "_")
                    .Replace("__", "_")
                ;
        }

        private static readonly Dictionary<Type, Action<ExcelRange>> Formatters = new Dictionary<Type, Action<ExcelRange>>()
        {
            {typeof(DateTime), range => range.Style.Numberformat.Format = "yyyy-mm-dd" },
        };
        public static void WriteData(this ExcelWorksheet target, IEnumerable<object> dataRows, bool omitEmptyColumns = true, IEnumerable<string> headers = null)
        {
            var rows = dataRows.ToList();
            GetLines(rows, headers)
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


            if (omitEmptyColumns) OmitEmptyColumns(target);
        }

        private static void OmitEmptyColumns(ExcelWorksheet sheet)
        {
            Enumerable.Range(1, sheet.Dimension.Columns)
                .Select(colNum => new { colNum, colRange = sheet.Cells[2, colNum, sheet.Dimension.End.Row, colNum]})
                .Select(x => new { x.colNum, empty = x.colRange.All(c => c.Value == null)})
                .Where(x => x.empty)
                .Select(x => x.colNum)
                .Reverse()
                .ToList()
                .ForEach(sheet.DeleteColumn);
        }

        private static IEnumerable<IEnumerable<object>> GetLines(IList<object> rows, IEnumerable<string> headers)
        {
            if (!rows.Any())
            {
                yield break;
            }

            yield return headers;

            var props = rows.First().GetType().GetProperties();
            foreach (var row in rows)
            {
                yield return props.Select(prop => prop.GetValue(row));
            }
        }
    }
}
