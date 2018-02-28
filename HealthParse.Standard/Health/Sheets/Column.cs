using System.Collections;
using System.Collections.Generic;

namespace HealthParse.Standard.Health.Sheets
{
    public class Column<TKey>: IEnumerable<TKey>
    {
        private readonly Dictionary<TKey, object> _data;

        public string Header { get; set; }
        public string RangeName { get; set; }

        public Column()
        {
            _data = new Dictionary<TKey, object>();
        }

        public void Add(TKey key, object value)
        {
            _data.Add(key, value);
        }

        public object this[TKey key] => _data.GetValue(key);

        public IEnumerable<object> Values => _data.Values;

        public IEnumerator<TKey> GetEnumerator()
        {
            return _data.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _data.Keys.GetEnumerator();
        }
    }
}