using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace sdgsdgsdgdsgds;

public static class DynamicClassCreator
{
    public static Type CreateClassFromCode(string code, string name)
    {
        var systemRuntimeAssembly1 = Assembly.Load("System.Collections");
        MetadataReference systemRuntimeReference1 = MetadataReference.CreateFromFile(systemRuntimeAssembly1.Location);
        var systemRuntimeAssembly = Assembly.Load("System.Runtime");
        MetadataReference systemRuntimeReference = MetadataReference.CreateFromFile(systemRuntimeAssembly.Location);
        var systemRuntimeAssembly2 = Assembly.Load("System.Data");
        MetadataReference systemRuntimeReference2 = MetadataReference.CreateFromFile(systemRuntimeAssembly2.Location);

        // Создание компиляции с помощью компилятора Roslyn
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var assemblyName = Guid.NewGuid().ToString();
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Unsafe).Assembly.Location),
            systemRuntimeReference,
            systemRuntimeReference1,
            systemRuntimeReference2,
            // Добавьте другие необходимые ссылки на сборки
        };
 
        var compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Генерация сборки
        using var ms = new MemoryStream();
        var errorMessage = "";
        var result = compilation.Emit(ms);
        if (!result.Success)
        {
            var failures = result.Diagnostics.Where(diagnostic =>
                diagnostic.IsWarningAsError ||
                diagnostic.Severity == DiagnosticSeverity.Error);

            foreach (var diagnostic in failures)
            {
                errorMessage += diagnostic.Id + ":" +diagnostic.GetMessage() + "\n";
            }

            throw new ArgumentException(errorMessage);
        }

        ms.Seek(0, SeekOrigin.Begin);

        // Загрузка сборки и возврат созданного типа класса
        var assembly = Assembly.Load(ms.ToArray());
        return assembly.DefinedTypes.FirstOrDefault(x=>x.Name.Contains(name));
    }
}