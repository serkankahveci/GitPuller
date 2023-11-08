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
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            accessToken = configuration.GetSection("Git")["AccessToken"];

            if (string.IsNullOrEmpty(accessToken))
            {
                Console.Write("Enter Git Access Token: ");
                accessToken = Console.ReadLine();
            }

            var exec = new CommandExecuter();

            // Example usage of the provided access token
            var gitCloneCommand = $"git clone https://{accessToken}@github.com/your/repository.git";
            var result = exec.Execute(gitCloneCommand);

            Console.WriteLine(result);

            // Now you can use 'accessToken' for other Git commands

            Console.ReadLine();
        }
    }
}
