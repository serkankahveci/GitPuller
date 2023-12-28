using System;
using System.Threading.Tasks;

namespace GitPuller
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var form1 = new Form1();

                form1.ShowDialog();

                //var configurationManager = new ConfigurationManager();
                //string accessToken = configurationManager.GetAccessToken();

                //var githubOps = new GithubOperation(accessToken);

                //await githubOps.GetGithubAllRepositoryAndBranches();

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