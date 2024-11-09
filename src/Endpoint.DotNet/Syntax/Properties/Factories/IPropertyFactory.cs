using System.Collections.Generic;
using Endpoint.DotNet.Syntax.Units;

namespace Endpoint.DotNet.Syntax.Properties.Factories;

public interface IPropertyFactory
{
    Task<List<PropertyModel>> ResponsePropertiesCreateAsync(RequestType responseType, TypeDeclarationModel parent, string entityName);
}
