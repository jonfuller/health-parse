using System.Collections.Generic;
using NodaTime;
using OfficeOpenXml;

namespace HealthParse.Standard.Health.Sheets
{
    public interface IRawSheetBuilder
    {
        IEnumerable<object> BuildRawSheet();
        IEnumerable<string> Headers { get; }
        void Customize(ExcelWorksheet worksheet, ExcelWorkbook workbook);
    }

    public interface IMonthlySummaryBuilder<out TItem> where TItem : DatedItem
    {
        IEnumerable<TItem> BuildSummaryForDateRange(IRange<ZonedDateTime> dateRange);
    }
    public interface ISummarySheetBuilder<out TItem> where TItem : DatedItem
    {
        IEnumerable<TItem> BuildSummary();
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
