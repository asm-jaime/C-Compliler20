namespace OnlineCompiler.Server.Handlers;

public static class CheckLinkedList<T>
{
    public static bool CheckAddLast(LinkedList<T> list, Type type, object instance, T item)
    {
        list.AddLast(item);
        type.GetMethod("AddLast", new[] { typeof(T) }).Invoke(instance, new object[] { item });

        return list.Count == (int)type.GetProperty("Count").GetValue(instance);
    }

    public static bool CheckAddFirst(LinkedList<T> list, Type type, object instance, T item)
    {
        list.AddFirst(item);
        type.GetMethod("AddLast", new[] { typeof(T) }).Invoke(instance, new object[] { item });

        return list.Count == (int)type.GetProperty("Count").GetValue(instance);
    }

    public static bool CheckRemove(LinkedList<T> list, Type type, object instance, T item)
    {
        list.AddLast(item);
        //type.GetMethod("AddLast").Invoke(instance, new object[] { item });
        type.GetMethod("AddLast", new[] { typeof(T) }).Invoke(instance, new object[] { item });
        list.Remove(item);
        type.GetMethod("Remove", new[] { typeof(T) }).Invoke(instance, new object[] { item });

        return list.Count == (int)type.GetProperty("Count").GetValue(instance);
    }

    public static bool CheckClear(LinkedList<T> list, Type type, object instance, T item)
    {
        list.AddLast(item);
        type.GetMethod("AddLast", new[] { typeof(T) }).Invoke(instance, new object[] { item });
        list.Clear();
        type.GetMethod("Clear").Invoke(instance, null);

        return list.Count == (int)type.GetProperty("Count").GetValue(instance);
    }

    public static bool CheckContains(LinkedList<T> list, Type type, object instance, T item)
    {
        list.AddLast(item);
        type.GetMethod("AddLast", new[] { typeof(T) }).Invoke(instance, new object[] { item });

        bool originContains = list.Contains(item);
        bool constructedContains = (bool)type.GetMethod("Contains").Invoke(instance, new object[] { item });

        return originContains == constructedContains;
    }
}

