using System.Collections.Generic;
using System.Linq;

namespace HealthParse.Standard.Health.Sheets
{
    public class Dataset<TKey>
    {
        private readonly List<Column<TKey>> _columns;

        public Dataset(params Column<TKey>[] columns)
        {
            _columns = new List<Column<TKey>>(columns);
        }
        public Dataset(KeyColumn<TKey> keyColumn, params Column<TKey>[] columns)
        {
            KeyColumn = keyColumn;
            _columns = new List<Column<TKey>>(columns);
        }

        public bool Keyed => KeyColumn != null;
        public KeyColumn<TKey> KeyColumn { get; }

        public IEnumerable<Column<TKey>> Columns
        {
            get
            {
                foreach (var column in _columns)
                {
                    yield return column;
                }
            }
        }

        public bool Any()
        {
            return Columns.Any(c => c.Any());
        }
    }
}