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

    private static (Type, object) GetInstanceOfSortedList(string code)
    {
        var type = DynamicClassCreator.CreateClassFromCode(code, "SortedList");
        Type constructedType = type.MakeGenericType(typeof(T), typeof(T));
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

    public static bool CheckLinkedList(string code, T item)
    {
        LinkedList<T> list = new LinkedList<T>();
        var (type, instance) = GetInstance(code, "LinkedList");

        return CheckLinkedList<T>.CheckAddLast(list, type, instance, item)
            && CheckLinkedList<T>.CheckAddFirst(list, type, instance, item)
            && CheckLinkedList<T>.CheckRemove(list, type, instance, item)
            && CheckLinkedList<T>.CheckClear(list, type, instance, item)
            && CheckLinkedList<T>.CheckContains(list, type, instance, item);
    }

    public static bool CheckSortedList(string code, T item)
    {
        SortedList<T, T> list = new SortedList<T, T>();
        var (type, instance) = GetInstanceOfSortedList(code);

        var item1 = new KeyValuePair<T, T>(item, item);
        var item2 = new KeyValuePair<T, T>(item, item);

        return CheckSortedList<T>.CheckAdd(type, instance, item1)
            //&& CheckSortedList<T>.CheckContainsKey(type, instance, item1.Key)
            //&& CheckSortedList<T>.CheckRemove(type, instance, item1.Key)
            && CheckSortedList<T>.CheckClear(type, instance);
            //&& CheckSortedList<T>.CheckIsSorted(type, instance, item1, item2);
    }

    public static bool CheckQueue(string code, T item)
    {
        Queue<T> queue = new Queue<T>();
        var (type, instance) = GetInstance(code, "Queue");

        return CheckQueue<T>.CheckEnqueue(queue, type, instance, item)
            && CheckQueue<T>.CheckDequeue(queue, type, instance)
            && CheckQueue<T>.CheckPeek(queue, type, instance, item)
            && CheckQueue<T>.CheckClear(queue, type, instance)
            && CheckQueue<T>.CheckContains(queue, type, instance, item);
    }

}

