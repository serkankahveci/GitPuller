using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitPuller
{
    public class CommandExecuter
    {
        public string Execute(string command)
        {           
            var processStartInfo = new ProcessStartInfo("powershell.exe", command);

            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.UseShellExecute = false;

            var process = new Process();
            process.StartInfo = processStartInfo;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();

            Console.WriteLine(output);

            return output;
        }
    }
}
