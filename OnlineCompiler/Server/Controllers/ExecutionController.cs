namespace OnlineCompiler.Server.Controllers;
using Microsoft.AspNetCore.Mvc;
using OnlineCompiler.Server.Handlers;
using OnlineCompiler.Shared;
using System.Reflection;

[ApiController]
[Route("[controller]")]
public class ExecutionController : ControllerBase
{
    private static Dictionary<string, CodeExecutor> _codeExecutors = new Dictionary<string, CodeExecutor>();

    private readonly ILogger<ExecutionController> _logger;

    public ExecutionController(ILogger<ExecutionController> logger)
    {
        _logger = logger;
    }

    public enum StructureType
    {
        Dictionary,
        HashSet,
        List,
        Queue,
        Stack,
        SortedList,
        LinkedList
    }

    private ExecutionInfo ProcessCode(string? code, StructureType structureType)
    {
        if (code == null) return null;

        var hints = new List<string>();

        try
        {
            Type genericType = structureType switch
            {
                StructureType.Dictionary => typeof(KeyValuePair<string, string>),
                StructureType.SortedList => typeof(KeyValuePair<string, string>),
                _ => typeof(string)
            };

            var dataType = DynamicClassCreator.CreateClassFromCode(code, structureType.ToString());
            Type constructedType = structureType switch
            {
                StructureType.Dictionary => dataType.MakeGenericType(typeof(string), typeof(string)),
                StructureType.SortedList => dataType.MakeGenericType(typeof(string), typeof(string)),
                _ => dataType.MakeGenericType(genericType),
            };

            if (constructedType == null) throw new Exception("Error");

            var hintMethod = typeof(HintReflectionHelper).GetMethod($"GetReflectionHints{structureType}", BindingFlags.Public | BindingFlags.Static);
            hintMethod?.Invoke(null, new object[] { code, constructedType, hints });

            bool checkResult = structureType switch
            {
                StructureType.List => CodeCompileChecker<string>.CheckList(code, "first"),
                StructureType.LinkedList => CodeCompileChecker<string>.CheckLinkedList(code, "first"),
                StructureType.SortedList => CodeCompileChecker<string>.CheckSortedList(code, "a", "b"),
                StructureType.Stack => CodeCompileChecker<string>.CheckStack(code, "first"),
                StructureType.HashSet => CodeCompileChecker<string>.CheckHashSet(code, "first"),
                StructureType.Dictionary => CodeCompileChecker<string>.CheckDictionary(code, "key", "value"),
                StructureType.Queue => CodeCompileChecker<string>.CheckQueue(code, "first"),
                _ => false,
            };

            if (checkResult.Equals(false)) return new ExecutionInfo(
                ExecutionInfo.ExecutionStatus.CompilationError,
                0,
                $"Структура {structureType} не прошла проверку на соответствие с поведением net standard на реальных данных",
                hints
            );
        }
        catch (ArgumentException e)
        {
            return new ExecutionInfo(ExecutionInfo.ExecutionStatus.CompilationError, e.Message);
        }
        catch (Exception ex)
        {
            return new ExecutionInfo(
                ExecutionInfo.ExecutionStatus.CompilationError,
                $"Произошла ошибка при запуске структуры {structureType}: {ex}"
            );
        }

        return new ExecutionInfo(hints.Count > 0 ? ExecutionInfo.ExecutionStatus.WithWarning : ExecutionInfo.ExecutionStatus.Finished, 111, "", hints);
    }

    /// <summary>
    /// Request for compiling and executing code
    /// </summary>
    /// <param name="code">C# code</param>
    /// <param name="structureType">type of structure that should be checked</param>
    /// <returns>Unique id of the operation</returns>
    [HttpPost]
    [Route("{structureType}")]
    public ExecutionInfo Post([FromBody] string? code, StructureType structureType)
    {
        return ProcessCode(code, structureType);
    }

    /// <summary>
    /// Getting execution information by id
    /// </summary>
    /// <param name="id">id</param>
    /// <returns>Execution Information</returns>
    [HttpGet]
    [Route("{id}")]
    public ExecutionInfo? Get(string id)
    {
        if (!_codeExecutors.ContainsKey(id))
            return null;
        return _codeExecutors[id].ExecutionInfo;
    }
}
