using System.Net;

using FluentValidation;

using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

using Sympli.Application.Common;
using Sympli.Application.Common.Interfaces;

namespace Sympli.Application.Features.SEO;

public class SearchController : ApiControllerBase
{
    [HttpGet("/api/seo-result")]
    public async Task<ActionResult<string>> GetSeoResult(CancellationToken cancellationToken, [FromQuery] string keywords = "e-settlements", [FromQuery] string url = "https://www.sympli.com.au")
    {
        return await Mediator.Send(new GetSeoResultQuery { Keywords = keywords, Url = url }, cancellationToken);
    }
}

public class GetSeoResultQuery : IRequest<string>
{
    public string? Keywords { get; set; }

    public string? Url { get; set; }
}

public class GetSeoResultQueryValidator : AbstractValidator<GetSeoResultQuery>
{
    public GetSeoResultQueryValidator()
    {
        RuleFor(v => v.Keywords)
            .MaximumLength(200)
            .NotEmpty();

        RuleFor(v => v.Url)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            .WithErrorCode(HttpStatusCode.BadRequest.ToString())
            .WithMessage("Url is not valid")
            .NotEmpty();
    }
}

public sealed class GetSeoResultQueryHandler : IRequestHandler<GetSeoResultQuery, string>
{
    private readonly IEnumerable<ISearchService> _searchServices;
    private readonly IMemoryCache _memoryCache;

    public GetSeoResultQueryHandler(IEnumerable<ISearchService> searchServices,
        IMemoryCache memoryCache)
    {
        _searchServices = searchServices;
        _memoryCache = memoryCache;
    }

    public async Task<string> Handle(GetSeoResultQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"{typeof(GetSeoResultQueryHandler).FullName}+{nameof(Handle)}+keywords:{request.Keywords}";

        if (!_memoryCache.TryGetValue(cacheKey, out string cachedValue))
        {
            var results = _searchServices.Select(s => s.Search(request.Keywords, request.Url));

            await Task.WhenAll(results);

            var resultString = results.Select(s => string.Format("From {0} : {1}", s.Result.SearchEngine, s.Result.Result));

            cachedValue = $"SEO result for keyword: {request.Keywords} in the top 100. {string.Join(".", resultString)}";

            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            };

            _memoryCache.Set(cacheKey, cachedValue, cacheEntryOptions);
        }

        return cachedValue;
    }
}



