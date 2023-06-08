using System.Reflection;
using OnlineCompiler.Client.Pages;
using sdgsdgsdgdsgds;

var userCode = TemplateDictionary.UserDictionaryCode;
var code = TemplateDictionary.DictionaryCode;
var dicType = DynamicClassCreator.CreateClassFromCode(code, "Dictionary");
Type constructedType = dicType.MakeGenericType(typeof(string), typeof(string));
if (constructedType == null)
{
    throw new Exception("Error");
}

if (constructedType != null)
{
    var hints = new List<string>();
    var instance = Activator.CreateInstance(constructedType);
    var safasf = constructedType.GetMethods();
    //TryInsertTest
    var insertMethod = constructedType.GetMethod("TryInsert",
        BindingFlags.NonPublic | BindingFlags.Instance);
    var insertBody = insertMethod.GetMethodBody().LocalVariables;
    if (!insertBody.Any(x => x.LocalType.ToString().Contains("Dictionary`2+Entry[System.String,System.String][]")))
    {
        hints.Add("Метод TryInsert: Попробуйте добавить в код использование Entry[], присвоив значение _entries");
    }

    if (!insertBody.Any(x => x.LocalType.ToString().Contains("System.Collections.Generic.IEqualityComparer`1[System.String]")))
    {
        hints.Add(
            "Метод TryInsert: Попробуйте добавить в код использование IEqualityComparer<TKey>, присвоив значение _comparer. После чего вычислять по нему GetHashCode()");
    }
    
    var userMethodCode = GetUserCode("private bool TryInsert(TKey key, TValue value, InsertionBehavior behavior)",
        "private void Resize() => Resize(ExpandPrime(_count), false);", code);

    if (!userMethodCode.Contains("if (key == null)") && !userMethodCode.Contains("if (key is null)"))
    {
        hints.Add("Метод TryInsert: Добавьте проверку key на null в начале метода");
    }
    
    if (!userMethodCode.Contains("ref GetBucket("))
    {
        hints.Add("Метод TryInsert: Попробуйте использовать метод GetBucket, передавая в него hashCode. Важно, что GetBucket нужно вызывать с ref");
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
    
    if (!removeBody.Any(x => x.LocalType.ToString().Contains("Dictionary`2+Entry[System.String,System.String][]")))
    {
        hints.Add("Метод Remove: Попробуйте добавить в код использование Entry[], присвоив значение _entries");
    }
    
    userMethodCode = GetUserCode("public bool Remove(TKey key)",
        "public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)", code);
    
    if (!userMethodCode.Contains("ref GetBucket("))
    {
        hints.Add("Метод Remove: Попробуйте использовать метод GetBucket, передавая в него hashCode. Важно, что GetBucket нужно вызывать с ref");
    }
    if (!userMethodCode.Contains("StartOfFreeList"))
    {
        hints.Add("Метод Remove:  Попробуйте добавить использование StartOfFreeList, для вычисления  entry.next");
    }
    if (!userMethodCode.Contains("uint"))
    {
        hints.Add("Метод Remove: Для хранения hashCode и количества коллизий необходимо использовать uint");
    }

    //FindTest
    var findMethod = constructedType.GetMethod("FindValue",
        BindingFlags.NonPublic | BindingFlags.Instance);
    
    var findBody = findMethod.GetMethodBody().LocalVariables;

    if (!findBody.Any(x => x.LocalType.ToString().Contains("Dictionary`2+Entry[System.String,System.String]")))
    {
        hints.Add("Метод FindValue: Для работы с Entry используйте ref Unsafe.NullRef<Entry>()");
    }
    
    if (!findBody.Any(x => x.LocalType.ToString().Contains("System.UInt32")))
    {
        hints.Add("Метод FindValue: Для хранения hashCode и количества коллизий необходимо использовать uint");
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


static string GetUserCode(string a, string b, string code)
{
    int length = code.IndexOf(b) - code.IndexOf(a);
    var userMethodCode =
        code.Substring(code.IndexOf(a), length);
    return userMethodCode;
}