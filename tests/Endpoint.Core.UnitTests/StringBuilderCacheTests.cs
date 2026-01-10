// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;

namespace Endpoint.Core.UnitTests;

public class StringBuilderCacheTests
{
    #region Acquire Tests

    [Fact]
    public void Acquire_DefaultCapacity_ReturnsStringBuilder()
    {
        var sb = StringBuilderCache.Acquire();
        Assert.NotNull(sb);
        Assert.IsType<StringBuilder>(sb);
        StringBuilderCache.Release(sb);
    }

    [Fact]
    public void Acquire_WithSmallCapacity_ReturnsStringBuilder()
    {
        var sb = StringBuilderCache.Acquire(32);
        Assert.NotNull(sb);
        Assert.True(sb.Capacity >= 32);
        StringBuilderCache.Release(sb);
    }

    [Fact]
    public void Acquire_WithLargeCapacity_ReturnsNewStringBuilder()
    {
        // Capacity larger than MaxBuilderSize (360) should return a new StringBuilder
        var sb = StringBuilderCache.Acquire(500);
        Assert.NotNull(sb);
        Assert.True(sb.Capacity >= 500);
    }

    [Fact]
    public void Acquire_MultipleTimes_ReturnsDistinctInstances()
    {
        var sb1 = StringBuilderCache.Acquire();
        var sb2 = StringBuilderCache.Acquire();
        Assert.NotSame(sb1, sb2);
        StringBuilderCache.Release(sb1);
        StringBuilderCache.Release(sb2);
    }

    [Fact]
    public void Acquire_WithZeroCapacity_ReturnsStringBuilder()
    {
        var sb = StringBuilderCache.Acquire(0);
        Assert.NotNull(sb);
        StringBuilderCache.Release(sb);
    }

    #endregion

    #region Release Tests

    [Fact]
    public void Release_SmallBuilder_CachesIt()
    {
        var sb = new StringBuilder(100);
        sb.Append("Test content");
        StringBuilderCache.Release(sb);

        // Acquire should return the cached builder (cleared)
        var acquired = StringBuilderCache.Acquire(100);
        Assert.Equal(0, acquired.Length);
        StringBuilderCache.Release(acquired);
    }

    [Fact]
    public void Release_LargeBuilder_DoesNotCache()
    {
        // Create a builder larger than MaxBuilderSize
        var sb = new StringBuilder(500);
        StringBuilderCache.Release(sb);

        // Should not affect the cache for small builders
        var acquired = StringBuilderCache.Acquire();
        Assert.NotNull(acquired);
        StringBuilderCache.Release(acquired);
    }

    [Fact]
    public void Release_AndReacquire_ReturnsCleanBuilder()
    {
        var sb = StringBuilderCache.Acquire();
        sb.Append("Some content that should be cleared");
        StringBuilderCache.Release(sb);

        var reacquired = StringBuilderCache.Acquire();
        Assert.Equal(0, reacquired.Length);
        StringBuilderCache.Release(reacquired);
    }

    #endregion

    #region GetStringAndRelease Tests

    [Fact]
    public void GetStringAndRelease_ReturnsCorrectString()
    {
        var sb = StringBuilderCache.Acquire();
        sb.Append("Hello, World!");

        var result = StringBuilderCache.GetStringAndRelease(sb);

        Assert.Equal("Hello, World!", result);
    }

    [Fact]
    public void GetStringAndRelease_EmptyBuilder_ReturnsEmptyString()
    {
        var sb = StringBuilderCache.Acquire();

        var result = StringBuilderCache.GetStringAndRelease(sb);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetStringAndRelease_ReleasesBuilderToCache()
    {
        var sb = StringBuilderCache.Acquire();
        sb.Append("Test");

        var result = StringBuilderCache.GetStringAndRelease(sb);

        // After release, acquiring should give us a clean builder
        var newSb = StringBuilderCache.Acquire();
        Assert.Equal(0, newSb.Length);
        StringBuilderCache.Release(newSb);
    }

    [Fact]
    public void GetStringAndRelease_WithMultipleAppends_ReturnsCorrectString()
    {
        var sb = StringBuilderCache.Acquire();
        sb.Append("Hello");
        sb.Append(", ");
        sb.Append("World");
        sb.Append("!");

        var result = StringBuilderCache.GetStringAndRelease(sb);

        Assert.Equal("Hello, World!", result);
    }

    [Fact]
    public void GetStringAndRelease_WithNewLines_ReturnsCorrectString()
    {
        var sb = StringBuilderCache.Acquire();
        sb.AppendLine("Line1");
        sb.AppendLine("Line2");
        sb.Append("Line3");

        var result = StringBuilderCache.GetStringAndRelease(sb);

        Assert.Equal($"Line1{Environment.NewLine}Line2{Environment.NewLine}Line3", result);
    }

    #endregion

    #region Capacity Edge Cases

    [Fact]
    public void Acquire_AtMaxBuilderSize_ReturnsStringBuilder()
    {
        var sb = StringBuilderCache.Acquire(360);
        Assert.NotNull(sb);
        Assert.True(sb.Capacity >= 360);
        StringBuilderCache.Release(sb);
    }

    [Fact]
    public void Acquire_JustOverMaxBuilderSize_ReturnsNewBuilder()
    {
        var sb = StringBuilderCache.Acquire(361);
        Assert.NotNull(sb);
        Assert.True(sb.Capacity >= 361);
    }

    [Fact]
    public void Release_AtMaxBuilderSize_CachesIt()
    {
        var sb = new StringBuilder(360);
        sb.Append("Test");
        StringBuilderCache.Release(sb);

        var acquired = StringBuilderCache.Acquire(100);
        Assert.Equal(0, acquired.Length);
        StringBuilderCache.Release(acquired);
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public void Acquire_FromMultipleThreads_ReturnsValidBuilders()
    {
        var results = new StringBuilder[10];
        var tasks = new Task[10];

        for (int i = 0; i < 10; i++)
        {
            int index = i;
            tasks[i] = Task.Run(() =>
            {
                results[index] = StringBuilderCache.Acquire();
                results[index].Append($"Thread {index}");
            });
        }

        Task.WaitAll(tasks);

        for (int i = 0; i < 10; i++)
        {
            Assert.NotNull(results[i]);
            Assert.Contains($"Thread {i}", results[i].ToString());
            StringBuilderCache.Release(results[i]);
        }
    }

    [Fact]
    public void GetStringAndRelease_FromMultipleThreads_ReturnsCorrectStrings()
    {
        var results = new string[10];
        var tasks = new Task[10];

        for (int i = 0; i < 10; i++)
        {
            int index = i;
            tasks[i] = Task.Run(() =>
            {
                var sb = StringBuilderCache.Acquire();
                sb.Append($"Result {index}");
                results[index] = StringBuilderCache.GetStringAndRelease(sb);
            });
        }

        Task.WaitAll(tasks);

        for (int i = 0; i < 10; i++)
        {
            Assert.Equal($"Result {i}", results[i]);
        }
    }

    #endregion

    #region Usage Pattern Tests

    [Fact]
    public void TypicalUsagePattern_WorksCorrectly()
    {
        // Simulate typical usage: acquire, build string, release
        var sb = StringBuilderCache.Acquire();
        sb.Append("public class ");
        sb.Append("MyClass");
        sb.AppendLine(" {");
        sb.AppendLine("    // Implementation");
        sb.Append("}");

        var result = StringBuilderCache.GetStringAndRelease(sb);

        Assert.Contains("public class MyClass", result);
        Assert.Contains("// Implementation", result);
    }

    [Fact]
    public void RepeatedAcquireRelease_WorksCorrectly()
    {
        for (int i = 0; i < 100; i++)
        {
            var sb = StringBuilderCache.Acquire();
            sb.Append($"Iteration {i}");
            var result = StringBuilderCache.GetStringAndRelease(sb);
            Assert.Equal($"Iteration {i}", result);
        }
    }

    #endregion
}
