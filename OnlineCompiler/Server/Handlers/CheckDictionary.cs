namespace OnlineCompiler.Server.Handlers;

public static class CheckDictionary<T>
{
    public static bool CheckAdd(Dictionary<string, T> dict, Type type, object instance, KeyValuePair<string, T> item)
    {
        dict.Add(item.Key, item.Value);
        type.GetMethod("Add").Invoke(instance, new object[] { item.Key, item.Value });

        return dict.Count == (int)type.GetProperty("Count").GetValue(instance);
    }

    public static bool CheckRemove(Dictionary<string, T> dict, Type type, object instance, KeyValuePair<string, T> item)
    {
        dict.Add(item.Key, item.Value);
        type.GetMethod("Add").Invoke(instance, new object[] { item.Key, item.Value });
        dict.Remove(item.Key);
        type.GetMethod("Remove").Invoke(instance, new object[] { item.Key });

        return dict.Count == (int)type.GetProperty("Count").GetValue(instance);
    }

    public static bool CheckClear(Dictionary<string, T> dict, Type type, object instance)
    {
        dict.Clear();
        type.GetMethod("Clear").Invoke(instance, null);

        return dict.Count == (int)type.GetProperty("Count").GetValue(instance);
    }

    public static bool CheckContainsKey(Dictionary<string, T> dict, Type type, object instance, string key)
    {
        bool originContains = dict.ContainsKey(key);
        bool constructedContains = (bool)type.GetMethod("ContainsKey").Invoke(instance, new object[] { key });

        return originContains == constructedContains;
    }

    public static bool CheckContainsValue(Dictionary<string, T> dict, Type type, object instance, T value)
    {
        bool originContains = dict.ContainsValue(value);
        bool constructedContains = (bool)type.GetMethod("ContainsValue").Invoke(instance, new object[] { value });

        return originContains == constructedContains;
    }
}
