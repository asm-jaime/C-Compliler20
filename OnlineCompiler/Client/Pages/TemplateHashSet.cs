namespace OnlineCompiler.Client.Pages;

public static class TemplateHashSet
{
    public static string HashSetCode=@"
using System.Collections;
HashSet<string> cities = new HashSet<string>
{
    ""Mumbai"",
    ""Vadodra"",
    ""Surat"",
    ""Ahmedabad""
};

foreach (var city in cities)
{
    Console.WriteLine(city);
}

public class HashSet<T> : ICollection<T>, IEnumerable<T>, ICollection, IEnumerable
{
    private T[] items;
    private int count;

    public HashSet()
    {
        items = new T[0];
        count = 0;
    }

    public void Add(T item)
    {
        if (!Contains(item))
        {
            Array.Resize(ref items, count + 1);
            items[count] = item;
            count++;
        }
    }

    public void Clear()
    {
        items = new T[0];
        count = 0;
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

    public void CopyTo(T[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public bool Remove(T item)
    {
        for (int i = 0; i < count; i++)
        {
            if (items[i].Equals(item))
            {
                for (int j = i; j < count - 1; j++)
                {
                    items[j] = items[j + 1];
                }
                count--;
                Array.Resize(ref items, count);
                return true;
            }
        }
        return false;
    }

    public void CopyTo(Array array, int index)
    {
        throw new NotImplementedException();
    }

    public int Count
    {
        get { return count; }
    }

    public bool IsSynchronized { get; }
    public object SyncRoot { get; }

    public bool IsReadOnly { get; }
            
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
    
    public static string UserHashSetCode=@"
using System.Collections;

public class HashSet<T> : ICollection<T>, IEnumerable<T>, ICollection, IEnumerable
{
    private T[] items;
    private int count;

    public HashSet()
    {
        items = new T[0];
        count = 0;
    }

    public void Add(T item)
    {
   
    }

    public void Clear()
    {
       
    }

    public bool Contains(T item)
    {
      
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
       
    }

    public bool Remove(T item)
    {
       
    }

    public void CopyTo(Array array, int index)
    {
        
    }

    public int Count
    {
       
    }

    public bool IsSynchronized { get; }
    public object SyncRoot { get; }

    public bool IsReadOnly { get; }
            
    public IEnumerator<T> GetEnumerator()
    {
      
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      
    }
}";
}