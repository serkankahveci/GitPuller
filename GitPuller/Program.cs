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
            var configurationManager = new ConfigurationManager();
            string accessToken = configurationManager.GetAccessToken();

            //var exec = new CommandExecuter();

            var githubOps = new GithubOperation(accessToken);

            githubOps.GetGithubAllRepositoryAndBranchs();
            githubOps.ExecuteGitCommands();

            Console.ReadLine();
        }
    }
}