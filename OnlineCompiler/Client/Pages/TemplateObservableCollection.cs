namespace OnlineCompiler.Client.Pages;

public static class TemplateObservableCollection
{
    public static string ObservableCollectionCode=@"using System;
using System.Collections;
using System.Collections.Generic;

public class ObservableCollection<T> : IEnumerable<T>
{
    private List<T> items = new List<T>();

    public event Action<T> ItemAdded;

    public event Action<T> ItemRemoved;

    public int Count => items.Count;

    public T this[int index]
    {
        get { return items[index]; }
        set
        {
            items[index] = value;
            OnItemAdded(value);
        }
    }

    public void Add(T item)
    {
        items.Add(item);
        OnItemAdded(item);
    }

    public bool Remove(T item)
    {
        bool success = items.Remove(item);
        if (success)
        {
            OnItemRemoved(item);
        }
        return success;
    }

    public IEnumerator<T> GetEnumerator() => items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    protected virtual void OnItemAdded(T item) => ItemAdded?.Invoke(item);

    protected virtual void OnItemRemoved(T item) => ItemRemoved?.Invoke(item);
}
";
    
    public static string UserObservableCollectionCode=@"using System;
using System.Collections;
using System.Collections.Generic;

public class ObservableCollection<T> : IEnumerable<T>
{
    private List<T> items = new List<T>();

    public event Action<T> ItemAdded;

    public event Action<T> ItemRemoved;

    public int Count => items.Count;

    public T this[int index]
    {

    }

    public void Add(T item)
    {

    }

    public bool Remove(T item)
    {

    }

    public IEnumerator<T> GetEnumerator() => items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    protected virtual void OnItemAdded(T item) => ItemAdded?.Invoke(item);

    protected virtual void OnItemRemoved(T item) => ItemRemoved?.Invoke(item);
}
";
}