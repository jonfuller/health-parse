using System;
using System.Collections.Generic;
using OfficeOpenXml;

namespace HealthParse.Standard.Health.Sheets
{
    public interface ISheetBuilder
    {
        void Build(ExcelWorksheet sheet);
    }
    public interface ISheetBuilder<TItem> : ISheetBuilder where TItem : DatedItem
    {
        IEnumerable<TItem> BuildSummary();
        IEnumerable<TItem> BuildSummaryForDateRange(IRange<DateTime> dateRange);
    }

    public class DatedItem
    {
        public DatedItem(int year, int month)
            : this(new DateTime(year, month, 1))
        {

        }
        protected DatedItem(DateTime date)
        {
            Date = date;
        }

        public DateTime Date { get; }
    }
}
