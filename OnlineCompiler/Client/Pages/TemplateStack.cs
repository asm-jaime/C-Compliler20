namespace OnlineCompiler.Client.Pages;

public static class TemplateStack
{
    public static string StackCode = @"
using System;
using System.Collections;
using System.Collections.Generic;

public class Stack<T> : IEnumerable<T>
{
    private T[] items;
    private int count;

    public Stack()
    {
        items = new T[4];
        count = 0;
    }

    public int Count
    {
        get { return count; }
    }

    public void Push(T item)
    {
        if (count == items.Length)
        {
            T[] newItems = new T[2 * items.Length];
            Array.Copy(items, 0, newItems, 0, count);
            items = newItems;
        }

        items[count++] = item;
    }

    public T Pop()
    {
        if (count == 0)
        {
            throw new InvalidOperationException();
        }

        T item = items[--count];
        items[count] = default(T); // Avoids memory leaks
        return item;
    }

    public T Peek()
    {
        if (count == 0)
        {
            throw new InvalidOperationException();
        }

        return items[count - 1];
    }

    public void Clear()
    {
        Array.Clear(items, 0, count);
        count = 0;
    }

    public bool Contains(T item)
    {
        EqualityComparer<T> comparer = EqualityComparer<T>.Default;
        for (int i = 0; i < count; i++)
        {
            if (comparer.Equals(items[i], item))
            {
                return true;
            }
        }

        return false;
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = count - 1; i >= 0; i--)
        {
            yield return items[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

";
    
    public static string UserStackCode=@"
    using System.Collections;

    public class Stack<T> : IEnumerable<T>
    {
        private T[] items;
        private int count;

        public Stack()
        {
            
        }

        public int Count
        {
            
        }

        public void Push(T item)
        {
           
        }

        public T Pop()
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

        public IEnumerator<T> GetEnumerator()
        {
            
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
           
        }
    }";
}