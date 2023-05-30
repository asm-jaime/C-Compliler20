using System.Diagnostics;

namespace OnlineCompiler.Server.SandBox
{
    /// <summary>
    /// Unprivileged sandbox based on Bubblewrap https://github.com/containers/bubblewrap
    /// </summary>
    public class BubblewrapSandbox : SandBox
    {
        /// <summary>
        /// Unique relative working directory name
        /// </summary>
        private string? _SandBoxWorkingDirectory;

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
            if (_SandBoxWorkingDirectory != null)
                throw new Exception("Already initialized");
            // Generating WorkingDirectoryName
            _SandBoxWorkingDirectory = Guid.NewGuid().ToString();
            // Creating WorkingDirectory
#pragma warning disable CS8604 // Possible null reference argument.
            Directory.CreateDirectory(SandBoxWorkingDirectory);
#pragma warning restore CS8604 // Possible null reference argument.
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task InitializeSandBoxAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            // Nothing to do asynchronously
            InitializeSandBox();
        }
        public override string Output
        {
            get
            {
                _Output += MainProcess?.StandardOutput.ReadToEnd() ?? string.Empty;
                return _Output;
            }
        }

        /// <summary>
        /// Full unique working directory name
        /// </summary>
        public override string? SandBoxWorkingDirectory => _SandBoxWorkingDirectory != null ? Path.Combine("/usr/", _SandBoxWorkingDirectory) : null;

        public override int ExecuteCommandInSandbox(string command)
        {
            if (_SandBoxWorkingDirectory == null)
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
            ProcessStartInfo processStartInfo = new ProcessStartInfo("bwrap",
                "--ro-bind /usr /usr --dir /tmp --dir /var --proc /proc --dev /dev --symlink usr/lib /lib --symlink usr/lib64 /lib64 " +
                "--symlink usr/bin /bin --symlink usr/sbin /sbin --chdir / --unshare-all --die-with-parent /bin/sh " + command)
            {
                RedirectStandardOutput = true
            };
            MainProcess = Process.Start(processStartInfo) ?? throw new Exception("Failed to start sandbox process for a command");
            MainProcess.WaitForExit();
            return MainProcess.ExitCode;
        }

        public override async Task<int> ExecuteCommandInSandboxAsync(string command)
        {
            if (_SandBoxWorkingDirectory == null)
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
            ProcessStartInfo processStartInfo = new ProcessStartInfo("bwrap",
                "--ro-bind /usr /usr --dir /tmp --dir /var --proc /proc --dev /dev --symlink usr/lib /lib --symlink usr/lib64 /lib64 " +
                "--symlink usr/bin /bin --symlink usr/sbin /sbin --chdir / --unshare-all --die-with-parent /bin/sh " + command)
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
            // Performing Bubblewrap Sandbox cleanup
            if (SandBoxWorkingDirectory != null)
            {
                Directory.Delete(SandBoxWorkingDirectory, true);

            }
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task DisposeAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            // Nothing to do asynchronously
            Dispose();
        }
    }
}
