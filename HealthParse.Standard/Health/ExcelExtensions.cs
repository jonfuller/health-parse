using System;
using System.Collections.Generic;
using System.Linq;
using HealthParse.Standard.Health.Sheets;
using HealthParse.Standard.Settings;
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
        public static void WriteData<T>(this ExcelWorksheet sheet, Dataset<T> sheetData, bool omitEmptyColumns = true)
        {
            if (sheetData.Keyed)
            {
                WriteKeyColumn(sheetData.KeyColumn, 1, sheet);
            }
            sheetData.Columns
                .Where(c => !omitEmptyColumns || c.Any())
                .Select((column, i) => new { column, i})
                .ToList().ForEach(c =>
                {
                    var columnNumber = sheetData.Keyed
                        ? c.i + 2
                        : c.i + 1;
                    var columnLetter = ColumnLetter(columnNumber);

                    if (sheetData.Keyed)
                        WriteColumn(c.column, sheetData.KeyColumn, columnNumber, sheet);
                    else
                    {
                        WriteColumn(c.column, c.i+1, sheet);
                    }

                    if (c.column.RangeName != null)
                    {
                        sheet.Workbook.Names.Add(
                            $"{sheet.Name.Rangify()}_{c.column.RangeName}",
                            sheet.Cells[$"{columnLetter}:{columnLetter}"]);
                    }
                });
        }

        private static string ColumnLetter(int i)
        {
            return new string((char) (i + 'A'), 1);
        }

        private static void WriteKeyColumn<T>(KeyColumn<T> keyColumn, int colNum, ExcelWorksheet sheet)
        {
            WriteColumn(keyColumn, keyColumn, colNum, sheet);
        }
        private static void WriteColumn<T>(Column<T> column, KeyColumn<T> keyColumn, int columnNum, ExcelWorksheet target)
        {
            target.Cells[1, columnNum].Value = column.Header;
            keyColumn
                .OrderByDescending(c => c)
                .Select((key, i) => new { value = column[key], rowNum = i + 2 })
                .ToList().ForEach(item =>
                {
                    var cell = target.Cells[item.rowNum, columnNum];
                    var formatter = item.value != null && Formatters.ContainsKey(item.value.GetType())
                        ? Formatters[item.value.GetType()]
                        : range => { };

                    cell.Value = item.value;
                    formatter(cell);
                });
        }
        private static void WriteColumn<T>(Column<T> column, int columnNum, ExcelWorksheet target)
        {
            target.Cells[1, columnNum].Value = column.Header;
            column.Values
                .Select((value, i) => new { value, rowNum = i + 2 })
                .ToList().ForEach(item =>
                {
                    var cell = target.Cells[item.rowNum, columnNum];
                    var formatter = item.value != null && Formatters.ContainsKey(item.value.GetType())
                        ? Formatters[item.value.GetType()]
                        : range => { };

                    cell.Value = item.value;
                    formatter(cell);
                });
        }

        public static void PlaceCustomSheets(this ExcelWorkbook workbook,
            CustomSheetsPlacement placement,
            IEnumerable<ExcelWorksheet> customSheets,
            string summarySheetName,
            IList<string> monthSummaryNames)
        {
            var customSheetsList = customSheets.ToList();
            foreach (var customSheet in customSheetsList)
            {
                workbook.Worksheets.Add(customSheet.Name, customSheet);
            }

            switch (placement)
            {
                case CustomSheetsPlacement.AfterSummary:
                    foreach (var customSheet in customSheetsList)
                    {
                        workbook.Worksheets.MoveAfter(customSheet.Name, summarySheetName);
                    }
                    break;
                case CustomSheetsPlacement.AfterMonthlySummaries:
                    if (monthSummaryNames.IsEmpty()) break;
                    var lastMonth = monthSummaryNames.Last();

                    foreach (var customSheet in customSheetsList)
                    {
                        workbook.Worksheets.MoveAfter(customSheet.Name, lastMonth);
                    }

                    break;
                case CustomSheetsPlacement.First:
                    foreach (var customSheet in customSheetsList)
                    {
                        workbook.Worksheets.MoveToStart(customSheet.Name);
                    }

                    break;
                case CustomSheetsPlacement.Last:
                default:
                    // do nothing, they're already at the end
                    break;
            }
        }
    }
}
