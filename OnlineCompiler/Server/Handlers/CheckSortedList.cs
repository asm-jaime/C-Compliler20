namespace OnlineCompiler.Server.Handlers;

public static class CheckSortedList<T>
{
    public static bool CheckAdd(Type type, object instance, KeyValuePair<T, T> item)
    {
        SortedList<T, T> list = new SortedList<T, T>();
        list.Add(item.Key, item.Value);
        type.GetMethod("Add").Invoke(instance, new object[] { item.Key, item.Value });

        return list.Count == (int)type.GetProperty("Count").GetValue(instance);
    }

    public static bool CheckContainsKey(Type type, object instance, T key)
    {
        var item1 = new KeyValuePair<T, T>(key, key);
        SortedList<T, T> list = new SortedList<T, T>();
        list.Add(item1.Key, item1.Value);
        bool originContains = list.ContainsKey(item1.Key);
        bool constructedContains = (bool)type.GetMethod("ContainsKey").Invoke(instance, new object[] { key });

        return originContains == constructedContains;
    }

    public static bool CheckRemove(Type type, object instance, T key)
    {
        SortedList<T, T> list = new SortedList<T, T>() { { key, key } };
        list.Remove(key);
        type.GetMethod("Remove").Invoke(instance, new object[] { key });

        return list.Count == (int)type.GetProperty("Count").GetValue(instance);
    }

    public static bool CheckClear(Type type, object instance)
    {
        SortedList<T, T> list = new SortedList<T, T>() { { default(T), default(T) } };
        list.Clear();
        type.GetMethod("Clear").Invoke(instance, null);

        return list.Count == (int)type.GetProperty("Count").GetValue(instance);
    }

    public static bool CheckIsSorted(Type type, object instance, KeyValuePair<T, T> item1, KeyValuePair<T, T> item2)
    {
        SortedList<T, T> list = new SortedList<T, T>();
        list.Add(item1.Key, item1.Value);
        list.Add(item2.Key, item2.Value);
        type.GetMethod("Add").Invoke(instance, new object[] { item1.Key, item1.Value });
        type.GetMethod("Add").Invoke(instance, new object[] { item2.Key, item2.Value });

        return list.Keys.SequenceEqual(((SortedList<T, T>)instance).Keys);
    }
}

