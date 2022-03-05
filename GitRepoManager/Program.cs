using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitRepoManager.GithubAPI;
using Newtonsoft.Json;

namespace GitRepoManager
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var owner = ConfigurationManager.AppSettings["gitowner"]; //git username
            var pageCount = Int32.Parse(ConfigurationManager.AppSettings["gitpagecount"]); //100 repos per page
            var path = ConfigurationManager.AppSettings["logpath"]; //person comp file path to store results

            var gitAPI = new GithubAPI.GithubAPI();
            List<GetAllUserRepos_Response> repos = gitAPI.GetAllOwnerRepos_Helper(owner, pageCount);

            var count = 0;
            foreach(GetAllUserRepos_Response repo in repos)
            {
                Console.WriteLine(repo.name);
                gitAPI.SetRepoPrivate(owner, repo.name, count);
                count++;
            }

            try
            {
                File.WriteAllLines(path + "git_run_log_" + DateTime.Now.ToString("mm_dd_ss") + ".txt", gitAPI.PrivateResults);
            }
            catch(Exception ex)
            {
                Console.WriteLine(JsonConvert.SerializeObject(ex));
            }
        }
    }
}
