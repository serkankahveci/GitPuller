using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Octokit;

namespace GitPuller
{

    public class GithubOperation
    {
        private GitHubClient github;

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
                var branchList = GetAllBranchsFromCurrentRepository(repository);
                foreach (var branch in branchList)
                {
                    Console.WriteLine(branch.Name);
                    Console.WriteLine("\n");
                }
                Console.WriteLine("--------");
            }

            Console.ReadLine();
        }

        public IReadOnlyList<Branch> GetAllBranchsFromCurrentRepository(Repository repository)
        {
            return github.Repository.Branch.GetAll(repository.Id).Result;
        }


    }
}