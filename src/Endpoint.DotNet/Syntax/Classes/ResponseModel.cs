// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using Endpoint.DotNet.Services;
using Endpoint.DotNet.Syntax.Properties;
using Endpoint.DotNet.Syntax.Types;

namespace Endpoint.DotNet.Syntax.Classes;

using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;
public class ResponseModel : ClassModel
{
    public ResponseModel(ClassModel entity, RouteType routeType, INamingConventionConverter namingConventionConverter)
        : base(string.Empty)
    {
        var entityNamePascalCasePlural = namingConventionConverter.Convert(NamingConvention.PascalCase, entity.Name, true);

        Name = routeType switch
        {
            RouteType.Create => $"Create{entity.Name}Response",
            RouteType.Update => $"Update{entity.Name}Response",
            RouteType.Delete => $"Delete{entity.Name}Response",
            RouteType.Get => $"Get{entityNamePascalCasePlural}Response",
            RouteType.GetById => $"Get{entity.Name}ByIdResponse",
            RouteType.Page => $"Get{entityNamePascalCasePlural}PageResponse",
            _ => throw new NotSupportedException()
        };

        if (routeType == RouteType.Create
            || routeType == RouteType.Update
            || routeType == RouteType.Delete
            || routeType == RouteType.GetById)
        {
            Properties.Add(new PropertyModel(this, AccessModifier.Public, new TypeModel($"{entity.Name}Dto"), $"{entity.Name}", PropertyAccessorModel.GetSet, required: true));
        }

        if (routeType == RouteType.Get)
        {
            Properties.Add(new PropertyModel(this, AccessModifier.Public, new TypeModel("List")
            {
                GenericTypeParameters = new List<TypeModel>
                {
                    new TypeModel($"{entity.Name}Dto"),
                },
            }, $"{entityNamePascalCasePlural}", PropertyAccessorModel.GetSet, required: true));
        }

        if (routeType == RouteType.Page)
        {
            Properties.Add(new PropertyModel(this, AccessModifier.Public, new TypeModel("int"), "Length", PropertyAccessorModel.GetSet, required: true));

            Properties.Add(new PropertyModel(this, AccessModifier.Public, new TypeModel("List")
            {
                GenericTypeParameters = new List<TypeModel>()
                {
                    new TypeModel($"{entity.Name}Dto"),
                },
            }, "Entities ", PropertyAccessorModel.GetSet, required: true));
        }
    }
}
