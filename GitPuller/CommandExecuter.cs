using System;
using System.Diagnostics;

namespace GitPuller
{
    public class CommandExecuter
    {
        public string Execute(string command, string directory)
        {
            try
            {
                var processStartInfo = new ProcessStartInfo("powershell.exe", command);

                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.RedirectStandardError = true;
                processStartInfo.UseShellExecute = false;
                processStartInfo.WorkingDirectory = directory;

                var process = new Process();
                process.StartInfo = processStartInfo;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                Console.WriteLine(output);

                if (!string.IsNullOrWhiteSpace(error))
                {
                    Console.WriteLine($"Error occurred: {error}");
                    return error;
                }

                return output;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred: {ex.Message}");
                return ex.Message;
            }
        }
    }
}
