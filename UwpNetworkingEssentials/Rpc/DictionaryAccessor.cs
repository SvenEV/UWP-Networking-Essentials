using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace UwpNetworkingEssentials.Rpc
{
    internal static class DictionaryAccessorExtensions
    {
        public static DictionaryAccessor<TKey, TValue, TSelectedValue> ToDictionaryAccessor<TKey, TValue, TSelectedValue>(
            this IReadOnlyDictionary<TKey, TValue> dictionary, Func<TValue, TSelectedValue> selector)
        {
            return new DictionaryAccessor<TKey, TValue, TSelectedValue>(dictionary, selector);
        }
    }

    internal class DictionaryAccessor<TKey, TValue, TSelectedValue> : IReadOnlyDictionary<TKey, TSelectedValue>
    {
        private readonly IReadOnlyDictionary<TKey, TValue> _dictionary;
        private readonly Func<TValue, TSelectedValue> _selectValue;

        public DictionaryAccessor(IReadOnlyDictionary<TKey, TValue> dictionary, Func<TValue, TSelectedValue> selectValue)
        {
            _dictionary = dictionary;
            _selectValue = selectValue;
        }

        public TSelectedValue this[TKey key] => _selectValue(_dictionary[key]);

        public IEnumerable<TKey> Keys => _dictionary.Keys;

        public IEnumerable<TSelectedValue> Values => _dictionary.Values.Select(_selectValue);

        public int Count => _dictionary.Count;

        public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

        public IEnumerator<KeyValuePair<TKey, TSelectedValue>> GetEnumerator()
        {
            return _dictionary
                .Select(kvp => new KeyValuePair<TKey, TSelectedValue>(kvp.Key, _selectValue(kvp.Value)))
                .GetEnumerator();
        }

        public bool TryGetValue(TKey key, out TSelectedValue value)
        {
            if (_dictionary.TryGetValue(key, out var v))
            {
                value = _selectValue(v);
                return true;
            }

            value = default(TSelectedValue);
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
