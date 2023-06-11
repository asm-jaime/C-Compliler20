namespace OnlineCompiler.Server.Handlers;

public static class CheckList<T>
{
    public static bool CheckAdd(List<T> list, Type type, object instance, T item)
    {
        list.Add(item);
        type.GetMethod("Add").Invoke(instance, new object[] { item });

        return list.Count == (int)type.GetProperty("Count").GetValue(instance);
    }

    public static bool CheckRemove(List<T> list, Type type, object instance, T item)
    {
        list.Add(item);
        type.GetMethod("Add").Invoke(instance, new object[] { item });
        list.Remove(item);
        type.GetMethod("Remove").Invoke(instance, new object[] { item });

        return list.Count == (int)type.GetProperty("Count").GetValue(instance);
    }

    public static bool CheckInsert(List<T> list, Type type, object instance, T item)
    {
        list.Insert(0, item);
        type.GetMethod("Insert").Invoke(instance, new object[] { 0, item });

        var origin = list[0];
        var constructed = (T)type.GetProperty("Item").GetValue(instance, new object[] { 0 });

        return origin.Equals(constructed);
    }

    public static bool CheckClear(List<T> list, Type type, object instance, T item)
    {
        list.Add(item);
        type.GetMethod("Add").Invoke(instance, new object[] { item });
        list.Clear();
        type.GetMethod("Clear").Invoke(instance, null);

        return list.Count == (int)type.GetProperty("Count").GetValue(instance);
    }

    public static bool CheckContains(List<T> list, Type type, object instance, T item)
    {
        list.Add(item);
        type.GetMethod("Add").Invoke(instance, new object[] { item });

        bool originContains = list.Contains(item);
        bool constructedContains = (bool)type.GetMethod("Contains").Invoke(instance, new object[] { item });

        return originContains == constructedContains;
    }
}

