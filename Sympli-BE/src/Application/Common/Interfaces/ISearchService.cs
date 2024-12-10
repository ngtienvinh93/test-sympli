using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sympli.Application.Common.Models.SEO;

namespace Sympli.Application.Common.Interfaces
{
    public interface ISearchService
    {
        string SearchEngine { get; }

        Task<SearchResult> Search(string keywords, string url);
    }
}
