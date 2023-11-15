using System;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace GitPuller
{
    internal class Program
    {
        static string accessToken;

        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                   .SetBasePath(@"C:\Users\XYZ\source\repos\GitPuller\GitPuller")
                   .AddJsonFile("appsettings.json").Build();

            accessToken = config.GetSection("Git")["AccessToken"];

            if (string.IsNullOrEmpty(accessToken))
            {
                Console.Write("Enter Git Access Token: ");
                accessToken = Console.ReadLine();
            }

            var exec = new CommandExecuter();

            Console.WriteLine("Stashing ...");
            var gitStashCommand = "git stash save -S 'New Save'";
            var stashResult = exec.Execute(gitStashCommand);
            Console.WriteLine(stashResult);

            Console.WriteLine("TEST");
            var test = "git sdasdas";
            var testResult = exec.Execute(test);
            Console.WriteLine(testResult);
            Console.WriteLine("TEST");

            Console.WriteLine("Cloning ...");
            var gitCloneCommand = $"git clone https://{accessToken}@github.com/serkankahveci/GitPuller.git";
            var cloneResult = exec.Execute(gitCloneCommand);
            Console.WriteLine(cloneResult);

            Console.WriteLine("Git clone completed.");

            Console.WriteLine("Pulling ...");
            var gitPullCommand = "git pull origin master";
            var pullResult = exec.Execute(gitPullCommand);
            Console.WriteLine(pullResult);

            Console.WriteLine("Popping ...");
            var gitStashPop = "git stash pop stash@{0}";
            var popResult = exec.Execute(gitStashPop);
            Console.WriteLine(popResult);

            Console.WriteLine("...");
            var popList = "git stash list";
            var popListResult = exec.Execute(popList);
            Console.WriteLine(popListResult);

            Console.ReadLine();
            //maNga nasıl birinci olamadı yaw
        }
    }
}
