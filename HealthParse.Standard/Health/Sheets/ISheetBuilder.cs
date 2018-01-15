using OfficeOpenXml;

namespace HealthParse.Standard.Health.Sheets
{
    public interface ISheetBuilder
    {
        void Build(ExcelWorksheet sheet);
    }
}
