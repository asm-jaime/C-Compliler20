namespace OnlineCompiler.Client.Pages;

public static class TemplateSortedList
{
    public static string SortedListCode=@"
using System;
using System.Collections;
using System.Collections.Generic;

public class SortedList<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
{
    private List<KeyValuePair<TKey, TValue>> items;
    private IComparer<TKey> comparer;

    public SortedList() : this(null)
    {
    }

    public SortedList(IComparer<TKey>? comparer)
    {
        items = new List<KeyValuePair<TKey, TValue>>();
        this.comparer = comparer ?? Comparer<TKey>.Default;
    }

    public void Add(TKey key, TValue value)
    {
        int index = FindInsertionIndex(key);
        items.Insert(index, new KeyValuePair<TKey, TValue>(key, value));
    }

    public bool Remove(TKey key)
    {
        int index = FindIndex(key);
        if (index >= 0)
        {
            items.RemoveAt(index);
            return true;
        }
        return false;
    }

    public TValue GetValueOrDefault(TKey key)
    {
        int index = FindIndex(key);
        return (index >= 0) ? items[index].Value : default(TValue);
    }

    public TValue this[TKey key]
    {
        get { return GetValueOrDefault(key); }
        set { Add(key, value); }
    }

    public void Clear()
    {
        items.Clear();
    }

    private int FindInsertionIndex(TKey key)
    {
        int index = items.BinarySearch(new KeyValuePair<TKey, TValue>(key, default(TValue)),
                                       new KeyValuePairComparer(comparer));
        return (index >= 0) ? index : ~index;
    }

    private int FindIndex(TKey key)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (comparer.Compare(items[i].Key, key) == 0)
            {
                return i;
            }
        }
        return -1;
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private class KeyValuePairComparer : IComparer<KeyValuePair<TKey, TValue>>
    {
        private IComparer<TKey> comparer;

        public KeyValuePairComparer(IComparer<TKey> comparer)
        {
            this.comparer = comparer;
        }

        public int Compare(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
        {
            return comparer.Compare(x.Key, y.Key);
        }
    }
}
 
";
    
    public static string UserSortedListCode=@"
using System;
using System.Collections;
using System.Collections.Generic;

public class SortedList<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
{
    private List<KeyValuePair<TKey, TValue>> items;
    private IComparer<TKey> comparer;

    public SortedList() : this(null)
    {
    }

    public SortedList(IComparer<TKey>? comparer)
    {

    }

    public void Add(TKey key, TValue value)
    {

    }

    public bool Remove(TKey key)
    {

    }

    public TValue GetValueOrDefault(TKey key)
    {

    }

    public TValue this[TKey key]
    {

    }

    public void Clear()
    {

    }

    private int FindInsertionIndex(TKey key)
    {

    }

    private int FindIndex(TKey key)
    {

    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        
    }

    private class KeyValuePairComparer : IComparer<KeyValuePair<TKey, TValue>>
    {
        private IComparer<TKey> comparer;

        public KeyValuePairComparer(IComparer<TKey> comparer)
        {
            
        }

        public int Compare(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
        {
            
        }
    }
}
";
}