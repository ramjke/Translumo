using System;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Translumo.Utils.Http;

namespace Translumo.Update
{
    public class GithubApiClient : IReleasesClient
    {
        public string Owner { get; set; }

        public string Repository { get; set; }


        private readonly HttpReader _reader;

        private const string BASE_API_URL = "https://api.github.com";

        public GithubApiClient(string owner, string repository)
        {
            this.Owner = owner;
            this.Repository = repository;
            _reader = new HttpReader();
            _reader.Accept = "application/vnd.github+json";
        }

        public async Task<Version> GetLastVersionAsync()
        {
            var releases = await GetReleasesAsync();
            if (!releases.Any())
            {
                return null;
            }

            var versionReplacer = new Regex(@"v[\. ]*");

            return releases
                .Select(release => new Version(versionReplacer.Replace(release.TagName, string.Empty)))
                .Max();
        }

        private async Task<ReleaseItemResponse[]> GetReleasesAsync()
        {
            var targetUrl = $"{BASE_API_URL}/repos/{Owner}/{Repository}/releases";
            var response = await _reader.RequestWebDataAsync(targetUrl, HttpMethods.GET);
            if (!response.IsSuccessful)
            {
                throw new InvalidOperationException($"Failed to receive Version from Github", response.InnerException);
            }

            return JsonSerializer.Deserialize<ReleaseItemResponse[]>(response.Body);
        }
    }
}
