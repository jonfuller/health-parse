using System;
using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml;

namespace HealthParse.Standard.Health
{
    public static class ExcelExtensions
    {
        private static readonly Dictionary<Type, Action<ExcelRange>> Formatters = new Dictionary<Type, Action<ExcelRange>>()
        {
            {typeof(DateTime), range => range.Style.Numberformat.Format = "yyyy-mm-dd" },
        };
        public static void WriteData(this ExcelWorksheet target, IEnumerable<object> rows, bool headersIncluded = false, bool addInferredHeaders = true, bool omitEmptyColumns = true)
        {
            var hasHeaders = headersIncluded || addInferredHeaders;

            GetLines(rows, headersIncluded, addInferredHeaders)
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


            if (omitEmptyColumns) OmitEmptyColumns(target, hasHeaders);
        }

        private static void OmitEmptyColumns(ExcelWorksheet sheet, bool hasHeaders)
        {
            Enumerable.Range(1, sheet.Dimension.Columns)
                .Select(colNum => new { colNum, colRange = sheet.Cells[hasHeaders ? 2 : 1, colNum, sheet.Dimension.End.Row, colNum]})
                .Select(x => new { x.colNum, empty = x.colRange.All(c => c.Value == null)})
                .Where(x => x.empty)
                .Select(x => x.colNum)
                .Reverse()
                .ToList()
                .ForEach(sheet.DeleteColumn);
        }

        private static IEnumerable<IEnumerable<object>> GetLines(IEnumerable<object> rows, bool headersIncluded, bool addInferredHeaders)
        {
            var rowsList = rows.ToList();
            if (!rowsList.Any())
            {
                yield break;
            }

            var first = rowsList.First();
            var props = first.GetType().GetProperties();

            if (!headersIncluded && addInferredHeaders)
            {
                yield return props.Select(prop => prop.Name);
            }
            foreach (var row in rowsList)
            {
                yield return props.Select(prop => prop.GetValue(row));
            }
        }
    }
}
