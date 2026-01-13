// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.DotNet.Artifacts.Files;
using Endpoint.DotNet.Artifacts.Projects;
using Endpoint.DotNet.Syntax;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Constructors;
using Endpoint.DotNet.Syntax.Expressions;
using Endpoint.DotNet.Syntax.Fields;
using Endpoint.DotNet.Syntax.Interfaces;
using Endpoint.DotNet.Syntax.Methods;
using Endpoint.DotNet.Syntax.Params;
using Endpoint.DotNet.Syntax.Properties;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.RateLimiting;

using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

public class RateLimitingArtifactFactory : IRateLimitingArtifactFactory
{
    private readonly ILogger<RateLimitingArtifactFactory> logger;

    public RateLimitingArtifactFactory(ILogger<RateLimitingArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding RateLimiting.Core files");
        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");

        // Entities
        project.Files.Add(CreateRateLimitRuleFile(entitiesDir));
        project.Files.Add(CreateQuotaUsageFile(entitiesDir));

        // Interfaces
        project.Files.Add(CreateIRateLimitRepositoryFile(interfacesDir));
        project.Files.Add(CreateIRateLimitServiceFile(interfacesDir));

        // Events
        project.Files.Add(CreateRateLimitExceededEventFile(eventsDir));
        project.Files.Add(CreateQuotaResetEventFile(eventsDir));

        // DTOs
        project.Files.Add(CreateRateLimitStatusDtoFile(dtosDir));
        project.Files.Add(CreateQuotaDtoFile(dtosDir));
        project.Files.Add(CreateCreateRateLimitRuleRequestFile(dtosDir));
        project.Files.Add(CreateRateLimitRuleDtoFile(dtosDir));
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding RateLimiting.Infrastructure files");
        var dataDir = Path.Combine(project.Directory, "Data");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");

        project.Files.Add(CreateRateLimitingDbContextFile(dataDir));
        project.Files.Add(CreateRateLimitRepositoryFile(repositoriesDir));
        project.Files.Add(CreateRateLimitServiceFile(servicesDir));
        project.Files.Add(CreateInfrastructureConfigureServicesFile(project.Directory));
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding RateLimiting.Api files");
        var controllersDir = Path.Combine(project.Directory, "Controllers");

        project.Files.Add(CreateRateLimitsControllerFile(controllersDir));
        project.Files.Add(CreateRateLimitRulesControllerFile(controllersDir));
        project.Files.Add(CreateProgramFile(project.Directory));
        project.Files.Add(CreateAppSettingsFile(project.Directory));
    }

    #region Core Layer Files

    private static CodeFileModel<ClassModel> CreateRateLimitRuleFile(string directory)
    {
        var classModel = new ClassModel("RateLimitRule");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "RateLimitRuleId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Description", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Endpoint", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "RequestLimit", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "TimeWindowSeconds", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("RateLimitScope"), "Scope", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "RateLimitScope.PerUser" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsEnabled", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "true" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "UpdatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));

        return new CodeFileModel<ClassModel>(classModel, "RateLimitRule", directory, CSharp)
        {
            Namespace = "RateLimiting.Core.Entities"
        };
    }

    private static CodeFileModel<ClassModel> CreateQuotaUsageFile(string directory)
    {
        var classModel = new ClassModel("QuotaUsage");

        classModel.Usings.Add(new UsingModel("RateLimiting.Core.Entities"));

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "QuotaUsageId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "RateLimitRuleId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("RateLimitRule"), "RateLimitRule", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "null!" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Identifier", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "RequestCount", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "WindowStart", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "WindowEnd", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "LastRequestAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "QuotaUsage", directory, CSharp)
        {
            Namespace = "RateLimiting.Core.Entities"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateIRateLimitRepositoryFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IRateLimitRepository");

        interfaceModel.Usings.Add(new UsingModel("RateLimiting.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("RateLimitRule") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "ruleId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByEndpointAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("RateLimitRule") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "endpoint", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetAllAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("RateLimitRule")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetEnabledRulesAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("RateLimitRule")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("RateLimitRule")] },
            Params =
            [
                new ParamModel { Name = "rule", Type = new TypeModel("RateLimitRule") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "UpdateAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "rule", Type = new TypeModel("RateLimitRule") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "DeleteAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "ruleId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetQuotaUsageAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("QuotaUsage") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "ruleId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "identifier", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "AddOrUpdateQuotaUsageAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("QuotaUsage")] },
            Params =
            [
                new ParamModel { Name = "usage", Type = new TypeModel("QuotaUsage") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IRateLimitRepository", directory, CSharp)
        {
            Namespace = "RateLimiting.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateIRateLimitServiceFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IRateLimitService");

        interfaceModel.Usings.Add(new UsingModel("RateLimiting.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "CheckRateLimitAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("RateLimitResult")] },
            Params =
            [
                new ParamModel { Name = "endpoint", Type = new TypeModel("string") },
                new ParamModel { Name = "identifier", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "IsRateLimitedAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("bool")] },
            Params =
            [
                new ParamModel { Name = "endpoint", Type = new TypeModel("string") },
                new ParamModel { Name = "identifier", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "IncrementRequestCountAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "endpoint", Type = new TypeModel("string") },
                new ParamModel { Name = "identifier", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "ResetQuotaAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "ruleId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "identifier", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetStatusAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("RateLimitStatus")] },
            Params =
            [
                new ParamModel { Name = "endpoint", Type = new TypeModel("string") },
                new ParamModel { Name = "identifier", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IRateLimitService", directory, CSharp)
        {
            Namespace = "RateLimiting.Core.Interfaces"
        };
    }

    private static CodeFileModel<ClassModel> CreateRateLimitExceededEventFile(string directory)
    {
        var classModel = new ClassModel("RateLimitExceededEvent")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "RateLimitRuleId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Endpoint", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Identifier", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "RequestCount", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "RequestLimit", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "WindowStart", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "WindowEnd", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "RateLimitExceededEvent", directory, CSharp)
        {
            Namespace = "RateLimiting.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreateQuotaResetEventFile(string directory)
    {
        var classModel = new ClassModel("QuotaResetEvent")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "RateLimitRuleId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Identifier", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "PreviousRequestCount", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "PreviousWindowEnd", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "NewWindowStart", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "QuotaResetEvent", directory, CSharp)
        {
            Namespace = "RateLimiting.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreateRateLimitStatusDtoFile(string directory)
    {
        var classModel = new ClassModel("RateLimitStatusDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Endpoint", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Identifier", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "CurrentCount", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "Limit", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "Remaining", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "ResetAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsLimited", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));

        return new CodeFileModel<ClassModel>(classModel, "RateLimitStatusDto", directory, CSharp)
        {
            Namespace = "RateLimiting.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateQuotaDtoFile(string directory)
    {
        var classModel = new ClassModel("QuotaDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "RuleId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "RuleName", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Endpoint", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Identifier", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "Used", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "Limit", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "Remaining", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "UsagePercentage", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "WindowStart", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "WindowEnd", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));

        return new CodeFileModel<ClassModel>(classModel, "QuotaDto", directory, CSharp)
        {
            Namespace = "RateLimiting.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateCreateRateLimitRuleRequestFile(string directory)
    {
        var classModel = new ClassModel("CreateRateLimitRuleRequest")
        {
            Sealed = true
        };

        classModel.Usings.Add(new UsingModel("System.ComponentModel.DataAnnotations"));

        var nameProperty = new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)], required: true);
        nameProperty.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Required" });
        nameProperty.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "StringLength", Template = "100" });
        classModel.Properties.Add(nameProperty);

        var descriptionProperty = new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Description", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]);
        descriptionProperty.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "StringLength", Template = "500" });
        classModel.Properties.Add(descriptionProperty);

        var endpointProperty = new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Endpoint", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)], required: true);
        endpointProperty.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Required" });
        endpointProperty.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "StringLength", Template = "500" });
        classModel.Properties.Add(endpointProperty);

        var requestLimitProperty = new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "RequestLimit", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]);
        requestLimitProperty.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Required" });
        requestLimitProperty.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Range", Template = "1, int.MaxValue, ErrorMessage = \"Request limit must be at least 1\"" });
        classModel.Properties.Add(requestLimitProperty);

        var timeWindowProperty = new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "TimeWindowSeconds", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]);
        timeWindowProperty.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Required" });
        timeWindowProperty.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Range", Template = "1, 86400, ErrorMessage = \"Time window must be between 1 and 86400 seconds\"" });
        classModel.Properties.Add(timeWindowProperty);

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Scope", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]) { DefaultValue = "\"PerUser\"" });

        return new CodeFileModel<ClassModel>(classModel, "CreateRateLimitRuleRequest", directory, CSharp)
        {
            Namespace = "RateLimiting.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateRateLimitRuleDtoFile(string directory)
    {
        var classModel = new ClassModel("RateLimitRuleDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "RateLimitRuleId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Description", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Endpoint", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "RequestLimit", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "TimeWindowSeconds", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Scope", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]) { DefaultValue = "\"PerUser\"" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsEnabled", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));

        return new CodeFileModel<ClassModel>(classModel, "RateLimitRuleDto", directory, CSharp)
        {
            Namespace = "RateLimiting.Core.DTOs"
        };
    }

    #endregion

    #region Infrastructure Layer Files

    private static CodeFileModel<ClassModel> CreateRateLimitingDbContextFile(string directory)
    {
        var classModel = new ClassModel("RateLimitingDbContext");

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("RateLimiting.Core.Entities"));

        classModel.Implements.Add(new TypeModel("DbContext"));

        var constructor = new ConstructorModel(classModel, "RateLimitingDbContext")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "options", Type = new TypeModel("DbContextOptions") { GenericTypeParameters = [new TypeModel("RateLimitingDbContext")] } }],
            BaseParams = ["options"]
        };
        classModel.Constructors.Add(constructor);

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("RateLimitRule")] }, "RateLimitRules", [new PropertyAccessorModel(PropertyAccessorType.Get, "Set<RateLimitRule>()")]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("QuotaUsage")] }, "QuotaUsages", [new PropertyAccessorModel(PropertyAccessorType.Get, "Set<QuotaUsage>()")]));

        classModel.Methods.Add(new MethodModel
        {
            Name = "OnModelCreating",
            AccessModifier = AccessModifier.Protected,
            Override = true,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "modelBuilder", Type = new TypeModel("ModelBuilder") }],
            Body = new ExpressionModel(@"modelBuilder.Entity<RateLimitRule>(entity =>
        {
            entity.HasKey(r => r.RateLimitRuleId);
            entity.Property(r => r.Name).IsRequired().HasMaxLength(100);
            entity.Property(r => r.Endpoint).IsRequired().HasMaxLength(500);
            entity.Property(r => r.Description).HasMaxLength(500);
            entity.HasIndex(r => r.Endpoint);
            entity.HasIndex(r => r.IsEnabled);
        });

        modelBuilder.Entity<QuotaUsage>(entity =>
        {
            entity.HasKey(q => q.QuotaUsageId);
            entity.Property(q => q.Identifier).IsRequired().HasMaxLength(500);
            entity.HasOne(q => q.RateLimitRule).WithMany().HasForeignKey(q => q.RateLimitRuleId);
            entity.HasIndex(q => new { q.RateLimitRuleId, q.Identifier }).IsUnique();
            entity.HasIndex(q => q.WindowEnd);
        });")
        });

        return new CodeFileModel<ClassModel>(classModel, "RateLimitingDbContext", directory, CSharp)
        {
            Namespace = "RateLimiting.Infrastructure.Data"
        };
    }

    private static CodeFileModel<ClassModel> CreateRateLimitRepositoryFile(string directory)
    {
        var classModel = new ClassModel("RateLimitRepository");

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("RateLimiting.Core.Entities"));
        classModel.Usings.Add(new UsingModel("RateLimiting.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("RateLimiting.Infrastructure.Data"));

        classModel.Implements.Add(new TypeModel("IRateLimitRepository"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "context",
            Type = new TypeModel("RateLimitingDbContext"),
            AccessModifier = AccessModifier.Private,
            Readonly = true
        });

        var constructor = new ConstructorModel(classModel, "RateLimitRepository")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "context", Type = new TypeModel("RateLimitingDbContext") }],
            Body = new ExpressionModel("this.context = context;")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("RateLimitRule") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "ruleId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.RateLimitRules.FirstOrDefaultAsync(r => r.RateLimitRuleId == ruleId, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByEndpointAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("RateLimitRule") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "endpoint", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.RateLimitRules.FirstOrDefaultAsync(r => r.Endpoint == endpoint && r.IsEnabled, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetAllAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("RateLimitRule")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.RateLimitRules.ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetEnabledRulesAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("RateLimitRule")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.RateLimitRules.Where(r => r.IsEnabled).ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("RateLimitRule")] },
            Params =
            [
                new ParamModel { Name = "rule", Type = new TypeModel("RateLimitRule") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"rule.RateLimitRuleId = Guid.NewGuid();
        await context.RateLimitRules.AddAsync(rule, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return rule;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "UpdateAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "rule", Type = new TypeModel("RateLimitRule") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"rule.UpdatedAt = DateTime.UtcNow;
        context.RateLimitRules.Update(rule);
        await context.SaveChangesAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "DeleteAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "ruleId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var rule = await context.RateLimitRules.FindAsync(new object[] { ruleId }, cancellationToken);
        if (rule != null)
        {
            context.RateLimitRules.Remove(rule);
            await context.SaveChangesAsync(cancellationToken);
        }")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetQuotaUsageAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("QuotaUsage") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "ruleId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "identifier", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.QuotaUsages
            .Include(q => q.RateLimitRule)
            .FirstOrDefaultAsync(q => q.RateLimitRuleId == ruleId && q.Identifier == identifier, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddOrUpdateQuotaUsageAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("QuotaUsage")] },
            Params =
            [
                new ParamModel { Name = "usage", Type = new TypeModel("QuotaUsage") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var existing = await GetQuotaUsageAsync(usage.RateLimitRuleId, usage.Identifier, cancellationToken);
        if (existing == null)
        {
            usage.QuotaUsageId = Guid.NewGuid();
            await context.QuotaUsages.AddAsync(usage, cancellationToken);
        }
        else
        {
            existing.RequestCount = usage.RequestCount;
            existing.WindowStart = usage.WindowStart;
            existing.WindowEnd = usage.WindowEnd;
            existing.LastRequestAt = usage.LastRequestAt;
            context.QuotaUsages.Update(existing);
        }
        await context.SaveChangesAsync(cancellationToken);
        return existing ?? usage;")
        });

        return new CodeFileModel<ClassModel>(classModel, "RateLimitRepository", directory, CSharp)
        {
            Namespace = "RateLimiting.Infrastructure.Repositories"
        };
    }

    private static CodeFileModel<ClassModel> CreateRateLimitServiceFile(string directory)
    {
        var classModel = new ClassModel("RateLimitService");

        classModel.Usings.Add(new UsingModel("RateLimiting.Core.Entities"));
        classModel.Usings.Add(new UsingModel("RateLimiting.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("IRateLimitService"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "repository",
            Type = new TypeModel("IRateLimitRepository"),
            AccessModifier = AccessModifier.Private,
            Readonly = true
        });

        var constructor = new ConstructorModel(classModel, "RateLimitService")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "repository", Type = new TypeModel("IRateLimitRepository") }],
            Body = new ExpressionModel("this.repository = repository;")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "CheckRateLimitAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("RateLimitResult")] },
            Params =
            [
                new ParamModel { Name = "endpoint", Type = new TypeModel("string") },
                new ParamModel { Name = "identifier", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var rule = await repository.GetByEndpointAsync(endpoint, cancellationToken);
        if (rule == null)
        {
            return new RateLimitResult { IsAllowed = true, RemainingRequests = int.MaxValue, RequestLimit = int.MaxValue };
        }

        var usage = await repository.GetQuotaUsageAsync(rule.RateLimitRuleId, identifier, cancellationToken);
        var now = DateTime.UtcNow;

        if (usage == null || usage.WindowEnd < now)
        {
            return new RateLimitResult
            {
                IsAllowed = true,
                RemainingRequests = rule.RequestLimit - 1,
                RequestLimit = rule.RequestLimit,
                ResetTime = now.AddSeconds(rule.TimeWindowSeconds)
            };
        }

        var remaining = rule.RequestLimit - usage.RequestCount;
        var isAllowed = remaining > 0;

        return new RateLimitResult
        {
            IsAllowed = isAllowed,
            RemainingRequests = Math.Max(0, remaining - 1),
            RequestLimit = rule.RequestLimit,
            ResetTime = usage.WindowEnd,
            RetryAfterSeconds = isAllowed ? null : ((int)(usage.WindowEnd - now).TotalSeconds).ToString()
        };")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "IsRateLimitedAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("bool")] },
            Params =
            [
                new ParamModel { Name = "endpoint", Type = new TypeModel("string") },
                new ParamModel { Name = "identifier", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var result = await CheckRateLimitAsync(endpoint, identifier, cancellationToken);
        return !result.IsAllowed;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "IncrementRequestCountAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "endpoint", Type = new TypeModel("string") },
                new ParamModel { Name = "identifier", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var rule = await repository.GetByEndpointAsync(endpoint, cancellationToken);
        if (rule == null) return;

        var now = DateTime.UtcNow;
        var usage = await repository.GetQuotaUsageAsync(rule.RateLimitRuleId, identifier, cancellationToken);

        if (usage == null || usage.WindowEnd < now)
        {
            usage = new QuotaUsage
            {
                RateLimitRuleId = rule.RateLimitRuleId,
                Identifier = identifier,
                RequestCount = 1,
                WindowStart = now,
                WindowEnd = now.AddSeconds(rule.TimeWindowSeconds),
                LastRequestAt = now
            };
        }
        else
        {
            usage.RequestCount++;
            usage.LastRequestAt = now;
        }

        await repository.AddOrUpdateQuotaUsageAsync(usage, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "ResetQuotaAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "ruleId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "identifier", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var rule = await repository.GetByIdAsync(ruleId, cancellationToken);
        if (rule == null) return;

        var now = DateTime.UtcNow;
        var usage = new QuotaUsage
        {
            RateLimitRuleId = ruleId,
            Identifier = identifier,
            RequestCount = 0,
            WindowStart = now,
            WindowEnd = now.AddSeconds(rule.TimeWindowSeconds),
            LastRequestAt = now
        };

        await repository.AddOrUpdateQuotaUsageAsync(usage, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetStatusAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("RateLimitStatus")] },
            Params =
            [
                new ParamModel { Name = "endpoint", Type = new TypeModel("string") },
                new ParamModel { Name = "identifier", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var rule = await repository.GetByEndpointAsync(endpoint, cancellationToken);
        if (rule == null)
        {
            return new RateLimitStatus
            {
                Endpoint = endpoint,
                Identifier = identifier,
                CurrentCount = 0,
                Limit = int.MaxValue,
                Remaining = int.MaxValue,
                WindowStart = DateTime.UtcNow,
                WindowEnd = DateTime.MaxValue
            };
        }

        var usage = await repository.GetQuotaUsageAsync(rule.RateLimitRuleId, identifier, cancellationToken);
        var now = DateTime.UtcNow;

        if (usage == null || usage.WindowEnd < now)
        {
            return new RateLimitStatus
            {
                Endpoint = endpoint,
                Identifier = identifier,
                CurrentCount = 0,
                Limit = rule.RequestLimit,
                Remaining = rule.RequestLimit,
                WindowStart = now,
                WindowEnd = now.AddSeconds(rule.TimeWindowSeconds)
            };
        }

        return new RateLimitStatus
        {
            Endpoint = endpoint,
            Identifier = identifier,
            CurrentCount = usage.RequestCount,
            Limit = rule.RequestLimit,
            Remaining = Math.Max(0, rule.RequestLimit - usage.RequestCount),
            WindowStart = usage.WindowStart,
            WindowEnd = usage.WindowEnd
        };")
        });

        return new CodeFileModel<ClassModel>(classModel, "RateLimitService", directory, CSharp)
        {
            Namespace = "RateLimiting.Infrastructure.Services"
        };
    }

    private static CodeFileModel<ClassModel> CreateInfrastructureConfigureServicesFile(string directory)
    {
        var classModel = new ClassModel("ConfigureServices")
        {
            Static = true
        };

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.Configuration"));
        classModel.Usings.Add(new UsingModel("RateLimiting.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("RateLimiting.Infrastructure.Data"));
        classModel.Usings.Add(new UsingModel("RateLimiting.Infrastructure.Repositories"));
        classModel.Usings.Add(new UsingModel("RateLimiting.Infrastructure.Services"));

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddRateLimitingInfrastructure",
            AccessModifier = AccessModifier.Public,
            Static = true,
            ReturnType = new TypeModel("IServiceCollection"),
            Params =
            [
                new ParamModel { Name = "services", Type = new TypeModel("IServiceCollection"), ExtensionMethodParam = true },
                new ParamModel { Name = "configuration", Type = new TypeModel("IConfiguration") }
            ],
            Body = new ExpressionModel(@"services.AddDbContext<RateLimitingDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString(""RateLimitingDb"") ??
                @""Server=.\SQLEXPRESS;Database=RateLimitingDb;Trusted_Connection=True;TrustServerCertificate=True""));

        services.AddScoped<IRateLimitRepository, RateLimitRepository>();
        services.AddScoped<IRateLimitService, RateLimitService>();
        return services;")
        });

        return new CodeFileModel<ClassModel>(classModel, "ConfigureServices", directory, CSharp)
        {
            Namespace = "Microsoft.Extensions.DependencyInjection"
        };
    }

    #endregion

    #region API Layer Files

    private static CodeFileModel<ClassModel> CreateRateLimitsControllerFile(string directory)
    {
        var classModel = new ClassModel("RateLimitsController");

        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.Mvc"));
        classModel.Usings.Add(new UsingModel("RateLimiting.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("RateLimiting.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("ControllerBase"));

        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ApiController" });
        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Route", Template = "\"api/ratelimits\"" });

        classModel.Fields.Add(new FieldModel { Name = "rateLimitService", Type = new TypeModel("IRateLimitService"), AccessModifier = AccessModifier.Private, Readonly = true });
        classModel.Fields.Add(new FieldModel { Name = "repository", Type = new TypeModel("IRateLimitRepository"), AccessModifier = AccessModifier.Private, Readonly = true });

        var constructor = new ConstructorModel(classModel, "RateLimitsController")
        {
            AccessModifier = AccessModifier.Public,
            Params =
            [
                new ParamModel { Name = "rateLimitService", Type = new TypeModel("IRateLimitService") },
                new ParamModel { Name = "repository", Type = new TypeModel("IRateLimitRepository") }
            ],
            Body = new ExpressionModel(@"this.rateLimitService = rateLimitService;
        this.repository = repository;")
        };
        classModel.Constructors.Add(constructor);

        var getStatusMethod = new MethodModel
        {
            Name = "GetStatus",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("RateLimitStatusDto")] }] },
            Params =
            [
                new ParamModel { Name = "endpoint", Type = new TypeModel("string"), Attribute = "[FromQuery]" },
                new ParamModel { Name = "identifier", Type = new TypeModel("string"), Attribute = "[FromQuery]" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(identifier))
        {
            return BadRequest(""Endpoint and identifier are required"");
        }

        var status = await rateLimitService.GetStatusAsync(endpoint, identifier, cancellationToken);
        return Ok(new RateLimitStatusDto
        {
            Endpoint = status.Endpoint,
            Identifier = status.Identifier,
            CurrentCount = status.CurrentCount,
            Limit = status.Limit,
            Remaining = status.Remaining,
            ResetAt = status.WindowEnd,
            IsLimited = status.Remaining <= 0
        });")
        };
        getStatusMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"status\"" });
        classModel.Methods.Add(getStatusMethod);

        var getQuotasMethod = new MethodModel
        {
            Name = "GetQuotas",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("QuotaDto")] }] }] },
            Params =
            [
                new ParamModel { Name = "identifier", Type = new TypeModel("string") { Nullable = true }, Attribute = "[FromQuery]" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var rules = await repository.GetEnabledRulesAsync(cancellationToken);
        var quotas = new List<QuotaDto>();

        foreach (var rule in rules)
        {
            var currentIdentifier = identifier ?? ""anonymous"";
            var usage = await repository.GetQuotaUsageAsync(rule.RateLimitRuleId, currentIdentifier, cancellationToken);
            var now = DateTime.UtcNow;

            var used = (usage != null && usage.WindowEnd >= now) ? usage.RequestCount : 0;
            var remaining = rule.RequestLimit - used;

            quotas.Add(new QuotaDto
            {
                RuleId = rule.RateLimitRuleId,
                RuleName = rule.Name,
                Endpoint = rule.Endpoint,
                Identifier = currentIdentifier,
                Used = used,
                Limit = rule.RequestLimit,
                Remaining = Math.Max(0, remaining),
                UsagePercentage = rule.RequestLimit > 0 ? (double)used / rule.RequestLimit * 100 : 0,
                WindowStart = usage?.WindowStart ?? now,
                WindowEnd = usage?.WindowEnd ?? now.AddSeconds(rule.TimeWindowSeconds)
            });
        }

        return Ok(quotas);")
        };
        getQuotasMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"quotas\"" });
        classModel.Methods.Add(getQuotasMethod);

        return new CodeFileModel<ClassModel>(classModel, "RateLimitsController", directory, CSharp)
        {
            Namespace = "RateLimiting.Api.Controllers"
        };
    }

    private static CodeFileModel<ClassModel> CreateRateLimitRulesControllerFile(string directory)
    {
        var classModel = new ClassModel("RateLimitRulesController");

        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.Mvc"));
        classModel.Usings.Add(new UsingModel("RateLimiting.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("RateLimiting.Core.Entities"));
        classModel.Usings.Add(new UsingModel("RateLimiting.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("ControllerBase"));

        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ApiController" });
        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Route", Template = "\"api/ratelimit-rules\"" });

        classModel.Fields.Add(new FieldModel { Name = "repository", Type = new TypeModel("IRateLimitRepository"), AccessModifier = AccessModifier.Private, Readonly = true });

        var constructor = new ConstructorModel(classModel, "RateLimitRulesController")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "repository", Type = new TypeModel("IRateLimitRepository") }],
            Body = new ExpressionModel("this.repository = repository;")
        };
        classModel.Constructors.Add(constructor);

        var getAllMethod = new MethodModel
        {
            Name = "GetAll",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("RateLimitRuleDto")] }] }] },
            Params = [new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }],
            Body = new ExpressionModel(@"var rules = await repository.GetAllAsync(cancellationToken);
        return Ok(rules.Select(r => new RateLimitRuleDto
        {
            RateLimitRuleId = r.RateLimitRuleId,
            Name = r.Name,
            Description = r.Description,
            Endpoint = r.Endpoint,
            RequestLimit = r.RequestLimit,
            TimeWindowSeconds = r.TimeWindowSeconds,
            Scope = r.Scope.ToString(),
            IsEnabled = r.IsEnabled,
            CreatedAt = r.CreatedAt
        }));")
        };
        getAllMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet" });
        classModel.Methods.Add(getAllMethod);

        var getByIdMethod = new MethodModel
        {
            Name = "GetById",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("RateLimitRuleDto")] }] },
            Params =
            [
                new ParamModel { Name = "id", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var rule = await repository.GetByIdAsync(id, cancellationToken);
        if (rule == null) return NotFound();

        return Ok(new RateLimitRuleDto
        {
            RateLimitRuleId = rule.RateLimitRuleId,
            Name = rule.Name,
            Description = rule.Description,
            Endpoint = rule.Endpoint,
            RequestLimit = rule.RequestLimit,
            TimeWindowSeconds = rule.TimeWindowSeconds,
            Scope = rule.Scope.ToString(),
            IsEnabled = rule.IsEnabled,
            CreatedAt = rule.CreatedAt
        });")
        };
        getByIdMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"{id:guid}\"" });
        classModel.Methods.Add(getByIdMethod);

        var createMethod = new MethodModel
        {
            Name = "Create",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("RateLimitRuleDto")] }] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("CreateRateLimitRuleRequest"), Attribute = "[FromBody]" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var existing = await repository.GetByEndpointAsync(request.Endpoint, cancellationToken);
        if (existing != null)
        {
            return BadRequest(""A rate limit rule already exists for this endpoint"");
        }

        var rule = new RateLimitRule
        {
            Name = request.Name,
            Description = request.Description,
            Endpoint = request.Endpoint,
            RequestLimit = request.RequestLimit,
            TimeWindowSeconds = request.TimeWindowSeconds,
            Scope = Enum.TryParse<RateLimitScope>(request.Scope, out var scope) ? scope : RateLimitScope.PerUser
        };

        var created = await repository.AddAsync(rule, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = created.RateLimitRuleId }, new RateLimitRuleDto
        {
            RateLimitRuleId = created.RateLimitRuleId,
            Name = created.Name,
            Description = created.Description,
            Endpoint = created.Endpoint,
            RequestLimit = created.RequestLimit,
            TimeWindowSeconds = created.TimeWindowSeconds,
            Scope = created.Scope.ToString(),
            IsEnabled = created.IsEnabled,
            CreatedAt = created.CreatedAt
        });")
        };
        createMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpPost" });
        classModel.Methods.Add(createMethod);

        var deleteMethod = new MethodModel
        {
            Name = "Delete",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult")] },
            Params =
            [
                new ParamModel { Name = "id", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var rule = await repository.GetByIdAsync(id, cancellationToken);
        if (rule == null) return NotFound();

        await repository.DeleteAsync(id, cancellationToken);
        return NoContent();")
        };
        deleteMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpDelete", Template = "\"{id:guid}\"" });
        classModel.Methods.Add(deleteMethod);

        return new CodeFileModel<ClassModel>(classModel, "RateLimitRulesController", directory, CSharp)
        {
            Namespace = "RateLimiting.Api.Controllers"
        };
    }

    private static FileModel CreateProgramFile(string directory)
    {
        return new FileModel("Program", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                var builder = WebApplication.CreateBuilder(args);

                builder.Services.AddRateLimitingInfrastructure(builder.Configuration);
                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();
                builder.Services.AddHealthChecks();

                var app = builder.Build();

                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();
                app.MapControllers();
                app.MapHealthChecks("/health");
                app.Run();
                """
        };
    }

    private static FileModel CreateAppSettingsFile(string directory)
    {
        return new FileModel("appsettings", directory, ".json")
        {
            Body = """
                {
                  "ConnectionStrings": {
                    "RateLimitingDb": "Server=.\\SQLEXPRESS;Database=RateLimitingDb;Trusted_Connection=True;TrustServerCertificate=True"
                  },
                  "Logging": {
                    "LogLevel": {
                      "Default": "Information"
                    }
                  },
                  "AllowedHosts": "*"
                }
                """
        };
    }

    #endregion
}
