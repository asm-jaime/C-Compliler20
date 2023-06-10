namespace OnlineCompiler.Server.Handlers;

public static class CodeCompileChecker<T>
{

    public static bool CheckPushPushPop(Stack<T> stack, string code, T item)
    {
        var stackType = DynamicClassCreator.CreateClassFromCode(code, "Stack");
        Type constructedType = stackType.MakeGenericType(typeof(T));
        var stackInstance = Activator.CreateInstance(constructedType);

        stack.Push(item);
        stack.Push(item);
        constructedType.GetMethod("Push").Invoke(stackInstance, new object[] {item});
        constructedType.GetMethod("Push").Invoke(stackInstance, new object[] {item});

        var origin = stack.Pop();
        var constructed = (T)constructedType.GetMethod("Pop").Invoke(stackInstance, null);

        return origin.Equals(constructed);
    }
}
