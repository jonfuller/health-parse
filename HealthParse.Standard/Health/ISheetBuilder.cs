using OfficeOpenXml;

namespace HealthParse.Standard.Health
{
    public interface ISheetBuilder
    {
        void Build(ExcelWorksheet sheet);
    }
}
