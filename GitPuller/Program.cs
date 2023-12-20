using System;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Octokit;
using System.Threading.Tasks;

namespace GitPuller
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var configurationManager = new ConfigurationManager();
                string accessToken = configurationManager.GetAccessToken();

                var githubOps = new GithubOperation(accessToken);

                await githubOps.GetGithubAllRepositoryAndBranches();

                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}