namespace Endpoint.DotNet.Services;

public interface IObjectCache
{
    TResponse FromCacheOrService<TResponse>(Func<TResponse> action, string key);
}
