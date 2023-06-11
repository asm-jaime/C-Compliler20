namespace OnlineCompiler.Server.Handlers;

public static class CheckStack<T>
{
    public static bool CheckPush(Stack<T> stack, Type type, object instance, T item)
    {
        stack.Push(item);
        stack.Push(item);
        type.GetMethod("Push").Invoke(instance, new object[] { item });
        type.GetMethod("Push").Invoke(instance, new object[] { item });

        var origin = stack.Pop();
        var constructed = (T)type.GetMethod("Pop").Invoke(instance, null);

        return origin.Equals(constructed);
    }

    public static bool CheckPeek(Stack<T> stack, Type type, object instance, T item)
    {
        stack.Push(item);
        type.GetMethod("Push").Invoke(instance, new object[] { item });

        var origin = stack.Peek();
        var constructed = (T)type.GetMethod("Peek").Invoke(instance, null);

        return origin.Equals(constructed);
    }

    public static bool CheckTryPeek(Stack<T> stack, Type type, object instance, T item)
    {
        stack.Push(item);
        type.GetMethod("Push").Invoke(instance, new object[] { item });

        bool originSuccess = stack.TryPeek(out T originResult);
        bool constructedSuccess = (bool)type.GetMethod("TryPeek").Invoke(instance, new object[] { null });

        return originSuccess == constructedSuccess;
    }

    public static bool CheckClear(Stack<T> stack, Type type, object instance, T item)
    {
        stack.Push(item);
        type.GetMethod("Push").Invoke(instance, new object[] { item });
        stack.Clear();
        type.GetMethod("Clear").Invoke(instance, null);

        return stack.Count == (int)type.GetProperty("Count").GetValue(instance);
    }

    public static bool CheckContains(Stack<T> stack, Type type, object instance, T item)
    {
        stack.Push(item);
        type.GetMethod("Push").Invoke(instance, new object[] { item });

        bool originContains = stack.Contains(item);
        bool constructedContains = (bool)type.GetMethod("Contains").Invoke(instance, new object[] { item });

        return originContains == constructedContains;
    }

    public static bool CheckToArray(Stack<T> stack, Type type, object instance, T item)
    {
        stack.Push(item);
        type.GetMethod("Push").Invoke(instance, new object[] { item });

        var originArray = stack.ToArray();
        var constructedArray = (T[])type.GetMethod("ToArray").Invoke(instance, null);

        return originArray.SequenceEqual(constructedArray);
    }
}

