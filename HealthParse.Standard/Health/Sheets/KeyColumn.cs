using System.Collections.Generic;
using System.Linq;

namespace HealthParse.Standard.Health.Sheets
{
    public class KeyColumn<TKey> : Column<TKey>
    {
        public KeyColumn(IEnumerable<TKey> keys)
        {
            foreach (var key in keys)
            {
                Add(key, key);
            }
        }
        public KeyColumn() : this(Enumerable.Empty<TKey>())
        {
        }

        public void Add(TKey key)
        {
            Add(key, key);
        }
    }
}