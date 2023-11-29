using System;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Octokit;

namespace GitPuller
{
    internal class Program
    {
        static string accessToken;

        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(@"C:\Users\XYZ\source\repos\GitPuller\GitPuller")
                .AddJsonFile("appsettings.json")
                .Build();

            accessToken = config.GetSection("Git")["AccessToken"];

            if (string.IsNullOrEmpty(accessToken))
            {
                Console.Write("Enter Git Access Token: ");
                accessToken = Console.ReadLine();
            }

            var exec = new CommandExecuter();

            var github = new GitHubClient(new ProductHeaderValue("GitPuller"));
            github.Credentials = new Credentials(accessToken);

            var repositories = github.Repository.GetAllForCurrent().Result;

            foreach (var repository in repositories)
            {
                Console.WriteLine("Repository Name: " + repository.Name);
                Console.WriteLine("-------");

                var branches = github.Repository.Branch.GetAll(repository.Id).Result;

                foreach (var branch in branches)
                {
                    Console.WriteLine($"{branch.Name}");

                    Console.WriteLine("TEST");
                    var test = "git sdasdas";
                    var testResult = exec.Execute(test);
                    Console.WriteLine(testResult);
                    Console.WriteLine("TEST");

                    Thread.Sleep(4000);
                }

                Console.WriteLine("-------");
            }
                Console.ReadLine();
        }
    }
}
