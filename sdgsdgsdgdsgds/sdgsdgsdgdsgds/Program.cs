using System.Reflection;
using OnlineCompiler.Client.Pages;
using sdgsdgsdgdsgds;

var code = TemplateList.ListCode;
var dicType = DynamicClassCreator.CreateClassFromCode(code, "List");
Type constructedType = dicType.MakeGenericType(typeof(string));
if (constructedType == null)
{
    throw new Exception("Error");
}

if (constructedType != null)
{
    var hints = new List<string>();
    //AddTest
    var addFirstMethod = constructedType.GetMethod("Add");
    var insertBody = addFirstMethod.GetMethodBody().LocalVariables;
    if (!insertBody.Any(x =>
            x.LocalType.ToString().Contains("System.String[]")))
    {
        hints.Add("Метод Add: Рекомендуется использовать _items, присвоив значение в локальную переменную");
    }
    
    if (!insertBody.Any(x =>
            x.LocalType.ToString().Contains("System.Int32")))
    {
        hints.Add("Метод Add: Рекомендуется использовать _size, присвоив значение в локальную переменную");
    }

    var userMethodCode = GetUserCode("public void Add(T item)",
        "private void AddWithResize(T item)",
        code);

    if (!userMethodCode.Contains("uint"))
    {
        hints.Add("Метод Add: Используйте каст к uint при сравнении size");
    }

    if (!userMethodCode.Contains("_version"))
    {
        hints.Add("Метод Add: Обратите внимание на _version, необходимо увеличивать его значение");
    }
    
    if (!userMethodCode.Contains("AddWithResize"))
    {
        hints.Add("Метод Add: При вставке элемента необходимо проверять _size и при необходимости вызывать метод AddWithResize()");
    }
    
    //Remove test
    userMethodCode = GetUserCode("public void RemoveAt(int index)",
        "public struct Enumerator : IEnumerator<T>, IEnumerator", code);

    if (!userMethodCode.Contains("uint"))
    {
        hints.Add("Метод RemoveAt: Используйте каст к uint при сравнении size и работой с index");
    }

    if (!userMethodCode.Contains("Array.Copy"))
    {
        hints.Add("Метод RemoveAt: Используйте Array.Copy, если index меньше _size");
    }

    if (!userMethodCode.Contains("IsReferenceOrContainsReferences"))
    {
        hints.Add("Метод RemoveAt: Попробуйте добавить использование RuntimeHelpers.IsReferenceOrContainsReferences");
    }
    
    if (!userMethodCode.Contains("_version"))
    {
        hints.Add("Метод RemoveAt: Обратите внимание на _version, необходимо увеличивать его значение");
    }

    //FindTest
    userMethodCode = GetUserCode("public T? Find(Predicate<T> match)",
        "public List<T> FindAll(Predicate<T> match)", code);

    if (!userMethodCode.Contains("match == null") && !userMethodCode.Contains("match is null"))
    {
        hints.Add("Метод Find: Обработайте случай, когда match равен null");
    }

    if (!userMethodCode.Contains("_size"))
    {
        hints.Add("Метод Find: Для поиска значений рекомендуется использовать for с 0 элемента до _size");
    }

    if (!userMethodCode.Contains("match("))
    {
        hints.Add("Метод Find: Попробуйте использовать предикат match при работе с элементом List");
    }

    if (!userMethodCode.Contains("default"))
    {
        hints.Add("Метод Find: При отсутствии элемента, рекомендуется вернуть default");
    }
}


static string GetUserCode(string a, string b, string code)
{
    int length = code.IndexOf(b) - code.IndexOf(a);
    var userMethodCode =
        code.Substring(code.IndexOf(a), length);
    return userMethodCode;
}