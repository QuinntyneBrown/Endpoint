using Endpoint.DotNet.Syntax.Classes;

namespace Endpoint.DotNet.Syntax.Types;

public interface ITypeFactory
{
    Task<TypeModel> Create(ClassModel @class);
}
