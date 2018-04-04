using System;
using System.Collections.Generic;
using System.Linq;
using HealthParse.Standard.Health.Sheets;
using HealthParse.Standard.Settings;
using NodaTime;
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
            {typeof(ZonedDateTime), range => range.Style.Numberformat.Format = "yyyy-MM-dd hh:mm" },
            {typeof(LocalDate), range => range.Style.Numberformat.Format = "yyyy-MM-dd" },
            {typeof((int Year, int Month)), range => range.Style.Numberformat.Format = "yyyy-MM" },
        };

        private static readonly Dictionary<Type, Func<object, object>> Mappers = new Dictionary<Type, Func<object, object>>()
        {
            {typeof(ZonedDateTime), value => ((ZonedDateTime)value).ToDateTimeUnspecified()},
            {typeof(LocalDate), value => ((LocalDate)value).ToDateTimeUnspecified()},
            {typeof((int Year, int Month)), value =>
                {
                    var month = ((int Year, int Month)) value;
                    return new DateTime(month.Year, month.Month, 1);
                }
            },
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
                    var columnLetter = ColumnLetter(columnNumber-1);

                    if (sheetData.Keyed)
                        WriteColumn(c.column, sheetData.KeyColumn, columnNumber, sheet);
                    else
                    {
                        WriteColumn(c.column, columnNumber, sheet);
                    }

                    if (c.column.RangeName != null)
                    {
                        sheet.Workbook.Names.Add(
                            $"{sheet.Name.Rangify()}_{c.column.RangeName}",
                            sheet.Cells[$"{columnLetter}:{columnLetter}"]);
                    }
                });
        }

        /// <summary>
        /// Expects zero-based column number
        /// </summary>
        private static string ColumnLetter(int i)
        {
            const int alphabetLength = 26;

            return i / alphabetLength == 0
                ? new string((char) (i + 'A'), 1)
                : $"{ColumnLetter(i / alphabetLength - 1)}{ColumnLetter(i % alphabetLength)}";
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
                    WriteValue(target.Cells[item.rowNum, columnNum], item.value);
                });
        }

        private static void WriteColumn<T>(Column<T> column, int columnNum, ExcelWorksheet target)
        {
            target.Cells[1, columnNum].Value = column.Header;
            column.Values
                .Select((value, i) => new { value, rowNum = i + 2 })
                .ToList().ForEach(item =>
                {
                    WriteValue(target.Cells[item.rowNum, columnNum], item.value);
                });
        }

        private static void WriteValue(ExcelRange cell, object value)
        {
            var mapper = value != null && Mappers.ContainsKey(value.GetType())
                ? Mappers[value.GetType()]
                : v => v;
            var formatter = value != null && Formatters.ContainsKey(value.GetType())
                ? Formatters[value.GetType()]
                : range => { };

            cell.Value = mapper(value);
            formatter(cell);
        }

        public static void PlaceCustomSheets(this ExcelWorkbook workbook,
            CustomSheetsPlacement placement,
            IEnumerable<ExcelWorksheet> customSheets,
            string summarySheetName)
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
