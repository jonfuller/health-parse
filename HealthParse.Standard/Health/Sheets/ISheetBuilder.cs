using System;
using System.Collections.Generic;
using OfficeOpenXml;

namespace HealthParse.Standard.Health.Sheets
{
    public interface ISheetBuilder
    {
        void Build(ExcelWorksheet sheet);
    }
    public interface ISheetBuilder<TMonthly> : ISheetBuilder where TMonthly : MonthlyItem
    {
        IEnumerable<TMonthly> BuildSummary();
    }

    public class MonthlyItem
    {
        private readonly int _year;
        private readonly int _month;

        public MonthlyItem(int year, int month)
        {
            _year = year;
            _month = month;
        }

        public DateTime Date { get { return new DateTime(_year, _month, 1); } }
    }
}
