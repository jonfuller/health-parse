using System.Collections.Generic;
using NodaTime;
using OfficeOpenXml;

namespace HealthParse.Standard.Health.Sheets
{
    public interface ISheetBuilder
    {
        IEnumerable<object> BuildRawSheet();
        IEnumerable<string> Headers { get; }
        void Customize(ExcelWorksheet worksheet, ExcelWorkbook workbook);
    }
    public interface ISheetBuilder<out TItem> : ISheetBuilder where TItem : DatedItem
    {
        IEnumerable<TItem> BuildSummary();
        IEnumerable<TItem> BuildSummaryForDateRange(IRange<ZonedDateTime> dateRange);
    }

    public class DatedItem
    {
        public DatedItem(int year, int month)
            : this(new LocalDate(year, month, 1))
        {
        }

        protected DatedItem(LocalDate date)
        {
            Date = date;
        }

        public LocalDate Date { get; }
    }
}
