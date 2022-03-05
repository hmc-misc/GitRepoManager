using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Newtonsoft.Json;

namespace GitRepoManager.GithubAPI
{
    public class GithubAPI
    {
        private string BaseUrl { get; } = "https://api.github.com";
        private string PathGetAllOwnerRepos { get; } = "/users/{owner}/repos";
        private string PathRepo { get; } = "/repos/{owner}/{repo}";
        private string _token { get; } = ConfigurationManager.AppSettings["gittoken"];

        public string[] PrivateResults;

        public List<GetAllUserRepos_Response> GetAllOwnerRepos_Helper(string owner, int pageCount)
        {
            var result = new List<GetAllUserRepos_Response>();
            for (int i = 1; i <= pageCount; i++)
            {
                var page = GetAllUserRepos(owner, i);
                foreach (var repo in page)
                {
                    result.Add(repo);
                }
            }

            PrivateResults = new string[result.Count];

            return result;
        }

        public List<GetAllUserRepos_Response> GetAllUserRepos(string owner, int page)
        {
            var result = new List<GetAllUserRepos_Response>();
            try
            {
                var url = String.Concat(BaseUrl, PathGetAllOwnerRepos.Replace("{owner}", owner), "?per_page=100&page=", page);
                RestClient client = new RestClient(url);
                RestRequest req = new RestRequest(url, Method.Get);

                req.AddHeader("Authentication", String.Concat("Basic ", Base64Encode(_token)));
                Task<RestResponse> respAsync = ExecuteAsyncHelper(client, req);
                RestResponse resp = respAsync.Result;
                result = JsonConvert.DeserializeObject<List<GetAllUserRepos_Response>>(resp.Content);
            }
            catch (Exception ex)
            {
                Console.WriteLine(JsonConvert.SerializeObject(ex));
            }
            return result;
        }

        public bool SetRepoPrivate(string owner, string repo, int index)
        {
            var result = false;
            try
            {
                var url = Uri.EscapeUriString(String.Concat(BaseUrl, PathRepo.Replace("{owner}", owner).Replace("{repo}", repo)));
                RestClient client = new RestClient(url);
                RestRequest req = new RestRequest(url, Method.Patch);
                req.AddBody(new PrivateBody());

                req.AddHeader("Authorization", String.Concat("token ", _token));
                Task<RestResponse> respAsync = ExecuteAsyncHelper(client, req);
                RestResponse resp = respAsync.Result;
                PrivateResults[index] = index + JsonConvert.SerializeObject(resp) + Environment.NewLine;
                result = resp.IsSuccessful;
            }
            catch (Exception ex)
            {
                Console.WriteLine(JsonConvert.SerializeObject(ex));
            }
            return result;
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        private async Task<RestResponse> ExecuteAsyncHelper(RestClient client, RestRequest req)
        {
            Task<RestResponse> respAsync = client.ExecuteAsync(req);
            RestResponse resp = await respAsync;
            return resp;
        }

        public class PrivateBody
        {
            public string visibility { get; set; } = "private";
        }

    }
}
