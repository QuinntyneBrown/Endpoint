using System.Collections.Generic;
using Endpoint.DotNet.Services;
using Endpoint.DotNet.Syntax.Units;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Syntax.AccessModifier;
using static Endpoint.DotNet.Syntax.Properties.PropertyAccessorModel;
using static Endpoint.DotNet.Syntax.Types.TypeModel;

namespace Endpoint.DotNet.Syntax.Properties.Factories;

public class PropertyFactory : IPropertyFactory
{
    private readonly ILogger<PropertyFactory> logger;
    private readonly INamingConventionConverter namingConventionConverter;

    public PropertyFactory(ILogger<PropertyFactory> logger, INamingConventionConverter namingConventionConverter)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
    }

    public async Task<List<PropertyModel>> ResponsePropertiesCreateAsync(RequestType responseType, TypeDeclarationModel parent, string entityName)
    {
        logger.LogInformation("Creating Response Properties. {entityName}", entityName);

        var model = new List<PropertyModel>();

        switch (responseType)
        {
            case RequestType.Get:
                var entityNamePascalCasePlural = namingConventionConverter.Convert(NamingConvention.PascalCase, entityName, pluralize: true);

                model.Add(new (parent, Public, ListOf($"{entityName}Dto"), entityNamePascalCasePlural, GetSet));
                break;

            default:
                throw new InvalidOperationException();
        }

        return model;
    }

    /*    public async Task<List<PropertyModel>> ResponsePropertiesCreateAsync(TypeDeclarationModel parent, string properties)
        {
            _logger.LogInformation("Creating Properties");

            var model = new List<PropertyModel>();

            var props = properties.Split(',');

            foreach(var prop in props) {

                var parts = prop.Split(':');

                model.Add(new (parent, AccessModifier.Public, new(parts[1]), parts[0], PropertyAccessorModel.GetSet));
            }

            return model;
        }*/
}
