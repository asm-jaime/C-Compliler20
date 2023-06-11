using NUnit.Framework;
using OnlineCompiler.Client.Pages;
using OnlineCompiler.Server.Handlers;

namespace test
{
    [TestFixture]
    public class CheckHashSetTests
    {
        private static string _invalidHashSetCode = @"
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace System.Collections.Generic
{
    public class HashSet<T> : IReadOnlyCollection<T>, ICollection<T>
    {
        public const int HashCollisionThreshold = 100;

        /// <summary>
        /// When constructing a hashset from an existing collection, it may contain duplicates,
        /// so this is used as the max acceptable excess ratio of capacity to count. Note that
        /// this is only used on the ctor and not to automatically shrink if the hashset has, e.g,
        /// a lot of adds followed by removes. Users must explicitly shrink by calling TrimExcess.
        /// This is set to 3 because capacity is acceptable as 2x rounded up to nearest prime.
        /// </summary>
        public const int ShrinkThreshold = 3;

        public const int StartOfFreeList = -3;

        public int[]? _buckets;
        private Entry[]? _entries;
        public int _count;
        public int _freeList;
        public int _freeCount;
        public int _version;
        public IEqualityComparer<T>? _comparer;

        public HashSet() : this((IEqualityComparer<T>?)null)
        {
        }

        public HashSet(IEqualityComparer<T>? comparer)
        {
            if (comparer is not null &&
                comparer != EqualityComparer<T>
                    .Default) // first check for null to avoid forcing default comparer instantiation unnecessarily
            {
                _comparer = comparer;
            }

            // Special-case EqualityComparer<string>.Default, StringComparer.Ordinal, and StringComparer.OrdinalIgnoreCase.
            // We use a non-randomized comparer for improved perf, falling back to a randomized comparer if the
            // hash buckets become unbalanced.
            if (typeof(T) == typeof(string))
            {
                if (_comparer is null)
                {
                    _comparer = (IEqualityComparer<T>)EqualityComparer<string?>.Default;
                }
                else if (ReferenceEquals(_comparer, StringComparer.Ordinal))
                {
                    _comparer = (IEqualityComparer<T>)StringComparer.Ordinal;
                }
                else if (ReferenceEquals(_comparer, StringComparer.OrdinalIgnoreCase))
                {
                    _comparer = (IEqualityComparer<T>)StringComparer.OrdinalIgnoreCase;
                }
            }
        }

        public HashSet(int capacity) : this(capacity, null)
        {
        }

        public HashSet(IEnumerable<T> collection) : this(collection, null)
        {
        }

        public HashSet(IEnumerable<T> collection, IEqualityComparer<T>? comparer) : this(comparer)
        {
            if (collection == null)
            {
                throw new InvalidOperationException(""Invalid operation: Enum operation can't happen."");
            }

            if (collection is HashSet<T> otherAsHashSet && EqualityComparersAreEqual(this, otherAsHashSet))
            {
                throw new InvalidOperationException(""Invalid operation: Enum operation can't happen."");
            }
            else
            {
                // To avoid excess resizes, first set size based on collections count. The collection may
                // contain duplicates, so call TrimExcess if resulting HashSet is larger than the threshold.
                if (collection is ICollection<T> coll)
                {
                    int count = coll.Count;
                    if (count > 0)
                    {
                        Initialize(count);
                    }
                }

                UnionWith(collection);

                if (_count > 0 && _entries!.Length / _count > ShrinkThreshold)
                {
                    TrimExcess();
                }
            }
        }

        public HashSet(int capacity, IEqualityComparer<T>? comparer) : this(comparer)
        {
            if (capacity < 0)
            {
                throw new InvalidOperationException(""Invalid operation: Enum operation can't happen."");
            }

            if (capacity > 0)
            {
                Initialize(capacity);
            }
        }

        protected HashSet(StreamingContext context)
        {
        }


        private int ExpandPrime(int count)
        {
            // This implementation is a simplified version of HashHelpers.ExpandPrime
            // that generates prime numbers for hash table capacities.
            int newSize = 2 * count;

            // Skip even numbers, they are not prime
            if (newSize % 2 == 0)
                newSize++;

            while (!IsPrime(newSize))
                newSize += 2;

            return newSize;
        }

        private static int GetPrime(int min)
        {
            if (min < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(min), ""Capacity cannot be negative."");
            }

            // Use a simple algorithm to find the next prime number greater than or equal to min
            for (int i = min | 1; i < int.MaxValue; i += 2)
            {
                if (IsPrime(i))
                {
                    return i;
                }
            }

            return min;
        }

        private static bool IsPrime(int number)
        {
            if (number < 2)
            {
                return false;
            }

            int sqrt = (int)Math.Sqrt(number);
            for (int i = 2; i <= sqrt; i++)
            {
                if (number % i == 0)
                {
                    return false;
                }
            }

            return true;
        }

        void ICollection<T>.Add(T item) => AddIfNotPresent(item, out _);

        public void Clear()
        {
            int count = _count;
            if (count > 0)
            {
                Array.Clear(_buckets, 0, _buckets.Length - 1);
                _count = 0;
                _freeList = -1;
                _freeCount = 0;
                Array.Clear(_entries, 0, count);
            }
        }

        public bool Contains(T item) => FindItemIndex(item) >= 0;
        public void CopyTo(T[] array) => CopyTo(array, 0, Count);
        public void CopyTo(T[] array, int arrayIndex) => CopyTo(array, arrayIndex, Count);

        public void CopyTo(T[] array, int arrayIndex, int count)
        {
            if (array == null)
            {
                throw new Exception();
            }

            // Check array index valid index into array.
            if (arrayIndex < 0)
            {
                throw new Exception();
            }

            // Also throw if count less than 0.
            if (count < 0)
            {
                throw new Exception();
            }

            // Will the array, starting at arrayIndex, be able to hold elements? Note: not
            // checking arrayIndex >= array.Length (consistency with list of allowing
            // count of 0; subsequent check takes care of the rest)
            if (arrayIndex > array.Length || count > array.Length - arrayIndex)
            {
                throw new Exception();
            }

            Entry[]? entries = _entries;
            for (int i = 0; i < _count && count != 0; i++)
            {
                ref Entry entry = ref entries![i];
                if (entry.Next >= -1)
                {
                    array[arrayIndex++] = entry.Value;
                    count--;
                }
            }
        }

        private int FindItemIndex(T item)
        {
           //Нужна реализация
           //throw new NotImplementedException();
           return 0;

        }

        private ref int GetBucketRef(int hashCode)
        {
            int[] buckets = _buckets!;
#if TARGET_64BIT
            return ref buckets[HashHelpers.FastMod((uint)hashCode, (uint)buckets.Length, _fastModMultiplier)];
#else
            return ref buckets[(uint)hashCode % (uint)buckets.Length];
#endif
        }


        public bool Remove(T item)
        {
            //Нужна реализация
            //throw new NotImplementedException();
            return false;
        }


        public int Count => _count - _freeCount;

        public bool IsReadOnly { get; }

        public Enumerator GetEnumerator() => new Enumerator(this);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Add(T item) => AddIfNotPresent(item, out _);

        public bool TryGetValue(T equalValue, [MaybeNullWhen(false)] out T actualValue)
        {
            if (_buckets != null)
            {
                int index = FindItemIndex(equalValue);
                if (index >= 0)
                {
                    actualValue = _entries![index].Value;
                    return true;
                }
            }

            actualValue = default;
            return false;
        }

        public void UnionWith(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new InvalidOperationException(""Invalid operation: Enum operation can't happen."");
            }

            foreach (T item in other)
            {
                AddIfNotPresent(item, out _);
            }
        }

        public int RemoveWhere(Predicate<T> match)
        {
            if (match == null)
            {
                throw new InvalidOperationException(""Invalid operation: Enum operation can't happen."");
            }

            Entry[]? entries = _entries;
            int numRemoved = 0;
            for (int i = 0; i < _count; i++)
            {
                ref Entry entry = ref entries![i];
                if (entry.Next >= -1)
                {
                    // Cache value in case delegate removes it
                    T value = entry.Value;
                    if (match(value))
                    {
                        // Check again that remove actually removed it.
                        if (Remove(value))
                        {
                            numRemoved++;
                        }
                    }
                }
            }

            return numRemoved;
        }

        public IEqualityComparer<T> Comparer
        {
            get
            {
                if (typeof(T) == typeof(string))
                {
                    return (IEqualityComparer<T>)GetUnderlyingEqualityComparer((IEqualityComparer<string?>?)_comparer);
                }
                else
                {
                    return _comparer ?? EqualityComparer<T>.Default;
                }
            }
        }

        public int EnsureCapacity(int capacity)
        {
            if (capacity < 0)
            {
                throw new InvalidOperationException(""Invalid operation: Enum operation can't happen."");
            }

            int currentCapacity = _entries == null ? 0 : _entries.Length;
            if (currentCapacity >= capacity)
            {
                return currentCapacity;
            }

            if (_buckets == null)
            {
                return Initialize(capacity);
            }

            int newSize = GetPrime(capacity);
            Resize(newSize, forceNewHashCodes: false);
            return newSize;
        }

        private void Resize()
        {
            int newSize = ExpandPrime(_count);
            // Call the overloaded Resize method with the new size
            Resize(newSize, forceNewHashCodes: false);
        }

        private void Resize(int newSize, bool forceNewHashCodes)
        {
            var entries = new Entry[newSize];

            int count = _count;
            Array.Copy(_entries, entries, count);

            if (!typeof(T).IsValueType && forceNewHashCodes)
            {
                _comparer = (IEqualityComparer<T>)StringComparer.Ordinal;
                
                for (int i = 0; i < count; i++)
                {
                    ref Entry entry = ref entries[i];
                    if (entry.Next >= -1)
                    {
                        entry.HashCode = entry.Value != null ? _comparer!.GetHashCode(entry.Value) : 0;
                    }
                }

                if (ReferenceEquals(_comparer, EqualityComparer<T>.Default))
                {
                    _comparer = null;
                }
            }

            // Assign member variables after both arrays allocated to guard against corruption from OOM if second fails
            _buckets = new int[newSize];

            for (int i = 0; i < count; i++)
            {
                ref Entry entry = ref entries[i];
                if (entry.Next >= -1)
                {
                    ref int bucket = ref GetBucketRef(entry.HashCode);
                    entry.Next = bucket - 1; // Value in _buckets is 1-based
                    bucket = i + 1;
                }
            }

            _entries = entries;
        }

        public void TrimExcess()
        {
            int capacity = Count;

            int newSize = GetPrime(capacity);
            Entry[] oldEntries = _entries;
            int currentCapacity = oldEntries == null ? 0 : oldEntries.Length;
            if (newSize >= currentCapacity)
            {
                return;
            }

            int oldCount = _count;
            _version++;
            Initialize(newSize);
            Entry[] entries = _entries;
            int count = 0;
            for (int i = 0; i < oldCount; i++)
            {
                int hashCode = oldEntries[i].HashCode;
                if (oldEntries[i].Next >= -1)
                {
                    ref Entry entry = ref entries![count];
                    entry = oldEntries[i];
                    ref int bucket = ref GetBucketRef(hashCode);
                    entry.Next = bucket - 1; // Value in _buckets is 1-based
                    bucket = count + 1;
                    count++;
                }
            }

            _count = capacity;
            _freeCount = 0;
        }

        private int Initialize(int capacity)
        {
            int size = GetPrime(capacity);
            var buckets = new int[size];
            var entries = new Entry[size];

            // Assign member variables after both arrays are allocated to guard against corruption from OOM if second fails.
            _freeList = -1;
            _buckets = buckets;
            _entries = entries;

            return size;
        }

        private bool AddIfNotPresent(T value, out int location)
        {
            //Нужна реализация
            //throw new NotImplementedException();
            return false;
        }

        private static IEqualityComparer<string?> GetUnderlyingEqualityComparer(IEqualityComparer<string?>? outerComparer)
        {
            if (outerComparer is null)
            {
                return EqualityComparer<string?>.Default;
            }
            else
            {
                return outerComparer;
            }
        }

        internal static bool EqualityComparersAreEqual(HashSet<T> set1, HashSet<T> set2) =>
            set1.Comparer.Equals(set2.Comparer);

        private struct Entry
        {
            public int HashCode;

            public int Next;

            public T Value;
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly HashSet<T> _hashSet;
            private readonly int _version;
            private int _index;
            private T _current;

            internal Enumerator(HashSet<T> hashSet)
            {
                _hashSet = hashSet;
                _version = hashSet._version;
                _index = 0;
                _current = default!;
            }

            public bool MoveNext()
            {
                if (_version != _hashSet._version)
                {
                    throw new InvalidOperationException(""Invalid operation: Enum operation can't happen."");
                }

                // Use unsigned comparison since we set index to dictionary.count+1 when the enumeration ends.
                // dictionary.count+1 could be negative if dictionary.count is int.MaxValue
                while (_index < _hashSet._count)
                {
                    ref Entry entry = ref _hashSet._entries![_index++];
                    if (entry.Next >= -1)
                    {
                        _current = entry.Value;
                        return true;
                    }
                }

                _index = _hashSet._count + 1;
                _current = default!;
                return false;
            }

            public T Current => _current;

            public void Dispose()
            {
            }

            object? IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || (_index == _hashSet._count + 1))
                    {
                        throw new InvalidOperationException(""Invalid operation: Enum operation can't happen."");
                    }

                    return _current;
                }
            }

            void IEnumerator.Reset()
            {
                if (_version != _hashSet._version)
                {
                    throw new InvalidOperationException(""Invalid operation: Enum operation can't happen."");
                }

                _index = 0;
                _current = default!;
            }
        }
    }
}";

        private static string _validHashSetCode = TemplateHashSet.HashSetCode;

        [Test]
        public void TestHashSet()
        {
            Assert.AreEqual(CodeCompileChecker<int>.CheckHashSet(_validHashSetCode, 8), true);
            //Assert.AreEqual(CodeCompileChecker<int>.CheckHashSet(_invalidHashSetCode, 8), false);
        }
    }
}

