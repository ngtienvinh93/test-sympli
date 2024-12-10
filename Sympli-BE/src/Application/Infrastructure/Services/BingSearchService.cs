using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Sympli.Application.Common.Interfaces;
using Sympli.Application.Common.Models.SEO;

namespace Sympli.Application.Infrastructure.Services
{
    public class BingSearchService : ISearchService
    {
        public string SearchEngine => "Bing";

        public async Task<SearchResult> Search(string keywords, string url)
        {
            string encodedKeywords = WebUtility.UrlEncode(keywords);

            // Base URL for Bing search
            string searchUrl = $"https://www.bing.com/search?q={encodedKeywords}&count=100";

            // Download the HTML of the Bing search result page
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

                string htmlContent = client.DownloadString(searchUrl);

                // Find all result links in the HTML content
                var matches = Regex.Matches(htmlContent, @"<a href=\""(https?:\/\/[^\s\""]+)\""");

                // List to store the positions where the URL is found
                List<int> positions = new List<int>();

                // Loop through the matches to find the target URL
                int position = 0;
                foreach (Match match in matches)
                {
                    string resultUrl = match.Groups[1].Value;

                    // Only check actual search result URLs (filter out ads, navigation links, etc.)
                    if (resultUrl.StartsWith("https://") || resultUrl.StartsWith("http://"))
                    {
                        position++;
                        if (resultUrl.Contains(url, StringComparison.OrdinalIgnoreCase))
                        {
                            positions.Add(position);
                        }

                        if (position >= 100) break;
                    }
                }

                return new() { SearchEngine = SearchEngine, Result = positions.Count > 0 ? string.Join(", ", positions) : "0" };
            }                
        }
    }
}


