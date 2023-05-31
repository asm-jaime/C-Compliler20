using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
using System.Runtime.CompilerServices;

public class DynamicClassCreator
{
    public static Type CreateClassFromCode(string code, string name)
    {
        Assembly systemRuntimeAssembly1 = Assembly.Load("System.Collections");
        MetadataReference systemRuntimeReference1 = MetadataReference.CreateFromFile(systemRuntimeAssembly1.Location);
        Assembly systemRuntimeAssembly = Assembly.Load("System.Runtime");
        MetadataReference systemRuntimeReference = MetadataReference.CreateFromFile(systemRuntimeAssembly.Location);
        Assembly systemRuntimeAssembly2 = Assembly.Load("System.Data");
        MetadataReference systemRuntimeReference2 = MetadataReference.CreateFromFile(systemRuntimeAssembly2.Location);

        // Создание компиляции с помощью компилятора Roslyn
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
        string assemblyName = Guid.NewGuid().ToString();
        MetadataReference[] references = new MetadataReference[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Unsafe).Assembly.Location),
            systemRuntimeReference,
            systemRuntimeReference1,
            systemRuntimeReference2,
            // Добавьте другие необходимые ссылки на сборки
        };
 
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Генерация сборки
        using (var ms = new System.IO.MemoryStream())
        {
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

                throw new Exception(errorMessage);
            }

            ms.Seek(0, System.IO.SeekOrigin.Begin);

            // Загрузка сборки и возврат созданного типа класса
            Assembly assembly = Assembly.Load(ms.ToArray());
            return assembly.DefinedTypes.FirstOrDefault(x=>x.Name.Contains(name));
        }
    }
}