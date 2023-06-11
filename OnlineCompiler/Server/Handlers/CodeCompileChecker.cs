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
    
    private static (Type, object) GetInstanceOfDictionary(string code)
    {
        var type = DynamicClassCreator.CreateClassFromCode(code, "Dictionary");
        Type constructedType = type.MakeGenericType(typeof(string), typeof(T));
        var instance = Activator.CreateInstance(constructedType);
        return (constructedType, instance);
    }

    private static (Type, object) GetInstanceOfSortedList(string code)
    {
        var type = DynamicClassCreator.CreateClassFromCode(code, "SortedList");
        Type constructedType = type.MakeGenericType(typeof(string), typeof(T));
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

    public static bool CheckSortedList(string code, T first, T second)
    {
        SortedList<string, T> list = new SortedList<string, T>();
        var (type, instance) = GetInstanceOfSortedList(code);

        var item1 = new KeyValuePair<string, T>("first", first);
        var item2 = new KeyValuePair<string, T>("second", second);

        return CheckSortedList<T>.CheckAdd(type, instance, item1)
            && CheckSortedList<T>.CheckContainsKey(type, instance, item1.Key)
            && CheckSortedList<T>.CheckRemove(type, instance, item1.Key)
            && CheckSortedList<T>.CheckClear(type, instance)
            && CheckSortedList<T>.CheckIsSorted(type, instance, item1, item2);
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
    public static bool CheckHashSet(string code, T item)
    {
        HashSet<T> set = new HashSet<T>();
        var (type, instance) = GetInstance(code, "HashSet");

        return CheckHashSet<T>.CheckAdd(set, type, instance, item)
            && CheckHashSet<T>.CheckRemove(set, type, instance, item)
            && CheckHashSet<T>.CheckClear(set, type, instance, item)
            && CheckHashSet<T>.CheckContains(set, type, instance, default(T))
            && CheckHashSet<T>.CheckUnionSet(set, type, instance, new HashSet<T>());
    }
    public static bool CheckDictionary(string code, string key, T value)
    {
        Dictionary<string, T> dict = new Dictionary<string, T>();
        var (type, instance) = GetInstanceOfDictionary(code);

        return CheckDictionary<T>.CheckAdd(dict, type, instance, new KeyValuePair<string, T>(key, value))
            && CheckDictionary<T>.CheckRemove(dict, type, instance, new KeyValuePair<string, T>("new", value))
            && CheckDictionary<T>.CheckClear(dict, type, instance)
            && CheckDictionary<T>.CheckContainsKey(dict, type, instance, key)
            && CheckDictionary<T>.CheckContainsValue(dict, type, instance, value);
    }

    public static bool CheckObservableCollection(string code, T item)
    {
        var (type, instance) = GetInstance(code, "ObservableCollection");

        return CheckObservableCollection<T>.CheckAdd(type, instance, item)
            && CheckObservableCollection<T>.CheckRemove(type, instance, item)
            && CheckObservableCollection<T>.CheckClear(type, instance)
            && CheckObservableCollection<T>.CheckInsert(type, instance, item)
            && CheckObservableCollection<T>.CheckContains(type, instance, item);
    }
}

