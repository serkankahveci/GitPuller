using System;
using System.Collections.Generic;
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
            var exec = new CommandExecuter();

            //// Stash
            //Console.WriteLine("Stashing ...");
            //var gitStashCommand = "git stash save -S 'New Save'";
            //var stashResult = exec.Execute(gitStashCommand);
            //Console.WriteLine(stashResult);

            //// Clone
            //Console.WriteLine("Cloning ...");
            //var gitCloneCommand = $"git clone https://{accessToken}@github.com/serkankahveci/GitPuller.git";
            //var cloneResult = exec.Execute(gitCloneCommand);
            //Console.WriteLine(cloneResult);

            //// Pull
            //Console.WriteLine("Pulling ...");
            //var gitPullCommand = "git pull origin master";
            //var pullResult = exec.Execute(gitPullCommand);
            //Console.WriteLine(pullResult);

            //// Pop
            //Console.WriteLine("Popping ...");
            //var gitStashPop = "git stash pop stash@{0}";
            //var popResult = exec.Execute(gitStashPop);
            //Console.WriteLine(popResult);

            Console.WriteLine("TEST");
            var test = "git pull";
            var testResult = exec.Execute(test);

            Console.WriteLine(testResult);
            Console.WriteLine(branchName);
            Console.WriteLine(repoName);
            Console.WriteLine("TEST");
        }

    }
}