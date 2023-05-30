namespace OnlineCompiler.Server.SandBox
{
    public abstract class SandBox : IDisposable
    {
        /// <summary>
        /// Sandboxed app memory limit (in KB)
        /// </summary>
        public virtual int MemoryLimit { get; set; } = 100*1024;

        /// <summary>
        /// Sandboxed app execution time limit in seconds
        /// </summary>
        public virtual int TimeLimit { get; set; } = 30;

        /// <summary>
        /// Limits the disk space usage for sandboxed app (in KB)
        /// </summary>
        public virtual int DiskLimit { get; set; } = 1024;

        /// <summary>
        /// Sandbox files directory
        /// </summary>
        public abstract string? SandBoxWorkingDirectory { get; }

        /// <summary>
        /// Initializes main components and starts the sandbox
        /// </summary>
        public abstract void InitializeSandBox();
        /// <summary>
        /// Asynchronously initializes main components and starts the sandbox
        /// </summary>
        public abstract Task InitializeSandBoxAsync();

        /// <summary>
        /// Executes command in sandbox
        /// </summary>
        /// <param name="command">Command</param>
        /// <returns>Exit code</returns>
        public abstract int ExecuteCommandInSandbox(string command);
        /// <summary>
        /// Asynchronously executes command in sandbox
        /// </summary>
        /// <param name="command">Command</param>
        /// <returns>Exit code</returns>
        public abstract Task<int> ExecuteCommandInSandboxAsync(string command);

        /// <summary>
        /// Output of app, executed in sandbox
        /// </summary>
        public abstract string Output { get; }

        /// <summary>
        /// Shutdowns sandbox and performs cleanup
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// Asynchronously shutdowns sandbox and performs cleanup
        /// </summary>
        public abstract Task DisposeAsync();
    }
}
