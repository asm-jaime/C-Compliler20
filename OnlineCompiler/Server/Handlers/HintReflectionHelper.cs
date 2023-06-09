using System.Reflection;

namespace OnlineCompiler.Server.Handlers;

public static class HintReflectionHelper
{
    private static string GetUserCode(string a, string b, string code)
    {
        int length = code.IndexOf(b) - code.IndexOf(a);
        var userMethodCode =
            code.Substring(code.IndexOf(a), length);
        return userMethodCode;
    }

    public static void GetReflectionHints(string code, Type constructedType, List<string> hints)
    {
        //TryInsertTest
        var insertMethod = constructedType.GetMethod("TryInsert",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var insertBody = insertMethod.GetMethodBody().LocalVariables;
        if (!insertBody.Any(x =>
                x.LocalType.ToString().Contains("Dictionary`2+Entry[System.String,System.String][]")))
        {
            hints.Add(
                "Метод TryInsert: Попробуйте добавить в код использование Entry[], присвоив значение _entries");
        }

        if (!insertBody.Any(x =>
                x.LocalType.ToString()
                    .Contains("System.Collections.Generic.IEqualityComparer`1[System.String]")))
        {
            hints.Add(
                "Метод TryInsert: Попробуйте добавить в код использование IEqualityComparer<TKey>, присвоив значение _comparer. После чего вычислять по нему GetHashCode()");
        }

        var userMethodCode = GetUserCode(
            "private bool TryInsert(TKey key, TValue value, InsertionBehavior behavior)",
            "private void Resize() => Resize(ExpandPrime(_count), false);", code);

        if (!userMethodCode.Contains("if (key == null)") && !userMethodCode.Contains("if (key is null)"))
        {
            hints.Add("Метод TryInsert: Добавьте проверку key на null в начале метода");
        }

        if (!userMethodCode.Contains("ref GetBucket("))
        {
            hints.Add(
                "Метод TryInsert: Попробуйте использовать метод GetBucket, передавая в него hashCode. Важно, что GetBucket нужно вызывать с ref");
        }

        if (!userMethodCode.Contains("GetHashCode("))
        {
            hints.Add("Метод TryInsert: В методе необходимо использовать метод GetHashCode()");
        }

        if (!userMethodCode.Contains("Resize()"))
        {
            hints.Add("Метод TryInsert: В методе необходимо использовать метод Resize()");
        }

        //Remove test
        var removeMethod = constructedType.GetMethod("Remove");
        var removeBody = removeMethod.GetMethodBody().LocalVariables;

        if (!removeBody.Any(x =>
                x.LocalType.ToString().Contains("Dictionary`2+Entry[System.String,System.String][]")))
        {
            hints.Add(
                "Метод Remove: Попробуйте добавить в код использование Entry[], присвоив значение _entries");
        }

        userMethodCode = GetUserCode("public bool Remove(TKey key)",
            "public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)", code);

        if (!userMethodCode.Contains("ref GetBucket("))
        {
            hints.Add(
                "Метод Remove: Попробуйте использовать метод GetBucket, передавая в него hashCode. Важно, что GetBucket нужно вызывать с ref");
        }

        if (!userMethodCode.Contains("StartOfFreeList"))
        {
            hints.Add(
                "Метод Remove:  Попробуйте добавить использование StartOfFreeList, для вычисления  entry.next");
        }

        if (!userMethodCode.Contains("uint"))
        {
            hints.Add(
                "Метод Remove: Для хранения hashCode и количества коллизий необходимо использовать uint");
        }

        //FindTest
        var findMethod = constructedType.GetMethod("FindValue",
            BindingFlags.NonPublic | BindingFlags.Instance);

        var findBody = findMethod.GetMethodBody().LocalVariables;

        if (!findBody.Any(x =>
                x.LocalType.ToString().Contains("Dictionary`2+Entry[System.String,System.String]")))
        {
            hints.Add("Метод FindValue: Для работы с Entry используйте ref Unsafe.NullRef<Entry>()");
        }

        if (!findBody.Any(x => x.LocalType.ToString().Contains("System.UInt32")))
        {
            hints.Add(
                "Метод FindValue: Для хранения hashCode и количества коллизий необходимо использовать uint");
        }

        userMethodCode = GetUserCode("private ref TValue FindValue(TKey key)",
            "private static int GetPrime(int min)", code);

        if (!userMethodCode.Contains("GetHashCode("))
        {
            hints.Add("Метод FindValue: В методе необходимо использовать метод GetHashCode()");
        }

        if (!userMethodCode.Contains("IsValueType"))
        {
            hints.Add("Метод FindValue: Необходимо обрабатывать ValueType, используя IsValueType");
        }

        if (!userMethodCode.Contains("_comparer"))
        {
            hints.Add("Метод FindValue: Используйте _comparer для сравнения значений");
        }
    }

    public static void GetReflectionHintsHashSet(string code, Type constructedType, List<string> hints)
    {
        //AddTest
        var insertMethod = constructedType.GetMethod("AddIfNotPresent",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var insertBody = insertMethod.GetMethodBody().LocalVariables;
        if (!insertBody.Any(x =>
                x.LocalType.ToString().Contains("System.Collections.Generic.HashSet`1+Entry[System.String]")))
        {
            hints.Add(
                "Метод AddIfNotPresent: Попробуйте добавить в код использование Entry[], присвоив значение _entries");
        }

        if (!insertBody.Any(x =>
                x.LocalType.ToString().Contains("System.Collections.Generic.IEqualityComparer`1[System.String]")))
        {
            hints.Add(
                "Метод AddIfNotPresent: Попробуйте добавить в код использование IEqualityComparer<T>, присвоив значение _comparer. После чего вычислять по нему GetHashCode(). При null в _comparer код должен продолжать работать.");
        }

        var userMethodCode = GetUserCode("private bool AddIfNotPresent(T value, out int location)",
            "private static IEqualityComparer<string?> GetUnderlyingEqualityComparer(IEqualityComparer<string?>? outerComparer)",
            code);

        if (!userMethodCode.Contains("Initialize(0)"))
        {
            hints.Add("Метод AddIfNotPresent: Добавьте проверку _buckets на null. И сделайте Initialize, при null");
        }

        if (!userMethodCode.Contains("ref"))
        {
            hints.Add(
                "Метод AddIfNotPresent: С buckets нужно работать при помощи ref. Попробуйте использовать GetBucketRef");
        }

        if (!userMethodCode.Contains("GetHashCode("))
        {
            hints.Add("Метод AddIfNotPresent: В методе необходимо использовать метод GetHashCode()");
        }

        if (!userMethodCode.Contains(".IsValueType"))
        {
            hints.Add(
                "Метод AddIfNotPresent: Типы значений никогда не вызывают rehash. Но нужно обработать collision threshold для ссылочных типов.");
        }

        if (!userMethodCode.Contains("Resize("))
        {
            hints.Add(
                "Метод AddIfNotPresent: Попробуйте использовать Resize() и FindItemIndex() при решении вопроса с коллизией");
        }

        //Remove test
        var removeMethod = constructedType.GetMethod("Remove");
        var removeBody = removeMethod.GetMethodBody().LocalVariables;

        if (!removeBody.Any(x =>
                x.LocalType.ToString().Contains("System.Collections.Generic.HashSet`1+Entry[System.String]")))
        {
            hints.Add("Метод Remove: Попробуйте добавить в код использование Entry[], присвоив значение _entries");
        }

        userMethodCode = GetUserCode("public bool Remove(T item)",
            "public int Count => _count - _freeCount;", code);

        if (!userMethodCode.Contains(".GetHashCode("))
        {
            hints.Add("Метод Remove: При работе со значением, постарайтесь использовать .GetHashCode()");
        }

        if (!userMethodCode.Contains("ref GetBucketRef("))
        {
            hints.Add("Метод Remove: При работе с bucket необходимо использовать ref GetBucketRef()");
        }

        if (!userMethodCode.Contains("EqualityComparer<T>.Default"))
        {
            hints.Add("Метод Remove: Если comparer null, то используйте EqualityComparer<T>.Default");
        }

        if (!userMethodCode.Contains(".Next"))
        {
            hints.Add("Метод Remove: Обратите внимание на структуру Entries и свойство entry.Next");
        }

        if (!userMethodCode.Contains("collis"))
        {
            hints.Add("Метод Remove: Обратите внимание на обработку коллизий");
        }

        //FindTest
        var findMethod = constructedType.GetMethod("FindItemIndex",
            BindingFlags.NonPublic | BindingFlags.Instance);

        var findBody = findMethod.GetMethodBody().LocalVariables;

        if (!findBody.Any(x => x.LocalType.ToString().Contains("System.Int32[]")))
        {
            hints.Add("Метод FindItemIndex: Для работы с buckets рекомендуется создать локальную переменную");
        }

        if (!findBody.Any(x =>
                x.LocalType.ToString().Contains("System.Collections.Generic.IEqualityComparer`1[System.String]")))
        {
            hints.Add("Метод FindItemIndex: Для работы с comparer рекомендуется создать локальную переменную");
        }

        userMethodCode = GetUserCode("private int FindItemIndex(T item)",
            "private ref int GetBucketRef(int hashCode)", code);

        if (!userMethodCode.Contains("GetHashCode("))
        {
            hints.Add("Метод FindItemIndex: В методе необходимо использовать метод GetHashCode()");
        }

        if (!userMethodCode.Contains("IsValueType"))
        {
            hints.Add("Метод FindItemIndex: Необходимо обрабатывать ValueType, используя IsValueType");
        }

        if (!userMethodCode.Contains("GetBucketRef("))
        {
            hints.Add("Метод FindItemIndex: При работе с bucket необходимо использовать GetBucketRef()");
        }

        if (!userMethodCode.Contains("return -1"))
        {
            hints.Add("Метод FindItemIndex: При отсутствии вхождения элемента - необходимо возвращать -1");
        }
    }
}