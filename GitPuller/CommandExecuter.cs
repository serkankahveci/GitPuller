using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

namespace GitPuller
{
    public class CommandExecuter
    {
        private readonly int _timeoutMs = 60000; // 60 seconds timeout
        
        public string Execute(string command, string directory)
        {
            return ExecuteAsync(command, directory).GetAwaiter().GetResult();
        }

        public async Task<string> ExecuteAsync(string command, string directory)
        {
            try
            {
                // Performance optimization: Use git directly instead of PowerShell
                var processStartInfo = CreateProcessStartInfo(command, directory);

                using (var process = new Process())
                {
                    process.StartInfo = processStartInfo;
                    
                    var outputBuilder = new StringBuilder();
                    var errorBuilder = new StringBuilder();

                    // Async output reading for better performance
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            outputBuilder.AppendLine(e.Data);
                        }
                    };

                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            errorBuilder.AppendLine(e.Data);
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // Wait for completion with timeout
                    var completed = await Task.Run(() => process.WaitForExit(_timeoutMs));
                    
                    if (!completed)
                    {
                        process.Kill();
                        return $"Command timed out after {_timeoutMs}ms: {command}";
                    }

                    var output = outputBuilder.ToString().Trim();
                    var error = errorBuilder.ToString().Trim();

                    if (process.ExitCode != 0 && !string.IsNullOrWhiteSpace(error))
                    {
                        Console.WriteLine($"Command failed with exit code {process.ExitCode}: {error}");
                        return error;
                    }

                    return output;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception executing command '{command}': {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

        private ProcessStartInfo CreateProcessStartInfo(string command, string directory)
        {
            // Performance optimization: Use git directly instead of PowerShell for git commands
            if (command.StartsWith("git ", StringComparison.OrdinalIgnoreCase))
            {
                var gitArgs = command.Substring(4); // Remove "git " prefix
                return new ProcessStartInfo("git", gitArgs)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = directory,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
            }
            else
            {
                // Fallback to PowerShell for non-git commands (if any)
                return new ProcessStartInfo("powershell.exe", $"-Command \"{command}\"")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = directory,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
            }
        }

        // Batch execution for improved performance when running multiple commands
        public async Task<string[]> ExecuteBatchAsync(string[] commands, string directory)
        {
            var tasks = commands.Select(command => ExecuteAsync(command, directory));
            return await Task.WhenAll(tasks);
        }

        // Optimized git status check
        public async Task<bool> HasUncommittedChanges(string directory)
        {
            var result = await ExecuteAsync("git status --porcelain", directory);
            return !string.IsNullOrWhiteSpace(result) && !result.StartsWith("Error:");
        }

        // Optimized git branch check
        public async Task<string> GetCurrentBranch(string directory)
        {
            var result = await ExecuteAsync("git branch --show-current", directory);
            return result?.Trim();
        }

        // Optimized git operations with better error handling
        public async Task<GitOperationResult> PullRepository(string directory, string branchName)
        {
            try
            {
                var currentBranch = await GetCurrentBranch(directory);
                var hasChanges = await HasUncommittedChanges(directory);
                
                var result = new GitOperationResult
                {
                    OriginalBranch = currentBranch,
                    HasUncommittedChanges = hasChanges,
                    Success = true
                };

                // Stash changes if needed
                if (hasChanges)
                {
                    result.StashOutput = await ExecuteAsync("git stash push -m 'Auto-stash before pull'", directory);
                    if (result.StashOutput.StartsWith("Error:"))
                    {
                        result.Success = false;
                        result.ErrorMessage = result.StashOutput;
                        return result;
                    }
                }

                // Pull the repository
                result.PullOutput = await ExecuteAsync($"git pull origin {branchName}", directory);
                if (result.PullOutput.StartsWith("Error:"))
                {
                    result.Success = false;
                    result.ErrorMessage = result.PullOutput;
                }

                // Restore stashed changes if needed
                if (hasChanges && result.Success)
                {
                    result.StashPopOutput = await ExecuteAsync("git stash pop", directory);
                    if (result.StashPopOutput.StartsWith("Error:"))
                    {
                        result.Success = false;
                        result.ErrorMessage = result.StashPopOutput;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }

    // Helper class for structured git operation results
    public class GitOperationResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string OriginalBranch { get; set; }
        public bool HasUncommittedChanges { get; set; }
        public string StashOutput { get; set; }
        public string PullOutput { get; set; }
        public string StashPopOutput { get; set; }
        
        public override string ToString()
        {
            if (!Success)
                return $"Failed: {ErrorMessage}";
                
            var result = new StringBuilder();
            result.AppendLine($"Pull completed successfully");
            if (HasUncommittedChanges)
                result.AppendLine("Changes were stashed and restored");
            if (!string.IsNullOrEmpty(PullOutput))
                result.AppendLine($"Pull output: {PullOutput}");
            return result.ToString();
        }
    }
}
