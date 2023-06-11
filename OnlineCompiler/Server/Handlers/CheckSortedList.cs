namespace OnlineCompiler.Server.Handlers;

public static class CheckSortedList<T>
{
    public static bool CheckAdd(Type type, object instance, KeyValuePair<string, T> item)
    {
        var list = new SortedList<string, T>();
        list.Add(item.Key, item.Value);
        type.GetMethod("Add").Invoke(instance, new object[] { item.Key, item.Value });

        return list.Count == (int)type.GetProperty("Count").GetValue(instance);
    }

    public static bool CheckContainsKey(Type type, object instance, string key)
    {
        var item1 = new KeyValuePair<string, T>(key, default);
        SortedList<string, T> list = new SortedList<string, T>();
        list.Add(item1.Key, item1.Value);
        bool originContains = list.ContainsKey(item1.Key);
        bool constructedContains = (bool)type.GetMethod("ContainsKey").Invoke(instance, new object[] { key });

        return originContains == constructedContains;
    }

    public static bool CheckRemove(Type type, object instance, string key)
    {
        SortedList<string, T> list = new SortedList<string, T>() { { key, default } };
        list.Remove(key);
        type.GetMethod("Remove").Invoke(instance, new object[] { key });

        return list.Count == (int)type.GetProperty("Count").GetValue(instance);
    }

    public static bool CheckClear(Type type, object instance)
    {
        SortedList<string, T> list = new SortedList<string, T>() { { "test", default } };
        list.Clear();
        type.GetMethod("Clear").Invoke(instance, null);

        return list.Count == (int)type.GetProperty("Count").GetValue(instance);
    }

    public static bool CheckIsSorted(Type type, object instance, KeyValuePair<string, T> item1, KeyValuePair<string, T> item2)
    {
        type.GetMethod("Add").Invoke(instance, new object[] { item2.Key, item2.Value });
        type.GetMethod("Add").Invoke(instance, new object[] { item1.Key, item1.Value });

        var instanceKeys = ((dynamic)instance).Keys;

        return item2.Key == instanceKeys[1] && item1.Key == instanceKeys[0];
    }
}

