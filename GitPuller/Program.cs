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

            Console.WriteLine("Stashing changes. Please wait...");
            var gitStashCommand = "git stash";
            var stashResult = exec.Execute(gitStashCommand);

            Console.WriteLine(stashResult);
            Console.WriteLine("Git stash operation completed.");


            Console.WriteLine("Cloning the repository. Please wait...");
            var gitCloneCommand = $"git clone https://{accessToken}@github.com/serkankahveci/GitPuller.git";
            var cloneResult = exec.Execute(gitCloneCommand);

            Console.WriteLine(cloneResult);

            Console.WriteLine("Git clone operation completed.");


            Console.WriteLine("Pulling the latest changes from the repository. Please wait...");
            var gitPullCommand = "git pull origin master";
            var pullResult = exec.Execute(gitPullCommand);

            Console.WriteLine(pullResult);
            Console.WriteLine("Git pull operation completed.");


            Console.ReadLine();

        }
    }
}
