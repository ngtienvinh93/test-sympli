using System.Net;
using System.Text.RegularExpressions;

using Sympli.Application.Common.Interfaces;
using Sympli.Application.Common.Models.SEO;

namespace Sympli.Application.Infrastructure.Services
{
    public class GoogleSearchService : ISearchService
    {
        public string SearchEngine => "Google";

        public async Task<SearchResult> Search(string keywords, string url)
        {
            string encodedKeywords = WebUtility.UrlEncode(keywords);

            string searchUrl = $"https://www.google.com/search?q={encodedKeywords}&num=100";

            using WebClient client = new WebClient();
            {
                client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

                string htmlContent = await client.DownloadStringTaskAsync(new Uri(searchUrl));

                string rsoDivContent = ExtractResultDiv(htmlContent);
                if (string.IsNullOrEmpty(rsoDivContent))
                {
                    Console.WriteLine("No match found for RSO div.");
                    return new() { SearchEngine = "Google", Result = "0" };
                }

                var aTags = GetFirstAnchorsFromEachChildDiv(rsoDivContent);
                var positions = GetMatchedUrlPositions(aTags, url);
                return new() { SearchEngine = SearchEngine, Result = positions.Count > 0 ? string.Join(", ", positions) : "0" };
            }
        }

        private string ExtractResultDiv(string htmlContent)
        {
            // Result div container has id="rso"
            string startP = @"<div[^>]*id=""rso""[^>]*>";
            string openP = @"<div[^>]*>";
            string closeP = @"<\/div>";

            string pattern = $@"{startP}((?'nested'{openP})|{closeP}(?'-nested')|[\w\W]*?)*{closeP}";
            Regex regex = new Regex(pattern, RegexOptions.Singleline);

            Match match = regex.Match(htmlContent);
            return match.Success ? match.Value : string.Empty;
        }

        private List<string> GetFirstAnchorsFromEachChildDiv(string rsoDivContent)
        {
            List<string> anchors = new List<string>();

            string anchorPattern = @"<div[^>]*>([\s\S]*?)(<a[^>]*jsname\s*=\s*""[^""]+""[^>]*href\s*=\s*""[^""]+""[^>]*>[\s\S]*?<\/a>)[\s\S]*?<\/div>";
            Regex anchorRegex = new Regex(anchorPattern, RegexOptions.Singleline);

            MatchCollection matches = anchorRegex.Matches(rsoDivContent);
            foreach (Match match in matches)
            {
                string anchor = match.Groups[2].Value;
                anchors.Add(anchor);
            }

            return anchors;
        }

        private List<int> GetMatchedUrlPositions(List<string> aTags, string url)
        {
            var positions = new List<int>();
            for (int i = 0; i < aTags.Count && i < 100; i++)
            {
                if (aTags[i].Contains(url))
                {
                    positions.Add(i + 1);
                }
            }
            return positions;
        }

    }
}
