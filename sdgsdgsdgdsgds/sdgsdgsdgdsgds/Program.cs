using System.Reflection;
using OnlineCompiler.Client.Pages;
using sdgsdgsdgdsgds;

var code = TemplateLinkedList.LinkedListCode;
var dicType = DynamicClassCreator.CreateClassFromCode(code, "LinkedList");
Type constructedType = dicType.MakeGenericType(typeof(string));
if (constructedType == null)
{
    throw new Exception("Error");
}

if (constructedType != null)
{
    var hints = new List<string>();
    //AddTest
    var addFirstMethod = constructedType
        .GetMethods()
        .First(mi => mi.Name == "AddFirst" && mi.ReturnType != typeof(void));
    var insertBody = addFirstMethod.GetMethodBody().LocalVariables;
    if (!insertBody.Any(x =>
            x.LocalType.ToString().Contains("System.Collections.Generic.LinkedListNode`1[System.String]")))
    {
        hints.Add("Метод AddFirst: Попробуйте использовать LinkedListNode");
    }

    var userMethodCode = GetUserCode("public LinkedListNode<T> AddFirst(T value)",
        "public void AddFirst(LinkedListNode<T> node)",
        code);

    if (!userMethodCode.Contains("head == null") && !userMethodCode.Contains("head is null"))
    {
        hints.Add("Метод AddFirst: Добавьте проверку head на null");
    }

    if (!userMethodCode.Contains("InternalInsertNodeToEmptyList"))
    {
        hints.Add(
            "Метод AddFirst: Попробуйте использовать InternalInsertNodeToEmptyList");
    }
    
    var addFirstMethod1 = constructedType
        .GetMethods()
        .First(mi => mi.Name == "AddBefore" && mi.ReturnType != typeof(void));
    var insertBody1 = addFirstMethod1.GetMethodBody().LocalVariables;
    
    if (!insertBody1.Any(x =>
            x.LocalType.ToString().Contains("System.Collections.Generic.LinkedListNode`1[System.String]")))
    {
        hints.Add("Метод AddBefore: Попробуйте использовать LinkedListNode");
    }
    
    var userMethodCode1 = GetUserCode("public LinkedListNode<T> AddBefore(LinkedListNode<T> node, T value)",
        "public void AddBefore(LinkedListNode<T> node, LinkedListNode<T> newNode)",
        code);
    
    if (!userMethodCode1.Contains("ValidateNode("))
    {
        hints.Add("Метод AddBefore: Добавьте проверку ValidateNode()");
    }
    if (!userMethodCode1.Contains("InternalInsertNodeBefore"))
    {
        hints.Add("Метод AddBefore: Попробуйте использовать InternalInsertNodeToEmptyList");
    }
    if (!userMethodCode1.Contains("node == head"))
    {
        hints.Add("Метод AddBefore: Добавьте проверку того, что node == head, с присвоением result к head");
    }
    
    var addFirstMethod2 = constructedType
        .GetMethods()
        .First(mi => mi.Name == "AddAfter" && mi.ReturnType != typeof(void));
    var insertBody2 = addFirstMethod2.GetMethodBody().LocalVariables;
    if (!insertBody2.Any(x =>
            x.LocalType.ToString().Contains("System.Collections.Generic.LinkedListNode")))
    {
        hints.Add("Метод AddAfter: Попробуйте использовать LinkedListNode");
    }
    var userMethodCode2 = GetUserCode("public LinkedListNode<T> AddAfter(LinkedListNode<T> node, T value)",
        "public void AddAfter(LinkedListNode<T> node, LinkedListNode<T> newNode)",
        code);
    if (!userMethodCode2.Contains("ValidateNode("))
    {
        hints.Add("Метод AddAfter: Добавьте проверку ValidateNode() для node");
    }
    if (!userMethodCode2.Contains("InternalInsertNodeBefore"))
    {
        hints.Add("Метод AddAfter: Попробуйте использовать InternalInsertNodeToEmptyList");
    }
    //Remove test
    userMethodCode = GetUserCode("public bool Remove(T value)",
        "public void Remove(LinkedListNode<T> node)", code);

    if (!userMethodCode.Contains("Find("))
    {
        hints.Add("Метод Remove: Необходимо использовать метод Find() для значения value");
    }

    if (!userMethodCode.Contains("InternalRemoveNode("))
    {
        hints.Add("Метод Remove: Добавьте использование метода InternalRemoveNode()");
    }

    if (!userMethodCode.Contains("node != null") && !userMethodCode.Contains("node is not null"))
    {
        hints.Add("Метод Remove: Добавьте проверку node на null");
    }

    //FindTest
    var findMethod = constructedType.GetMethod("Find");

    var findBody = findMethod.GetMethodBody().LocalVariables;

    if (!findBody.Any(x => x.LocalType.ToString().Contains("System.Collections.Generic.LinkedListNode")))
    {
        hints.Add("Метод Find: Используйте LinkedListNode с присвоением в него head");
    }

    if (!findBody.Any(x =>
            x.LocalType.ToString().Contains("System.Collections.Generic.EqualityComparer`1[System.String]")))
    {
        hints.Add("Метод Find: Для значения comparer используйте EqualityComparer<T>.Default");
    }

    userMethodCode = GetUserCode("public LinkedListNode<T>? Find(T value)",
        "public LinkedListNode<T>? FindLast(T value)", code);

    if (!userMethodCode.Contains(".Equals("))
    {
        hints.Add("Метод Find: Для сравнения значений value и node.item используйте Equals()");
    }

    if (!userMethodCode.Contains("return null"))
    {
        hints.Add("Метод Find: Если значение не было найдено, возвращайте null");
    }

    if (!userMethodCode.Contains("node != null") && !userMethodCode.Contains("node is not null"))
    {
        hints.Add("Метод Find: Добавьте проверку node на null");
    }

    if (!userMethodCode.Contains(".next"))
    {
        hints.Add("Метод Find: Для обработки всех значений node воспользуйтесь node.next");
    }
}


static string GetUserCode(string a, string b, string code)
{
    int length = code.IndexOf(b) - code.IndexOf(a);
    var userMethodCode =
        code.Substring(code.IndexOf(a), length);
    return userMethodCode;
}