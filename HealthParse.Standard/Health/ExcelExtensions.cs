using OfficeOpenXml;
using System.Collections.Generic;
using System.Linq;

namespace HealthParse.Standard.Health
{
    public static class ExcelExtensions
    {
        public static void WriteData(this ExcelWorksheet target, IEnumerable<object> rows)
        {
            GetLines(rows)
                .SelectMany((row, rowNum) => row.Select((value, columnNum) => new { value, rowNum, columnNum }))
                .ToList()
                .ForEach(item => target.Cells[item.rowNum + 1, item.columnNum + 1].Value = item.value);
        }

        private static IEnumerable<IEnumerable<string>> GetLines(IEnumerable<object> rows)
        {
            if (!rows.Any())
            {
                yield break;
            }

            var first = rows.First();
            var props = first.GetType().GetProperties();

            yield return props.Select(prop => prop.Name);
            foreach (var row in rows)
            {
                yield return props.Select(prop => prop.GetValue(row).ToString());
            }
        }

    }
}
