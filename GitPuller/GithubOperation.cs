using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Octokit;

namespace GitPuller
{

    public class GithubOperation
    {
        private GitHubClient github;
        private string branchName;
        private string repoName;

        public GithubOperation(string accessToken)
        {
            github = new GitHubClient(new ProductHeaderValue("GitPuller"));
            github.Credentials = new Credentials(accessToken);
        }

        public void GetGithubAllRepositoryAndBranchs()
        {
            var repositories = github.Repository.GetAllForCurrent().Result;

            foreach (var repository in repositories)
            {
                Console.Write(repository.Name);
                Console.WriteLine("\n");
                repoName = repository.Name;
                var branchList = GetAllBranchsFromCurrentRepository(repository);
                foreach (var branch in branchList)
                {
                    Console.WriteLine(branch.Name);
                    Console.WriteLine("\n");
                    branchName = branch.Name;
                    ExecuteGitCommands();
                }
                Console.WriteLine("--------");
            }
        }

        public IReadOnlyList<Branch> GetAllBranchsFromCurrentRepository(Repository repository)
        {
            return github.Repository.Branch.GetAll(repository.Id).Result;
        }

        public void ExecuteGitCommands()
        {
            if (!CheckRepo(repoName))
            {
                Console.WriteLine("Local repository not found. Skipping ExecuteGitCommands.");
                return;
            }

            var exec = new CommandExecuter();

            // Clone
            Console.WriteLine($"Cloning {repoName}...");
            var gitCloneCommand = $"git clone https://github.com/{repoName}.git";
            var cloneResult = exec.Execute(gitCloneCommand);
            Console.WriteLine(cloneResult);

            //// Pull
            //Console.WriteLine($"Pulling {branchName} from {repoName}...");
            //var gitPullCommand = $"git pull origin {branchName}";
            //var pullResult = exec.Execute(gitPullCommand);
            //Console.WriteLine(pullResult);

            // You can add more Git commands as needed based on your requirements.

            Console.WriteLine("TEST");
            var test = $"git status";
            var testResult = exec.Execute(test);
            Console.WriteLine(testResult);

            Console.WriteLine(branchName);
            Console.WriteLine(repoName);
            Console.WriteLine("TEST");
        }

        public static bool CheckRepo(string repoName) 
        {
            var localRepoPath = $"~\\{repoName}";

            return Directory.Exists(localRepoPath);
        }
    }
}