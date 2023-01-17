﻿using Microsoft.Extensions.DependencyInjection;

namespace Endpoint.Core.Abstractions;

public abstract class SyntaxGenerationStrategyBase<T>: ISyntaxGenerationStrategy
{
    private readonly IServiceProvider _serviceProvider;
    public SyntaxGenerationStrategyBase(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public virtual bool CanHandle(object model, dynamic context = null) => model.GetType() == typeof(T);
    public string Create(dynamic model, dynamic context = null)
    {
        using(IServiceScope scope = _serviceProvider.CreateScope())
        {
            var syntaxGenerationStrategyFactory = scope.ServiceProvider
                .GetRequiredService<ISyntaxGenerationStrategyFactory>();

            return Create(syntaxGenerationStrategyFactory, model, context);
        }        
    }

    public abstract string Create(ISyntaxGenerationStrategyFactory syntaxGenerationStrategyFactory, T model, dynamic context = null);
    public virtual int Priority => 0;
}
