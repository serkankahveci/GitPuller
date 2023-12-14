using System;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Octokit;
using System.Threading.Tasks;

namespace GitPuller
{
    internal class Program
    {
        static string accessToken;

        static async Task Main(string[] args)
        {
            var configurationManager = new ConfigurationManager();
            string accessToken = configurationManager.GetAccessToken();

            var githubOps = new GithubOperation(accessToken);

            await githubOps.GetGithubAllRepositoryAndBranches();

            Console.ReadLine();
        }
    }
}