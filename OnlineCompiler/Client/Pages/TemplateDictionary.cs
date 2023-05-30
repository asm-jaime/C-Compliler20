namespace OnlineCompiler.Client.Pages;

public static class TemplateDictionary
{
public static string DictionaryCode = @"
using System.Collections;

var people = new Dictionary<int, string>()
{
    [5] = ""Tom"",
    [6] = ""Sam"",
    [7] = ""Bob""
};
            
foreach(var person in people)
{
    Console.WriteLine($""key: {person.Key}  value: {person.Value}"");
}

class Dictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
{
    private List<TKey> keys;
    private List<TValue> values;

    public Dictionary()
    {
        keys = new List<TKey>();
        values = new List<TValue>();
    }

    public void Add(TKey key, TValue value)
    {
        if (ContainsKey(key))
        {
            throw new ArgumentException(""An element with the same key already exists in the dictionary."");
        }
        keys.Add(key);
        values.Add(value);
    }

    public bool ContainsKey(TKey key)
    {
        for (int i = 0; i < keys.Count; i++)
        {
            if (keys[i].Equals(key))
            {
                return true;
            }
        }
        return false;
    }

    public bool Remove(TKey key)
    {
        for (int i = 0; i < keys.Count; i++)
        {
            if (keys[i].Equals(key))
            {
                keys.RemoveAt(i);
                values.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    public void Clear()
    {
        keys.Clear();
        values.Clear();
    }
    public TValue this[TKey key]
    {
        get
        {
            for (int i = 0; i < keys.Count; i++)
            {
                if (keys[i].Equals(key))
                {
                    return values[i];
                }
            }
            throw new KeyNotFoundException(""The specified key was not found in the dictionary."");
        }
        set
        {
            for (int i = 0; i < keys.Count; i++)
            {
                if (keys[i].Equals(key))
                {
                    values[i] = value;
                    return;
                }
            }
            Add(key, value);
        }
    }
    public bool ContainsValue(TValue value)
    {
        for (int i = 0; i < values.Count; i++)
        {
            if (values[i].Equals(value))
            {
                return true;
            }
        }
        return false;
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        for (int i = 0; i < keys.Count; i++)
        {
            yield return new KeyValuePair<TKey, TValue>(keys[i], values[i]);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    public int Count
    {
        get { return keys.Count; }
    }
}";

public static string UserDictionaryCode = @"
using System.Collections;

class Dictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
{
    private List<TKey> keys;
    private List<TValue> values;

    public Dictionary()
    {
        
    }

    public void Add(TKey key, TValue value)
    {
        
    }

    public bool ContainsKey(TKey key)
    {
        
    }

    public bool Remove(TKey key)
    {
       
    }

    public void Clear()
    {
        
    }
    public TValue this[TKey key]
    {
        
    }
    public bool ContainsValue(TValue value)
    {
        
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        
    }
    public int Count
    {
      
    }
}";
}
