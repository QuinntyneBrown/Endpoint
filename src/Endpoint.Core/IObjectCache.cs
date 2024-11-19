namespace Endpoint.Core;

public interface IObjectCache
{
    TResponse FromCacheOrService<TResponse>(Func<TResponse> action, string key);
}
