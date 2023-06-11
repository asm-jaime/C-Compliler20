namespace OnlineCompiler.Server.Handlers;

public static class CodeCompileChecker<T>
{
    private static (Type, object) GetInstance(string code, string strType)
    {
        var type = DynamicClassCreator.CreateClassFromCode(code, strType);
        Type constructedType = type.MakeGenericType(typeof(T));
        var instance = Activator.CreateInstance(constructedType);
        return (constructedType, instance);
    }

    public static bool CheckStack(string code, T item)
    {
        Stack<T> stack = new Stack<T>();
        var (type, instance) = GetInstance(code, "Stack");

        return CheckStack<T>.CheckPush(stack, type, instance, item)
            && CheckStack<T>.CheckPeek(stack, type, instance, item)
            && CheckStack<T>.CheckTryPeek(stack, type, instance, item)
            && CheckStack<T>.CheckClear(stack, type, instance, item)
            && CheckStack<T>.CheckContains(stack, type, instance, item)
            && CheckStack<T>.CheckToArray(stack, type, instance, item);
    }

    public static bool CheckList(string code, T item)
    {
        List<T> list = new List<T>();
        var (type, instance) = GetInstance(code, "List");

        return CheckList<T>.CheckAdd(list, type, instance, item)
            && CheckList<T>.CheckRemove(list, type, instance, item)
            && CheckList<T>.CheckInsert(list, type, instance, item)
            && CheckList<T>.CheckClear(list, type, instance, item)
            && CheckList<T>.CheckContains(list, type, instance, item);
    }
}

