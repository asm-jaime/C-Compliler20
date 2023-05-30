using System.Diagnostics;

namespace OnlineCompiler.Server.SandBox
{
    /// <summary>
    /// Sandbox based on isolate https://github.com/ioi/isolate
    /// </summary>
    public class IsolateSandbox : SandBox
    {
        /// <summary>
        /// Isolate only lets run 1000 sandboxes concurrently
        /// </summary>
        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1000, 1000);

        /// <summary>
        /// Array of free box-ids of Isolate Sandboxes 0..999<br />
        /// If id is free, IsSandBoxIDFree[id] = true
        /// </summary>
        private static readonly bool[] IsSandBoxIDFree = Enumerable.Range(0,1000).Select(x => true).ToArray();
        /// <summary>
        /// Lock object for acquiring SandBoxID from IsSandBoxIDFree array
        /// </summary>
        private static readonly object SandboxIDLock = new object();

        private int SandboxID = -1;
        /// <summary>
        /// Process of executing command in the sandbox
        /// </summary>
        private Process? MainProcess;
        /// <summary>
        /// All output on MainProcesses
        /// </summary>
        private string _Output = string.Empty;

        public override void InitializeSandBox()
        {
            if (SandboxID != -1)
                throw new Exception("Already initialized");
            // Acquiring SandboxID
            Semaphore.Wait();
            lock (SandboxIDLock)
            {
                SandboxID = Array.FindIndex(IsSandBoxIDFree, x => x);
                if (SandboxID == -1)
                    throw new Exception("Failed to aquire SandboxID");
                IsSandBoxIDFree[SandboxID] = false;
            }
            // Initializing Isolate box with SandboxID
            ProcessStartInfo processStartInfo = new ProcessStartInfo("isolate", "--cg --box-id=" + SandboxID + " --init")
            {
                RedirectStandardOutput = true
            };
            using Process initializationProcess = Process.Start(processStartInfo) ?? throw new Exception("Failed to start sandbox initialization process");
            initializationProcess.WaitForExit();
            if (initializationProcess.ExitCode != 0)
                throw new Exception("Sandbox initialization process finished with code: " + initializationProcess.ExitCode);
            // Setting working directory from the output
            _SandBoxWorkingDirectory = initializationProcess.StandardOutput.ReadToEnd().ReplaceLineEndings().Replace(Environment.NewLine, "");
        }

        public override async Task InitializeSandBoxAsync()
        {
            if (SandboxID != -1)
                throw new Exception("Already initialized");
            // Acquiring SandboxID
            await Semaphore.WaitAsync();
            lock (SandboxIDLock)
            {
                SandboxID = Array.FindIndex(IsSandBoxIDFree, x => x);
                if (SandboxID == -1)
                    throw new Exception("Failed to aquire SandboxID");
                IsSandBoxIDFree[SandboxID] = false;
            }
            // Initializing Isolate box with SandboxID
            ProcessStartInfo processStartInfo = new ProcessStartInfo("isolate", "--cg --box-id=" + SandboxID + " --init")
            {
                RedirectStandardOutput = true
            };
            using Process initializationProcess = Process.Start(processStartInfo) ?? throw new Exception("Failed to start sandbox initialization process");
            await initializationProcess.WaitForExitAsync();
            if (initializationProcess.ExitCode != 0)
                throw new Exception("Sandbox initialization process finished with code: " + initializationProcess.ExitCode);
            // Setting working directory from the output
            _SandBoxWorkingDirectory = initializationProcess.StandardOutput.ReadToEnd().ReplaceLineEndings().Replace(Environment.NewLine, "");
        }
        public override string Output
        {
            get
            {
                _Output += MainProcess?.StandardOutput.ReadToEnd() ?? string.Empty;
                return _Output;
            }
        }

        private string? _SandBoxWorkingDirectory;
        public override string? SandBoxWorkingDirectory => _SandBoxWorkingDirectory;

        public override int ExecuteCommandInSandbox(string command)
        {
            if (SandboxID == -1)
                throw new Exception("Not initialized");
            // Two or more commands can't be executed concurrently in one sandbox
            if (!MainProcess?.HasExited ?? false)
                throw new Exception("Command is already executing in the sandbox");
            // If this isn't the first command executed in the sandbox, performing MainProcess disposal
            if (MainProcess != null)
            {
                _Output += MainProcess.StandardOutput.ReadToEnd();
                MainProcess.Dispose();
                MainProcess = null;
            }
            ProcessStartInfo processStartInfo = new ProcessStartInfo("isolate", "--cg --processes=1024 --box-id=" + SandboxID + " --run " +
                "--cg-mem=" + MemoryLimit + " --time=" + TimeLimit + " --fsize=" + DiskLimit +
                " -- " + command)
            {
                RedirectStandardOutput = true
            };
            MainProcess = Process.Start(processStartInfo) ?? throw new Exception("Failed to start sandbox process for a command");
            MainProcess.WaitForExit();
            return MainProcess.ExitCode;
        }

        public override async Task<int> ExecuteCommandInSandboxAsync(string command)
        {
            if (SandboxID == -1)
                throw new Exception("Not initialized");
            // Two or more commands can't be executed concurrently in one sandbox
            if (!MainProcess?.HasExited ?? false)
                throw new Exception("Command is already executing in the sandbox");
            // If this isn't the first command executed in the sandbox, performing MainProcess disposal
            if (MainProcess != null)
            {
                _Output += MainProcess.StandardOutput.ReadToEnd();
                MainProcess.Dispose();
                MainProcess = null;
            }
            ProcessStartInfo processStartInfo = new ProcessStartInfo("isolate", "--cg --processes=1024 --box-id=" + SandboxID + " --run " +
                "--cg-mem=" + MemoryLimit + " --time=" + TimeLimit + " --fsize=" + DiskLimit +
                " -- " + command)
            {
                RedirectStandardOutput = true
            };
            MainProcess = Process.Start(processStartInfo) ?? throw new Exception("Failed to start sandbox process for a command");
            await MainProcess.WaitForExitAsync();
            return MainProcess.ExitCode;
        }

        public override void Dispose()
        {
            // Command execution process stopping and disposal
            if (MainProcess != null)
            {
                _Output += MainProcess.StandardOutput.ReadToEnd();
                if (!MainProcess.HasExited)
                    MainProcess.Kill();
                MainProcess.Dispose();
                MainProcess = null;
            }
            // Performing Isolate Sandbox cleanup and freeing SandboxID
            if (SandboxID != -1)
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo("isolate", "--cg --box-id=" + SandboxID + " --cleanup");
                processStartInfo.UseShellExecute = true;
                using Process disposalProcess = Process.Start(processStartInfo) ?? throw new Exception("Failed to start sandbox cleanup process");
                disposalProcess.WaitForExit();
                if (disposalProcess.ExitCode != 0)
                    throw new Exception("Sandbox cleanup process finished with code: " + disposalProcess.ExitCode);
                IsSandBoxIDFree[SandboxID] = true;
                SandboxID = -1;
                Semaphore.Release();
            }
        }

        public override async Task DisposeAsync()
        {
            // Command execution process stopping and disposal
            if (MainProcess != null)
            {
                _Output += MainProcess.StandardOutput.ReadToEnd();
                if (!MainProcess.HasExited)
                    MainProcess.Kill();
                MainProcess.Dispose();
                MainProcess = null;
            }
            // Performing Isolate Sandbox cleanup and freeing SandboxID
            if (SandboxID != -1)
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo("isolate", "--cg --box-id=" + SandboxID + " --cleanup");
                processStartInfo.UseShellExecute = true;
                using Process disposalProcess = Process.Start(processStartInfo) ?? throw new Exception("Failed to start sandbox cleanup process");
                await disposalProcess.WaitForExitAsync();
                if (disposalProcess.ExitCode != 0)
                    throw new Exception("Sandbox cleanup process finished with code: " + disposalProcess.ExitCode);
                IsSandBoxIDFree[SandboxID] = true;
                SandboxID = -1;
                Semaphore.Release();
            }
        }
    }
}
