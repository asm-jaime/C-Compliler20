using OnlineCompiler.Shared;
using Microsoft.AspNetCore.Mvc;

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
        public string Post([FromBody]string? code)
        {
            if (code == null)
                return "0";
            string guid = Guid.NewGuid().ToString();
            CodeExecutor ce = new CodeExecutor(code);
            _codeExecutors.Add(guid, ce);
            Task.Run(async () =>
            {
                await ce.PerformCodeExecutionAsync();
                await Task.Delay(5000);
                _codeExecutors.Remove(guid);
            });
            return guid;
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