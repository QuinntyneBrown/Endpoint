﻿using Endpoint.Core.Models.Syntax;
using Endpoint.Core.Models.Syntax.Classes;
using Endpoint.Core.Models.Syntax.Entities;
using Endpoint.Core.Models.Syntax.Entities.Legacy;
using Endpoint.Core.Models.Syntax.RouteHandlers;
using Endpoint.Core.Services;
using Endpoint.Core.ValueObjects;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Endpoint.Core.Models.Artifacts.Files;

public class FileModelFactory: IFileModelFactory
{
    private readonly IRouteHandlerModelFactory _routeHandlerModelFactory;

    private readonly ILegacyAggregateModelFactory _aggregateRootModelFactory;

    public FileModelFactory(ILegacyAggregateModelFactory aggregateRootModelFactory, IRouteHandlerModelFactory routeHandlerModelFactory)
    {
        _aggregateRootModelFactory = aggregateRootModelFactory;
        _routeHandlerModelFactory = routeHandlerModelFactory;
    }

    public TemplatedFileModel CreateTemplate(string template, string name, string directory, string extension = "cs", string filename = null, Dictionary<string, object> tokens = null)
    {
        return new TemplatedFileModel(template, name, directory, extension, tokens);
    }

    public EntityFileModel Create(EntityModel model, string directory)
    {
        return new EntityFileModel(model, directory);
    }

    public CSharpTemplatedFileModel CreateCSharp(string template, string @namespace, string name, string directory, Dictionary<string, object> tokens = null)
    {
        return new CSharpTemplatedFileModel(template, @namespace, name, directory, tokens ?? new TokensBuilder()
            .With("Name", (Token)name)
            .With("Namespace", (Token)@namespace)
            .Build());
    }
    
    public TemplatedFileModel LaunchSettingsJson(string projectDirectory, string projectName, int port)
    {
        return new TemplatedFileModel("LaunchSettings", "LaunchSettings", projectDirectory, "json", new TokensBuilder()
            .With(nameof(projectName), (Token)projectName)
            .With(nameof(port), (Token)$"{port}")
            .With("SslPort", (Token)$"{port + 1}")
            .Build());
    }

    public TemplatedFileModel AppSettings(string projectDirectory, string projectName, string dbContextName)
    {
        return new TemplatedFileModel("AppSettings", "appSettings", projectDirectory, "json", new TokensBuilder()
            .With(nameof(dbContextName), (Token)dbContextName)
            .With("Namespace", (Token)projectName)
            .Build());
    }

    public FileModel CreateCSharp<T>(T classModel, string directory)
        where T : TypeDeclarationModel
        => new ObjectFileModel<T>(classModel, classModel.UsingDirectives, classModel.Name, directory, "cs");

    public FileModel CreateResponseBase(string directory)
    {
        var classModel = new ClassModel("ResponseBase");

        return CreateCSharp(classModel, directory);
    }

    public FileModel CreateIQueryableExtensions(string directory)
    {
        var classModel = new ClassModel("IQueryableExtensions");

        return CreateCSharp(classModel, directory);
    }

    public FileModel CreateCoreUsings(string directory)
    {
        var usings = new string[]
        {
            "MediatR",
            "Microsoft.Extensions.Logging",
            "Microsoft.EntityFrameworkCore",
            "FluentValidation"
        }.Select(x => $"global using {x};");

        return new ContentFileModel(new StringBuilder()
            .AppendJoin(Environment.NewLine, usings)
            .ToString(), "Usings", directory, "cs");
    }
}
