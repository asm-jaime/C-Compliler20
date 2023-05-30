using OnlineCompiler.Server.SandBox;
using OnlineCompiler.Shared;
using System.Diagnostics;

namespace OnlineCompiler.Server
{
    /// <summary>
    /// Class for performing code execution, including all the work with creating/deleting the sandbox and sending back ExecutionInfo
    /// </summary>
    public class CodeExecutor
    {
        private long _CompilerTime;

        /// <summary>
        /// Code to be compiled and executed
        /// </summary>
        private readonly string Code = string.Empty;
        /// <summary>
        /// Sandbox, used to execute code
        /// </summary>
        private SandBox.SandBox? SandBox;
        /// <summary>
        /// Status of the execution
        /// </summary>
        private ExecutionInfo.ExecutionStatus Status = ExecutionInfo.ExecutionStatus.Preparing;
        /// <summary>
        /// Override of output of sandbox if necessary
        /// </summary>
        private string? _OutputOverride = null;
        public CodeExecutor(string code)
        {
            Code = code;
        }

        public async Task PerformCodeExecutionAsync()
        {
            using (Environment.UserName == "root" ? SandBox = new IsolateSandbox() : SandBox = new BubblewrapSandbox())
            {
                try
                {
                    await SandBox.InitializeSandBoxAsync();
                    if (SandBox.SandBoxWorkingDirectory == null)
                        throw new ArgumentNullException("SandBox.SandBoxWorkingDirectory", "SandBoxWorkingDirectory null after initialization");
                    Status = ExecutionInfo.ExecutionStatus.Compiling;
                    await ExecuteCommandOutsideOfSandboxAsync("/usr/bin/dotnet", " new console -o \"" + Path.Combine(SandBox.SandBoxWorkingDirectory, "box/") + "\"");
                    File.WriteAllText(Path.Combine(SandBox.SandBoxWorkingDirectory, "box", "Program.cs"), Code);
                    (int,string) compilationResult = await ExecuteCommandOutsideOfSandboxAsync("/usr/bin/dotnet", "build " + Path.Combine(SandBox.SandBoxWorkingDirectory, "box"));
                    // ExitCode != 0
                    if (compilationResult.Item1 != 0)
                    {
                        // Overriding sandbox output with Output of the compiler
                        _OutputOverride = compilationResult.Item2;
                        Status = ExecutionInfo.ExecutionStatus.CompilationError;
                        return;
                    }

                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    Status = ExecutionInfo.ExecutionStatus.Running;
                    await SandBox.ExecuteCommandInSandboxAsync("/box/bin/Debug/net6.0/box");
                    Status = ExecutionInfo.ExecutionStatus.Finished;
                    stopwatch.Stop();
                    _CompilerTime = stopwatch.ElapsedMilliseconds;
                    Console.WriteLine(_CompilerTime);
                } catch (Exception e)
                {
                    Console.WriteLine("Code execution exception:" + e.Message + Environment.NewLine + e.StackTrace);
                }
            }
        }

        /// <summary>
        /// Asynchronously executes command on server outside of sandbox
        /// </summary>
        /// <param name="path">Path to app</param>
        /// <param name="args">App arguments</param>
        /// <returns>ExitCode and Output</returns>
        /// <exception cref="Exception">Throws exception if starting the process failed for any reason</exception>
        private static async Task<(int, string)> ExecuteCommandOutsideOfSandboxAsync(string path, string args)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo(path, args)
            {
                RedirectStandardOutput = true
            };
            using Process process = Process.Start(processStartInfo) ?? throw new Exception("Failed to execute command outside of sandbox");
            await process.WaitForExitAsync();
            return (process.ExitCode, process.StandardOutput.ReadToEnd());
        }

        public ExecutionInfo? ExecutionInfo
        {
            get
            {
                if (SandBox == null)
                    return null;
                return new ExecutionInfo(Status, _CompilerTime, _OutputOverride ?? SandBox.Output);
            }
        }
    }
}
