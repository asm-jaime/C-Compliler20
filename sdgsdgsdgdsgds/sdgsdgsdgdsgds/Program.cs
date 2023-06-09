using System.Reflection;
using OnlineCompiler.Client.Pages;
using sdgsdgsdgdsgds;

var code = TemplateQueue.QueueCode;
var dicType = DynamicClassCreator.CreateClassFromCode(code, "Queue");
Type constructedType = dicType.MakeGenericType(typeof(string));
if (constructedType == null)
{
    throw new Exception("Error");
}

if (constructedType != null)
{
    var hints = new List<string>();
    //AddTest
    /*var addFirstMethod = constructedType.GetMethod("Enqueue");*/
    /*var insertBody = addFirstMethod.GetMethodBody().LocalVariables;
    if (!insertBody.Any(x =>
            x.LocalType.ToString().Contains("System.String[]")))
    {
        hints.Add("Метод Add: Рекомендуется использовать _items, присвоив значение в локальную переменную");
    }*/
    
    /*if (!insertBody.Any(x =>
            x.LocalType.ToString().Contains("System.Int32")))
    {
        hints.Add("Метод Add: Рекомендуется использовать _size, присвоив значение в локальную переменную");
    }*/

    var userMethodCode = GetUserCode("public void Enqueue(T item)",
        "internal static T[] ToArray<T>(IEnumerable<T> source, out int length)",
        code);

    if (!userMethodCode.Contains("_size"))
    {
        hints.Add("Метод Enqueue: Добавить проверку _size с длинной масива");
    }

    if (!userMethodCode.Contains("Grow("))
    {
        hints.Add("Метод Enqueue: Используйте Grow() для увеличения _size массива");
    }
    
    if (!userMethodCode.Contains("MoveNext("))
    {
        hints.Add("Метод Enqueue: Сместите указатель tail с помощью метода MoveNext(), используя ref");
    }
    
    //Remove test
    var removeMethod = constructedType.GetMethod("Dequeue");
    var removeBOdy = removeMethod.GetMethodBody().LocalVariables;
    if (!removeBOdy.Any(x =>
            x.LocalType.ToString().Contains("System.String[]")))
    {
        hints.Add("Метод Dequeue: Рекомендуется использовать _array, присвоив значение в локальную переменную");
    }
    
    if (!removeBOdy.Any(x =>
            x.LocalType.ToString().Contains("System.Int32")))
    {
        hints.Add("Метод Dequeue: Рекомендуется использовать _head, присвоив значение в локальную переменную");
    }
    
    userMethodCode = GetUserCode("public T Dequeue()",
        "public bool TryDequeue([MaybeNullWhen(false)] out T result)", code);

    if (!userMethodCode.Contains("IsReferenceOrContainsReferences"))
    {
        hints.Add("Метод Dequeue: Добавьте проверку RuntimeHelpers.IsReferenceOrContainsReferences");
    }

    if (!userMethodCode.Contains("ThrowForEmptyQueue"))
    {
        hints.Add("Метод Dequeue: При пустои queue рекомендуется использовать ThrowForEmptyQueue()");
    }

    if (!userMethodCode.Contains("MoveNext"))
    {
        hints.Add("Метод Dequeue: Сместите указатель _head при помощи MoveNext()");
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