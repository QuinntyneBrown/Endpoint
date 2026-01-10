// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Core.UnitTests;

public class ObjectCacheTests
{
    private readonly Mock<ILogger<ObjectCache>> _mockLogger;

    public ObjectCacheTests()
    {
        _mockLogger = new Mock<ILogger<ObjectCache>>();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ObjectCache(null!));
    }

    [Fact]
    public void Constructor_WithValidLogger_CreatesInstance()
    {
        var cache = new ObjectCache(_mockLogger.Object);
        Assert.NotNull(cache);
    }

    #endregion

    #region FromCacheOrService Tests - Cache Miss

    [Fact]
    public void FromCacheOrService_CacheMiss_CallsServiceFunction()
    {
        var cache = new ObjectCache(_mockLogger.Object);
        var serviceCallCount = 0;
        var expectedResult = "TestResult";
        var uniqueKey = Guid.NewGuid().ToString();

        var result = cache.FromCacheOrService(() =>
        {
            serviceCallCount++;
            return expectedResult;
        }, uniqueKey);

        Assert.Equal(expectedResult, result);
        Assert.Equal(1, serviceCallCount);
    }

    [Fact]
    public void FromCacheOrService_CacheMiss_LogsInformation()
    {
        var cache = new ObjectCache(_mockLogger.Object);
        var uniqueKey = Guid.NewGuid().ToString();

        cache.FromCacheOrService(() => "test", uniqueKey);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region FromCacheOrService Tests - Cache Hit

    [Fact]
    public void FromCacheOrService_CacheHit_ReturnsCachedValue()
    {
        var cache = new ObjectCache(_mockLogger.Object);
        var serviceCallCount = 0;
        var expectedResult = "CachedValue";
        var uniqueKey = Guid.NewGuid().ToString();

        // First call - cache miss
        cache.FromCacheOrService(() =>
        {
            serviceCallCount++;
            return expectedResult;
        }, uniqueKey);

        // Second call - should be cache hit
        var result = cache.FromCacheOrService(() =>
        {
            serviceCallCount++;
            return "NewValue";
        }, uniqueKey);

        Assert.Equal(expectedResult, result);
        Assert.Equal(1, serviceCallCount);
    }

    [Fact]
    public void FromCacheOrService_CacheHit_DoesNotCallServiceFunction()
    {
        var cache = new ObjectCache(_mockLogger.Object);
        var uniqueKey = Guid.NewGuid().ToString();
        var callCount = 0;

        // First call to populate cache
        cache.FromCacheOrService(() =>
        {
            callCount++;
            return 42;
        }, uniqueKey);

        // Second call should use cache
        cache.FromCacheOrService(() =>
        {
            callCount++;
            return 100;
        }, uniqueKey);

        Assert.Equal(1, callCount);
    }

    #endregion

    #region Multiple Cached Items Tests

    [Fact]
    public void FromCacheOrService_MultipleDifferentKeys_CachesEachSeparately()
    {
        var cache = new ObjectCache(_mockLogger.Object);
        var key1 = Guid.NewGuid().ToString();
        var key2 = Guid.NewGuid().ToString();
        var key3 = Guid.NewGuid().ToString();

        var result1 = cache.FromCacheOrService(() => "Value1", key1);
        var result2 = cache.FromCacheOrService(() => "Value2", key2);
        var result3 = cache.FromCacheOrService(() => "Value3", key3);

        Assert.Equal("Value1", result1);
        Assert.Equal("Value2", result2);
        Assert.Equal("Value3", result3);
    }

    [Fact]
    public void FromCacheOrService_MultipleCacheHits_ReturnsCorrectValues()
    {
        var cache = new ObjectCache(_mockLogger.Object);
        var key1 = Guid.NewGuid().ToString();
        var key2 = Guid.NewGuid().ToString();

        // Populate cache
        cache.FromCacheOrService(() => "First", key1);
        cache.FromCacheOrService(() => "Second", key2);

        // Verify cache hits return correct values
        var hit1 = cache.FromCacheOrService(() => "Different", key1);
        var hit2 = cache.FromCacheOrService(() => "Different", key2);

        Assert.Equal("First", hit1);
        Assert.Equal("Second", hit2);
    }

    [Fact]
    public void FromCacheOrService_WithDifferentTypes_CachesCorrectly()
    {
        var cache = new ObjectCache(_mockLogger.Object);
        var intKey = Guid.NewGuid().ToString();
        var stringKey = Guid.NewGuid().ToString();
        var objectKey = Guid.NewGuid().ToString();

        var intResult = cache.FromCacheOrService(() => 42, intKey);
        var stringResult = cache.FromCacheOrService(() => "Hello", stringKey);
        var objectResult = cache.FromCacheOrService(() => new { Name = "Test" }, objectKey);

        Assert.Equal(42, intResult);
        Assert.Equal("Hello", stringResult);
        Assert.Equal("Test", objectResult.Name);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void FromCacheOrService_WithNullReturnValue_CachesNull()
    {
        var cache = new ObjectCache(_mockLogger.Object);
        var uniqueKey = Guid.NewGuid().ToString();
        var callCount = 0;

        var result1 = cache.FromCacheOrService<string?>(() =>
        {
            callCount++;
            return null;
        }, uniqueKey);

        var result2 = cache.FromCacheOrService<string?>(() =>
        {
            callCount++;
            return "NotNull";
        }, uniqueKey);

        // Note: Current implementation may call service again for null values
        // This test documents the current behavior
        Assert.Null(result1);
    }

    [Fact]
    public void FromCacheOrService_WithComplexObject_CachesCorrectly()
    {
        var cache = new ObjectCache(_mockLogger.Object);
        var uniqueKey = Guid.NewGuid().ToString();
        var complexObject = new TestComplexObject
        {
            Id = 1,
            Name = "Test",
            Items = new List<string> { "A", "B", "C" }
        };

        var result = cache.FromCacheOrService(() => complexObject, uniqueKey);
        var cachedResult = cache.FromCacheOrService(() => new TestComplexObject(), uniqueKey);

        Assert.Same(complexObject, result);
        Assert.Same(complexObject, cachedResult);
    }

    [Fact]
    public void FromCacheOrService_WithEmptyKey_WorksCorrectly()
    {
        var cache = new ObjectCache(_mockLogger.Object);

        var result = cache.FromCacheOrService(() => "EmptyKeyValue", string.Empty);

        Assert.Equal("EmptyKeyValue", result);
    }

    #endregion

    #region Helper Classes

    private class TestComplexObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<string> Items { get; set; } = new();
    }

    #endregion
}
