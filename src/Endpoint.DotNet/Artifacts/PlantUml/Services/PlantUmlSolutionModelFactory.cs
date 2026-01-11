// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Endpoint.DotNet.Artifacts.Files;
using Endpoint.DotNet.Artifacts.PlantUml.Models;
using Endpoint.DotNet.Artifacts.Projects;
using Endpoint.DotNet.Artifacts.Projects.Enums;
using Endpoint.DotNet.Artifacts.Solutions;
using Endpoint.DotNet.Services;
using Endpoint.DotNet.Syntax;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Classes.Factories;
using Endpoint.DotNet.Syntax.Constructors;
using Endpoint.DotNet.Syntax.Entities;
using Endpoint.DotNet.Syntax.Methods;
using Endpoint.DotNet.Syntax.Params;
using Endpoint.DotNet.Syntax.Properties;
using Endpoint.DotNet.Syntax.Types;
using Endpoint.DotNet.Syntax.Units;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.Artifacts.PlantUml.Services;

using IFileFactory = Endpoint.DotNet.Artifacts.Files.Factories.IFileFactory;
using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

public class PlantUmlSolutionModelFactory : IPlantUmlSolutionModelFactory
{
    private readonly ILogger<PlantUmlSolutionModelFactory> logger;
    private readonly IClassFactory classFactory;
    private readonly IFileFactory fileFactory;
    private readonly INamingConventionConverter namingConventionConverter;

    public PlantUmlSolutionModelFactory(
        ILogger<PlantUmlSolutionModelFactory> logger,
        IClassFactory classFactory,
        IFileFactory fileFactory,
        INamingConventionConverter namingConventionConverter)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.classFactory = classFactory ?? throw new ArgumentNullException(nameof(classFactory));
        this.fileFactory = fileFactory ?? throw new ArgumentNullException(nameof(fileFactory));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
    }

    public async Task<SolutionModel> CreateAsync(PlantUmlSolutionModel plantUmlModel, string solutionName, string outputDirectory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating solution model from PlantUML: {SolutionName}", solutionName);

        var solution = new SolutionModel(solutionName, outputDirectory);

        // Create Core project
        var coreProject = CreateCoreProject(plantUmlModel, solutionName, solution.SrcDirectory);
        solution.Projects.Add(coreProject);

        // Create Infrastructure project
        var infrastructureProject = CreateInfrastructureProject(plantUmlModel, solutionName, solution.SrcDirectory);
        solution.Projects.Add(infrastructureProject);

        // Create Api project
        var apiProject = CreateApiProject(plantUmlModel, solutionName, solution.SrcDirectory);
        solution.Projects.Add(apiProject);

        // Set up project dependencies
        solution.DependOns.Add(new DependsOnModel(apiProject, infrastructureProject));
        solution.DependOns.Add(new DependsOnModel(infrastructureProject, coreProject));

        return solution;
    }

    private ProjectModel CreateCoreProject(PlantUmlSolutionModel plantUmlModel, string solutionName, string srcDirectory)
    {
        var projectName = $"{solutionName}.Core";
        var project = new ProjectModel(DotNetProjectType.ClassLib, projectName, srcDirectory)
        {
            Order = 1
        };

        // Add NuGet packages
        project.Packages.Add(new PackageModel("MediatR", "12.2.0"));
        project.Packages.Add(new PackageModel("FluentValidation", "11.9.0"));
        project.Packages.Add(new PackageModel("FluentValidation.DependencyInjectionExtensions", "11.9.0"));
        project.Packages.Add(new PackageModel("Microsoft.EntityFrameworkCore", "8.0.0"));
        project.Packages.Add(new PackageModel("Microsoft.Extensions.Logging.Abstractions", "8.0.0"));

        // Generate entity models
        foreach (var entity in plantUmlModel.GetEntities())
        {
            GenerateEntityFiles(project, entity, plantUmlModel, solutionName);
        }

        // Generate enum files
        foreach (var enumModel in plantUmlModel.GetAllEnums())
        {
            GenerateEnumFile(project, enumModel);
        }

        // Generate IContext interface in Core project
        GenerateContextInterface(project, plantUmlModel, solutionName);

        return project;
    }

    private void GenerateContextInterface(ProjectModel project, PlantUmlSolutionModel solutionModel, string solutionName)
    {
        var contextDirectory = Path.Combine(project.Directory, "Data");
        var entities = solutionModel.GetEntities().ToList();
        var interfaceContent = GenerateCoreContextInterfaceContent(entities, solutionName, project.Namespace);
        project.Files.Add(new Files.ContentFileModel(interfaceContent, $"I{solutionName}Context", contextDirectory, ".cs"));
    }

    private string GenerateCoreContextInterfaceContent(List<PlantUmlClassModel> entities, string solutionName, string projectNamespace)
    {
        var dbSets = string.Join("\n    ", entities.Select(e => $"DbSet<{e.Name}> {GetPluralName(e.Name)} {{ get; set; }}"));
        var usings = string.Join("\n", entities.Select(e => $"using {projectNamespace}.Models.{e.Name};"));

        return $@"// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
{usings}

namespace {projectNamespace}.Data;

public interface I{solutionName}Context
{{
    {dbSets}

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}}
";
    }

    private ProjectModel CreateInfrastructureProject(PlantUmlSolutionModel plantUmlModel, string solutionName, string srcDirectory)
    {
        var projectName = $"{solutionName}.Infrastructure";
        var project = new ProjectModel(DotNetProjectType.ClassLib, projectName, srcDirectory)
        {
            Order = 2
        };

        // Add NuGet packages
        project.Packages.Add(new PackageModel("Microsoft.EntityFrameworkCore", "8.0.0"));
        project.Packages.Add(new PackageModel("Microsoft.EntityFrameworkCore.SqlServer", "8.0.0"));
        project.Packages.Add(new PackageModel("Microsoft.EntityFrameworkCore.Design", "8.0.0"));
        project.Packages.Add(new PackageModel("Microsoft.EntityFrameworkCore.Tools", "8.0.0"));

        // Add reference to Core project
        project.References.Add($"..{Path.DirectorySeparatorChar}{solutionName}.Core{Path.DirectorySeparatorChar}{solutionName}.Core.csproj");

        // Generate DbContext
        GenerateDbContextFile(project, plantUmlModel, solutionName);

        return project;
    }

    private ProjectModel CreateApiProject(PlantUmlSolutionModel plantUmlModel, string solutionName, string srcDirectory)
    {
        var projectName = $"{solutionName}.Api";
        var project = new ProjectModel(DotNetProjectType.WebApi, projectName, srcDirectory)
        {
            Order = 3
        };

        // Add NuGet packages
        project.Packages.Add(new PackageModel("MediatR", "12.2.0"));
        project.Packages.Add(new PackageModel("Swashbuckle.AspNetCore", "6.5.0"));
        project.Packages.Add(new PackageModel("Swashbuckle.AspNetCore.Annotations", "6.5.0"));

        // Add reference to Infrastructure project
        project.References.Add($"..{Path.DirectorySeparatorChar}{solutionName}.Infrastructure{Path.DirectorySeparatorChar}{solutionName}.Infrastructure.csproj");

        // Generate controllers for aggregate roots
        foreach (var aggregate in plantUmlModel.GetAggregates())
        {
            GenerateControllerFile(project, aggregate, solutionName);
        }

        // Generate Program.cs
        GenerateProgramFile(project, solutionName);

        return project;
    }

    private void GenerateEntityFiles(ProjectModel project, PlantUmlClassModel plantUmlClass, PlantUmlSolutionModel solutionModel, string solutionName)
    {
        var entityName = plantUmlClass.Name;
        var entityDirectory = Path.Combine(project.Directory, "Models", entityName);

        // Generate entity class
        var entityClass = CreateEntityClassModel(plantUmlClass, project.Namespace);
        project.Files.Add(new CodeFileModel<ClassModel>(entityClass, entityClass.Usings, entityName, entityDirectory, ".cs"));

        // Generate DTO class
        var dtoClass = CreateDtoClassModel(plantUmlClass, project.Namespace);
        project.Files.Add(new CodeFileModel<ClassModel>(dtoClass, dtoClass.Usings, $"{entityName}Dto", entityDirectory, ".cs"));

        // Generate Extensions class
        var extensionsClass = CreateExtensionsClassModel(plantUmlClass);
        project.Files.Add(new CodeFileModel<ClassModel>(extensionsClass, extensionsClass.Usings, $"{entityName}Extensions", entityDirectory, ".cs"));

        // Only generate CRUD operations for aggregate roots
        if (plantUmlClass.IsAggregate)
        {
            // Use "Features" prefix to avoid namespace collision with model names
            var cqrsDirectory = Path.Combine(project.Directory, "Features", entityName);
            GenerateCrudOperations(project, plantUmlClass, cqrsDirectory, solutionName);
        }
    }

    private ClassModel CreateEntityClassModel(PlantUmlClassModel plantUmlClass, string projectNamespace)
    {
        var classModel = new ClassModel(plantUmlClass.Name);

        foreach (var property in plantUmlClass.Properties)
        {
            var propertyModel = CreatePropertyModel(classModel, property, projectNamespace);
            classModel.Properties.Add(propertyModel);
        }

        return classModel;
    }

    private static bool IsPrimitiveType(string typeName)
    {
        var primitives = new[] { "string", "int", "long", "bool", "decimal", "double", "float", "DateTime", "DateTimeOffset", "Guid", "byte", "short" };
        return primitives.Contains(typeName, StringComparer.OrdinalIgnoreCase);
    }

    private ClassModel CreateDtoClassModel(PlantUmlClassModel plantUmlClass, string projectNamespace)
    {
        var classModel = new ClassModel($"{plantUmlClass.Name}Dto");

        foreach (var property in plantUmlClass.Properties)
        {
            var propertyModel = CreatePropertyModel(classModel, property, projectNamespace);
            classModel.Properties.Add(propertyModel);
        }

        return classModel;
    }

    private ClassModel CreateExtensionsClassModel(PlantUmlClassModel plantUmlClass)
    {
        var entityName = plantUmlClass.Name;
        var classModel = new ClassModel($"{entityName}Extensions")
        {
            Static = true
        };

        // Add ToDto method
        var toDtoMethod = new MethodModel
        {
            Name = "ToDto",
            ReturnType = new TypeModel($"{entityName}Dto"),
            Static = true,
            AccessModifier = AccessModifier.Public,
            Body = new Syntax.Expressions.ExpressionModel("throw new NotImplementedException();")
        };
        toDtoMethod.Params.Add(new ParamModel
        {
            Name = "entity",
            Type = new TypeModel(entityName),
            ExtensionMethodParam = true
        });
        classModel.AddMethod(toDtoMethod);

        return classModel;
    }

    private PropertyModel CreatePropertyModel(TypeDeclarationModel parent, PlantUmlPropertyModel plantUmlProperty, string projectNamespace = null)
    {
        var typeModel = CreateTypeModel(plantUmlProperty, projectNamespace);
        var accessors = PropertyAccessorModel.GetSet;

        return new PropertyModel(
            parent,
            ConvertVisibility(plantUmlProperty.Visibility),
            typeModel,
            plantUmlProperty.Name,
            accessors,
            plantUmlProperty.IsRequired,
            plantUmlProperty.IsKey);
    }

    private TypeModel CreateTypeModel(PlantUmlPropertyModel property, string projectNamespace = null)
    {
        TypeModel typeModel;

        if (property.IsCollection && !string.IsNullOrEmpty(property.GenericTypeArgument))
        {
            // Use fully qualified name for non-primitive types to avoid namespace collision
            var genericTypeName = property.GenericTypeArgument;
            if (!IsPrimitiveType(genericTypeName) && !string.IsNullOrEmpty(projectNamespace))
            {
                genericTypeName = $"Models.{property.GenericTypeArgument}.{property.GenericTypeArgument}";
            }

            typeModel = new TypeModel(property.CollectionType ?? "List")
            {
                GenericTypeParameters = new List<TypeModel>
                {
                    new TypeModel(genericTypeName)
                }
            };
        }
        else
        {
            typeModel = new TypeModel(MapTypeName(property.Type))
            {
                Nullable = property.IsNullable
            };
        }

        return typeModel;
    }

    private void GenerateCrudOperations(ProjectModel project, PlantUmlClassModel entity, string directory, string solutionName)
    {
        var entityName = entity.Name;

        // Create Request/Response/Handler for each CRUD operation
        GenerateCreateOperation(project, entity, directory, solutionName);
        GenerateGetByIdOperation(project, entity, directory, solutionName);
        GenerateGetAllOperation(project, entity, directory, solutionName);
        GenerateUpdateOperation(project, entity, directory, solutionName);
        GenerateDeleteOperation(project, entity, directory, solutionName);
    }

    private static PlantUmlPropertyModel? GetKeyProperty(PlantUmlClassModel entity)
    {
        return entity.Properties.FirstOrDefault(p => p.IsKey);
    }

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;
        return char.ToLowerInvariant(name[0]) + name[1..];
    }

    private void GenerateCreateOperation(ProjectModel project, PlantUmlClassModel entity, string directory, string solutionName)
    {
        var entityName = entity.Name;
        var modelsNamespace = $"{project.Namespace}.Models.{entityName}";
        var keyProperty = GetKeyProperty(entity);
        var keyPropertyName = keyProperty?.Name ?? $"{entityName}Id";

        // Create Request
        var requestClass = new ClassModel($"Create{entityName}Request");
        requestClass.Usings.Add(new UsingModel("MediatR"));
        requestClass.Usings.Add(new UsingModel($"{project.Namespace}.Models"));
        requestClass.Implements.Add(new TypeModel($"IRequest<Create{entityName}Response>"));
        foreach (var property in entity.Properties.Where(p => !p.IsKey && !IsAuditProperty(p.Name)))
        {
            requestClass.Properties.Add(CreatePropertyModel(requestClass, property, project.Namespace));
        }
        project.Files.Add(new CodeFileModel<ClassModel>(requestClass, requestClass.Usings, $"Create{entityName}Request", directory, ".cs"));

        // Create Response
        var responseClass = new ClassModel($"Create{entityName}Response");
        responseClass.Usings.Add(new UsingModel(modelsNamespace));
        responseClass.Properties.Add(new PropertyModel(responseClass, AccessModifier.Public, new TypeModel($"{entityName}Dto"), entityName, PropertyAccessorModel.GetSet));
        project.Files.Add(new CodeFileModel<ClassModel>(responseClass, responseClass.Usings, $"Create{entityName}Response", directory, ".cs"));

        // Create Handler with implementation
        var handlerContent = GenerateCreateHandlerContent(entity, solutionName, project.Namespace, keyPropertyName);
        project.Files.Add(new Files.ContentFileModel(handlerContent, $"Create{entityName}Handler", directory, ".cs"));

        // Create Validator
        var validatorContent = GenerateCreateValidatorContent(entity, project.Namespace);
        project.Files.Add(new Files.ContentFileModel(validatorContent, $"Create{entityName}Validator", directory, ".cs"));
    }

    private string GenerateCreateHandlerContent(PlantUmlClassModel entity, string solutionName, string projectNamespace, string keyPropertyName)
    {
        var entityName = entity.Name;
        var entityNameCamel = ToCamelCase(entityName);
        var nonKeyProperties = entity.Properties.Where(p => !p.IsKey && !IsAuditProperty(p.Name)).ToList();

        var propertyAssignments = string.Join("\n            ", nonKeyProperties.Select(p => $"{p.Name} = request.{p.Name},"));

        // Use type alias to avoid namespace collision between Features.{EntityName} namespace and Models.{EntityName}.{EntityName} type
        return $@"// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;
using Microsoft.EntityFrameworkCore;
using {projectNamespace}.Data;
using {entityName}Entity = {projectNamespace}.Models.{entityName}.{entityName};
using {entityName}Ext = {projectNamespace}.Models.{entityName}.{entityName}Extensions;

namespace {projectNamespace}.Features.{entityName};

public class Create{entityName}Handler : IRequestHandler<Create{entityName}Request, Create{entityName}Response>
{{
    private readonly I{solutionName}Context _context;

    public Create{entityName}Handler(I{solutionName}Context context)
    {{
        _context = context;
    }}

    public async Task<Create{entityName}Response> Handle(Create{entityName}Request request, CancellationToken cancellationToken)
    {{
        var {entityNameCamel} = new {entityName}Entity
        {{
            {keyPropertyName} = Guid.NewGuid().ToString(),
            {propertyAssignments}
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        }};

        _context.{GetPluralName(entityName)}.Add({entityNameCamel});
        await _context.SaveChangesAsync(cancellationToken);

        return new Create{entityName}Response
        {{
            {entityName} = {entityName}Ext.ToDto({entityNameCamel})
        }};
    }}
}}
";
    }

    private string GenerateCreateValidatorContent(PlantUmlClassModel entity, string projectNamespace)
    {
        var entityName = entity.Name;
        var requiredProperties = entity.Properties.Where(p => !p.IsKey && !IsAuditProperty(p.Name) && p.IsRequired && p.Type.ToLower() == "string").ToList();

        var rules = string.Join("\n        ", requiredProperties.Select(p => $"RuleFor(x => x.{p.Name}).NotEmpty();"));

        return $@"// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using FluentValidation;

namespace {projectNamespace}.Features.{entityName};

public class Create{entityName}Validator : AbstractValidator<Create{entityName}Request>
{{
    public Create{entityName}Validator()
    {{
        {rules}
    }}
}}
";
    }

    private void GenerateGetByIdOperation(ProjectModel project, PlantUmlClassModel entity, string directory, string solutionName)
    {
        var entityName = entity.Name;
        var modelsNamespace = $"{project.Namespace}.Models.{entityName}";
        var keyProperty = GetKeyProperty(entity);
        var keyPropertyName = keyProperty?.Name ?? $"{entityName}Id";

        // Create Request
        var requestClass = new ClassModel($"Get{entityName}ByIdRequest");
        requestClass.Usings.Add(new UsingModel("MediatR"));
        requestClass.Implements.Add(new TypeModel($"IRequest<Get{entityName}ByIdResponse>"));
        requestClass.Properties.Add(new PropertyModel(requestClass, AccessModifier.Public, new TypeModel("string"), keyPropertyName, PropertyAccessorModel.GetSet));
        project.Files.Add(new CodeFileModel<ClassModel>(requestClass, requestClass.Usings, $"Get{entityName}ByIdRequest", directory, ".cs"));

        // Create Response
        var responseClass = new ClassModel($"Get{entityName}ByIdResponse");
        responseClass.Usings.Add(new UsingModel(modelsNamespace));
        responseClass.Properties.Add(new PropertyModel(responseClass, AccessModifier.Public, new TypeModel($"{entityName}Dto"), entityName, PropertyAccessorModel.GetSet));
        project.Files.Add(new CodeFileModel<ClassModel>(responseClass, responseClass.Usings, $"Get{entityName}ByIdResponse", directory, ".cs"));

        // Create Handler with implementation
        var handlerContent = GenerateGetByIdHandlerContent(entity, solutionName, project.Namespace, keyPropertyName);
        project.Files.Add(new Files.ContentFileModel(handlerContent, $"Get{entityName}ByIdHandler", directory, ".cs"));
    }

    private string GenerateGetByIdHandlerContent(PlantUmlClassModel entity, string solutionName, string projectNamespace, string keyPropertyName)
    {
        var entityName = entity.Name;
        var entityNameCamel = ToCamelCase(entityName);

        // Use type alias to avoid namespace collision between Features.{EntityName} namespace and Models.{EntityName}.{EntityName} type
        return $@"// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;
using Microsoft.EntityFrameworkCore;
using {projectNamespace}.Data;
using {entityName}Entity = {projectNamespace}.Models.{entityName}.{entityName};
using {entityName}Ext = {projectNamespace}.Models.{entityName}.{entityName}Extensions;

namespace {projectNamespace}.Features.{entityName};

public class Get{entityName}ByIdHandler : IRequestHandler<Get{entityName}ByIdRequest, Get{entityName}ByIdResponse>
{{
    private readonly I{solutionName}Context _context;

    public Get{entityName}ByIdHandler(I{solutionName}Context context)
    {{
        _context = context;
    }}

    public async Task<Get{entityName}ByIdResponse> Handle(Get{entityName}ByIdRequest request, CancellationToken cancellationToken)
    {{
        var {entityNameCamel} = await _context.{GetPluralName(entityName)}
            .FirstOrDefaultAsync(x => x.{keyPropertyName} == request.{keyPropertyName}, cancellationToken);

        if ({entityNameCamel} == null)
        {{
            throw new KeyNotFoundException($""{entityName} with ID '{{request.{keyPropertyName}}}' was not found."");
        }}

        return new Get{entityName}ByIdResponse
        {{
            {entityName} = {entityName}Ext.ToDto({entityNameCamel})
        }};
    }}
}}
";
    }

    private void GenerateGetAllOperation(ProjectModel project, PlantUmlClassModel entity, string directory, string solutionName)
    {
        var entityName = entity.Name;
        var pluralName = GetPluralName(entityName);
        var modelsNamespace = $"{project.Namespace}.Models.{entityName}";

        // Create Request
        var requestClass = new ClassModel($"Get{pluralName}Request");
        requestClass.Usings.Add(new UsingModel("MediatR"));
        requestClass.Implements.Add(new TypeModel($"IRequest<Get{pluralName}Response>"));
        requestClass.Properties.Add(new PropertyModel(requestClass, AccessModifier.Public, new TypeModel("int"), "PageIndex", PropertyAccessorModel.GetSet) { DefaultValue = "0" });
        requestClass.Properties.Add(new PropertyModel(requestClass, AccessModifier.Public, new TypeModel("int"), "PageSize", PropertyAccessorModel.GetSet) { DefaultValue = "20" });
        requestClass.Properties.Add(new PropertyModel(requestClass, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "SearchTerm", PropertyAccessorModel.GetSet));
        requestClass.Properties.Add(new PropertyModel(requestClass, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "SortBy", PropertyAccessorModel.GetSet));
        requestClass.Properties.Add(new PropertyModel(requestClass, AccessModifier.Public, new TypeModel("bool"), "SortDescending", PropertyAccessorModel.GetSet) { DefaultValue = "false" });
        project.Files.Add(new CodeFileModel<ClassModel>(requestClass, requestClass.Usings, $"Get{pluralName}Request", directory, ".cs"));

        // Create Response
        var responseClass = new ClassModel($"Get{pluralName}Response");
        responseClass.Usings.Add(new UsingModel(modelsNamespace));
        responseClass.Properties.Add(new PropertyModel(responseClass, AccessModifier.Public, TypeModel.ListOf($"{entityName}Dto"), pluralName, PropertyAccessorModel.GetSet) { DefaultValue = "new()" });
        responseClass.Properties.Add(new PropertyModel(responseClass, AccessModifier.Public, new TypeModel("int"), "TotalCount", PropertyAccessorModel.GetSet));
        responseClass.Properties.Add(new PropertyModel(responseClass, AccessModifier.Public, new TypeModel("int"), "PageIndex", PropertyAccessorModel.GetSet));
        responseClass.Properties.Add(new PropertyModel(responseClass, AccessModifier.Public, new TypeModel("int"), "PageSize", PropertyAccessorModel.GetSet));
        project.Files.Add(new CodeFileModel<ClassModel>(responseClass, responseClass.Usings, $"Get{pluralName}Response", directory, ".cs"));

        // Create Handler with implementation
        var handlerContent = GenerateGetAllHandlerContent(entity, solutionName, project.Namespace);
        project.Files.Add(new Files.ContentFileModel(handlerContent, $"Get{pluralName}Handler", directory, ".cs"));
    }

    private string GenerateGetAllHandlerContent(PlantUmlClassModel entity, string solutionName, string projectNamespace)
    {
        var entityName = entity.Name;
        var pluralName = GetPluralName(entityName);
        var searchableProperties = entity.Properties.Where(p => p.Type.ToLower() == "string" && !p.IsKey).ToList();

        var searchConditions = searchableProperties.Any()
            ? string.Join(" ||\n                    ", searchableProperties.Select(p => $"x.{p.Name}.Contains(request.SearchTerm)"))
            : "false";

        // Use type alias to avoid namespace collision
        return $@"// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;
using Microsoft.EntityFrameworkCore;
using {projectNamespace}.Data;
using {entityName}Entity = {projectNamespace}.Models.{entityName}.{entityName};
using {entityName}Ext = {projectNamespace}.Models.{entityName}.{entityName}Extensions;

namespace {projectNamespace}.Features.{entityName};

public class Get{pluralName}Handler : IRequestHandler<Get{pluralName}Request, Get{pluralName}Response>
{{
    private readonly I{solutionName}Context _context;

    public Get{pluralName}Handler(I{solutionName}Context context)
    {{
        _context = context;
    }}

    public async Task<Get{pluralName}Response> Handle(Get{pluralName}Request request, CancellationToken cancellationToken)
    {{
        var query = _context.{pluralName}.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {{
            query = query.Where(x =>
                {searchConditions});
        }}

        // Get total count before paging
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {{
            ""createdat"" => request.SortDescending ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            ""modifiedat"" => request.SortDescending ? query.OrderByDescending(x => x.ModifiedAt) : query.OrderBy(x => x.ModifiedAt),
            _ => query.OrderByDescending(x => x.CreatedAt)
        }};

        // Apply paging
        var items = await query
            .Skip(request.PageIndex * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new Get{pluralName}Response
        {{
            {pluralName} = items.Select(x => {entityName}Ext.ToDto(x)).ToList(),
            TotalCount = totalCount,
            PageIndex = request.PageIndex,
            PageSize = request.PageSize
        }};
    }}
}}
";
    }

    private void GenerateUpdateOperation(ProjectModel project, PlantUmlClassModel entity, string directory, string solutionName)
    {
        var entityName = entity.Name;
        var modelsNamespace = $"{project.Namespace}.Models.{entityName}";
        var keyProperty = GetKeyProperty(entity);
        var keyPropertyName = keyProperty?.Name ?? $"{entityName}Id";

        // Create Request
        var requestClass = new ClassModel($"Update{entityName}Request");
        requestClass.Usings.Add(new UsingModel("MediatR"));
        requestClass.Usings.Add(new UsingModel($"{project.Namespace}.Models"));
        requestClass.Implements.Add(new TypeModel($"IRequest<Update{entityName}Response>"));
        requestClass.Properties.Add(new PropertyModel(requestClass, AccessModifier.Public, new TypeModel("string"), keyPropertyName, PropertyAccessorModel.GetSet));
        foreach (var property in entity.Properties.Where(p => !p.IsKey && !IsAuditProperty(p.Name)))
        {
            requestClass.Properties.Add(CreatePropertyModel(requestClass, property, project.Namespace));
        }
        project.Files.Add(new CodeFileModel<ClassModel>(requestClass, requestClass.Usings, $"Update{entityName}Request", directory, ".cs"));

        // Create Response
        var responseClass = new ClassModel($"Update{entityName}Response");
        responseClass.Usings.Add(new UsingModel(modelsNamespace));
        responseClass.Properties.Add(new PropertyModel(responseClass, AccessModifier.Public, new TypeModel($"{entityName}Dto"), entityName, PropertyAccessorModel.GetSet));
        project.Files.Add(new CodeFileModel<ClassModel>(responseClass, responseClass.Usings, $"Update{entityName}Response", directory, ".cs"));

        // Create Handler with implementation
        var handlerContent = GenerateUpdateHandlerContent(entity, solutionName, project.Namespace, keyPropertyName);
        project.Files.Add(new Files.ContentFileModel(handlerContent, $"Update{entityName}Handler", directory, ".cs"));

        // Create Validator
        var validatorContent = GenerateUpdateValidatorContent(entity, project.Namespace, keyPropertyName);
        project.Files.Add(new Files.ContentFileModel(validatorContent, $"Update{entityName}Validator", directory, ".cs"));
    }

    private string GenerateUpdateHandlerContent(PlantUmlClassModel entity, string solutionName, string projectNamespace, string keyPropertyName)
    {
        var entityName = entity.Name;
        var entityNameCamel = ToCamelCase(entityName);
        var nonKeyProperties = entity.Properties.Where(p => !p.IsKey && !IsAuditProperty(p.Name)).ToList();

        var propertyUpdates = string.Join("\n        ", nonKeyProperties.Select(p => $"{entityNameCamel}.{p.Name} = request.{p.Name};"));

        // Use type alias to avoid namespace collision
        return $@"// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;
using Microsoft.EntityFrameworkCore;
using {projectNamespace}.Data;
using {entityName}Entity = {projectNamespace}.Models.{entityName}.{entityName};
using {entityName}Ext = {projectNamespace}.Models.{entityName}.{entityName}Extensions;

namespace {projectNamespace}.Features.{entityName};

public class Update{entityName}Handler : IRequestHandler<Update{entityName}Request, Update{entityName}Response>
{{
    private readonly I{solutionName}Context _context;

    public Update{entityName}Handler(I{solutionName}Context context)
    {{
        _context = context;
    }}

    public async Task<Update{entityName}Response> Handle(Update{entityName}Request request, CancellationToken cancellationToken)
    {{
        var {entityNameCamel} = await _context.{GetPluralName(entityName)}
            .FirstOrDefaultAsync(x => x.{keyPropertyName} == request.{keyPropertyName}, cancellationToken);

        if ({entityNameCamel} == null)
        {{
            throw new KeyNotFoundException($""{entityName} with ID '{{request.{keyPropertyName}}}' was not found."");
        }}

        // Update all properties
        {propertyUpdates}
        {entityNameCamel}.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new Update{entityName}Response
        {{
            {entityName} = {entityName}Ext.ToDto({entityNameCamel})
        }};
    }}
}}
";
    }

    private string GenerateUpdateValidatorContent(PlantUmlClassModel entity, string projectNamespace, string keyPropertyName)
    {
        var entityName = entity.Name;
        var requiredProperties = entity.Properties.Where(p => !p.IsKey && !IsAuditProperty(p.Name) && p.IsRequired && p.Type.ToLower() == "string").ToList();

        var rules = new List<string> { $"RuleFor(x => x.{keyPropertyName}).NotEmpty();" };
        rules.AddRange(requiredProperties.Select(p => $"RuleFor(x => x.{p.Name}).NotEmpty();"));
        var rulesStr = string.Join("\n        ", rules);

        return $@"// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using FluentValidation;

namespace {projectNamespace}.Features.{entityName};

public class Update{entityName}Validator : AbstractValidator<Update{entityName}Request>
{{
    public Update{entityName}Validator()
    {{
        {rulesStr}
    }}
}}
";
    }

    private void GenerateDeleteOperation(ProjectModel project, PlantUmlClassModel entity, string directory, string solutionName)
    {
        var entityName = entity.Name;
        var keyProperty = GetKeyProperty(entity);
        var keyPropertyName = keyProperty?.Name ?? $"{entityName}Id";

        // Create Request
        var requestClass = new ClassModel($"Delete{entityName}Request");
        requestClass.Usings.Add(new UsingModel("MediatR"));
        requestClass.Implements.Add(new TypeModel($"IRequest<Delete{entityName}Response>"));
        requestClass.Properties.Add(new PropertyModel(requestClass, AccessModifier.Public, new TypeModel("string"), keyPropertyName, PropertyAccessorModel.GetSet));
        project.Files.Add(new CodeFileModel<ClassModel>(requestClass, requestClass.Usings, $"Delete{entityName}Request", directory, ".cs"));

        // Create Response
        var responseClass = new ClassModel($"Delete{entityName}Response");
        responseClass.Properties.Add(new PropertyModel(responseClass, AccessModifier.Public, new TypeModel("bool"), "Success", PropertyAccessorModel.GetSet));
        responseClass.Properties.Add(new PropertyModel(responseClass, AccessModifier.Public, new TypeModel("string"), "Message", PropertyAccessorModel.GetSet));
        project.Files.Add(new CodeFileModel<ClassModel>(responseClass, responseClass.Usings, $"Delete{entityName}Response", directory, ".cs"));

        // Create Handler with implementation
        var handlerContent = GenerateDeleteHandlerContent(entity, solutionName, project.Namespace, keyPropertyName);
        project.Files.Add(new Files.ContentFileModel(handlerContent, $"Delete{entityName}Handler", directory, ".cs"));
    }

    private string GenerateDeleteHandlerContent(PlantUmlClassModel entity, string solutionName, string projectNamespace, string keyPropertyName)
    {
        var entityName = entity.Name;
        var entityNameCamel = ToCamelCase(entityName);

        // Use type alias to avoid namespace collision
        return $@"// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;
using Microsoft.EntityFrameworkCore;
using {projectNamespace}.Data;
using {entityName}Entity = {projectNamespace}.Models.{entityName}.{entityName};

namespace {projectNamespace}.Features.{entityName};

public class Delete{entityName}Handler : IRequestHandler<Delete{entityName}Request, Delete{entityName}Response>
{{
    private readonly I{solutionName}Context _context;

    public Delete{entityName}Handler(I{solutionName}Context context)
    {{
        _context = context;
    }}

    public async Task<Delete{entityName}Response> Handle(Delete{entityName}Request request, CancellationToken cancellationToken)
    {{
        var {entityNameCamel} = await _context.{GetPluralName(entityName)}
            .FirstOrDefaultAsync(x => x.{keyPropertyName} == request.{keyPropertyName}, cancellationToken);

        if ({entityNameCamel} == null)
        {{
            return new Delete{entityName}Response
            {{
                Success = false,
                Message = $""{entityName} with ID '{{request.{keyPropertyName}}}' was not found.""
            }};
        }}

        _context.{GetPluralName(entityName)}.Remove({entityNameCamel});
        await _context.SaveChangesAsync(cancellationToken);

        return new Delete{entityName}Response
        {{
            Success = true,
            Message = $""{entityName} with ID '{{request.{keyPropertyName}}}' was successfully deleted.""
        }};
    }}
}}
";
    }

    private void GenerateEnumFile(ProjectModel project, PlantUmlEnumModel plantUmlEnum)
    {
        var enumDirectory = Path.Combine(project.Directory, "Models");
        var content = GenerateEnumContent(plantUmlEnum, project.Namespace);
        project.Files.Add(new Files.ContentFileModel(content, plantUmlEnum.Name, enumDirectory, ".cs"));
    }

    private string GenerateEnumContent(PlantUmlEnumModel enumModel, string projectNamespace)
    {
        var values = string.Join(",\n    ", enumModel.Values);
        return $@"// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace {projectNamespace}.Models;

public enum {enumModel.Name}
{{
    {values}
}}
";
    }

    private void GenerateDbContextFile(ProjectModel project, PlantUmlSolutionModel solutionModel, string solutionName)
    {
        var dbContextDirectory = Path.Combine(project.Directory, "Data");
        var entities = solutionModel.GetEntities().ToList();
        var content = GenerateDbContextContent(entities, solutionName, project.Namespace);
        project.Files.Add(new Files.ContentFileModel(content, $"{solutionName}DbContext", dbContextDirectory, ".cs"));
        // Note: IContext interface is now generated in Core project
    }

    private string GenerateDbContextContent(List<PlantUmlClassModel> entities, string solutionName, string projectNamespace)
    {
        var dbSets = string.Join("\n    ", entities.Select(e => $"public DbSet<{e.Name}> {GetPluralName(e.Name)} {{ get; set; }}"));
        var usings = string.Join("\n", entities.Select(e => $"using {solutionName}.Core.Models.{e.Name};"));

        return $@"// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using {solutionName}.Core.Data;
{usings}

namespace {projectNamespace}.Data;

public class {solutionName}DbContext : DbContext, I{solutionName}Context
{{
    public {solutionName}DbContext(DbContextOptions<{solutionName}DbContext> options)
        : base(options)
    {{
    }}

    {dbSets}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {{
        base.OnModelCreating(modelBuilder);
    }}
}}
";
    }

    private void GenerateControllerFile(ProjectModel project, PlantUmlClassModel aggregate, string solutionName)
    {
        var entityName = aggregate.Name;
        var pluralName = GetPluralName(entityName);
        var controllerDirectory = Path.Combine(project.Directory, "Controllers");
        var content = GenerateControllerContent(aggregate, solutionName, project.Namespace);
        project.Files.Add(new Files.ContentFileModel(content, $"{entityName}Controller", controllerDirectory, ".cs"));
    }

    private string GenerateControllerContent(PlantUmlClassModel entity, string solutionName, string projectNamespace)
    {
        var entityName = entity.Name;
        var entityNameLower = char.ToLowerInvariant(entityName[0]) + entityName[1..];
        var pluralName = GetPluralName(entityName);

        return $@"// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;
using Microsoft.AspNetCore.Mvc;
using {solutionName}.Core.Features.{entityName};

namespace {projectNamespace}.Controllers;

[ApiController]
[Route(""api/[controller]"")]
public class {entityName}Controller : ControllerBase
{{
    private readonly IMediator _mediator;

    public {entityName}Controller(IMediator mediator)
    {{
        _mediator = mediator;
    }}

    [HttpGet]
    [ProducesResponseType(typeof(Get{pluralName}Response), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {{
        var request = new Get{pluralName}Request
        {{
            PageIndex = pageIndex,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            SortBy = sortBy,
            SortDescending = sortDescending
        }};

        var response = await _mediator.Send(request);
        return Ok(response);
    }}

    [HttpGet(""{{id}}"")]
    [ProducesResponseType(typeof(Get{entityName}ByIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {{
        var request = new Get{entityName}ByIdRequest {{ {entityName}Id = id }};
        var response = await _mediator.Send(request);
        return Ok(response);
    }}

    [HttpPost]
    [ProducesResponseType(typeof(Create{entityName}Response), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] Create{entityName}Request request)
    {{
        var response = await _mediator.Send(request);
        return CreatedAtAction(
            nameof(GetById),
            new {{ id = response.{entityName}.{entityName}Id }},
            response);
    }}

    [HttpPut(""{{id}}"")]
    [ProducesResponseType(typeof(Update{entityName}Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(string id, [FromBody] Update{entityName}Request request)
    {{
        request.{entityName}Id = id;
        var response = await _mediator.Send(request);
        return Ok(response);
    }}

    [HttpDelete(""{{id}}"")]
    [ProducesResponseType(typeof(Delete{entityName}Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {{
        var request = new Delete{entityName}Request {{ {entityName}Id = id }};
        var response = await _mediator.Send(request);
        return Ok(response);
    }}
}}
";
    }

    private void GenerateProgramFile(ProjectModel project, string solutionName)
    {
        var content = GenerateProgramContent(solutionName, project.Namespace);
        project.Files.Add(new Files.ContentFileModel(content, "Program", project.Directory, ".cs"));
    }

    private string GenerateProgramContent(string solutionName, string projectNamespace)
    {
        return $@"// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using {solutionName}.Core.Data;
using {solutionName}.Infrastructure.Data;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MediatR - register from Core project
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<I{solutionName}Context>());

// FluentValidation - register from Core project
builder.Services.AddValidatorsFromAssemblyContaining<I{solutionName}Context>();

// Database
builder.Services.AddDbContext<{solutionName}DbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString(""DefaultConnection"")));

builder.Services.AddScoped<I{solutionName}Context>(provider => provider.GetRequiredService<{solutionName}DbContext>());

// CORS
builder.Services.AddCors(options =>
{{
    options.AddPolicy(""AllowAll"", policy =>
    {{
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    }});
}});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{{
    app.UseSwagger();
    app.UseSwaggerUI();
}}

app.UseHttpsRedirection();
app.UseCors(""AllowAll"");
app.UseAuthorization();
app.MapControllers();

app.Run();
";
    }

    private static AccessModifier ConvertVisibility(PlantUmlVisibility visibility)
    {
        return visibility switch
        {
            PlantUmlVisibility.Public => AccessModifier.Public,
            PlantUmlVisibility.Private => AccessModifier.Private,
            PlantUmlVisibility.Protected => AccessModifier.Protected,
            PlantUmlVisibility.Package => AccessModifier.Internal,
            _ => AccessModifier.Public
        };
    }

    private static string MapTypeName(string plantUmlType)
    {
        return plantUmlType?.ToLower() switch
        {
            "datetime" => "DateTime",
            "datetimeoffset" => "DateTimeOffset",
            "guid" => "Guid",
            "int" => "int",
            "long" => "long",
            "decimal" => "decimal",
            "double" => "double",
            "bool" => "bool",
            "string" => "string",
            _ => plantUmlType
        };
    }

    private static bool IsAuditProperty(string propertyName)
    {
        var auditProperties = new[] { "CreatedAt", "ModifiedAt", "CreatedBy", "ModifiedBy" };
        return auditProperties.Any(p => p.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
    }

    private static string GetPluralName(string name)
    {
        if (name.EndsWith("y"))
        {
            return name[..^1] + "ies";
        }

        if (name.EndsWith("s") || name.EndsWith("x") || name.EndsWith("ch") || name.EndsWith("sh"))
        {
            return name + "es";
        }

        return name + "s";
    }
}
