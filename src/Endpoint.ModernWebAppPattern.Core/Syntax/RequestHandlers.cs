// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DomainDrivenDesign.Core.Models;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Fields;
using Endpoint.DotNet.Syntax.Params;
using Endpoint.ModernWebAppPattern.Core.Syntax.Expressions.RequestHandlers;
using Humanizer;

namespace Endpoint.ModernWebAppPattern.Core.Syntax;

using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

public class RequestHandlers : List<ClassModel> { 

    public static RequestHandlers Create(string aggregateName, string productName, string boundedContextName = "")
    {
        var requestHandlers = new RequestHandlers();

        var (aggregate, dataContext) = AggregateModel.Create(aggregateName, productName);

        boundedContextName ??= aggregateName.Pluralize();

        var aggregateNamespace = $"{productName}.Models.{aggregateName}";

        var boundedContext = new BoundedContext(boundedContextName)
        {
            Aggregates = [aggregate]
        };

        foreach (var command in aggregate.Commands)
        {
            var commandHandlerModel = new ClassModel($"{command.Name}Handler");

            commandHandlerModel.Implements.Add(new($"IRequestHandler<{command.Name}Request, {command.Name}Response>"));

            commandHandlerModel.Usings.Add(new("MediatR"));

            commandHandlerModel.Usings.Add(new(aggregateNamespace));

            commandHandlerModel.Fields =
                [
                    FieldModel.LoggerOf($"{command.Name}Handler"),
                    new() { Name = "_context", Type = new($"I{boundedContext.Name}DbContext") }
                ];

            commandHandlerModel.Constructors.Add(new(commandHandlerModel, commandHandlerModel.Name)
            {
                Params =
                [
                    ParamModel.LoggerOf($"{command.Name}Handler"),
                    new() { Name = "context", Type = new($"I{boundedContext.Name}DbContext") }
                ]
            });

            commandHandlerModel.Methods.Add(new()
            {
                Name = "Handle",
                Params = [
                    new() { Name = "request", Type = new($"{command.Name}Request") },
                    ParamModel.CancellationToken
                ],
                Async = true,
                ReturnType = TypeModel.TaskOf($"{command.Name}Response"),
                Body = command.Kind switch
                {
                    RequestKind.Create => new CreateRequestHandlerExpressionModel(command),
                    RequestKind.Update => new UpdateRequestHandlerExpressionModel(command),
                    RequestKind.Delete => new DeleteRequestHandlerExpressionModel(command),
                    _ => throw new NotImplementedException()
                }
            });

            requestHandlers.Add(commandHandlerModel);
        }

        foreach (var query in aggregate.Queries)
        {
            var queryHandlerModel = new ClassModel($"{query.Name}Handler");

            queryHandlerModel.Usings.Add(new("MediatR"));

            queryHandlerModel.Usings.Add(new(aggregateNamespace));

            queryHandlerModel.Implements.Add(new($"IRequestHandler<{query.Name}Request, {query.Name}Response>"));

            queryHandlerModel.Fields =
                [
                    FieldModel.LoggerOf($"{query.Name}Handler"),
                    new() { Name = "_context", Type = new($"I{boundedContext.Name}DbContext") }
                ];

            queryHandlerModel.Constructors.Add(new(queryHandlerModel, queryHandlerModel.Name)
            {
                Params =
                [
                    ParamModel.LoggerOf($"{query.Name}Handler"),
                    new() { Name = "context", Type = new($"I{boundedContext.Name}DbContext") }
                ]
            });

            queryHandlerModel.Methods.Add(new()
            {
                Name = "Handle",
                Params = [
                    new() { Name = "request", Type = new($"{query.Name}Request") },
                    ParamModel.CancellationToken
                ],
                Async = true,
                ReturnType = TypeModel.TaskOf($"{query.Name}Response"),
                Body = query.Kind switch
                {
                    RequestKind.GetById => new GetByIdRequestHandlerExpressionModel(query),
                    RequestKind.Get => new GetRequestHandlerExpressionModel(query),
                    _ => throw new NotImplementedException()
                }
            });

            queryHandlerModel.Usings.Add(new("MediatR"));

            queryHandlerModel.Usings.Add(new(aggregateNamespace));

            requestHandlers.Add(queryHandlerModel);
        }

        return requestHandlers;
    }
}
