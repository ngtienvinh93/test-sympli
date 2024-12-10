using Microsoft.Extensions.Caching.Memory;

using Moq;

using Sympli.Application.Common.Interfaces;
using Sympli.Application.Common.Models.SEO;
using Sympli.Application.Features.SEO;

namespace Sympli.Application.UnitTests.Feature.SEO
{
    public class SearchTests
    {
        private readonly Mock<IMemoryCache> _memoryCacheMock;
        private readonly Mock<ISearchService> _searchServiceMock;
        private readonly GetSeoResultQueryHandler _handler;

        public SearchTests()
        {
            _memoryCacheMock = new Mock<IMemoryCache>();
            _searchServiceMock = new Mock<ISearchService>();

            // Create a list of mock search services
            var searchServices = new List<ISearchService> { _searchServiceMock.Object };

            // Instantiate the handler
            _handler = new GetSeoResultQueryHandler(searchServices, _memoryCacheMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsCorrectResult_WhenCacheIsEmpty()
        {
            // Arrange
            var keywords = "e-settlements";
            var url = "https://www.sympli.com.au";

            var query = new GetSeoResultQuery { Keywords = keywords, Url = url };

            // Mock search service behavior
            _searchServiceMock
                .Setup(s => s.Search(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SearchResult()
                {
                    SearchEngine = "MockSearchEngine",
                    Result = "1, 10, 33"
                });

            // Mock IMemoryCache (cache miss scenario)
            object cacheValue = null;
            _memoryCacheMock
                .Setup(m => m.TryGetValue(It.IsAny<object>(), out cacheValue))
                .Returns(false);

            // Mock setting cache value
            _memoryCacheMock
                .Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(Mock.Of<ICacheEntry>);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Contains("MockSearchEngine", result);
            Assert.Contains("1, 10, 33", result);
        }

        [Fact]
        public async Task Handle_CombinesResults_FromMultipleSearchServices()
        {
            // Arrange
            var keywords = "e-settlements";
            var url = "https://www.sympli.com.au";

            var query = new GetSeoResultQuery { Keywords = keywords, Url = url };

            // Mock two different search services
            var searchServiceMock1 = new Mock<ISearchService>();
            searchServiceMock1
                .Setup(s => s.Search(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SearchResult
                {
                    SearchEngine = "SearchEngine1",
                    Result = "1, 5, 9"
                });

            var searchServiceMock2 = new Mock<ISearchService>();
            searchServiceMock2
                .Setup(s => s.Search(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SearchResult
                {
                    SearchEngine = "SearchEngine2",
                    Result = "2, 7"
                });

            var searchServices = new List<ISearchService>
    {
        searchServiceMock1.Object,
        searchServiceMock2.Object
    };

            var handler = new GetSeoResultQueryHandler(searchServices, _memoryCacheMock.Object);

            // Mock cache miss scenario
            object cacheValue = null;
            _memoryCacheMock
                .Setup(m => m.TryGetValue(It.IsAny<object>(), out cacheValue))
                .Returns(false);

            // Mock setting cache value
            _memoryCacheMock
                .Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(Mock.Of<ICacheEntry>);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Contains("SearchEngine1 : 1, 5, 9", result);
            Assert.Contains("SearchEngine2 : 2, 7", result);
        }
    }
}
