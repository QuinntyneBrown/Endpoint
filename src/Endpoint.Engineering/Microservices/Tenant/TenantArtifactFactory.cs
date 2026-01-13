// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.DotNet.Artifacts.Files;
using Endpoint.DotNet.Artifacts.Projects;
using Endpoint.DotNet.Syntax;
using Endpoint.DotNet.Syntax.Attributes;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Attributes;
using Endpoint.DotNet.Syntax.Constructors;
using Endpoint.DotNet.Syntax.Attributes;
using Endpoint.DotNet.Syntax.Expressions;
using Endpoint.DotNet.Syntax.Attributes;
using Endpoint.DotNet.Syntax.Fields;
using Endpoint.DotNet.Syntax.Attributes;
using Endpoint.DotNet.Syntax.Interfaces;
using Endpoint.DotNet.Syntax.Attributes;
using Endpoint.DotNet.Syntax.Methods;
using Endpoint.DotNet.Syntax.Attributes;
using Endpoint.DotNet.Syntax.Params;
using Endpoint.DotNet.Syntax.Attributes;
using Endpoint.DotNet.Syntax.Properties;
using Endpoint.DotNet.Syntax.Attributes;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.Tenant;

using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

/// <summary>
/// Factory for creating Tenant microservice artifacts.
/// Manages tenant registration, settings, and subscription management.
/// </summary>
public class TenantArtifactFactory : ITenantArtifactFactory
{
    private readonly ILogger<TenantArtifactFactory> logger;

    public TenantArtifactFactory(ILogger<TenantArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding Tenant.Core files");
        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");

        // Entities
        project.Files.Add(CreateTenantEntityFile(entitiesDir));
        project.Files.Add(CreateTenantSettingEntityFile(entitiesDir));

        // Interfaces
        project.Files.Add(CreateITenantRepositoryFile(interfacesDir));

        // Events
        project.Files.Add(CreateTenantCreatedEventFile(eventsDir));
        project.Files.Add(CreateTenantSuspendedEventFile(eventsDir));

        // DTOs
        project.Files.Add(CreateTenantDtoFile(dtosDir));
        project.Files.Add(CreateCreateTenantRequestFile(dtosDir));
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Tenant.Infrastructure files");
        var dataDir = Path.Combine(project.Directory, "Data");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");

        // DbContext
        project.Files.Add(CreateTenantDbContextFile(dataDir));

        // Repositories
        project.Files.Add(CreateTenantRepositoryFile(repositoriesDir));

        // ConfigureServices
        project.Files.Add(CreateInfrastructureConfigureServicesFile(project.Directory));
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Tenant.Api files");
        var controllersDir = Path.Combine(project.Directory, "Controllers");

        // Controllers
        project.Files.Add(CreateTenantsControllerFile(controllersDir));

        // Program.cs
        project.Files.Add(CreateProgramFile(project.Directory));

        // appsettings.json
        project.Files.Add(CreateAppSettingsFile(project.Directory));
    }

    #region Core Layer Files

    private static CodeFileModel<ClassModel> CreateTenantEntityFile(string directory)
    {
        var classModel = new ClassModel("Tenant");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "TenantId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Slug", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("TenantStatus"), "Status", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "TenantStatus.Active" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("TenantTier"), "Tier", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "TenantTier.Basic" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "SubscriptionExpiry", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("ICollection") { GenericTypeParameters = [new TypeModel("TenantSetting")] }, "Settings", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "new List<TenantSetting>()" });

        return new CodeFileModel<ClassModel>(classModel, "Tenant", directory, CSharp)
        {
            Namespace = "Tenant.Core.Entities",
            AdditionalCode = """

                public enum TenantStatus { Active, Suspended, Deactivated }
                public enum TenantTier { Basic, Standard, Enterprise }
                """
        };
    }

    private static CodeFileModel<ClassModel> CreateTenantSettingEntityFile(string directory)
    {
        var classModel = new ClassModel("TenantSetting");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "SettingId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "TenantId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Tenant"), "Tenant", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "null!" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Key", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Value", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));

        return new CodeFileModel<ClassModel>(classModel, "TenantSetting", directory, CSharp)
        {
            Namespace = "Tenant.Core.Entities"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateITenantRepositoryFile(string directory)
    {
        var interfaceModel = new InterfaceModel("ITenantRepository");

        interfaceModel.Usings.Add(new UsingModel("Tenant.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Entities.Tenant") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "tenantId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetBySlugAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Entities.Tenant") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "slug", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetAllAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Entities.Tenant")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Entities.Tenant")] },
            Params =
            [
                new ParamModel { Name = "tenant", Type = new TypeModel("Entities.Tenant") },
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
                new ParamModel { Name = "tenant", Type = new TypeModel("Entities.Tenant") },
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
                new ParamModel { Name = "tenantId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "ITenantRepository", directory, CSharp)
        {
            Namespace = "Tenant.Core.Interfaces"
        };
    }

    private static CodeFileModel<ClassModel> CreateTenantCreatedEventFile(string directory)
    {
        var classModel = new ClassModel("TenantCreatedEvent")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "TenantId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Slug", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "TenantCreatedEvent", directory, CSharp)
        {
            Namespace = "Tenant.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreateTenantSuspendedEventFile(string directory)
    {
        var classModel = new ClassModel("TenantSuspendedEvent")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "TenantId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Reason", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "TenantSuspendedEvent", directory, CSharp)
        {
            Namespace = "Tenant.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreateTenantDtoFile(string directory)
    {
        var classModel = new ClassModel("TenantDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "TenantId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Slug", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Status", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "\"Active\"" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Tier", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "\"Basic\"" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "TenantDto", directory, CSharp)
        {
            Namespace = "Tenant.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateCreateTenantRequestFile(string directory)
    {
        var classModel = new ClassModel("CreateTenantRequest")
        {
            Sealed = true
        };

        classModel.Usings.Add(new UsingModel("System.ComponentModel.DataAnnotations"));

        var nameProperty = new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true);
        nameProperty.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Required" });
        classModel.Properties.Add(nameProperty);

        var slugProperty = new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Slug", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true);
        slugProperty.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Required" });
        classModel.Properties.Add(slugProperty);

        return new CodeFileModel<ClassModel>(classModel, "CreateTenantRequest", directory, CSharp)
        {
            Namespace = "Tenant.Core.DTOs"
        };
    }

    #endregion

    #region Infrastructure Layer Files

    private static CodeFileModel<ClassModel> CreateTenantDbContextFile(string directory)
    {
        var classModel = new ClassModel("TenantDbContext");

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Tenant.Core.Entities"));

        classModel.Implements.Add(new TypeModel("DbContext"));

        var constructor = new ConstructorModel(classModel, "TenantDbContext")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "options", Type = new TypeModel("DbContextOptions") { GenericTypeParameters = [new TypeModel("TenantDbContext")] } }],
            BaseParams = ["options"]
        };
        classModel.Constructors.Add(constructor);

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("Core.Entities.Tenant")] }, "Tenants", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("TenantSetting")] }, "TenantSettings", [new PropertyAccessorModel(PropertyAccessorType.Get)]));

        classModel.Methods.Add(new MethodModel
        {
            Name = "OnModelCreating",
            AccessModifier = AccessModifier.Protected,
            Override = true,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "modelBuilder", Type = new TypeModel("ModelBuilder") }],
            Body = new ExpressionModel(@"modelBuilder.Entity<Core.Entities.Tenant>(entity =>
        {
            entity.HasKey(t => t.TenantId);
            entity.Property(t => t.Name).IsRequired().HasMaxLength(200);
            entity.Property(t => t.Slug).IsRequired().HasMaxLength(100);
            entity.HasIndex(t => t.Slug).IsUnique();
        });

        modelBuilder.Entity<TenantSetting>(entity =>
        {
            entity.HasKey(s => s.SettingId);
            entity.HasOne(s => s.Tenant).WithMany(t => t.Settings).HasForeignKey(s => s.TenantId);
        });")
        });

        return new CodeFileModel<ClassModel>(classModel, "TenantDbContext", directory, CSharp)
        {
            Namespace = "Tenant.Infrastructure.Data"
        };
    }

    private static CodeFileModel<ClassModel> CreateTenantRepositoryFile(string directory)
    {
        var classModel = new ClassModel("TenantRepository");

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Tenant.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Tenant.Infrastructure.Data"));

        classModel.Implements.Add(new TypeModel("ITenantRepository"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "context",
            Type = new TypeModel("TenantDbContext"),
            AccessModifier = AccessModifier.Private,
            ReadOnly = true
        });

        var constructor = new ConstructorModel(classModel, "TenantRepository")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "context", Type = new TypeModel("TenantDbContext") }],
            Body = new ExpressionModel("this.context = context;")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Core.Entities.Tenant") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "tenantId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.Tenants.Include(t => t.Settings).FirstOrDefaultAsync(t => t.TenantId == tenantId, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetBySlugAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Core.Entities.Tenant") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "slug", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.Tenants.Include(t => t.Settings).FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetAllAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Core.Entities.Tenant")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.Tenants.Include(t => t.Settings).ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Core.Entities.Tenant")] },
            Params =
            [
                new ParamModel { Name = "tenant", Type = new TypeModel("Core.Entities.Tenant") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"tenant.TenantId = Guid.NewGuid();
        await context.Tenants.AddAsync(tenant, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return tenant;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "UpdateAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "tenant", Type = new TypeModel("Core.Entities.Tenant") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"context.Tenants.Update(tenant);
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
                new ParamModel { Name = "tenantId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var tenant = await context.Tenants.FindAsync(new object[] { tenantId }, cancellationToken);
        if (tenant != null)
        {
            context.Tenants.Remove(tenant);
            await context.SaveChangesAsync(cancellationToken);
        }")
        });

        return new CodeFileModel<ClassModel>(classModel, "TenantRepository", directory, CSharp)
        {
            Namespace = "Tenant.Infrastructure.Repositories"
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
        classModel.Usings.Add(new UsingModel("Tenant.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Tenant.Infrastructure.Data"));
        classModel.Usings.Add(new UsingModel("Tenant.Infrastructure.Repositories"));

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddTenantInfrastructure",
            AccessModifier = AccessModifier.Public,
            Static = true,
            ReturnType = new TypeModel("IServiceCollection"),
            Params =
            [
                new ParamModel { Name = "services", Type = new TypeModel("IServiceCollection"), ExtensionMethodParam = true },
                new ParamModel { Name = "configuration", Type = new TypeModel("IConfiguration") }
            ],
            Body = new ExpressionModel(@"services.AddDbContext<TenantDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString(""TenantDb"") ??
                @""Server=.\SQLEXPRESS;Database=TenantDb;Trusted_Connection=True;TrustServerCertificate=True""));

        services.AddScoped<ITenantRepository, TenantRepository>();
        return services;")
        });

        return new CodeFileModel<ClassModel>(classModel, "ConfigureServices", directory, CSharp)
        {
            Namespace = "Microsoft.Extensions.DependencyInjection"
        };
    }

    #endregion

    #region API Layer Files

    private static CodeFileModel<ClassModel> CreateTenantsControllerFile(string directory)
    {
        var classModel = new ClassModel("TenantsController");

        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.Mvc"));
        classModel.Usings.Add(new UsingModel("Tenant.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("Tenant.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("ControllerBase"));

        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ApiController" });
        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Route", Template = "\"api/[controller]\"" });

        classModel.Fields.Add(new FieldModel
        {
            Name = "repository",
            Type = new TypeModel("ITenantRepository"),
            AccessModifier = AccessModifier.Private,
            ReadOnly = true
        });

        var constructor = new ConstructorModel(classModel, "TenantsController")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "repository", Type = new TypeModel("ITenantRepository") }],
            Body = new ExpressionModel("this.repository = repository;")
        };
        classModel.Constructors.Add(constructor);

        var getAllMethod = new MethodModel
        {
            Name = "GetAll",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("TenantDto")] }] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var tenants = await repository.GetAllAsync(cancellationToken);
        return Ok(tenants.Select(t => new TenantDto
        {
            TenantId = t.TenantId,
            Name = t.Name,
            Slug = t.Slug,
            Status = t.Status.ToString(),
            Tier = t.Tier.ToString(),
            CreatedAt = t.CreatedAt
        }));")
        };
        getAllMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet" });
        classModel.Methods.Add(getAllMethod);

        var getByIdMethod = new MethodModel
        {
            Name = "GetById",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("TenantDto")] }] },
            Params =
            [
                new ParamModel { Name = "id", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var tenant = await repository.GetByIdAsync(id, cancellationToken);
        if (tenant == null) return NotFound();
        return Ok(new TenantDto
        {
            TenantId = tenant.TenantId,
            Name = tenant.Name,
            Slug = tenant.Slug,
            Status = tenant.Status.ToString(),
            Tier = tenant.Tier.ToString(),
            CreatedAt = tenant.CreatedAt
        });")
        };
        getByIdMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"{id:guid}\"" });
        classModel.Methods.Add(getByIdMethod);

        var createMethod = new MethodModel
        {
            Name = "Create",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("TenantDto")] }] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("CreateTenantRequest"), Attribute = new AttributeModel() { Name = "FromBody" } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var existing = await repository.GetBySlugAsync(request.Slug, cancellationToken);
        if (existing != null) return BadRequest(""Slug already exists"");

        var tenant = new Core.Entities.Tenant { Name = request.Name, Slug = request.Slug };
        var created = await repository.AddAsync(tenant, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.TenantId }, new TenantDto
        {
            TenantId = created.TenantId,
            Name = created.Name,
            Slug = created.Slug,
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
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IActionResult")] },
            Params =
            [
                new ParamModel { Name = "id", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"await repository.DeleteAsync(id, cancellationToken);
        return NoContent();")
        };
        deleteMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpDelete", Template = "\"{id:guid}\"" });
        classModel.Methods.Add(deleteMethod);

        return new CodeFileModel<ClassModel>(classModel, "TenantsController", directory, CSharp)
        {
            Namespace = "Tenant.Api.Controllers"
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

                builder.Services.AddTenantInfrastructure(builder.Configuration);
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
                    "TenantDb": "Server=.\\SQLEXPRESS;Database=TenantDb;Trusted_Connection=True;TrustServerCertificate=True"
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
