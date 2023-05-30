namespace OnlineCompiler.Client.Pages;

public static class TemplateList
{
    public static string ListCode = @"
    using System.Collections;

    var people = new List<string>() { ""Tom"", ""Bob"", ""Sam"" };
               
    foreach (var person in people)
    {
        Console.WriteLine(person);
    }
    class List<T>:IEnumerable<T>
    {
        private T[] items;
        private int count;

        public List()
        {
            items = new T[4];
            count = 0;
        }

        public void Add(T item)
        {
            if (count == items.Length)
            {
                Array.Resize(ref items, items.Length * 2);
            }
            items[count++] = item;
        }

        public void AddRange(IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                Add(item);
            }
        }

        public bool Contains(T item)
        {
            for (int i = 0; i < count; i++)
            {
                if (items[i].Equals(item))
                {
                    return true;
                }
            }
            return false;
        }

        public int Count
        {
            get { return count; }
        }

        public T this[int index]
        {
            get { return items[index]; }
            set { items[index] = value; }
        }

        public void Clear()
        {
            Array.Clear(items, 0, count);
            count = 0;
        }

        public bool Remove(T item)
        {
            for (int i = 0; i < count; i++)
            {
                if (items[i].Equals(item))
                {
                    // Shift remaining items left
                    Array.Copy(items, i + 1, items, i, count - i - 1);
                    count--;
                    return true;
                }
            }
            return false;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < count; i++)
            {
                yield return items[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }";
    
    public static string UserListCode = @"
    using System.Collections;

    
    class List<T>:IEnumerable<T>
    {
        private T[] items;
        private int count;

        public List()
        {
            
        }

        public void Add(T item)
        {
           
        }

        public void AddRange(IEnumerable<T> collection)
        {
            
        }

        public bool Contains(T item)
        {
           
        }

        public int Count
        {
            
        }

        public T this[int index]
        {
            
        }

        public void Clear()
        {
           
        }

        public bool Remove(T item)
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