using OnlineCompiler.Shared;
using Microsoft.AspNetCore.Mvc;
using OnlineCompiler.Server.Handlers;

namespace OnlineCompiler.Server.Controllers
{
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

        /// <summary>
        /// Request for compiling and executing code
        /// </summary>
        /// <param name="code">C# code</param>
        /// <returns>Unique id of the operation</returns>
        [HttpPost]
        public ExecutionInfo Post([FromBody]string? code)
        {
            if (code == null)
                return null;
            try
            {
                var dicType = DynamicClassCreator.CreateClassFromCode(code, "Dictionary");
                Type constructedType = dicType.MakeGenericType(typeof(string),typeof(string));
                if (constructedType != null)
                {
                    var carInstance = Activator.CreateInstance(constructedType);
                    constructedType.GetMethod("Add").Invoke(carInstance, new Object[]{"suka", "suk"});
                    constructedType.GetMethod("Add").Invoke(carInstance, new Object[]{"wtf", "wtf1"});
                }
            }
            catch (ArgumentException e)
            {
                return new ExecutionInfo(ExecutionInfo.ExecutionStatus.CompilationError, 242, e.Message);
            }
            catch (Exception ex)
            {
                return new ExecutionInfo(ExecutionInfo.ExecutionStatus.CompilationError, 0, $"Произошла ошибка при запуске: {ex}");
            }
            
            return new ExecutionInfo(ExecutionInfo.ExecutionStatus.Finished, 111, "");
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
}