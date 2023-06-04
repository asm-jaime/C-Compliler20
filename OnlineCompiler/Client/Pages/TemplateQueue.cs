namespace OnlineCompiler.Client.Pages;

public static class TemplateQueue
{
    public static string QueueCode = @"using System;
using System.Collections;
using System.Collections.Generic;

public class Queue<T> : IEnumerable<T>, ICollection, IEnumerable
{
    private T[] _array;
    private int _head;
    private int _tail;
    private int _size;
    private int _version;
    private const int DefaultCapacity = 4;
    private const int MinimumGrow = 4;
    private const int GrowFactor = 200;

    public Queue()
    {
        _array = new T[0];
        _head = 0;
        _tail = 0;
        _size = 0;
        _version = 0;
    }

    public Queue(int capacity)
    {
        if (capacity < 0)
            throw new ArgumentOutOfRangeException(nameof(capacity));

        _array = new T[capacity];
        _head = 0;
        _tail = 0;
        _size = 0;
        _version = 0;
    }

    public Queue(IEnumerable<T> collection)
    {
        if (collection == null)
            throw new ArgumentNullException(nameof(collection));

        _array = new T[4];
        _size = 0;
        _version = 0;

        foreach (T item in collection)
            Enqueue(item);
    }

    public void CopyTo(Array array, int index)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));

        if (index < 0 || index > array.Length)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (array.Length - index < _size)
            throw new ArgumentException();

        int numToCopy = _size;
        if (numToCopy == 0)
            return;

        int firstPart = (_array.Length - _head < numToCopy) ? _array.Length - _head : numToCopy;
        Array.Copy(_array, _head, array, index, firstPart);

        numToCopy -= firstPart;
        if (numToCopy > 0)
            Array.Copy(_array, 0, array, index + _array.Length - _head, numToCopy);
    }

    public int Count => _size;

    public bool IsSynchronized => false;

    public object SyncRoot => this;

    public void Enqueue(T item)
    {
        if (_size == _array.Length)
        {
            int newCapacity = _array.Length == 0 ? DefaultCapacity : _array.Length * 2;
            SetCapacity(newCapacity);
        }

        _array[_tail] = item;
        _tail = (_tail + 1) % _array.Length;
        _size++;
    }

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

    public T Dequeue()
    {
        if (_size == 0)
            throw new InvalidOperationException();

        T removed = _array[_head];
        _array[_head] = default(T)!;
        _head = (_head + 1) % _array.Length;
        _size--;
        _version++;

        return removed;
    }

    public T Peek()
    {
        if (_size == 0)
            throw new InvalidOperationException();

        return _array[_head];
    }

    public void Clear()
    {
        if (_size > 0)
        {
            Array.Clear(_array, _head, _size);
            _head = 0;
            _tail = 0;
            _size = 0;
            _version++;
        }
    }

    public bool Contains(T item)
    {
        int index = _head;
        int count = _size;

        EqualityComparer<T> comparer = EqualityComparer<T>.Default;

        while (count-- > 0)
        {
            if (comparer.Equals(_array[index], item))
                return true;

            index = (index + 1) % _array.Length;
        }

        return false;
    }

    public T[] ToArray()
    {
        T[] array = new T[Count];
        if (Count == 0)
        {
            return array;
        }

        if (_head < _tail)
        {
            Array.Copy(_array, _head, array, 0, Count);
        }
        else
        {
            Array.Copy(_array, _head, array, 0, _array.Length - _head);
            Array.Copy(_array, 0, array, _array.Length - _head, _tail);
        }

        return array;
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = _head; i < _size + _head; i++)
        {
            yield return _array[i % _array.Length];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
 ";

    public static string UserQueueCode = @"
using System.Collections;
public class Queue<T> : IEnumerable<T>, ICollection, IEnumerable
{
    private T[] _array;
    private int _head;
    private int _tail;
    private int _size;
    private int _version;
    private const int DefaultCapacity = 4;
    private const int MinimumGrow = 4;
    private const int GrowFactor = 200;

    public Queue()
    {
        
    }

    public Queue(int capacity) 
    {
       
    }

    public Queue(IEnumerable<T> collection)
    {
       
    }

    public void CopyTo(Array array, int index)
    {
       
            
    }

    public int Count => _size;

    public bool IsSynchronized => false;

    public object SyncRoot => this;

    public void Enqueue(T item)
    {
                
    }

    private void SetCapacity(int capacity)
    {
        
    }

    public T Dequeue()
    {
      
    }

    public T Peek()
    {
       
    }

    public void Clear()
    {
      
    }

    public bool Contains(T item)
    {
        
    }

    public T[] ToArray()
    {
        
    }

    public IEnumerator<T> GetEnumerator()
    {
        
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        
    }
}";
}