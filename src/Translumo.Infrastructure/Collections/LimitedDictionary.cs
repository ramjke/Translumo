using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Translumo.Utils;

namespace Translumo.Infrastructure.Collections
{
    public class LimitedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        public TValue this[TKey key]
        {
            get => _dictionary[key];
            set => AddOrUpdate(key, value, false);
        }

        public ICollection<TKey> Keys => _dictionary.Keys;
        public ICollection<TValue> Values => _dictionary.Values;
        public int Count => _dictionary.Count;
        public bool IsReadOnly => _dictionary.IsReadOnly;

        public int Capacity
        {
            get => _capacity;
            set
            {
                if (value < 1)
                {
                    throw new ArgumentException($"Invalid capacity");
                }

                Clear();
                _capacity = value;
            }
        }
        public int BackupThresholdRatio { get; set; } = 2;

        private int BackupCacheThreshold => Capacity / BackupThresholdRatio;
        private int _capacity;
        private IDictionary<TKey, TValue> _dictionary;
        private IDictionary<TKey, TValue> _backupDictionary;

        public LimitedDictionary(int capacity, [CanBeNull] IEqualityComparer<TKey> equalityComparer)
        {
            if (capacity < 1)
            {
                throw new ArgumentException($"Invalid capacity");
            }

            _capacity = capacity;
            _dictionary = new Dictionary<TKey, TValue>(equalityComparer);
            _backupDictionary = new Dictionary<TKey, TValue>(equalityComparer);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            AddOrUpdate(item.Key, item.Value, true);
        }

        public void Clear()
        {
            _dictionary.Clear();
            _backupDictionary.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _dictionary.Contains(item);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            _backupDictionary.Remove(item);

            return _dictionary.Remove(item);
        }

        public void Add(TKey key, TValue value)
        {
            AddOrUpdate(key, value, true);
        }

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            _backupDictionary.Remove(key);

            return _dictionary.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        private void AddOrUpdate(TKey key, TValue value, bool onlyAdd)
        {
            var containsKey = _dictionary.ContainsKey(key);
            if (onlyAdd && containsKey)
            {
                throw new ArgumentException("Item already exists in dictionary");
            }

            if (!containsKey && _dictionary.Count() >= Capacity)
            {
                (_dictionary, _backupDictionary) = (_backupDictionary, _dictionary);
                _backupDictionary.Clear();
            }

            if (!_dictionary.TryGetValue(key, out var curValue) || !curValue.Equals(value))
            {
                _dictionary[key] = value;
                if (_dictionary.Count >= BackupCacheThreshold)
                {
                    _backupDictionary[key] = value;
                }
            }
        }
    }
}
