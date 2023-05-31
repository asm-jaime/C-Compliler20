namespace OnlineCompiler.Client.Pages;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

 public class Dictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary, IReadOnlyDictionary<TKey, TValue> where TKey : notnull
    {
        private int[]? _buckets;
        private Entry[]? _entries;
        private int _count;
        private int _freeList;
        private int _freeCount;
        private int _version;
        private IEqualityComparer<TKey>? _comparer;
        private KeyCollection? _keys;
        private ValueCollection? _values;
        private const int StartOfFreeList = -3;

        public Dictionary() : this(0, null) { }

        public Dictionary(int capacity) : this(capacity, null) { }

        public Dictionary(IEqualityComparer<TKey>? comparer) : this(0, comparer) { }

        public Dictionary(int capacity, IEqualityComparer<TKey>? comparer)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (capacity > 0)
            {
                Initialize(capacity);
            }

            if (comparer != null && comparer != EqualityComparer<TKey>.Default) // first check for null to avoid forcing default comparer instantiation unnecessarily
            {
                _comparer = comparer;
            }

            // Special-case EqualityComparer<string>.Default, StringComparer.Ordinal, and StringComparer.OrdinalIgnoreCase.
            // We use a non-randomized comparer for improved perf, falling back to a randomized comparer if the
            // hash buckets become unbalanced.

            if (typeof(TKey) == typeof(string))
            {
                if (_comparer is null)
                {
                    _comparer = (IEqualityComparer<TKey>)EqualityComparer<string?>.Default;
                }
                else if (ReferenceEquals(_comparer, StringComparer.Ordinal))
                {
                    _comparer = (IEqualityComparer<TKey>)StringComparer.Ordinal;
                }
                else if (ReferenceEquals(_comparer, StringComparer.OrdinalIgnoreCase))
                {
                    _comparer = (IEqualityComparer<TKey>)StringComparer.OrdinalIgnoreCase;
                }
            }
        }

        public Dictionary(IDictionary<TKey, TValue> dictionary) : this(dictionary, null) { }

        public Dictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey>? comparer) :
            this(dictionary != null ? dictionary.Count : 0, comparer)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException();
            }

            // It is likely that the passed-in dictionary is Dictionary<TKey,TValue>. When this is the case,
            // avoid the enumerator allocation and overhead by looping through the entries array directly.
            // We only do this when dictionary is Dictionary<TKey,TValue> and not a subclass, to maintain
            // back-compat with subclasses that may have overridden the enumerator behavior.
            if (dictionary.GetType() == typeof(Dictionary<TKey, TValue>))
            {
                Dictionary<TKey, TValue> d = (Dictionary<TKey, TValue>)dictionary;
                int count = d._count;
                Entry[]? entries = d._entries;
                for (int i = 0; i < count; i++)
                {
                    if (entries![i].next >= -1)
                    {
                        Add(entries[i].key, entries[i].value);
                    }
                }
                return;
            }

            foreach (KeyValuePair<TKey, TValue> pair in dictionary)
            {
                Add(pair.Key, pair.Value);
            }
        }

        public Dictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection) : this(collection, null) { }

        public Dictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey>? comparer) :
            this((collection as ICollection<KeyValuePair<TKey, TValue>>)?.Count ?? 0, comparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException();
            }

            foreach (KeyValuePair<TKey, TValue> pair in collection)
            {
                Add(pair.Key, pair.Value);
            }
        }

        public int Count => _count - _freeCount;

        public KeyCollection Keys => _keys ??= new KeyCollection(this);

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

        public ValueCollection Values => _values ??= new ValueCollection(this);

        ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

        public TValue this[TKey key]
        {
            get
            {
                ref TValue value = ref FindValue(key);
                if (!Unsafe.IsNullRef(ref value))
                {
                    return value;
                }

                throw new KeyNotFoundException();
            }
            set
            {
                bool modified = TryInsert(key, value, InsertionBehavior.OverwriteExisting);
                Debug.Assert(modified);
            }
        }

        public void Add(TKey key, TValue value)
        {
            bool modified = TryInsert(key, value, InsertionBehavior.ThrowOnExisting);
            Debug.Assert(modified); // If there was an existing key and the Add failed, an exception will already have been thrown.
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair) =>
            Add(keyValuePair.Key, keyValuePair.Value);

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
        {
            ref TValue value = ref FindValue(keyValuePair.Key);
            if (!Unsafe.IsNullRef(ref value) && EqualityComparer<TValue>.Default.Equals(value, keyValuePair.Value))
            {
                return true;
            }

            return false;
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
        {
            ref TValue value = ref FindValue(keyValuePair.Key);
            if (!Unsafe.IsNullRef(ref value) && EqualityComparer<TValue>.Default.Equals(value, keyValuePair.Value))
            {
                Remove(keyValuePair.Key);
                return true;
            }

            return false;
        }

        public void Clear()
        {
            int count = _count;
            if (count > 0)
            {

                Array.Clear(_buckets, 0, _buckets.Length);

                _count = 0;
                _freeList = -1;
                _freeCount = 0;
                Array.Clear(_entries, 0, count);
            }
        }

        public bool ContainsKey(TKey key) =>
            !Unsafe.IsNullRef(ref FindValue(key));

        public bool ContainsValue(TValue value)
        {
            Entry[]? entries = _entries;
            if (value == null)
            {
                for (int i = 0; i < _count; i++)
                {
                    if (entries![i].next >= -1 && entries[i].value == null)
                    {
                        return true;
                    }
                }
            }
            else if (typeof(TValue).IsValueType)
            {
                // ValueType: Devirtualize with EqualityComparer<TValue>.Default intrinsic
                for (int i = 0; i < _count; i++)
                {
                    if (entries![i].next >= -1 && EqualityComparer<TValue>.Default.Equals(entries[i].value, value))
                    {
                        return true;
                    }
                }
            }
            else
            {
                // Object type: Shared Generic, EqualityComparer<TValue>.Default won't devirtualize
                // https://github.com/dotnet/runtime/issues/10050
                // So cache in a local rather than get EqualityComparer per loop iteration
                EqualityComparer<TValue> defaultComparer = EqualityComparer<TValue>.Default;
                for (int i = 0; i < _count; i++)
                {
                    if (entries![i].next >= -1 && defaultComparer.Equals(entries[i].value, value))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException();
            }

            if ((uint)index > (uint)array.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (array.Length - index < Count)
            {
                throw new ArgumentException();
            }

            int count = _count;
            Entry[]? entries = _entries;
            for (int i = 0; i < count; i++)
            {
                if (entries![i].next >= -1)
                {
                    array[index++] = new KeyValuePair<TKey, TValue>(entries[i].key, entries[i].value);
                }
            }
        }

        public Enumerator GetEnumerator() => new Enumerator(this, Enumerator.KeyValuePair);

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() =>
            new Enumerator(this, Enumerator.KeyValuePair);
        

        private ref TValue FindValue(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException();
            }

            ref Entry entry = ref Unsafe.NullRef<Entry>();
            if (_buckets != null)
            {
                IEqualityComparer<TKey>? comparer = _comparer;
                if (comparer == null)
                {
                    uint hashCode = (uint)key.GetHashCode();
                    int i = GetBucket(hashCode);
                    Entry[]? entries = _entries;
                    uint collisionCount = 0;
                    if (typeof(TKey).IsValueType)
                    {
                        // ValueType: Devirtualize with EqualityComparer<TValue>.Default intrinsic

                        i--; // Value in _buckets is 1-based; subtract 1 from i. We do it here so it fuses with the following conditional.
                        do
                        {
                            // Should be a while loop https://github.com/dotnet/runtime/issues/9422
                            // Test in if to drop range check for following array access
                            if ((uint)i >= (uint)entries.Length)
                            {
                                goto ReturnNotFound;
                            }

                            entry = ref entries[i];
                            if (entry.hashCode == hashCode && EqualityComparer<TKey>.Default.Equals(entry.key, key))
                            {
                                goto ReturnFound;
                            }

                            i = entry.next;

                            collisionCount++;
                        } while (collisionCount <= (uint)entries.Length);

                        // The chain of entries forms a loop; which means a concurrent update has happened.
                        // Break out of the loop and throw, rather than looping forever.
                        goto ConcurrentOperation;
                    }
                    else
                    {
                        // Object type: Shared Generic, EqualityComparer<TValue>.Default won't devirtualize
                        // https://github.com/dotnet/runtime/issues/10050
                        // So cache in a local rather than get EqualityComparer per loop iteration
                        EqualityComparer<TKey> defaultComparer = EqualityComparer<TKey>.Default;

                        i--; // Value in _buckets is 1-based; subtract 1 from i. We do it here so it fuses with the following conditional.
                        do
                        {
                            // Should be a while loop https://github.com/dotnet/runtime/issues/9422
                            // Test in if to drop range check for following array access
                            if ((uint)i >= (uint)entries.Length)
                            {
                                goto ReturnNotFound;
                            }

                            entry = ref entries[i];
                            if (entry.hashCode == hashCode && defaultComparer.Equals(entry.key, key))
                            {
                                goto ReturnFound;
                            }

                            i = entry.next;

                            collisionCount++;
                        } while (collisionCount <= (uint)entries.Length);

                        // The chain of entries forms a loop; which means a concurrent update has happened.
                        // Break out of the loop and throw, rather than looping forever.
                        goto ConcurrentOperation;
                    }
                }
                else
                {
                    uint hashCode = (uint)comparer.GetHashCode(key);
                    int i = GetBucket(hashCode);
                    Entry[]? entries = _entries;
                    uint collisionCount = 0;
                    i--; // Value in _buckets is 1-based; subtract 1 from i. We do it here so it fuses with the following conditional.
                    do
                    {
                        // Should be a while loop https://github.com/dotnet/runtime/issues/9422
                        // Test in if to drop range check for following array access
                        if ((uint)i >= (uint)entries.Length)
                        {
                            goto ReturnNotFound;
                        }

                        entry = ref entries[i];
                        if (entry.hashCode == hashCode && comparer.Equals(entry.key, key))
                        {
                            goto ReturnFound;
                        }

                        i = entry.next;

                        collisionCount++;
                    } while (collisionCount <= (uint)entries.Length);

                    // The chain of entries forms a loop; which means a concurrent update has happened.
                    // Break out of the loop and throw, rather than looping forever.
                    goto ConcurrentOperation;
                }
            }

            goto ReturnNotFound;

        ConcurrentOperation:
        throw new InvalidOperationException();
        ReturnFound:
            ref TValue value = ref entry.value;
        Return:
            return ref value;
        ReturnNotFound:
            value = ref Unsafe.NullRef<TValue>();
            goto Return;
        }
        
        private static int GetPrime(int min)
        {
            int[] s_primes =
            {
                3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919,
                1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591,
                17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437,
                187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263,
                1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369
            };
            int HashPrime = 101;
            
            if (min < 0)
                throw new ArgumentException();

            foreach (int prime in s_primes)
            {
                if (prime >= min)
                    return prime;
            }

            // Outside of our predefined table. Compute the hard way.
            for (int i = (min | 1); i < int.MaxValue; i += 2)
            {
                if (IsPrime(i) && ((i - 1) % HashPrime != 0))
                    return i;
            }
            return min;
        }
        
        private static bool IsPrime(int candidate)
        {
            if ((candidate & 1) != 0)
            {
                int limit = (int)Math.Sqrt(candidate);
                for (int divisor = 3; divisor <= limit; divisor += 2)
                {
                    if ((candidate % divisor) == 0)
                        return false;
                }
                return true;
            }
            return candidate == 2;
        }
        
        public static int ExpandPrime(int oldSize)
        {
            int MaxPrimeArrayLength = 0x7FEFFFFD;
            int newSize = 2 * oldSize;

            // Allow the hashtables to grow to maximum possible size (~2G elements) before encountering capacity overflow.
            // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
            if ((uint)newSize > MaxPrimeArrayLength && MaxPrimeArrayLength > oldSize)
            {
                return MaxPrimeArrayLength;
            }

            return GetPrime(newSize);
        }

        private int Initialize(int capacity)
        {
            int size = GetPrime(capacity);
            int[] buckets = new int[size];
            Entry[] entries = new Entry[size];

            // Assign member variables after both arrays allocated to guard against corruption from OOM if second fails
            _freeList = -1;
            _buckets = buckets;
            _entries = entries;

            return size;
        }
        
        private enum InsertionBehavior : byte
        {
            None,
            OverwriteExisting,
            ThrowOnExisting,
        }

        private bool TryInsert(TKey key, TValue value, InsertionBehavior behavior)
        {
            if (key == null)
            {
                throw new ArgumentNullException();
            }

            if (_buckets == null)
            {
                Initialize(0);
            }
            Debug.Assert(_buckets != null);

            Entry[]? entries = _entries;
 

            IEqualityComparer<TKey>? comparer = _comparer;
            uint hashCode = (uint)((comparer == null) ? key.GetHashCode() : comparer.GetHashCode(key));

            uint collisionCount = 0;
            ref int bucket = ref GetBucket(hashCode);
            int i = bucket - 1; // Value in _buckets is 1-based

            if (comparer == null)
            {
                if (typeof(TKey).IsValueType)
                {
                    // ValueType: Devirtualize with EqualityComparer<TValue>.Default intrinsic
                    while (true)
                    {
                        // Should be a while loop https://github.com/dotnet/runtime/issues/9422
                        // Test uint in if rather than loop condition to drop range check for following array access
                        if ((uint)i >= (uint)entries.Length)
                        {
                            break;
                        }

                        if (entries[i].hashCode == hashCode && EqualityComparer<TKey>.Default.Equals(entries[i].key, key))
                        {
                            if (behavior == InsertionBehavior.OverwriteExisting)
                            {
                                entries[i].value = value;
                                return true;
                            }

                            if (behavior == InsertionBehavior.ThrowOnExisting)
                            {
                                throw new Exception();
                            }

                            return false;
                        }

                        i = entries[i].next;

                        collisionCount++;
                        if (collisionCount > (uint)entries.Length)
                        {
                            // The chain of entries forms a loop; which means a concurrent update has happened.
                            // Break out of the loop and throw, rather than looping forever.
                            throw new InvalidOperationException();
                        }
                    }
                }
                else
                {
                    // Object type: Shared Generic, EqualityComparer<TValue>.Default won't devirtualize
                    // https://github.com/dotnet/runtime/issues/10050
                    // So cache in a local rather than get EqualityComparer per loop iteration
                    EqualityComparer<TKey> defaultComparer = EqualityComparer<TKey>.Default;
                    while (true)
                    {
                        // Should be a while loop https://github.com/dotnet/runtime/issues/9422
                        // Test uint in if rather than loop condition to drop range check for following array access
                        if ((uint)i >= (uint)entries.Length)
                        {
                            break;
                        }

                        if (entries[i].hashCode == hashCode && defaultComparer.Equals(entries[i].key, key))
                        {
                            if (behavior == InsertionBehavior.OverwriteExisting)
                            {
                                entries[i].value = value;
                                return true;
                            }

                            if (behavior == InsertionBehavior.ThrowOnExisting)
                            {
                                throw new Exception();
                            }

                            return false;
                        }

                        i = entries[i].next;

                        collisionCount++;
                        if (collisionCount > (uint)entries.Length)
                        {
                            // The chain of entries forms a loop; which means a concurrent update has happened.
                            // Break out of the loop and throw, rather than looping forever.
                            throw new InvalidOperationException();
                        }
                    }
                }
            }
            else
            {
                while (true)
                {
                    // Should be a while loop https://github.com/dotnet/runtime/issues/9422
                    // Test uint in if rather than loop condition to drop range check for following array access
                    if ((uint)i >= (uint)entries.Length)
                    {
                        break;
                    }

                    if (entries[i].hashCode == hashCode && comparer.Equals(entries[i].key, key))
                    {
                        if (behavior == InsertionBehavior.OverwriteExisting)
                        {
                            entries[i].value = value;
                            return true;
                        }

                        if (behavior == InsertionBehavior.ThrowOnExisting)
                        {
                            throw new Exception();
                        }

                        return false;
                    }

                    i = entries[i].next;

                    collisionCount++;
                    if (collisionCount > (uint)entries.Length)
                    {
                        // The chain of entries forms a loop; which means a concurrent update has happened.
                        // Break out of the loop and throw, rather than looping forever.
                        throw new InvalidOperationException();
                    }
                }
            }

            int index;
            if (_freeCount > 0)
            {
                index = _freeList;
                _freeList = StartOfFreeList - entries[_freeList].next;
                _freeCount--;
            }
            else
            {
                int count = _count;
                if (count == entries.Length)
                {
                    Resize();
                    bucket = ref GetBucket(hashCode);
                }
                index = count;
                _count = count + 1;
                entries = _entries;
            }

            ref Entry entry = ref entries![index];
            entry.hashCode = hashCode;
            entry.next = bucket - 1; // Value in _buckets is 1-based
            entry.key = key;
            entry.value = value; // Value in _buckets is 1-based
            bucket = index + 1;
            _version++;
            uint HashCollisionThreshold = 100;

            // Value types never rehash
            if (!typeof(TKey).IsValueType && collisionCount > HashCollisionThreshold && comparer is IEqualityComparer<string?>)
            {
                // If we hit the collision threshold we'll need to switch to the comparer which is using randomized string hashing
                // i.e. EqualityComparer<string>.Default.
                Resize(entries.Length, true);
            }

            return true;
        }

        private void Resize() => Resize(ExpandPrime(_count), false);

        private void Resize(int newSize, bool forceNewHashCodes)
        {
            // Value types never rehash
            Debug.Assert(!forceNewHashCodes || !typeof(TKey).IsValueType);
            Debug.Assert(newSize >= _entries.Length);

            Entry[] entries = new Entry[newSize];

            int count = _count;
            Array.Copy(_entries, entries, count);

            if (!typeof(TKey).IsValueType && forceNewHashCodes)
            {

                for (int i = 0; i < count; i++)
                {
                    if (entries[i].next >= -1)
                    {
                        entries[i].hashCode = (uint)_comparer.GetHashCode(entries[i].key);
                    }
                }

                if (ReferenceEquals(_comparer, EqualityComparer<TKey>.Default))
                {
                    _comparer = null;
                }
            }

            // Assign member variables after both arrays allocated to guard against corruption from OOM if second fails
            _buckets = new int[newSize];
            for (int i = 0; i < count; i++)
            {
                if (entries[i].next >= -1)
                {
                    ref int bucket = ref GetBucket(entries[i].hashCode);
                    entries[i].next = bucket - 1; // Value in _buckets is 1-based
                    bucket = i + 1;
                }
            }

            _entries = entries;
        }

        public bool Remove(TKey key)
        {
            // The overload Remove(TKey key, out TValue value) is a copy of this method with one additional
            // statement to copy the value for entry being removed into the output parameter.
            // Code has been intentionally duplicated for performance reasons.

            if (key == null)
            {
                throw new ArgumentException();
            }

            if (_buckets != null)
            {
                uint collisionCount = 0;
                uint hashCode = (uint)(_comparer?.GetHashCode(key) ?? key.GetHashCode());
                ref int bucket = ref GetBucket(hashCode);
                Entry[]? entries = _entries;
                int last = -1;
                int i = bucket - 1; // Value in buckets is 1-based
                while (i >= 0)
                {
                    ref Entry entry = ref entries[i];

                    if (entry.hashCode == hashCode && (_comparer?.Equals(entry.key, key) ?? EqualityComparer<TKey>.Default.Equals(entry.key, key)))
                    {
                        if (last < 0)
                        {
                            bucket = entry.next + 1; // Value in buckets is 1-based
                        }
                        else
                        {
                            entries[last].next = entry.next;
                        }
                        
                        entry.next = StartOfFreeList - _freeList;

                        if (RuntimeHelpers.IsReferenceOrContainsReferences<TKey>())
                        {
                            entry.key = default!;
                        }

                        if (RuntimeHelpers.IsReferenceOrContainsReferences<TValue>())
                        {
                            entry.value = default!;
                        }

                        _freeList = i;
                        _freeCount++;
                        return true;
                    }

                    last = i;
                    i = entry.next;

                    collisionCount++;
                    if (collisionCount > (uint)entries.Length)
                    {
                        // The chain of entries forms a loop; which means a concurrent update has happened.
                        // Break out of the loop and throw, rather than looping forever.
                        throw new InvalidOperationException();
                    }
                }
            }
            return false;
        }

        public bool Remove(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            // This overload is a copy of the overload Remove(TKey key) with one additional
            // statement to copy the value for entry being removed into the output parameter.
            // Code has been intentionally duplicated for performance reasons.

            if (key == null)
            {
                throw new ArgumentNullException();
            }

            if (_buckets != null)
            {
                uint collisionCount = 0;
                uint hashCode = (uint)(_comparer?.GetHashCode(key) ?? key.GetHashCode());
                ref int bucket = ref GetBucket(hashCode);
                Entry[]? entries = _entries;
                int last = -1;
                int i = bucket - 1; // Value in buckets is 1-based
                while (i >= 0)
                {
                    ref Entry entry = ref entries[i];

                    if (entry.hashCode == hashCode && (_comparer?.Equals(entry.key, key) ?? EqualityComparer<TKey>.Default.Equals(entry.key, key)))
                    {
                        if (last < 0)
                        {
                            bucket = entry.next + 1; // Value in buckets is 1-based
                        }
                        else
                        {
                            entries[last].next = entry.next;
                        }

                        value = entry.value;
                        
                        entry.next = StartOfFreeList - _freeList;

                        if (RuntimeHelpers.IsReferenceOrContainsReferences<TKey>())
                        {
                            entry.key = default!;
                        }

                        if (RuntimeHelpers.IsReferenceOrContainsReferences<TValue>())
                        {
                            entry.value = default!;
                        }

                        _freeList = i;
                        _freeCount++;
                        return true;
                    }

                    last = i;
                    i = entry.next;

                    collisionCount++;
                    if (collisionCount > (uint)entries.Length)
                    {
                        // The chain of entries forms a loop; which means a concurrent update has happened.
                        // Break out of the loop and throw, rather than looping forever.
                        throw new InvalidOperationException();
                    }
                }
            }

            value = default;
            return false;
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            ref TValue valRef = ref FindValue(key);
            if (!Unsafe.IsNullRef(ref valRef))
            {
                value = valRef;
                return true;
            }

            value = default;
            return false;
        }

        public bool TryAdd(TKey key, TValue value) =>
            TryInsert(key, value, InsertionBehavior.None);

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index) =>
            CopyTo(array, index);

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException();
            }

            if (array.Rank != 1)
            {
                throw new ArgumentException();
            }

            if (array.GetLowerBound(0) != 0)
            {
                throw new ArgumentException();
            }

            if ((uint)index > (uint)array.Length)
            {
                throw new IndexOutOfRangeException();
            }

            if (array.Length - index < Count)
            {
                throw new ArgumentException();
            }

            if (array is KeyValuePair<TKey, TValue>[] pairs)
            {
                CopyTo(pairs, index);
            }
            else if (array is DictionaryEntry[] dictEntryArray)
            {
                Entry[]? entries = _entries;
                for (int i = 0; i < _count; i++)
                {
                    if (entries![i].next >= -1)
                    {
                        dictEntryArray[index++] = new DictionaryEntry(entries[i].key, entries[i].value);
                    }
                }
            }
            else
            {
                object[]? objects = array as object[];
                if (objects == null)
                {
                    throw new ArgumentException();
                }

                try
                {
                    int count = _count;
                    Entry[]? entries = _entries;
                    for (int i = 0; i < count; i++)
                    {
                        if (entries![i].next >= -1)
                        {
                            objects[index++] = new KeyValuePair<TKey, TValue>(entries[i].key, entries[i].value);
                        }
                    }
                }
                catch (ArrayTypeMismatchException)
                {
                    throw new ArgumentException();
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this, Enumerator.KeyValuePair);

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => this;

        bool IDictionary.IsFixedSize => false;

        bool IDictionary.IsReadOnly => false;

        ICollection IDictionary.Keys => Keys;

        ICollection IDictionary.Values => Values;

        object? IDictionary.this[object key]
        {
            get
            {
                if (IsCompatibleKey(key))
                {
                    ref TValue value = ref FindValue((TKey)key);
                    if (!Unsafe.IsNullRef(ref value))
                    {
                        return value;
                    }
                }

                return null;
            }
            set
            {
                if (key == null)
                {
                    throw new ArgumentException();
                }
                if (!(default(TValue) == null) && value == null)
                    throw new ArgumentNullException();

                try
                {
                    TKey tempKey = (TKey)key;
                    try
                    {
                        this[tempKey] = (TValue)value!;
                    }
                    catch (InvalidCastException)
                    {
                        throw new Exception();
                    }
                }
                catch (InvalidCastException)
                {
                           throw new Exception();
                }
            }
        }

        private static bool IsCompatibleKey(object key)
        {
            if (key == null)
            {
                throw new ArgumentNullException();
            }
            return key is TKey;
        }

        void IDictionary.Add(object key, object? value)
        {
            if (key == null)
            {
                throw new ArgumentNullException();
            }
            if (!(default(TValue) == null) && value == null)
                throw new ArgumentNullException();

            try
            {
                TKey tempKey = (TKey)key;

                try
                {
                    Add(tempKey, (TValue)value!);
                }
                catch (InvalidCastException)
                {
                    throw new Exception();
                }
            }
            catch (InvalidCastException)
            {
                throw new Exception();
            }
        }

        bool IDictionary.Contains(object key)
        {
            if (IsCompatibleKey(key))
            {
                return ContainsKey((TKey)key);
            }

            return false;
        }

        IDictionaryEnumerator IDictionary.GetEnumerator() => new Enumerator(this, Enumerator.DictEntry);

        void IDictionary.Remove(object key)
        {
            if (IsCompatibleKey(key))
            {
                Remove((TKey)key);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref int GetBucket(uint hashCode)
        {
            int[] buckets = _buckets!;
#if TARGET_64BIT
            return ref buckets[HashHelpers.FastMod(hashCode, (uint)buckets.Length, _fastModMultiplier)];
#else
            return ref buckets[hashCode % (uint)buckets.Length];
#endif
        }

        private struct Entry
        {
            public uint hashCode;
            /// <summary>
            /// 0-based index of next entry in chain: -1 means end of chain
            /// also encodes whether this entry _itself_ is part of the free list by changing sign and subtracting 3,
            /// so -2 means end of free list, -3 means index 0 but on free list, -4 means index 1 but on free list, etc.
            /// </summary>
            public int next;
            public TKey key;     // Key of entry
            public TValue value; // Value of entry
        }

        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDictionaryEnumerator
        {
            private readonly Dictionary<TKey, TValue> _dictionary;
            private readonly int _version;
            private int _index;
            private KeyValuePair<TKey, TValue> _current;
            private readonly int _getEnumeratorRetType;  // What should Enumerator.Current return?

            internal const int DictEntry = 1;
            internal const int KeyValuePair = 2;

            internal Enumerator(Dictionary<TKey, TValue> dictionary, int getEnumeratorRetType)
            {
                _dictionary = dictionary;
                _version = dictionary._version;
                _index = 0;
                _getEnumeratorRetType = getEnumeratorRetType;
                _current = default;
            }

            public bool MoveNext()
            {
                if (_version != _dictionary._version)
                {
                    throw new InvalidOperationException();
                }

                // Use unsigned comparison since we set index to dictionary.count+1 when the enumeration ends.
                // dictionary.count+1 could be negative if dictionary.count is int.MaxValue
                while ((uint)_index < (uint)_dictionary._count)
                {
                    ref Entry entry = ref _dictionary._entries![_index++];

                    if (entry.next >= -1)
                    {
                        _current = new KeyValuePair<TKey, TValue>(entry.key, entry.value);
                        return true;
                    }
                }

                _index = _dictionary._count + 1;
                _current = default;
                return false;
            }

            public KeyValuePair<TKey, TValue> Current => _current;

            public void Dispose() { }

            object? IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || (_index == _dictionary._count + 1))
                    {
                        throw new InvalidOperationException();
                    }

                    if (_getEnumeratorRetType == DictEntry)
                    {
                        return new DictionaryEntry(_current.Key, _current.Value);
                    }

                    return new KeyValuePair<TKey, TValue>(_current.Key, _current.Value);
                }
            }

            void IEnumerator.Reset()
            {
                if (_version != _dictionary._version)
                {
                    throw new InvalidOperationException();
                }

                _index = 0;
                _current = default;
            }

            DictionaryEntry IDictionaryEnumerator.Entry
            {
                get
                {
                    if (_index == 0 || (_index == _dictionary._count + 1))
                    {
                        throw new InvalidOperationException();
                    }

                    return new DictionaryEntry(_current.Key, _current.Value);
                }
            }

            object IDictionaryEnumerator.Key
            {
                get
                {
                    if (_index == 0 || (_index == _dictionary._count + 1))
                    {
                        throw new InvalidOperationException();
                    }

                    return _current.Key;
                }
            }

            object? IDictionaryEnumerator.Value
            {
                get
                {
                    if (_index == 0 || (_index == _dictionary._count + 1))
                    {
                        throw new InvalidOperationException();
                    }

                    return _current.Value;
                }
            }
        }
        
        public sealed class KeyCollection : ICollection<TKey>, ICollection, IReadOnlyCollection<TKey>
        {
            private readonly Dictionary<TKey, TValue> _dictionary;

            public KeyCollection(Dictionary<TKey, TValue> dictionary)
            {
                if (dictionary == null)
                {
                    throw new ArgumentNullException();
                }

                _dictionary = dictionary;
            }

            public Enumerator GetEnumerator() => new Enumerator(_dictionary);

            public void CopyTo(TKey[] array, int index)
            {
                if (array == null)
                {
                    throw new ArgumentNullException();
                }

                if (index < 0 || index > array.Length)
                {
                    throw new IndexOutOfRangeException();
                }

                if (array.Length - index < _dictionary.Count)
                {
                    throw new ArgumentNullException();
                }

                int count = _dictionary._count;
                Entry[]? entries = _dictionary._entries;
                for (int i = 0; i < count; i++)
                {
                    if (entries![i].next >= -1) array[index++] = entries[i].key;
                }
            }

            public int Count => _dictionary.Count;

            bool ICollection<TKey>.IsReadOnly => true;

            void ICollection<TKey>.Add(TKey item) =>
                throw new NotSupportedException();

            void ICollection<TKey>.Clear() =>
                throw new NotSupportedException();

            bool ICollection<TKey>.Contains(TKey item) =>
                _dictionary.ContainsKey(item);

            bool ICollection<TKey>.Remove(TKey item)
            {
                throw new NotSupportedException();
                return false;
            }

            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator() => new Enumerator(_dictionary);

            IEnumerator IEnumerable.GetEnumerator() => new Enumerator(_dictionary);

            void ICollection.CopyTo(Array array, int index)
            {
                if (array == null)
                {
                    throw new ArgumentNullException();
                }

                if (array.Rank != 1)
                {
                    throw new ArgumentNullException();
                }

                if (array.GetLowerBound(0) != 0)
                {
                    throw new ArgumentNullException();
                }

                if ((uint)index > (uint)array.Length)
                {
                    throw new IndexOutOfRangeException();
                }

                if (array.Length - index < _dictionary.Count)
                {
                    throw new ArgumentNullException();
                }

                if (array is TKey[] keys)
                {
                    CopyTo(keys, index);
                }
                else
                {
                    object[]? objects = array as object[];
                    if (objects == null)
                    {
                        throw new ArgumentNullException();
                    }

                    int count = _dictionary._count;
                    Entry[]? entries = _dictionary._entries;
                    try
                    {
                        for (int i = 0; i < count; i++)
                        {
                            if (entries![i].next >= -1) objects[index++] = entries[i].key;
                        }
                    }
                    catch (ArrayTypeMismatchException)
                    {
                        throw new ArgumentNullException();
                    }
                }
            }

            bool ICollection.IsSynchronized => false;

            object ICollection.SyncRoot => ((ICollection)_dictionary).SyncRoot;

            public struct Enumerator : IEnumerator<TKey>, IEnumerator
            {
                private readonly Dictionary<TKey, TValue> _dictionary;
                private int _index;
                private readonly int _version;
                private TKey? _currentKey;

                internal Enumerator(Dictionary<TKey, TValue> dictionary)
                {
                    _dictionary = dictionary;
                    _version = dictionary._version;
                    _index = 0;
                    _currentKey = default;
                }

                public void Dispose() { }

                public bool MoveNext()
                {
                    if (_version != _dictionary._version)
                    {
                        throw new InvalidOperationException();
                    }

                    while ((uint)_index < (uint)_dictionary._count)
                    {
                        ref Entry entry = ref _dictionary._entries![_index++];

                        if (entry.next >= -1)
                        {
                            _currentKey = entry.key;
                            return true;
                        }
                    }

                    _index = _dictionary._count + 1;
                    _currentKey = default;
                    return false;
                }

                public TKey Current => _currentKey!;

                object? IEnumerator.Current
                {
                    get
                    {
                        if (_index == 0 || (_index == _dictionary._count + 1))
                        {
                            throw new InvalidOperationException();
                        }

                        return _currentKey;
                    }
                }

                void IEnumerator.Reset()
                {
                    if (_version != _dictionary._version)
                    {
                        throw new InvalidOperationException();
                    }

                    _index = 0;
                    _currentKey = default;
                }
            }
        }
        
        public sealed class ValueCollection : ICollection<TValue>, ICollection, IReadOnlyCollection<TValue>
        {
            private readonly Dictionary<TKey, TValue> _dictionary;

            public ValueCollection(Dictionary<TKey, TValue> dictionary)
            {
                if (dictionary == null)
                {
                    throw new ArgumentNullException();
                }

                _dictionary = dictionary;
            }

            public Enumerator GetEnumerator() => new Enumerator(_dictionary);

            public void CopyTo(TValue[] array, int index)
            {
                if (array == null)
                {
                    throw new ArgumentNullException();
                }

                if ((uint)index > array.Length)
                {
                    throw new ArgumentNullException();
                }

                if (array.Length - index < _dictionary.Count)
                {
                    throw new ArgumentNullException();
                }

                int count = _dictionary._count;
                Entry[]? entries = _dictionary._entries;
                for (int i = 0; i < count; i++)
                {
                    if (entries![i].next >= -1) array[index++] = entries[i].value;
                }
            }

            public int Count => _dictionary.Count;

            bool ICollection<TValue>.IsReadOnly => true;

            void ICollection<TValue>.Add(TValue item) =>
                throw new ArgumentNullException();

            bool ICollection<TValue>.Remove(TValue item)
            {
                throw new ArgumentNullException();
                return false;
            }

            void ICollection<TValue>.Clear() =>
                throw new ArgumentNullException();

            bool ICollection<TValue>.Contains(TValue item) => _dictionary.ContainsValue(item);

            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => new Enumerator(_dictionary);

            IEnumerator IEnumerable.GetEnumerator() => new Enumerator(_dictionary);

            void ICollection.CopyTo(Array array, int index)
            {
                if (array == null)
                {
                    throw new ArgumentNullException();
                }

                if (array.Rank != 1)
                {
                    throw new ArgumentNullException();
                }

                if (array.GetLowerBound(0) != 0)
                {
                    throw new ArgumentNullException();
                }

                if ((uint)index > (uint)array.Length)
                {
                    throw new ArgumentNullException();
                }

                if (array.Length - index < _dictionary.Count)
                {
                    throw new ArgumentNullException();
                }

                if (array is TValue[] values)
                {
                    CopyTo(values, index);
                }
                else
                {
                    object[]? objects = array as object[];
                    if (objects == null)
                    {
                        throw new ArgumentNullException();
                    }

                    int count = _dictionary._count;
                    Entry[]? entries = _dictionary._entries;
                    try
                    {
                        for (int i = 0; i < count; i++)
                        {
                            if (entries![i].next >= -1) objects[index++] = entries[i].value!;
                        }
                    }
                    catch (ArrayTypeMismatchException)
                    {
                        throw new ArgumentNullException();
                    }
                }
            }

            bool ICollection.IsSynchronized => false;

            object ICollection.SyncRoot => ((ICollection)_dictionary).SyncRoot;

            public struct Enumerator : IEnumerator<TValue>, IEnumerator
            {
                private readonly Dictionary<TKey, TValue> _dictionary;
                private int _index;
                private readonly int _version;
                private TValue? _currentValue;

                internal Enumerator(Dictionary<TKey, TValue> dictionary)
                {
                    _dictionary = dictionary;
                    _version = dictionary._version;
                    _index = 0;
                    _currentValue = default;
                }

                public void Dispose() { }

                public bool MoveNext()
                {
                    if (_version != _dictionary._version)
                    {
                        throw new ArgumentNullException();
                    }

                    while ((uint)_index < (uint)_dictionary._count)
                    {
                        ref Entry entry = ref _dictionary._entries![_index++];

                        if (entry.next >= -1)
                        {
                            _currentValue = entry.value;
                            return true;
                        }
                    }
                    _index = _dictionary._count + 1;
                    _currentValue = default;
                    return false;
                }

                public TValue Current => _currentValue!;

                object? IEnumerator.Current
                {
                    get
                    {
                        if (_index == 0 || (_index == _dictionary._count + 1))
                        {
                            throw new ArgumentNullException();
                        }

                        return _currentValue;
                    }
                }

                void IEnumerator.Reset()
                {
                    if (_version != _dictionary._version)
                    {
                        throw new ArgumentNullException();
                    }

                    _index = 0;
                    _currentValue = default;
                }
            }
        }
    }