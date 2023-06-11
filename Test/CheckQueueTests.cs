using NUnit.Framework;
using OnlineCompiler.Client.Pages;
using OnlineCompiler.Server.Handlers;

namespace test;

[TestFixture]
public class CheckQueueTests
{
    private static string _invalidQueueCode = @"
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System.Collections.Generic
{
    public class Queue<T> : IReadOnlyCollection<T>
    {
        private T[] _array;
        private int _head; // The index from which to dequeue if the queue isn't empty.
        private int _tail; // The index at which to enqueue if the queue isn't full.
        private int _size; // Number of elements.
        private int _version;

        // Creates a queue with room for capacity objects. The default initial
        // capacity and grow factor are used.
        public Queue()
        {
            _array = Array.Empty<T>();
        }

        // Creates a queue with room for capacity objects. The default grow factor
        // is used.
        public Queue(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException();
            _array = new T[capacity];
        }

        // Fills a Queue with the elements of an ICollection.  Uses the enumerator
        // to get each of the elements.
        public Queue(IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            _array = ToArray(collection, out _size);
            if (_size != _array.Length) _tail = _size;
        }

        public int Count
        {
            get { return _size; }
        }

        // Removes all Objects from the queue.
        public void Clear()
        {
            if (_size != 0)
            {
                if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                {
                    if (_head < _tail)
                    {
                        Array.Clear(_array, _head, _size);
                    }
                    else
                    {
                        Array.Clear(_array, _head, _array.Length - _head);
                        Array.Clear(_array, 0, _tail);
                    }
                }

                _size = 0;
            }

            _head = 0;
            _tail = 0;
            _version++;
        }

        // Adds item to the tail of the queue.
        public void Enqueue(T item)
        {
             //Нужна реализация
             //throw new NotImplementedException();
        }

        internal static T[] ToArray<T>(IEnumerable<T> source, out int length)
        {
            if (source is ICollection<T> ic)
            {
                int count = ic.Count;
                if (count != 0)
                {
                    // Allocate an array of the desired size, then copy the elements into it. Note that this has the same
                    // issue regarding concurrency as other existing collections like List<T>. If the collection size
                    // concurrently changes between the array allocation and the CopyTo, we could end up either getting an
                    // exception from overrunning the array (if the size went up) or we could end up not filling as many
                    // items as 'count' suggests (if the size went down).  This is only an issue for concurrent collections
                    // that implement ICollection<T>, which as of .NET 4.6 is just ConcurrentDictionary<TKey, TValue>.
                    T[] arr = new T[count];
                    ic.CopyTo(arr, 0);
                    length = count;
                    return arr;
                }
            }
            else
            {
                using (var en = source.GetEnumerator())
                {
                    if (en.MoveNext())
                    {
                        const int DefaultCapacity = 4;
                        T[] arr = new T[DefaultCapacity];
                        arr[0] = en.Current;
                        int count = 1;

                        while (en.MoveNext())
                        {
                            if (count == arr.Length)
                            {
                                // This is the same growth logic as in List<T>:
                                // If the array is currently empty, we make it a default size.  Otherwise, we attempt to
                                // double the size of the array.  Doubling will overflow once the size of the array reaches
                                // 2^30, since doubling to 2^31 is 1 larger than Int32.MaxValue.  In that case, we instead
                                // constrain the length to be Array.MaxLength (this overflow check works because of the
                                // cast to uint).
                                int newLength = count << 1;
                                if ((uint) newLength > Array.MaxLength)
                                {
                                    newLength = Array.MaxLength <= count ? count + 1 : Array.MaxLength;
                                }

                                Array.Resize(ref arr, newLength);
                            }

                            arr[count++] = en.Current;
                        }

                        length = count;
                        return arr;
                    }
                }
            }

            length = 0;
            return Array.Empty<T>();
        }

        // GetEnumerator returns an IEnumerator over this Queue.  This
        // Enumerator will support removing.
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <internalonly/>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        // Removes the object at the head of the queue and returns it. If the queue
        // is empty, this method throws an
        // InvalidOperationException.
        public T Dequeue()
        {
             //Нужна реализация
             T result = default!;
             return result;
             //throw new NotImplementedException();
        }

        public bool TryDequeue([MaybeNullWhen(false)] out T result)
        {
            int head = _head;
            T[] array = _array;

            if (_size == 0)
            {
                result = default!;
                return false;
            }

            result = array[head];
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                array[head] = default!;
            }

            MoveNext(ref _head);
            _size--;
            _version++;
            return true;
        }

        // Returns the object at the head of the queue. The object remains in the
        // queue. If the queue is empty, this method throws an
        // InvalidOperationException.
        public T Peek()
        {
            if (_size == 0)
            {
                ThrowForEmptyQueue();
            }

            return _array[_head];
        }

        public bool TryPeek([MaybeNullWhen(false)] out T result)
        {
            if (_size == 0)
            {
                result = default!;
                return false;
            }

            result = _array[_head];
            return true;
        }

        // Returns true if the queue contains at least one object equal to item.
        // Equality is determined using EqualityComparer<T>.Default.Equals().
        public bool Contains(T item)
        {
            if (_size == 0)
            {
                return false;
            }

            if (_head < _tail)
            {
                return Array.IndexOf(_array, item, _head, _size) >= 0;
            }

            // We've wrapped around. Check both partitions, the least recently enqueued first.
            return
                Array.IndexOf(_array, item, _head, _array.Length - _head) >= 0 ||
                Array.IndexOf(_array, item, 0, _tail) >= 0;
        }
        

        // PRIVATE Grows or shrinks the buffer to hold capacity objects. Capacity
        // must be >= _size.
        private void SetCapacity(int capacity)
        {
            T[] newarray = new T[capacity];
            if (_size > 0)
            {
                if (_head < _tail)
                {
                    Array.Copy(_array, _head, newarray, 0, _size);
                }
                else
                {
                    Array.Copy(_array, _head, newarray, 0, _array.Length - _head);
                    Array.Copy(_array, 0, newarray, _array.Length - _head, _tail);
                }
            }

            _array = newarray;
            _head = 0;
            _tail = (_size == capacity) ? 0 : _size;
            _version++;
        }

        // Increments the index wrapping it if necessary.
        private void MoveNext(ref int index)
        {
            // It is tempting to use the remainder operator here but it is actually much slower
            // than a simple comparison and a rarely taken branch.
            // JIT produces better code than with ternary operator ?:
            int tmp = index + 1;
            if (tmp == _array.Length)
            {
                tmp = 0;
            }

            index = tmp;
        }

        private void ThrowForEmptyQueue()
        {
            Debug.Assert(_size == 0);
            throw new InvalidOperationException();
        }

        public int EnsureCapacity(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (_array.Length < capacity)
            {
                Grow(capacity);
            }

            return _array.Length;
        }

        private void Grow(int capacity)
        {

            const int GrowFactor = 2;
            const int MinimumGrow = 4;

            int newcapacity = GrowFactor * _array.Length;

            // Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
            // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
            if ((uint) newcapacity > Array.MaxLength) newcapacity = Array.MaxLength;

            // Ensure minimum growth is respected.
            newcapacity = Math.Max(newcapacity, _array.Length + MinimumGrow);

            // If the computed capacity is still less than specified, set to the original argument.
            // Capacities exceeding Array.MaxLength will be surfaced as OutOfMemoryException by Array.Resize.
            if (newcapacity < capacity) newcapacity = capacity;

            SetCapacity(newcapacity);
        }

        // Implements an enumerator for a Queue.  The enumerator uses the
        // internal version number of the list to ensure that no modifications are
        // made to the list while an enumeration is in progress.
        public struct Enumerator : IEnumerator<T>,
            System.Collections.IEnumerator
        {
            private readonly Queue<T> _q;
            private readonly int _version;
            private int _index; // -1 = not started, -2 = ended/disposed
            private T? _currentElement;

            internal Enumerator(Queue<T> q)
            {
                _q = q;
                _version = q._version;
                _index = -1;
                _currentElement = default;
            }

            public void Dispose()
            {
                _index = -2;
                _currentElement = default;
            }

            public bool MoveNext()
            {
                if (_version != _q._version) throw new InvalidOperationException();

                if (_index == -2)
                    return false;

                _index++;

                if (_index == _q._size)
                {
                    // We've run past the last element
                    _index = -2;
                    _currentElement = default;
                    return false;
                }

                // Cache some fields in locals to decrease code size
                T[] array = _q._array;
                int capacity = array.Length;

                // _index represents the 0-based index into the queue, however the queue
                // doesn't have to start from 0 and it may not even be stored contiguously in memory.

                int arrayIndex = _q._head + _index; // this is the actual index into the queue's backing array
                if (arrayIndex >= capacity)
                {
                    // NOTE: Originally we were using the modulo operator here, however
                    // on Intel processors it has a very high instruction latency which
                    // was slowing down the loop quite a bit.
                    // Replacing it with simple comparison/subtraction operations sped up
                    // the average foreach loop by 2x.

                    arrayIndex -= capacity; // wrap around if needed
                }

                _currentElement = array[arrayIndex];
                return true;
            }

            public T Current
            {
                get
                {
                    if (_index < 0)
                        ThrowEnumerationNotStartedOrEnded();
                    return _currentElement!;
                }
            }

            private void ThrowEnumerationNotStartedOrEnded()
            {
                Debug.Assert(_index == -1 || _index == -2);
                throw new InvalidOperationException();
            }

            object? IEnumerator.Current
            {
                get { return Current; }
            }

            void IEnumerator.Reset()
            {
                if (_version != _q._version) throw new InvalidOperationException();
                _index = -1;
                _currentElement = default;
            }
        }
    }
}";
    private static string _validQueueCode = TemplateQueue.QueueCode;

    [Test]
    public void TestQueue()
    {
        Assert.AreEqual(CodeCompileChecker<int>.CheckQueue(_validQueueCode, 8), true);
        Assert.AreEqual(CodeCompileChecker<int>.CheckQueue(_invalidQueueCode, 8), false);
    }
}

