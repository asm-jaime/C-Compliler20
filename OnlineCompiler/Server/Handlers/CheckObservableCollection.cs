using System.Collections.ObjectModel;

public static class CheckObservableCollection<T>
{
    public static bool CheckAdd(Type type, object instance, T item)
    {
        var list = new ObservableCollection<T>();
        list.Add(item);
        type.GetMethod("Add").Invoke(instance, new object[] { item });

        return list.Count == (int)type.GetProperty("Count").GetValue(instance);
    }

    public static bool CheckRemove(Type type, object instance, T item)
    {
        var list = new ObservableCollection<T>();
        list.Add(item);
        type.GetMethod("Add").Invoke(instance, new object[] { item });
        list.Remove(item);
        type.GetMethod("Remove").Invoke(instance, new object[] { item });

        return list.Count == (int)type.GetProperty("Count").GetValue(instance);
    }

    public static bool CheckClear(Type type, object instance)
    {
        var list = new ObservableCollection<T>();
        list.Add(default(T));
        type.GetMethod("Add").Invoke(instance, new object[] { default(T) });
        list.Clear();
        type.GetMethod("Clear").Invoke(instance, null);

        return list.Count == (int)type.GetProperty("Count").GetValue(instance);
    }

    public static bool CheckInsert(Type type, object instance, T item)
    {
        var list = new ObservableCollection<T>();
        list.Insert(0, item);
        type.GetMethod("Insert").Invoke(instance, new object[] { 0, item });

        return list.Count == (int)type.GetProperty("Count").GetValue(instance);
    }

    public static bool CheckContains(Type type, object instance, T item)
    {
        var list = new ObservableCollection<T>();
        list.Add(item);
        type.GetMethod("Add").Invoke(instance, new object[] { item });

        bool originContains = list.Contains(item);
        bool constructedContains = (bool)type.GetMethod("Contains").Invoke(instance, new object[] { item });

        return originContains == constructedContains;
    }
}

