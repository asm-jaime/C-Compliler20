namespace OnlineCompiler.Server.Handlers;

public static class CheckQueue<T>
{
    public static bool CheckEnqueue(Queue<T> queue, Type type, object instance, T item)
    {
        queue.Enqueue(item);
        type.GetMethod("Enqueue", new[] { typeof(T) }).Invoke(instance, new object[] { item });

        return queue.Count == (int)type.GetProperty("Count").GetValue(instance);
    }

    public static bool CheckDequeue(Queue<T> queue, Type type, object instance)
    {
        if (queue.Count > 0)
        {
            var originalRemoved = queue.Dequeue();
            var constructedRemoved = (T)type.GetMethod("Dequeue").Invoke(instance, null);
            return originalRemoved.Equals(constructedRemoved);
        }
        return true;
    }

    public static bool CheckPeek(Queue<T> queue, Type type, object instance, T item)
    {
        queue.Enqueue(item);
        type.GetMethod("Enqueue", new[] { typeof(T) }).Invoke(instance, new object[] { item });

        if (queue.Count > 0)
        {
            var originalPeek = queue.Peek();
            var constructedPeek = (T)type.GetMethod("Peek").Invoke(instance, null);
            return originalPeek.Equals(constructedPeek);
        }
        return true;
    }


    public static bool CheckClear(Queue<T> queue, Type type, object instance)
    {
        queue.Clear();
        type.GetMethod("Clear").Invoke(instance, null);

        return queue.Count == (int)type.GetProperty("Count").GetValue(instance);
    }

    public static bool CheckContains(Queue<T> queue, Type type, object instance, T item)
    {
        bool originContains = queue.Contains(item);
        bool constructedContains = (bool)type.GetMethod("Contains").Invoke(instance, new object[] { item });

        return originContains == constructedContains;
    }
}

