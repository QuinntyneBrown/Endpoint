// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Octokit;

namespace Endpoint.DotNet.Syntax;

public class SyntaxGenerator : ISyntaxGenerator
{
    private static readonly ConcurrentDictionary<Type, SyntaxGenerationStrategyBase> _syntaxGenerators = new();
    private readonly IServiceProvider _serviceProvider;

    public SyntaxGenerator(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _serviceProvider = serviceProvider;
    }

    public Task<string> GenerateAsync<T>(T model)
    {
        var handler = _syntaxGenerators.GetOrAdd(model.GetType(), static targetType =>
        {
            var wrapperType = typeof(SyntaxGenerationStrategyWrapperImplementation<>).MakeGenericType(targetType);
            var wrapper = Activator.CreateInstance(wrapperType) ?? throw new InvalidOperationException($"Could not create wrapper type for {targetType}");
            return (SyntaxGenerationStrategyBase)wrapper;
        });

        return handler.GenerateAsync(_serviceProvider, model, default);
    }
}

public abstract class SyntaxGenerationStrategyBase
{
    public abstract Task<string> GenerateAsync(IServiceProvider serviceProvider, object target, CancellationToken cancellationToken);
}

public abstract class SyntaxGenerationStrategyWrapper<T> : SyntaxGenerationStrategyBase
{
    public abstract Task<string> GenerateAsync(T target, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}

public class SyntaxGenerationStrategyWrapperImplementation<T> : SyntaxGenerationStrategyWrapper<T>
{
    private readonly ConcurrentDictionary<Type, IEnumerable<ISyntaxGenerationStrategy>> _strategies = [];

    public override async Task<string> GenerateAsync(IServiceProvider serviceProvider, object target, CancellationToken cancellationToken)
    {
        return await GenerateAsync((T)target, serviceProvider, cancellationToken).ConfigureAwait(false);
    }

    public override async Task<string> GenerateAsync(T target, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var handlers = serviceProvider.GetRequiredService<IEnumerable<ISyntaxGenerationStrategy<T>>>();

        var handler = handlers
            .OrderByDescending(x => x.GetPriority())
            .First();

        return await handler.GenerateAsync(target, cancellationToken);
    }
}

public interface ISyntaxGenerationStrategy<T>
{
    virtual int GetPriority() => 1;

    Task<string> GenerateAsync(T target, CancellationToken cancellationToken);
}