namespace OnlineCompiler.Client.Pages;

public static class TemplateSortedList
{
    public static string SortedListCode=@"
    using System.Runtime.CompilerServices;

namespace System.Collections.Generic
{
    public class SortedList<TKey, TValue> :
        IDictionary<TKey, TValue>, IDictionary, IReadOnlyDictionary<TKey, TValue> where TKey : notnull
    {
        private TKey[] keys; // Do not rename (binary serialization)
        private TValue[] values; // Do not rename (binary serialization)
        private int _size; // Do not rename (binary serialization)
        private int version; // Do not rename (binary serialization)
        private readonly IComparer<TKey> comparer; // Do not rename (binary serialization)
        private KeyList? keyList; // Do not rename (binary serialization)
        private ValueList? valueList; // Do not rename (binary serialization)

        private const int DefaultCapacity = 4;

        // Constructs a new sorted list. The sorted list is initially empty and has
        // a capacity of zero. Upon adding the first element to the sorted list the
        // capacity is increased to DefaultCapacity, and then increased in multiples of two as
        // required. The elements of the sorted list are ordered according to the
        // IComparable interface, which must be implemented by the keys of
        // all entries added to the sorted list.
        public SortedList()
        {
            keys = Array.Empty<TKey>();
            values = Array.Empty<TValue>();
            _size = 0;
            comparer = Comparer<TKey>.Default;
        }

        // Constructs a new sorted list. The sorted list is initially empty and has
        // a capacity of zero. Upon adding the first element to the sorted list the
        // capacity is increased to 16, and then increased in multiples of two as
        // required. The elements of the sorted list are ordered according to the
        // IComparable interface, which must be implemented by the keys of
        // all entries added to the sorted list.
        //
        public SortedList(int capacity)
        {
            if (capacity < 0)
                throw new InvalidOperationException();
            keys = new TKey[capacity];
            values = new TValue[capacity];
            comparer = Comparer<TKey>.Default;
        }

        // Constructs a new sorted list with a given IComparer
        // implementation. The sorted list is initially empty and has a capacity of
        // zero. Upon adding the first element to the sorted list the capacity is
        // increased to 16, and then increased in multiples of two as required. The
        // elements of the sorted list are ordered according to the given
        // IComparer implementation. If comparer is null, the
        // elements are compared to each other using the IComparable
        // interface, which in that case must be implemented by the keys of all
        // entries added to the sorted list.
        //
        public SortedList(IComparer<TKey>? comparer)
            : this()
        {
            if (comparer != null)
            {
                this.comparer = comparer;
            }
        }

        // Constructs a new sorted dictionary with a given IComparer
        // implementation and a given initial capacity. The sorted list is
        // initially empty, but will have room for the given number of elements
        // before any reallocations are required. The elements of the sorted list
        // are ordered according to the given IComparer implementation. If
        // comparer is null, the elements are compared to each other using
        // the IComparable interface, which in that case must be implemented
        // by the keys of all entries added to the sorted list.
        //
        public SortedList(int capacity, IComparer<TKey>? comparer)
            : this(comparer)
        {
            Capacity = capacity;
        }

        // Constructs a new sorted list containing a copy of the entries in the
        // given dictionary. The elements of the sorted list are ordered according
        // to the IComparable interface, which must be implemented by the
        // keys of all entries in the given dictionary as well as keys
        // subsequently added to the sorted list.
        //
        public SortedList(IDictionary<TKey, TValue> dictionary)
            : this(dictionary, null)
        {
        }

        // Constructs a new sorted list containing a copy of the entries in the
        // given dictionary. The elements of the sorted list are ordered according
        // to the given IComparer implementation. If comparer is
        // null, the elements are compared to each other using the
        // IComparable interface, which in that case must be implemented
        // by the keys of all entries in the given dictionary as well as keys
        // subsequently added to the sorted list.
        //
        public SortedList(IDictionary<TKey, TValue> dictionary, IComparer<TKey>? comparer)
            : this((dictionary != null ? dictionary.Count : 0), comparer)
        {
            if (dictionary == null)
                throw new InvalidOperationException();

            int count = dictionary.Count;
            if (count != 0)
            {
                TKey[] keys = this.keys;
                dictionary.Keys.CopyTo(keys, 0);
                dictionary.Values.CopyTo(values, 0);
                if (count > 1)
                {
                    comparer = Comparer; // obtain default if this is null.
                    Array.Sort<TKey, TValue>(keys, values, comparer);
                    for (int i = 1; i != keys.Length; ++i)
                    {
                        if (comparer.Compare(keys[i - 1], keys[i]) == 0)
                        {
                            throw new InvalidOperationException();
                        }
                    }
                }
            }

            _size = count;
        }

        // Adds an entry with the given key and value to this sorted list. An
        // ArgumentException is thrown if the key is already present in the sorted list.
        //
        public void Add(TKey key, TValue value)
        {
            if (key == null) throw new InvalidOperationException();
            int i = Array.BinarySearch<TKey>(keys, 0, _size, key, comparer);
            if (i >= 0)
                throw new InvalidOperationException();
            Insert(~i, key, value);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair)
        {
            Add(keyValuePair.Key, keyValuePair.Value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
        {
            int index = IndexOfKey(keyValuePair.Key);
            if (index >= 0 && EqualityComparer<TValue>.Default.Equals(values[index], keyValuePair.Value))
            {
                return true;
            }
            return false;
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
        {
            int index = IndexOfKey(keyValuePair.Key);
            if (index >= 0 && EqualityComparer<TValue>.Default.Equals(values[index], keyValuePair.Value))
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        // Returns the capacity of this sorted list. The capacity of a sorted list
        // represents the allocated length of the internal arrays used to store the
        // keys and values of the list, and thus also indicates the maximum number
        // of entries the list can contain before a reallocation of the internal
        // arrays is required.
        //
        public int Capacity
        {
            get
            {
                return keys.Length;
            }
            set
            {
                if (value != keys.Length)
                {
                    if (value < _size)
                    {
                        throw new InvalidOperationException();
                    }

                    if (value > 0)
                    {
                        TKey[] newKeys = new TKey[value];
                        TValue[] newValues = new TValue[value];
                        if (_size > 0)
                        {
                            Array.Copy(keys, newKeys, _size);
                            Array.Copy(values, newValues, _size);
                        }
                        keys = newKeys;
                        values = newValues;
                    }
                    else
                    {
                        keys = Array.Empty<TKey>();
                        values = Array.Empty<TValue>();
                    }
                }
            }
        }

        public IComparer<TKey> Comparer
        {
            get
            {
                return comparer;
            }
        }

        void IDictionary.Add(object key, object? value)
        {
            if (key == null)
                throw new InvalidOperationException();

            if (value == null && !(default(TValue) == null))    // null is an invalid value for Value types
                throw new InvalidOperationException();

            if (!(key is TKey))
                throw new InvalidOperationException();

            if (!(value is TValue) && value != null)            // null is a valid value for Reference Types
                throw new InvalidOperationException();

            Add((TKey)key, (TValue)value!);
        }

        // Returns the number of entries in this sorted list.
        public int Count
        {
            get
            {
                return _size;
            }
        }

        // Returns a collection representing the keys of this sorted list. This
        // method returns the same object as GetKeyList, but typed as an
        // ICollection instead of an IList.
        public IList<TKey> Keys
        {
            get
            {
                return GetKeyListHelper();
            }
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get
            {
                return GetKeyListHelper();
            }
        }

        ICollection IDictionary.Keys
        {
            get
            {
                return GetKeyListHelper();
            }
        }

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
        {
            get
            {
                return GetKeyListHelper();
            }
        }

        // Returns a collection representing the values of this sorted list. This
        // method returns the same object as GetValueList, but typed as an
        // ICollection instead of an IList.
        //
        public IList<TValue> Values
        {
            get
            {
                return GetValueListHelper();
            }
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get
            {
                return GetValueListHelper();
            }
        }

        ICollection IDictionary.Values
        {
            get
            {
                return GetValueListHelper();
            }
        }

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
        {
            get
            {
                return GetValueListHelper();
            }
        }

        private KeyList GetKeyListHelper()
        {
            if (keyList == null)
                keyList = new KeyList(this);
            return keyList;
        }

        private ValueList GetValueListHelper()
        {
            if (valueList == null)
                valueList = new ValueList(this);
            return valueList;
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get { return false; }
        }

        bool IDictionary.IsReadOnly
        {
            get { return false; }
        }

        bool IDictionary.IsFixedSize
        {
            get { return false; }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        // Synchronization root for this object.
        object ICollection.SyncRoot => this;

        // Removes all entries from this sorted list.
        public void Clear()
        {
            // clear does not change the capacity
            version++;
            // Don't need to doc this but we clear the elements so that the gc can reclaim the references.
            if (RuntimeHelpers.IsReferenceOrContainsReferences<TKey>())
            {
                Array.Clear(keys, 0, _size);
            }
            if (RuntimeHelpers.IsReferenceOrContainsReferences<TValue>())
            {
                Array.Clear(values, 0, _size);
            }
            _size = 0;
        }

        bool IDictionary.Contains(object key)
        {
            if (IsCompatibleKey(key))
            {
                return ContainsKey((TKey)key);
            }
            return false;
        }

        // Checks if this sorted list contains an entry with the given key.
        public bool ContainsKey(TKey key)
        {
            return IndexOfKey(key) >= 0;
        }

        // Checks if this sorted list contains an entry with the given value. The
        // values of the entries of the sorted list are compared to the given value
        // using the Object.Equals method. This method performs a linear
        // search and is substantially slower than the Contains
        // method.
        public bool ContainsValue(TValue value)
        {
            return IndexOfValue(value) >= 0;
        }

        // Copies the values in this SortedList to an array.
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new InvalidOperationException();
            }

            if (arrayIndex < 0 || arrayIndex > array.Length)
            {
                throw new InvalidOperationException();
            }

            if (array.Length - arrayIndex < Count)
            {
                throw new InvalidOperationException();
            }

            for (int i = 0; i < Count; i++)
            {
                KeyValuePair<TKey, TValue> entry = new KeyValuePair<TKey, TValue>(keys[i], values[i]);
                array[arrayIndex + i] = entry;
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new InvalidOperationException();
            }

            if (array.Rank != 1)
            {
                throw new InvalidOperationException();
            }

            if (array.GetLowerBound(0) != 0)
            {
                throw new InvalidOperationException();
            }

            if (index < 0 || index > array.Length)
            {
                throw new InvalidOperationException();
            }

            if (array.Length - index < Count)
            {
                throw new InvalidOperationException();
            }

            KeyValuePair<TKey, TValue>[]? keyValuePairArray = array as KeyValuePair<TKey, TValue>[];
            if (keyValuePairArray != null)
            {
                for (int i = 0; i < Count; i++)
                {
                    keyValuePairArray[i + index] = new KeyValuePair<TKey, TValue>(keys[i], values[i]);
                }
            }
            else
            {
                object[]? objects = array as object[];
                if (objects == null)
                {
                    throw new InvalidOperationException();
                }

                try
                {
                    for (int i = 0; i < Count; i++)
                    {
                        objects[i + index] = new KeyValuePair<TKey, TValue>(keys[i], values[i]);
                    }
                }
                catch (ArrayTypeMismatchException)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        private const int MaxArrayLength = 0X7FEFFFFF;

        // Ensures that the capacity of this sorted list is at least the given
        // minimum value. The capacity is increased to twice the current capacity
        // or to min, whichever is larger.
        private void EnsureCapacity(int min)
        {
            int newCapacity = keys.Length == 0 ? DefaultCapacity : keys.Length * 2;
            // Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
            // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
            if ((uint)newCapacity > MaxArrayLength) newCapacity = MaxArrayLength;
            if (newCapacity < min) newCapacity = min;
            Capacity = newCapacity;
        }

        // Returns the value of the entry at the given index.
        private TValue GetByIndex(int index)
        {
            if (index < 0 || index >= _size)
                throw new InvalidOperationException();
            return values[index];
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new Enumerator(this, Enumerator.DictEntry);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        // Returns the key of the entry at the given index.
        private TKey GetKey(int index)
        {
            if (index < 0 || index >= _size)
                throw new InvalidOperationException();
            return keys[index];
        }

        // Returns the value associated with the given key. If an entry with the
        // given key is not found, the returned value is null.
        public TValue this[TKey key]
        {
            get
            {
                int i = IndexOfKey(key);
                if (i >= 0)
                    return values[i];

                throw new InvalidOperationException();
            }
            set
            {
                if (((object)key) == null) throw new InvalidOperationException();
                int i = Array.BinarySearch<TKey>(keys, 0, _size, key, comparer);
                if (i >= 0)
                {
                    values[i] = value;
                    version++;
                    return;
                }
                Insert(~i, key, value);
            }
        }

        object? IDictionary.this[object key]
        {
            get
            {
                if (IsCompatibleKey(key))
                {
                    int i = IndexOfKey((TKey)key);
                    if (i >= 0)
                    {
                        return values[i];
                    }
                }

                return null;
            }
            set
            {
                if (!IsCompatibleKey(key))
                {
                    throw new InvalidOperationException();
                }

                if (value == null && !(default(TValue) == null))
                    throw new InvalidOperationException();

                TKey tempKey = (TKey)key;
                try
                {
                    this[tempKey] = (TValue)value!;
                }
                catch (InvalidCastException)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        // Returns the index of the entry with a given key in this sorted list. The
        // key is located through a binary search, and thus the average execution
        // time of this method is proportional to Log2(size), where
        // size is the size of this sorted list. The returned value is -1 if
        // the given key does not occur in this sorted list. Null is an invalid
        // key value.
        public int IndexOfKey(TKey key)
        {
            if (key == null)
                throw new InvalidOperationException();
            int ret = Array.BinarySearch<TKey>(keys, 0, _size, key, comparer);
            return ret >= 0 ? ret : -1;
        }

        // Returns the index of the first occurrence of an entry with a given value
        // in this sorted list. The entry is located through a linear search, and
        // thus the average execution time of this method is proportional to the
        // size of this sorted list. The elements of the list are compared to the
        // given value using the Object.Equals method.
        public int IndexOfValue(TValue value)
        {
            return Array.IndexOf(values, value, 0, _size);
        }

        // Inserts an entry with a given key and value at a given index.
        private void Insert(int index, TKey key, TValue value)
        {
            if (_size == keys.Length) EnsureCapacity(_size + 1);
            if (index < _size)
            {
                Array.Copy(keys, index, keys, index + 1, _size - index);
                Array.Copy(values, index, values, index + 1, _size - index);
            }
            keys[index] = key;
            values[index] = value;
            _size++;
            version++;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            int i = IndexOfKey(key);
            if (i >= 0)
            {
                value = values[i];
                return true;
            }

            value = default;
            return false;
        }

        // Removes the entry at the given index. The size of the sorted list is
        // decreased by one.
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _size)
                throw new InvalidOperationException();
            _size--;
            if (index < _size)
            {
                Array.Copy(keys, index + 1, keys, index, _size - index);
                Array.Copy(values, index + 1, values, index, _size - index);
            }
            if (RuntimeHelpers.IsReferenceOrContainsReferences<TKey>())
            {
                keys[_size] = default(TKey)!;
            }
            if (RuntimeHelpers.IsReferenceOrContainsReferences<TValue>())
            {
                values[_size] = default(TValue)!;
            }
            version++;
        }

        // Removes an entry from this sorted list. If an entry with the specified
        // key exists in the sorted list, it is removed. An ArgumentException is
        // thrown if the key is null.
        public bool Remove(TKey key)
        {
            int i = IndexOfKey(key);
            if (i >= 0)
                RemoveAt(i);
            return i >= 0;
        }

        void IDictionary.Remove(object key)
        {
            if (IsCompatibleKey(key))
            {
                Remove((TKey)key);
            }
        }

        private static bool IsCompatibleKey(object key)
        {
            if (key == null)
            {
                throw new InvalidOperationException();
            }

            return (key is TKey);
        }

        private struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDictionaryEnumerator
        {
            private readonly SortedList<TKey, TValue> _sortedList;
            private TKey? _key;
            private TValue? _value;
            private int _index;
            private readonly int _version;
            private readonly int _getEnumeratorRetType;  // What should Enumerator.Current return?

            internal const int KeyValuePair = 1;
            internal const int DictEntry = 2;

            internal Enumerator(SortedList<TKey, TValue> sortedList, int getEnumeratorRetType)
            {
                _sortedList = sortedList;
                _index = 0;
                _version = _sortedList.version;
                _getEnumeratorRetType = getEnumeratorRetType;
                _key = default;
                _value = default;
            }

            public void Dispose()
            {
                _index = 0;
                _key = default;
                _value = default;
            }

            object IDictionaryEnumerator.Key
            {
                get
                {
                    if (_index == 0 || (_index == _sortedList.Count + 1))
                    {
                        throw new InvalidOperationException();
                    }

                    return _key!;
                }
            }

            public bool MoveNext()
            {
                if (_version != _sortedList.version) throw new InvalidOperationException();

                if ((uint)_index < (uint)_sortedList.Count)
                {
                    _key = _sortedList.keys[_index];
                    _value = _sortedList.values[_index];
                    _index++;
                    return true;
                }

                _index = _sortedList.Count + 1;
                _key = default;
                _value = default;
                return false;
            }

            DictionaryEntry IDictionaryEnumerator.Entry
            {
                get
                {
                    if (_index == 0 || (_index == _sortedList.Count + 1))
                    {
                        throw new InvalidOperationException();
                    }

                    return new DictionaryEntry(_key!, _value);
                }
            }

            public KeyValuePair<TKey, TValue> Current => new KeyValuePair<TKey, TValue>(_key!, _value!);

            object? IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || (_index == _sortedList.Count + 1))
                    {
                        throw new InvalidOperationException();
                    }

                    if (_getEnumeratorRetType == DictEntry)
                    {
                        return new DictionaryEntry(_key!, _value);
                    }
                    else
                    {
                        return new KeyValuePair<TKey, TValue>(_key!, _value!);
                    }
                }
            }

            object? IDictionaryEnumerator.Value
            {
                get
                {
                    if (_index == 0 || (_index == _sortedList.Count + 1))
                    {
                        throw new InvalidOperationException();
                    }

                    return _value;
                }
            }

            void IEnumerator.Reset()
            {
                if (_version != _sortedList.version)
                {
                    throw new InvalidOperationException();
                }

                _index = 0;
                _key = default;
                _value = default;
            }
        }

        private sealed class SortedListKeyEnumerator : IEnumerator<TKey>, IEnumerator
        {
            private readonly SortedList<TKey, TValue> _sortedList;
            private int _index;
            private readonly int _version;
            private TKey? _currentKey;

            internal SortedListKeyEnumerator(SortedList<TKey, TValue> sortedList)
            {
                _sortedList = sortedList;
                _version = sortedList.version;
            }

            public void Dispose()
            {
                _index = 0;
                _currentKey = default;
            }

            public bool MoveNext()
            {
                if (_version != _sortedList.version)
                {
                    throw new InvalidOperationException();
                }

                if ((uint)_index < (uint)_sortedList.Count)
                {
                    _currentKey = _sortedList.keys[_index];
                    _index++;
                    return true;
                }

                _index = _sortedList.Count + 1;
                _currentKey = default;
                return false;
            }

            public TKey Current => _currentKey!;

            object? IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || (_index == _sortedList.Count + 1))
                    {
                        throw new InvalidOperationException();
                    }

                    return _currentKey;
                }
            }

            void IEnumerator.Reset()
            {
                if (_version != _sortedList.version)
                {
                    throw new InvalidOperationException();
                }
                _index = 0;
                _currentKey = default;
            }
        }

        private sealed class SortedListValueEnumerator : IEnumerator<TValue>, IEnumerator
        {
            private readonly SortedList<TKey, TValue> _sortedList;
            private int _index;
            private readonly int _version;
            private TValue? _currentValue;

            internal SortedListValueEnumerator(SortedList<TKey, TValue> sortedList)
            {
                _sortedList = sortedList;
                _version = sortedList.version;
            }

            public void Dispose()
            {
                _index = 0;
                _currentValue = default;
            }

            public bool MoveNext()
            {
                if (_version != _sortedList.version)
                {
                    throw new InvalidOperationException();
                }

                if ((uint)_index < (uint)_sortedList.Count)
                {
                    _currentValue = _sortedList.values[_index];
                    _index++;
                    return true;
                }

                _index = _sortedList.Count + 1;
                _currentValue = default;
                return false;
            }

            public TValue Current => _currentValue!;

            object? IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || (_index == _sortedList.Count + 1))
                    {
                        throw new InvalidOperationException();
                    }

                    return _currentValue;
                }
            }

            void IEnumerator.Reset()
            {
                if (_version != _sortedList.version)
                {
                    throw new InvalidOperationException();
                }
                _index = 0;
                _currentValue = default;
            }
        }

        public sealed class KeyList : IList<TKey>, ICollection
        {
            private readonly SortedList<TKey, TValue> _dict; // Do not rename (binary serialization)

            internal KeyList(SortedList<TKey, TValue> dictionary)
            {
                _dict = dictionary;
            }

            public int Count
            {
                get { return _dict._size; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            bool ICollection.IsSynchronized
            {
                get { return false; }
            }

            object ICollection.SyncRoot
            {
                get { return ((ICollection)_dict).SyncRoot; }
            }

            public void Add(TKey key)
            {
                throw new InvalidOperationException();
            }

            public void Clear()
            {
                throw new InvalidOperationException();
            }

            public bool Contains(TKey key)
            {
                return _dict.ContainsKey(key);
            }

            public void CopyTo(TKey[] array, int arrayIndex)
            {
                // defer error checking to Array.Copy
                Array.Copy(_dict.keys, 0, array, arrayIndex, _dict.Count);
            }

            void ICollection.CopyTo(Array array, int arrayIndex)
            {
                if (array != null && array.Rank != 1)
                    throw new InvalidOperationException();

                try
                {
                    // defer error checking to Array.Copy
                    Array.Copy(_dict.keys, 0, array!, arrayIndex, _dict.Count);
                }
                catch (ArrayTypeMismatchException)
                {
                    throw new InvalidOperationException();
                }
            }

            public void Insert(int index, TKey value)
            {
                throw new InvalidOperationException();
            }

            public TKey this[int index]
            {
                get
                {
                    return _dict.GetKey(index);
                }
                set
                {
                    throw new InvalidOperationException();
                }
            }

            public IEnumerator<TKey> GetEnumerator()
            {
                return new SortedListKeyEnumerator(_dict);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new SortedListKeyEnumerator(_dict);
            }

            public int IndexOf(TKey key)
            {
                if (((object)key) == null)
                    throw new InvalidOperationException();

                int i = Array.BinarySearch<TKey>(_dict.keys, 0,
                                          _dict.Count, key, _dict.comparer);
                if (i >= 0) return i;
                return -1;
            }

            public bool Remove(TKey key)
            {
                throw new InvalidOperationException();
                // return false;
            }

            public void RemoveAt(int index)
            {
                throw new InvalidOperationException();
            }
        }


        public sealed class ValueList : IList<TValue>, ICollection
        {
            private readonly SortedList<TKey, TValue> _dict; // Do not rename (binary serialization)

            internal ValueList(SortedList<TKey, TValue> dictionary)
            {
                _dict = dictionary;
            }

            public int Count
            {
                get { return _dict._size; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            bool ICollection.IsSynchronized
            {
                get { return false; }
            }

            object ICollection.SyncRoot
            {
                get { return ((ICollection)_dict).SyncRoot; }
            }

            public void Add(TValue key)
            {
                throw new InvalidOperationException();
            }

            public void Clear()
            {
                throw new InvalidOperationException();
            }

            public bool Contains(TValue value)
            {
                return _dict.ContainsValue(value);
            }

            public void CopyTo(TValue[] array, int arrayIndex)
            {
                // defer error checking to Array.Copy
                Array.Copy(_dict.values, 0, array, arrayIndex, _dict.Count);
            }

            void ICollection.CopyTo(Array array, int index)
            {
                if (array != null && array.Rank != 1)
                    throw new InvalidOperationException();

                try
                {
                    // defer error checking to Array.Copy
                    Array.Copy(_dict.values, 0, array!, index, _dict.Count);
                }
                catch (ArrayTypeMismatchException)
                {
                    throw new InvalidOperationException();
                }
            }

            public void Insert(int index, TValue value)
            {
                throw new InvalidOperationException();
            }

            public TValue this[int index]
            {
                get
                {
                    return _dict.GetByIndex(index);
                }
                set
                {
                    throw new InvalidOperationException();
                }
            }

            public IEnumerator<TValue> GetEnumerator()
            {
                return new SortedListValueEnumerator(_dict);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new SortedListValueEnumerator(_dict);
            }

            public int IndexOf(TValue value)
            {
                return Array.IndexOf(_dict.values, value, 0, _dict.Count);
            }

            public bool Remove(TValue value)
            {
                throw new InvalidOperationException();
                // return false;
            }

            public void RemoveAt(int index)
            {
                throw new InvalidOperationException();
            }
        }
    }
}
";
    
    public static string UserSortedListCode=@"
using System.Runtime.CompilerServices;

namespace System.Collections.Generic
{
    public class SortedList<TKey, TValue> :
        IDictionary<TKey, TValue>, IDictionary, IReadOnlyDictionary<TKey, TValue> where TKey : notnull
    {
        private TKey[] keys; // Do not rename (binary serialization)
        private TValue[] values; // Do not rename (binary serialization)
        private int _size; // Do not rename (binary serialization)
        private int version; // Do not rename (binary serialization)
        private readonly IComparer<TKey> comparer; // Do not rename (binary serialization)
        private KeyList? keyList; // Do not rename (binary serialization)
        private ValueList? valueList; // Do not rename (binary serialization)

        private const int DefaultCapacity = 4;

        // Constructs a new sorted list. The sorted list is initially empty and has
        // a capacity of zero. Upon adding the first element to the sorted list the
        // capacity is increased to DefaultCapacity, and then increased in multiples of two as
        // required. The elements of the sorted list are ordered according to the
        // IComparable interface, which must be implemented by the keys of
        // all entries added to the sorted list.
        public SortedList()
        {
            keys = Array.Empty<TKey>();
            values = Array.Empty<TValue>();
            _size = 0;
            comparer = Comparer<TKey>.Default;
        }

        // Constructs a new sorted list. The sorted list is initially empty and has
        // a capacity of zero. Upon adding the first element to the sorted list the
        // capacity is increased to 16, and then increased in multiples of two as
        // required. The elements of the sorted list are ordered according to the
        // IComparable interface, which must be implemented by the keys of
        // all entries added to the sorted list.
        //
        public SortedList(int capacity)
        {
            if (capacity < 0)
                throw new InvalidOperationException();
            keys = new TKey[capacity];
            values = new TValue[capacity];
            comparer = Comparer<TKey>.Default;
        }

        // Constructs a new sorted list with a given IComparer
        // implementation. The sorted list is initially empty and has a capacity of
        // zero. Upon adding the first element to the sorted list the capacity is
        // increased to 16, and then increased in multiples of two as required. The
        // elements of the sorted list are ordered according to the given
        // IComparer implementation. If comparer is null, the
        // elements are compared to each other using the IComparable
        // interface, which in that case must be implemented by the keys of all
        // entries added to the sorted list.
        //
        public SortedList(IComparer<TKey>? comparer)
            : this()
        {
            if (comparer != null)
            {
                this.comparer = comparer;
            }
        }

        // Constructs a new sorted dictionary with a given IComparer
        // implementation and a given initial capacity. The sorted list is
        // initially empty, but will have room for the given number of elements
        // before any reallocations are required. The elements of the sorted list
        // are ordered according to the given IComparer implementation. If
        // comparer is null, the elements are compared to each other using
        // the IComparable interface, which in that case must be implemented
        // by the keys of all entries added to the sorted list.
        //
        public SortedList(int capacity, IComparer<TKey>? comparer)
            : this(comparer)
        {
            Capacity = capacity;
        }

        // Constructs a new sorted list containing a copy of the entries in the
        // given dictionary. The elements of the sorted list are ordered according
        // to the IComparable interface, which must be implemented by the
        // keys of all entries in the given dictionary as well as keys
        // subsequently added to the sorted list.
        //
        public SortedList(IDictionary<TKey, TValue> dictionary)
            : this(dictionary, null)
        {
        }

        // Constructs a new sorted list containing a copy of the entries in the
        // given dictionary. The elements of the sorted list are ordered according
        // to the given IComparer implementation. If comparer is
        // null, the elements are compared to each other using the
        // IComparable interface, which in that case must be implemented
        // by the keys of all entries in the given dictionary as well as keys
        // subsequently added to the sorted list.
        //
        public SortedList(IDictionary<TKey, TValue> dictionary, IComparer<TKey>? comparer)
            : this((dictionary != null ? dictionary.Count : 0), comparer)
        {
            if (dictionary == null)
                throw new InvalidOperationException();

            int count = dictionary.Count;
            if (count != 0)
            {
                TKey[] keys = this.keys;
                dictionary.Keys.CopyTo(keys, 0);
                dictionary.Values.CopyTo(values, 0);
                if (count > 1)
                {
                    comparer = Comparer; // obtain default if this is null.
                    Array.Sort<TKey, TValue>(keys, values, comparer);
                    for (int i = 1; i != keys.Length; ++i)
                    {
                        if (comparer.Compare(keys[i - 1], keys[i]) == 0)
                        {
                            throw new InvalidOperationException();
                        }
                    }
                }
            }

            _size = count;
        }

        // Adds an entry with the given key and value to this sorted list. An
        // ArgumentException is thrown if the key is already present in the sorted list.
        //
        public void Add(TKey key, TValue value)
        {
          //Нужна реализация
          throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair)
        {
            Add(keyValuePair.Key, keyValuePair.Value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
        {
            int index = IndexOfKey(keyValuePair.Key);
            if (index >= 0 && EqualityComparer<TValue>.Default.Equals(values[index], keyValuePair.Value))
            {
                return true;
            }
            return false;
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
        {
            int index = IndexOfKey(keyValuePair.Key);
            if (index >= 0 && EqualityComparer<TValue>.Default.Equals(values[index], keyValuePair.Value))
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        // Returns the capacity of this sorted list. The capacity of a sorted list
        // represents the allocated length of the internal arrays used to store the
        // keys and values of the list, and thus also indicates the maximum number
        // of entries the list can contain before a reallocation of the internal
        // arrays is required.
        //
        public int Capacity
        {
            get
            {
                return keys.Length;
            }
            set
            {
                if (value != keys.Length)
                {
                    if (value < _size)
                    {
                        throw new InvalidOperationException();
                    }

                    if (value > 0)
                    {
                        TKey[] newKeys = new TKey[value];
                        TValue[] newValues = new TValue[value];
                        if (_size > 0)
                        {
                            Array.Copy(keys, newKeys, _size);
                            Array.Copy(values, newValues, _size);
                        }
                        keys = newKeys;
                        values = newValues;
                    }
                    else
                    {
                        keys = Array.Empty<TKey>();
                        values = Array.Empty<TValue>();
                    }
                }
            }
        }

        public IComparer<TKey> Comparer
        {
            get
            {
                return comparer;
            }
        }

        void IDictionary.Add(object key, object? value)
        {
            if (key == null)
                throw new InvalidOperationException();

            if (value == null && !(default(TValue) == null))    // null is an invalid value for Value types
                throw new InvalidOperationException();

            if (!(key is TKey))
                throw new InvalidOperationException();

            if (!(value is TValue) && value != null)            // null is a valid value for Reference Types
                throw new InvalidOperationException();

            Add((TKey)key, (TValue)value!);
        }

        // Returns the number of entries in this sorted list.
        public int Count
        {
            get
            {
                return _size;
            }
        }

        // Returns a collection representing the keys of this sorted list. This
        // method returns the same object as GetKeyList, but typed as an
        // ICollection instead of an IList.
        public IList<TKey> Keys
        {
            get
            {
                return GetKeyListHelper();
            }
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get
            {
                return GetKeyListHelper();
            }
        }

        ICollection IDictionary.Keys
        {
            get
            {
                return GetKeyListHelper();
            }
        }

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
        {
            get
            {
                return GetKeyListHelper();
            }
        }

        // Returns a collection representing the values of this sorted list. This
        // method returns the same object as GetValueList, but typed as an
        // ICollection instead of an IList.
        //
        public IList<TValue> Values
        {
            get
            {
                return GetValueListHelper();
            }
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get
            {
                return GetValueListHelper();
            }
        }

        ICollection IDictionary.Values
        {
            get
            {
                return GetValueListHelper();
            }
        }

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
        {
            get
            {
                return GetValueListHelper();
            }
        }

        private KeyList GetKeyListHelper()
        {
            if (keyList == null)
                keyList = new KeyList(this);
            return keyList;
        }

        private ValueList GetValueListHelper()
        {
            if (valueList == null)
                valueList = new ValueList(this);
            return valueList;
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get { return false; }
        }

        bool IDictionary.IsReadOnly
        {
            get { return false; }
        }

        bool IDictionary.IsFixedSize
        {
            get { return false; }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        // Synchronization root for this object.
        object ICollection.SyncRoot => this;

        // Removes all entries from this sorted list.
        public void Clear()
        {
            // clear does not change the capacity
            version++;
            // Don't need to doc this but we clear the elements so that the gc can reclaim the references.
            if (RuntimeHelpers.IsReferenceOrContainsReferences<TKey>())
            {
                Array.Clear(keys, 0, _size);
            }
            if (RuntimeHelpers.IsReferenceOrContainsReferences<TValue>())
            {
                Array.Clear(values, 0, _size);
            }
            _size = 0;
        }

        bool IDictionary.Contains(object key)
        {
            if (IsCompatibleKey(key))
            {
                return ContainsKey((TKey)key);
            }
            return false;
        }

        // Checks if this sorted list contains an entry with the given key.
        public bool ContainsKey(TKey key)
        {
            return IndexOfKey(key) >= 0;
        }

        // Checks if this sorted list contains an entry with the given value. The
        // values of the entries of the sorted list are compared to the given value
        // using the Object.Equals method. This method performs a linear
        // search and is substantially slower than the Contains
        // method.
        public bool ContainsValue(TValue value)
        {
            return IndexOfValue(value) >= 0;
        }

        // Copies the values in this SortedList to an array.
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new InvalidOperationException();
            }

            if (arrayIndex < 0 || arrayIndex > array.Length)
            {
                throw new InvalidOperationException();
            }

            if (array.Length - arrayIndex < Count)
            {
                throw new InvalidOperationException();
            }

            for (int i = 0; i < Count; i++)
            {
                KeyValuePair<TKey, TValue> entry = new KeyValuePair<TKey, TValue>(keys[i], values[i]);
                array[arrayIndex + i] = entry;
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new InvalidOperationException();
            }

            if (array.Rank != 1)
            {
                throw new InvalidOperationException();
            }

            if (array.GetLowerBound(0) != 0)
            {
                throw new InvalidOperationException();
            }

            if (index < 0 || index > array.Length)
            {
                throw new InvalidOperationException();
            }

            if (array.Length - index < Count)
            {
                throw new InvalidOperationException();
            }

            KeyValuePair<TKey, TValue>[]? keyValuePairArray = array as KeyValuePair<TKey, TValue>[];
            if (keyValuePairArray != null)
            {
                for (int i = 0; i < Count; i++)
                {
                    keyValuePairArray[i + index] = new KeyValuePair<TKey, TValue>(keys[i], values[i]);
                }
            }
            else
            {
                object[]? objects = array as object[];
                if (objects == null)
                {
                    throw new InvalidOperationException();
                }

                try
                {
                    for (int i = 0; i < Count; i++)
                    {
                        objects[i + index] = new KeyValuePair<TKey, TValue>(keys[i], values[i]);
                    }
                }
                catch (ArrayTypeMismatchException)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        private const int MaxArrayLength = 0X7FEFFFFF;

        // Ensures that the capacity of this sorted list is at least the given
        // minimum value. The capacity is increased to twice the current capacity
        // or to min, whichever is larger.
        private void EnsureCapacity(int min)
        {
            int newCapacity = keys.Length == 0 ? DefaultCapacity : keys.Length * 2;
            // Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
            // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
            if ((uint)newCapacity > MaxArrayLength) newCapacity = MaxArrayLength;
            if (newCapacity < min) newCapacity = min;
            Capacity = newCapacity;
        }

        // Returns the value of the entry at the given index.
        private TValue GetByIndex(int index)
        {
            if (index < 0 || index >= _size)
                throw new InvalidOperationException();
            return values[index];
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new Enumerator(this, Enumerator.DictEntry);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        // Returns the key of the entry at the given index.
        private TKey GetKey(int index)
        {
            if (index < 0 || index >= _size)
                throw new InvalidOperationException();
            return keys[index];
        }

        // Returns the value associated with the given key. If an entry with the
        // given key is not found, the returned value is null.
        public TValue this[TKey key]
        {
            get
            {
                int i = IndexOfKey(key);
                if (i >= 0)
                    return values[i];

                throw new InvalidOperationException();
            }
            set
            {
                if (((object)key) == null) throw new InvalidOperationException();
                int i = Array.BinarySearch<TKey>(keys, 0, _size, key, comparer);
                if (i >= 0)
                {
                    values[i] = value;
                    version++;
                    return;
                }
                Insert(~i, key, value);
            }
        }

        object? IDictionary.this[object key]
        {
            get
            {
                if (IsCompatibleKey(key))
                {
                    int i = IndexOfKey((TKey)key);
                    if (i >= 0)
                    {
                        return values[i];
                    }
                }

                return null;
            }
            set
            {
                if (!IsCompatibleKey(key))
                {
                    throw new InvalidOperationException();
                }

                if (value == null && !(default(TValue) == null))
                    throw new InvalidOperationException();

                TKey tempKey = (TKey)key;
                try
                {
                    this[tempKey] = (TValue)value!;
                }
                catch (InvalidCastException)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        // Returns the index of the entry with a given key in this sorted list. The
        // key is located through a binary search, and thus the average execution
        // time of this method is proportional to Log2(size), where
        // size is the size of this sorted list. The returned value is -1 if
        // the given key does not occur in this sorted list. Null is an invalid
        // key value.
        public int IndexOfKey(TKey key)
        {
           //Нужна реализация
            throw new NotImplementedException();
        }

        // Returns the index of the first occurrence of an entry with a given value
        // in this sorted list. The entry is located through a linear search, and
        // thus the average execution time of this method is proportional to the
        // size of this sorted list. The elements of the list are compared to the
        // given value using the Object.Equals method.
        public int IndexOfValue(TValue value)
        {
            return Array.IndexOf(values, value, 0, _size);
        }

        // Inserts an entry with a given key and value at a given index.
        private void Insert(int index, TKey key, TValue value)
        {
            if (_size == keys.Length) EnsureCapacity(_size + 1);
            if (index < _size)
            {
                Array.Copy(keys, index, keys, index + 1, _size - index);
                Array.Copy(values, index, values, index + 1, _size - index);
            }
            keys[index] = key;
            values[index] = value;
            _size++;
            version++;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            int i = IndexOfKey(key);
            if (i >= 0)
            {
                value = values[i];
                return true;
            }

            value = default;
            return false;
        }

        // Removes the entry at the given index. The size of the sorted list is
        // decreased by one.
        public void RemoveAt(int index)
        {
            //Нужна реализация
            throw new NotImplementedException();
        }

        // Removes an entry from this sorted list. If an entry with the specified
        // key exists in the sorted list, it is removed. An ArgumentException is
        // thrown if the key is null.
        public bool Remove(TKey key)
        {
            int i = IndexOfKey(key);
            if (i >= 0)
                RemoveAt(i);
            return i >= 0;
        }

        void IDictionary.Remove(object key)
        {
            if (IsCompatibleKey(key))
            {
                Remove((TKey)key);
            }
        }

        private static bool IsCompatibleKey(object key)
        {
            if (key == null)
            {
                throw new InvalidOperationException();
            }

            return (key is TKey);
        }

        private struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDictionaryEnumerator
        {
            private readonly SortedList<TKey, TValue> _sortedList;
            private TKey? _key;
            private TValue? _value;
            private int _index;
            private readonly int _version;
            private readonly int _getEnumeratorRetType;  // What should Enumerator.Current return?

            internal const int KeyValuePair = 1;
            internal const int DictEntry = 2;

            internal Enumerator(SortedList<TKey, TValue> sortedList, int getEnumeratorRetType)
            {
                _sortedList = sortedList;
                _index = 0;
                _version = _sortedList.version;
                _getEnumeratorRetType = getEnumeratorRetType;
                _key = default;
                _value = default;
            }

            public void Dispose()
            {
                _index = 0;
                _key = default;
                _value = default;
            }

            object IDictionaryEnumerator.Key
            {
                get
                {
                    if (_index == 0 || (_index == _sortedList.Count + 1))
                    {
                        throw new InvalidOperationException();
                    }

                    return _key!;
                }
            }

            public bool MoveNext()
            {
                if (_version != _sortedList.version) throw new InvalidOperationException();

                if ((uint)_index < (uint)_sortedList.Count)
                {
                    _key = _sortedList.keys[_index];
                    _value = _sortedList.values[_index];
                    _index++;
                    return true;
                }

                _index = _sortedList.Count + 1;
                _key = default;
                _value = default;
                return false;
            }

            DictionaryEntry IDictionaryEnumerator.Entry
            {
                get
                {
                    if (_index == 0 || (_index == _sortedList.Count + 1))
                    {
                        throw new InvalidOperationException();
                    }

                    return new DictionaryEntry(_key!, _value);
                }
            }

            public KeyValuePair<TKey, TValue> Current => new KeyValuePair<TKey, TValue>(_key!, _value!);

            object? IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || (_index == _sortedList.Count + 1))
                    {
                        throw new InvalidOperationException();
                    }

                    if (_getEnumeratorRetType == DictEntry)
                    {
                        return new DictionaryEntry(_key!, _value);
                    }
                    else
                    {
                        return new KeyValuePair<TKey, TValue>(_key!, _value!);
                    }
                }
            }

            object? IDictionaryEnumerator.Value
            {
                get
                {
                    if (_index == 0 || (_index == _sortedList.Count + 1))
                    {
                        throw new InvalidOperationException();
                    }

                    return _value;
                }
            }

            void IEnumerator.Reset()
            {
                if (_version != _sortedList.version)
                {
                    throw new InvalidOperationException();
                }

                _index = 0;
                _key = default;
                _value = default;
            }
        }

        private sealed class SortedListKeyEnumerator : IEnumerator<TKey>, IEnumerator
        {
            private readonly SortedList<TKey, TValue> _sortedList;
            private int _index;
            private readonly int _version;
            private TKey? _currentKey;

            internal SortedListKeyEnumerator(SortedList<TKey, TValue> sortedList)
            {
                _sortedList = sortedList;
                _version = sortedList.version;
            }

            public void Dispose()
            {
                _index = 0;
                _currentKey = default;
            }

            public bool MoveNext()
            {
                if (_version != _sortedList.version)
                {
                    throw new InvalidOperationException();
                }

                if ((uint)_index < (uint)_sortedList.Count)
                {
                    _currentKey = _sortedList.keys[_index];
                    _index++;
                    return true;
                }

                _index = _sortedList.Count + 1;
                _currentKey = default;
                return false;
            }

            public TKey Current => _currentKey!;

            object? IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || (_index == _sortedList.Count + 1))
                    {
                        throw new InvalidOperationException();
                    }

                    return _currentKey;
                }
            }

            void IEnumerator.Reset()
            {
                if (_version != _sortedList.version)
                {
                    throw new InvalidOperationException();
                }
                _index = 0;
                _currentKey = default;
            }
        }

        private sealed class SortedListValueEnumerator : IEnumerator<TValue>, IEnumerator
        {
            private readonly SortedList<TKey, TValue> _sortedList;
            private int _index;
            private readonly int _version;
            private TValue? _currentValue;

            internal SortedListValueEnumerator(SortedList<TKey, TValue> sortedList)
            {
                _sortedList = sortedList;
                _version = sortedList.version;
            }

            public void Dispose()
            {
                _index = 0;
                _currentValue = default;
            }

            public bool MoveNext()
            {
                if (_version != _sortedList.version)
                {
                    throw new InvalidOperationException();
                }

                if ((uint)_index < (uint)_sortedList.Count)
                {
                    _currentValue = _sortedList.values[_index];
                    _index++;
                    return true;
                }

                _index = _sortedList.Count + 1;
                _currentValue = default;
                return false;
            }

            public TValue Current => _currentValue!;

            object? IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || (_index == _sortedList.Count + 1))
                    {
                        throw new InvalidOperationException();
                    }

                    return _currentValue;
                }
            }

            void IEnumerator.Reset()
            {
                if (_version != _sortedList.version)
                {
                    throw new InvalidOperationException();
                }
                _index = 0;
                _currentValue = default;
            }
        }

        public sealed class KeyList : IList<TKey>, ICollection
        {
            private readonly SortedList<TKey, TValue> _dict; // Do not rename (binary serialization)

            internal KeyList(SortedList<TKey, TValue> dictionary)
            {
                _dict = dictionary;
            }

            public int Count
            {
                get { return _dict._size; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            bool ICollection.IsSynchronized
            {
                get { return false; }
            }

            object ICollection.SyncRoot
            {
                get { return ((ICollection)_dict).SyncRoot; }
            }

            public void Add(TKey key)
            {
                throw new InvalidOperationException();
            }

            public void Clear()
            {
                throw new InvalidOperationException();
            }

            public bool Contains(TKey key)
            {
                return _dict.ContainsKey(key);
            }

            public void CopyTo(TKey[] array, int arrayIndex)
            {
                // defer error checking to Array.Copy
                Array.Copy(_dict.keys, 0, array, arrayIndex, _dict.Count);
            }

            void ICollection.CopyTo(Array array, int arrayIndex)
            {
                if (array != null && array.Rank != 1)
                    throw new InvalidOperationException();

                try
                {
                    // defer error checking to Array.Copy
                    Array.Copy(_dict.keys, 0, array!, arrayIndex, _dict.Count);
                }
                catch (ArrayTypeMismatchException)
                {
                    throw new InvalidOperationException();
                }
            }

            public void Insert(int index, TKey value)
            {
                throw new InvalidOperationException();
            }

            public TKey this[int index]
            {
                get
                {
                    return _dict.GetKey(index);
                }
                set
                {
                    throw new InvalidOperationException();
                }
            }

            public IEnumerator<TKey> GetEnumerator()
            {
                return new SortedListKeyEnumerator(_dict);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new SortedListKeyEnumerator(_dict);
            }

            public int IndexOf(TKey key)
            {
                if (((object)key) == null)
                    throw new InvalidOperationException();

                int i = Array.BinarySearch<TKey>(_dict.keys, 0,
                                          _dict.Count, key, _dict.comparer);
                if (i >= 0) return i;
                return -1;
            }

            public bool Remove(TKey key)
            {
                throw new InvalidOperationException();
                // return false;
            }

            public void RemoveAt(int index)
            {
                throw new InvalidOperationException();
            }
        }


        public sealed class ValueList : IList<TValue>, ICollection
        {
            private readonly SortedList<TKey, TValue> _dict; // Do not rename (binary serialization)

            internal ValueList(SortedList<TKey, TValue> dictionary)
            {
                _dict = dictionary;
            }

            public int Count
            {
                get { return _dict._size; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            bool ICollection.IsSynchronized
            {
                get { return false; }
            }

            object ICollection.SyncRoot
            {
                get { return ((ICollection)_dict).SyncRoot; }
            }

            public void Add(TValue key)
            {
                throw new InvalidOperationException();
            }

            public void Clear()
            {
                throw new InvalidOperationException();
            }

            public bool Contains(TValue value)
            {
                return _dict.ContainsValue(value);
            }

            public void CopyTo(TValue[] array, int arrayIndex)
            {
                // defer error checking to Array.Copy
                Array.Copy(_dict.values, 0, array, arrayIndex, _dict.Count);
            }

            void ICollection.CopyTo(Array array, int index)
            {
                if (array != null && array.Rank != 1)
                    throw new InvalidOperationException();

                try
                {
                    // defer error checking to Array.Copy
                    Array.Copy(_dict.values, 0, array!, index, _dict.Count);
                }
                catch (ArrayTypeMismatchException)
                {
                    throw new InvalidOperationException();
                }
            }

            public void Insert(int index, TValue value)
            {
                throw new InvalidOperationException();
            }

            public TValue this[int index]
            {
                get
                {
                    return _dict.GetByIndex(index);
                }
                set
                {
                    throw new InvalidOperationException();
                }
            }

            public IEnumerator<TValue> GetEnumerator()
            {
                return new SortedListValueEnumerator(_dict);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new SortedListValueEnumerator(_dict);
            }

            public int IndexOf(TValue value)
            {
                return Array.IndexOf(_dict.values, value, 0, _dict.Count);
            }

            public bool Remove(TValue value)
            {
                throw new InvalidOperationException();
                // return false;
            }

            public void RemoveAt(int index)
            {
                throw new InvalidOperationException();
            }
        }
    }
}
";
}