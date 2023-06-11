namespace OnlineCompiler.Server.Handlers;

public static class CheckHashSet<T>
{
    public static bool CheckAdd(HashSet<T> set, Type type, object instance, T item)
    {
        set.Add(item);
        type.GetMethod("Add", new[] { typeof(T) }).Invoke(instance, new object[] { item });

        return set.Count == (int)type.GetProperty("Count").GetValue(instance);
    }

    public static bool CheckRemove(HashSet<T> set, Type type, object instance, T item)
    {
        set.Add(item);
        type.GetMethod("Add", new[] { typeof(T) }).Invoke(instance, new object[] { item });
        set.Remove(item);
        type.GetMethod("Remove", new[] { typeof(T) }).Invoke(instance, new object[] { item });

        return set.Count == (int)type.GetProperty("Count").GetValue(instance);
    }

    public static bool CheckClear(HashSet<T> set, Type type, object instance, T item)
    {
        set.Add(item);
        type.GetMethod("Add", new[] { typeof(T) }).Invoke(instance, new object[] { item });
        set.Clear();
        type.GetMethod("Clear").Invoke(instance, null);

        return set.Count == (int)type.GetProperty("Count").GetValue(instance);
    }

    public static bool CheckContains(HashSet<T> set, Type type, object instance, T item)
    {
        set.Add(item);
        type.GetMethod("Add", new[] { typeof(T) }).Invoke(instance, new object[] { item });

        bool originContains = set.Contains(item);
        bool constructedContains = (bool)type.GetMethod("Contains").Invoke(instance, new object[] { item });

        return originContains == constructedContains;
    }

    public static bool CheckIsSubsetOf(HashSet<T> set, Type type, object instance, HashSet<T> otherSet)
    {
        set.UnionWith(otherSet);
        type.GetMethod("UnionWith").Invoke(instance, new object[] { otherSet });

        bool originIsSubset = set.IsSubsetOf(otherSet);
        bool constructedIsSubset = (bool)type.GetMethod("IsSubsetOf").Invoke(instance, new object[] { otherSet });

        return originIsSubset == constructedIsSubset;
    }
}
