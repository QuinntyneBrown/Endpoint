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

namespace Endpoint.Engineering.Microservices.Geolocation;

using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

/// <summary>
/// Factory for creating Geolocation microservice artifacts according to geolocation-microservice.spec.md.
/// </summary>
public class GeolocationArtifactFactory : IGeolocationArtifactFactory
{
    private readonly ILogger<GeolocationArtifactFactory> logger;

    public GeolocationArtifactFactory(ILogger<GeolocationArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding Geolocation.Core files");

        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");

        // Entities
        project.Files.Add(CreateIAggregateRootFile(entitiesDir));
        project.Files.Add(CreateLocationEntityFile(entitiesDir));
        project.Files.Add(CreateGeoFenceEntityFile(entitiesDir));
        project.Files.Add(CreateRouteEntityFile(entitiesDir));

        // Interfaces
        project.Files.Add(CreateIDomainEventFile(interfacesDir));
        project.Files.Add(CreateILocationRepositoryFile(interfacesDir));
        project.Files.Add(CreateIGeocodingServiceFile(interfacesDir));
        project.Files.Add(CreateIRoutingServiceFile(interfacesDir));

        // Events
        project.Files.Add(CreateLocationUpdatedEventFile(eventsDir));
        project.Files.Add(CreateGeofenceEnteredEventFile(eventsDir));
        project.Files.Add(CreateGeofenceExitedEventFile(eventsDir));

        // DTOs
        var dtosDir = Path.Combine(project.Directory, "DTOs");
        project.Files.Add(CreateLocationDtoFile(dtosDir));
        project.Files.Add(CreateGeoFenceDtoFile(dtosDir));
        project.Files.Add(CreateRouteDtoFile(dtosDir));
        project.Files.Add(CreateGeocodeRequestFile(dtosDir));
        project.Files.Add(CreateGeocodeResponseFile(dtosDir));
        project.Files.Add(CreateDistanceRequestFile(dtosDir));
        project.Files.Add(CreateDistanceResponseFile(dtosDir));
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Geolocation.Infrastructure files");

        var dataDir = Path.Combine(project.Directory, "Data");
        var configurationsDir = Path.Combine(project.Directory, "Data", "Configurations");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");

        // DbContext
        project.Files.Add(CreateGeolocationDbContextFile(dataDir));

        // Entity Configurations
        project.Files.Add(CreateLocationConfigurationFile(configurationsDir));
        project.Files.Add(CreateGeoFenceConfigurationFile(configurationsDir));
        project.Files.Add(CreateRouteConfigurationFile(configurationsDir));

        // Repositories
        project.Files.Add(CreateLocationRepositoryFile(repositoriesDir));

        // Services
        project.Files.Add(CreateGeocodingServiceFile(servicesDir));
        project.Files.Add(CreateRoutingServiceFile(servicesDir));

        // ConfigureServices
        project.Files.Add(CreateInfrastructureConfigureServicesFile(project.Directory));
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Geolocation.Api files");

        var controllersDir = Path.Combine(project.Directory, "Controllers");

        // Controllers
        project.Files.Add(CreateLocationsControllerFile(controllersDir));

        // Program.cs
        project.Files.Add(CreateProgramFile(project.Directory));

        // appsettings.json
        project.Files.Add(CreateAppSettingsFile(project.Directory));
        project.Files.Add(CreateAppSettingsDevelopmentFile(project.Directory));
    }

    #region Core Layer Files

    private static CodeFileModel<InterfaceModel> CreateIAggregateRootFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IAggregateRoot");

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IAggregateRoot", directory, CSharp)
        {
            Namespace = "Geolocation.Core.Entities"
        };
    }

    private static CodeFileModel<ClassModel> CreateLocationEntityFile(string directory)
    {
        var classModel = new ClassModel("Location");

        classModel.Implements.Add(new TypeModel("IAggregateRoot"));

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "LocationId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "Latitude", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "Longitude", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double") { Nullable = true }, "Altitude", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double") { Nullable = true }, "Accuracy", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Address", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "City", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "State", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Country", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "PostalCode", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "EntityId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "EntityType", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "Timestamp", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "UpdatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));

        return new CodeFileModel<ClassModel>(classModel, "Location", directory, CSharp)
        {
            Namespace = "Geolocation.Core.Entities"
        };
    }

    private static CodeFileModel<ClassModel> CreateGeoFenceEntityFile(string directory)
    {
        var classModel = new ClassModel("GeoFence");

        classModel.Implements.Add(new TypeModel("IAggregateRoot"));

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "GeoFenceId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Description", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "CenterLatitude", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "CenterLongitude", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "RadiusMeters", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Polygon", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsActive", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "true" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "UpdatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));

        return new CodeFileModel<ClassModel>(classModel, "GeoFence", directory, CSharp)
        {
            Namespace = "Geolocation.Core.Entities"
        };
    }

    private static CodeFileModel<ClassModel> CreateRouteEntityFile(string directory)
    {
        var classModel = new ClassModel("Route");

        classModel.Implements.Add(new TypeModel("IAggregateRoot"));

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "RouteId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Description", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "StartLatitude", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "StartLongitude", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "EndLatitude", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "EndLongitude", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double") { Nullable = true }, "DistanceMeters", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int") { Nullable = true }, "DurationSeconds", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "EncodedPolyline", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Waypoints", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "UpdatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));

        return new CodeFileModel<ClassModel>(classModel, "Route", directory, CSharp)
        {
            Namespace = "Geolocation.Core.Entities"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateIDomainEventFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IDomainEvent");

        interfaceModel.Properties.Add(new PropertyModel(interfaceModel, AccessModifier.Public, new TypeModel("Guid"), "AggregateId", [new PropertyAccessorModel(PropertyAccessorType.Get, null)]));
        interfaceModel.Properties.Add(new PropertyModel(interfaceModel, AccessModifier.Public, new TypeModel("string"), "AggregateType", [new PropertyAccessorModel(PropertyAccessorType.Get, null)]));
        interfaceModel.Properties.Add(new PropertyModel(interfaceModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null)]));
        interfaceModel.Properties.Add(new PropertyModel(interfaceModel, AccessModifier.Public, new TypeModel("string"), "CorrelationId", [new PropertyAccessorModel(PropertyAccessorType.Get, null)]));

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IDomainEvent", directory, CSharp)
        {
            Namespace = "Geolocation.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateILocationRepositoryFile(string directory)
    {
        var interfaceModel = new InterfaceModel("ILocationRepository");

        interfaceModel.Usings.Add(new UsingModel("Geolocation.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Location") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "locationId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetAllAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Location")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByEntityAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Location")] }] },
            Params =
            [
                new ParamModel { Name = "entityId", Type = new TypeModel("string") },
                new ParamModel { Name = "entityType", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetWithinRadiusAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Location")] }] },
            Params =
            [
                new ParamModel { Name = "latitude", Type = new TypeModel("double") },
                new ParamModel { Name = "longitude", Type = new TypeModel("double") },
                new ParamModel { Name = "radiusMeters", Type = new TypeModel("double") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Location")] },
            Params =
            [
                new ParamModel { Name = "location", Type = new TypeModel("Location") },
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
                new ParamModel { Name = "location", Type = new TypeModel("Location") },
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
                new ParamModel { Name = "locationId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "ILocationRepository", directory, CSharp)
        {
            Namespace = "Geolocation.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateIGeocodingServiceFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IGeocodingService");

        interfaceModel.Usings.Add(new UsingModel("Geolocation.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GeocodeAddressAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Location") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "address", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "ReverseGeocodeAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("string") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "latitude", Type = new TypeModel("double") },
                new ParamModel { Name = "longitude", Type = new TypeModel("double") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "SearchAddressAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Location")] }] },
            Params =
            [
                new ParamModel { Name = "query", Type = new TypeModel("string") },
                new ParamModel { Name = "maxResults", Type = new TypeModel("int"), DefaultValue = "10" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IGeocodingService", directory, CSharp)
        {
            Namespace = "Geolocation.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateIRoutingServiceFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IRoutingService");

        interfaceModel.Usings.Add(new UsingModel("Geolocation.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "CalculateRouteAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Route") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "startLat", Type = new TypeModel("double") },
                new ParamModel { Name = "startLon", Type = new TypeModel("double") },
                new ParamModel { Name = "endLat", Type = new TypeModel("double") },
                new ParamModel { Name = "endLon", Type = new TypeModel("double") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "CalculateDistanceAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("double")] },
            Params =
            [
                new ParamModel { Name = "lat1", Type = new TypeModel("double") },
                new ParamModel { Name = "lon1", Type = new TypeModel("double") },
                new ParamModel { Name = "lat2", Type = new TypeModel("double") },
                new ParamModel { Name = "lon2", Type = new TypeModel("double") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "CalculateRouteWithWaypointsAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Route") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "waypoints", Type = new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("(double Latitude, double Longitude)")] } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IRoutingService", directory, CSharp)
        {
            Namespace = "Geolocation.Core.Interfaces"
        };
    }

    private static FileModel CreateLocationUpdatedEventFile(string directory)
    {
        return new FileModel("LocationUpdatedEvent", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Geolocation.Core.Interfaces;

                namespace Geolocation.Core.Events;

                /// <summary>
                /// Event raised when a location is updated.
                /// </summary>
                public sealed class LocationUpdatedEvent : IDomainEvent
                {
                    public Guid AggregateId { get; init; }

                    public string AggregateType => "Location";

                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

                    public required string CorrelationId { get; init; }

                    public double Latitude { get; init; }

                    public double Longitude { get; init; }

                    public string? EntityId { get; init; }

                    public string? EntityType { get; init; }
                }
                """
        };
    }

    private static FileModel CreateGeofenceEnteredEventFile(string directory)
    {
        return new FileModel("GeofenceEnteredEvent", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Geolocation.Core.Interfaces;

                namespace Geolocation.Core.Events;

                /// <summary>
                /// Event raised when an entity enters a geofence.
                /// </summary>
                public sealed class GeofenceEnteredEvent : IDomainEvent
                {
                    public Guid AggregateId { get; init; }

                    public string AggregateType => "GeoFence";

                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

                    public required string CorrelationId { get; init; }

                    public required Guid GeoFenceId { get; init; }

                    public required string GeoFenceName { get; init; }

                    public string? EntityId { get; init; }

                    public string? EntityType { get; init; }

                    public double Latitude { get; init; }

                    public double Longitude { get; init; }
                }
                """
        };
    }

    private static FileModel CreateGeofenceExitedEventFile(string directory)
    {
        return new FileModel("GeofenceExitedEvent", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Geolocation.Core.Interfaces;

                namespace Geolocation.Core.Events;

                /// <summary>
                /// Event raised when an entity exits a geofence.
                /// </summary>
                public sealed class GeofenceExitedEvent : IDomainEvent
                {
                    public Guid AggregateId { get; init; }

                    public string AggregateType => "GeoFence";

                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

                    public required string CorrelationId { get; init; }

                    public required Guid GeoFenceId { get; init; }

                    public required string GeoFenceName { get; init; }

                    public string? EntityId { get; init; }

                    public string? EntityType { get; init; }

                    public double Latitude { get; init; }

                    public double Longitude { get; init; }

                    public TimeSpan? DurationInside { get; init; }
                }
                """
        };
    }

    private static CodeFileModel<ClassModel> CreateLocationDtoFile(string directory)
    {
        var classModel = new ClassModel("LocationDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "LocationId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "Latitude", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "Longitude", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double") { Nullable = true }, "Altitude", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double") { Nullable = true }, "Accuracy", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Address", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "City", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "State", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Country", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "PostalCode", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "EntityId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "EntityType", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "Timestamp", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));

        return new CodeFileModel<ClassModel>(classModel, "LocationDto", directory, CSharp)
        {
            Namespace = "Geolocation.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateGeoFenceDtoFile(string directory)
    {
        var classModel = new ClassModel("GeoFenceDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "GeoFenceId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Description", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "CenterLatitude", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "CenterLongitude", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "RadiusMeters", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsActive", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));

        return new CodeFileModel<ClassModel>(classModel, "GeoFenceDto", directory, CSharp)
        {
            Namespace = "Geolocation.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateRouteDtoFile(string directory)
    {
        var classModel = new ClassModel("RouteDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "RouteId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Description", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "StartLatitude", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "StartLongitude", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "EndLatitude", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "EndLongitude", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double") { Nullable = true }, "DistanceMeters", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int") { Nullable = true }, "DurationSeconds", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "EncodedPolyline", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));

        return new CodeFileModel<ClassModel>(classModel, "RouteDto", directory, CSharp)
        {
            Namespace = "Geolocation.Core.DTOs"
        };
    }

    private static FileModel CreateGeocodeRequestFile(string directory)
    {
        return new FileModel("GeocodeRequest", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Geolocation.Core.DTOs;

                /// <summary>
                /// Request model for geocoding an address.
                /// </summary>
                public sealed class GeocodeRequest
                {
                    [Required]
                    public required string Address { get; init; }
                }
                """
        };
    }

    private static CodeFileModel<ClassModel> CreateGeocodeResponseFile(string directory)
    {
        var classModel = new ClassModel("GeocodeResponse")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "Latitude", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "Longitude", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "FormattedAddress", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "City", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "State", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Country", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "PostalCode", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));

        return new CodeFileModel<ClassModel>(classModel, "GeocodeResponse", directory, CSharp)
        {
            Namespace = "Geolocation.Core.DTOs"
        };
    }

    private static FileModel CreateDistanceRequestFile(string directory)
    {
        return new FileModel("DistanceRequest", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Geolocation.Core.DTOs;

                /// <summary>
                /// Request model for calculating distance between two points.
                /// </summary>
                public sealed class DistanceRequest
                {
                    [Required]
                    [Range(-90, 90)]
                    public double FromLatitude { get; init; }

                    [Required]
                    [Range(-180, 180)]
                    public double FromLongitude { get; init; }

                    [Required]
                    [Range(-90, 90)]
                    public double ToLatitude { get; init; }

                    [Required]
                    [Range(-180, 180)]
                    public double ToLongitude { get; init; }
                }
                """
        };
    }

    private static CodeFileModel<ClassModel> CreateDistanceResponseFile(string directory)
    {
        var classModel = new ClassModel("DistanceResponse")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "DistanceMeters", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "DistanceKilometers", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "DistanceMiles", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int") { Nullable = true }, "EstimatedDurationSeconds", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));

        return new CodeFileModel<ClassModel>(classModel, "DistanceResponse", directory, CSharp)
        {
            Namespace = "Geolocation.Core.DTOs"
        };
    }

    #endregion

    #region Infrastructure Layer Files

    private static CodeFileModel<ClassModel> CreateGeolocationDbContextFile(string directory)
    {
        var classModel = new ClassModel("GeolocationDbContext");

        classModel.Usings.Add(new UsingModel("Geolocation.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));

        classModel.Implements.Add(new TypeModel("DbContext"));

        var constructor = new ConstructorModel(classModel, "GeolocationDbContext")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "options", Type = new TypeModel("DbContextOptions") { GenericTypeParameters = [new TypeModel("GeolocationDbContext")] } }],
            BaseParams = ["options"]
        };
        classModel.Constructors.Add(constructor);

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("Location")] }, "Locations", [new PropertyAccessorModel(PropertyAccessorType.Get, "Set<Location>()")]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("GeoFence")] }, "GeoFences", [new PropertyAccessorModel(PropertyAccessorType.Get, "Set<GeoFence>()")]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("Route")] }, "Routes", [new PropertyAccessorModel(PropertyAccessorType.Get, "Set<Route>()")]));

        classModel.Methods.Add(new MethodModel
        {
            Name = "OnModelCreating",
            AccessModifier = AccessModifier.Protected,
            Override = true,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "modelBuilder", Type = new TypeModel("ModelBuilder") }],
            Body = new ExpressionModel(@"base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GeolocationDbContext).Assembly);")
        });

        return new CodeFileModel<ClassModel>(classModel, "GeolocationDbContext", directory, CSharp)
        {
            Namespace = "Geolocation.Infrastructure.Data"
        };
    }

    private static CodeFileModel<ClassModel> CreateLocationConfigurationFile(string directory)
    {
        var classModel = new ClassModel("LocationConfiguration");

        classModel.Usings.Add(new UsingModel("Geolocation.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore.Metadata.Builders"));

        classModel.Implements.Add(new TypeModel("IEntityTypeConfiguration") { GenericTypeParameters = [new TypeModel("Location")] });

        classModel.Methods.Add(new MethodModel
        {
            Name = "Configure",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "builder", Type = new TypeModel("EntityTypeBuilder") { GenericTypeParameters = [new TypeModel("Location")] } }],
            Body = new ExpressionModel(@"builder.HasKey(l => l.LocationId);

        builder.Property(l => l.Latitude)
            .IsRequired();

        builder.Property(l => l.Longitude)
            .IsRequired();

        builder.Property(l => l.Address)
            .HasMaxLength(500);

        builder.Property(l => l.City)
            .HasMaxLength(100);

        builder.Property(l => l.State)
            .HasMaxLength(100);

        builder.Property(l => l.Country)
            .HasMaxLength(100);

        builder.Property(l => l.PostalCode)
            .HasMaxLength(20);

        builder.Property(l => l.EntityId)
            .HasMaxLength(100);

        builder.Property(l => l.EntityType)
            .HasMaxLength(100);

        builder.Property(l => l.Timestamp)
            .IsRequired();

        builder.HasIndex(l => new { l.Latitude, l.Longitude });

        builder.HasIndex(l => new { l.EntityId, l.EntityType });")
        });

        return new CodeFileModel<ClassModel>(classModel, "LocationConfiguration", directory, CSharp)
        {
            Namespace = "Geolocation.Infrastructure.Data.Configurations"
        };
    }

    private static CodeFileModel<ClassModel> CreateGeoFenceConfigurationFile(string directory)
    {
        var classModel = new ClassModel("GeoFenceConfiguration");

        classModel.Usings.Add(new UsingModel("Geolocation.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore.Metadata.Builders"));

        classModel.Implements.Add(new TypeModel("IEntityTypeConfiguration") { GenericTypeParameters = [new TypeModel("GeoFence")] });

        classModel.Methods.Add(new MethodModel
        {
            Name = "Configure",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "builder", Type = new TypeModel("EntityTypeBuilder") { GenericTypeParameters = [new TypeModel("GeoFence")] } }],
            Body = new ExpressionModel(@"builder.HasKey(g => g.GeoFenceId);

        builder.Property(g => g.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(g => g.Description)
            .HasMaxLength(1000);

        builder.Property(g => g.CenterLatitude)
            .IsRequired();

        builder.Property(g => g.CenterLongitude)
            .IsRequired();

        builder.Property(g => g.RadiusMeters)
            .IsRequired();

        builder.HasIndex(g => g.Name);

        builder.HasIndex(g => new { g.CenterLatitude, g.CenterLongitude });")
        });

        return new CodeFileModel<ClassModel>(classModel, "GeoFenceConfiguration", directory, CSharp)
        {
            Namespace = "Geolocation.Infrastructure.Data.Configurations"
        };
    }

    private static CodeFileModel<ClassModel> CreateRouteConfigurationFile(string directory)
    {
        var classModel = new ClassModel("RouteConfiguration");

        classModel.Usings.Add(new UsingModel("Geolocation.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore.Metadata.Builders"));

        classModel.Implements.Add(new TypeModel("IEntityTypeConfiguration") { GenericTypeParameters = [new TypeModel("Route")] });

        classModel.Methods.Add(new MethodModel
        {
            Name = "Configure",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "builder", Type = new TypeModel("EntityTypeBuilder") { GenericTypeParameters = [new TypeModel("Route")] } }],
            Body = new ExpressionModel(@"builder.HasKey(r => r.RouteId);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Description)
            .HasMaxLength(1000);

        builder.Property(r => r.StartLatitude)
            .IsRequired();

        builder.Property(r => r.StartLongitude)
            .IsRequired();

        builder.Property(r => r.EndLatitude)
            .IsRequired();

        builder.Property(r => r.EndLongitude)
            .IsRequired();

        builder.HasIndex(r => r.Name);")
        });

        return new CodeFileModel<ClassModel>(classModel, "RouteConfiguration", directory, CSharp)
        {
            Namespace = "Geolocation.Infrastructure.Data.Configurations"
        };
    }

    private static CodeFileModel<ClassModel> CreateLocationRepositoryFile(string directory)
    {
        var classModel = new ClassModel("LocationRepository");

        classModel.Usings.Add(new UsingModel("Geolocation.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Geolocation.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Geolocation.Infrastructure.Data"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));

        classModel.Implements.Add(new TypeModel("ILocationRepository"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "context",
            Type = new TypeModel("GeolocationDbContext"),
            AccessModifier = AccessModifier.Private,
            Readonly = true
        });

        var constructor = new ConstructorModel(classModel, "LocationRepository")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "context", Type = new TypeModel("GeolocationDbContext") }],
            Body = new ExpressionModel("this.context = context ?? throw new ArgumentNullException(nameof(context));")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Location") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "locationId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.Locations
            .FirstOrDefaultAsync(l => l.LocationId == locationId, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetAllAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Location")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.Locations
            .OrderByDescending(l => l.Timestamp)
            .ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByEntityAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Location")] }] },
            Params =
            [
                new ParamModel { Name = "entityId", Type = new TypeModel("string") },
                new ParamModel { Name = "entityType", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.Locations
            .Where(l => l.EntityId == entityId && l.EntityType == entityType)
            .OrderByDescending(l => l.Timestamp)
            .ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetWithinRadiusAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Location")] }] },
            Params =
            [
                new ParamModel { Name = "latitude", Type = new TypeModel("double") },
                new ParamModel { Name = "longitude", Type = new TypeModel("double") },
                new ParamModel { Name = "radiusMeters", Type = new TypeModel("double") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"// Approximate conversion: 1 degree latitude = 111,000 meters
        var latDelta = radiusMeters / 111000.0;
        var lonDelta = radiusMeters / (111000.0 * Math.Cos(latitude * Math.PI / 180));

        var locations = await context.Locations
            .Where(l =>
                l.Latitude >= latitude - latDelta &&
                l.Latitude <= latitude + latDelta &&
                l.Longitude >= longitude - lonDelta &&
                l.Longitude <= longitude + lonDelta)
            .ToListAsync(cancellationToken);

        // Filter by actual distance using Haversine formula
        return locations.Where(l =>
            CalculateHaversineDistance(latitude, longitude, l.Latitude, l.Longitude) <= radiusMeters);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Location")] },
            Params =
            [
                new ParamModel { Name = "location", Type = new TypeModel("Location") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"location.LocationId = Guid.NewGuid();
        location.CreatedAt = DateTime.UtcNow;
        await context.Locations.AddAsync(location, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return location;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "UpdateAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "location", Type = new TypeModel("Location") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"location.UpdatedAt = DateTime.UtcNow;
        context.Locations.Update(location);
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
                new ParamModel { Name = "locationId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var location = await context.Locations.FindAsync(new object[] { locationId }, cancellationToken);
        if (location != null)
        {
            context.Locations.Remove(location);
            await context.SaveChangesAsync(cancellationToken);
        }")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "CalculateHaversineDistance",
            AccessModifier = AccessModifier.Private,
            Static = true,
            ReturnType = new TypeModel("double"),
            Params =
            [
                new ParamModel { Name = "lat1", Type = new TypeModel("double") },
                new ParamModel { Name = "lon1", Type = new TypeModel("double") },
                new ParamModel { Name = "lat2", Type = new TypeModel("double") },
                new ParamModel { Name = "lon2", Type = new TypeModel("double") }
            ],
            Body = new ExpressionModel(@"const double EarthRadiusMeters = 6371000;

        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusMeters * c;")
        });

        return new CodeFileModel<ClassModel>(classModel, "LocationRepository", directory, CSharp)
        {
            Namespace = "Geolocation.Infrastructure.Repositories"
        };
    }

    private static CodeFileModel<ClassModel> CreateGeocodingServiceFile(string directory)
    {
        var classModel = new ClassModel("GeocodingService");

        classModel.Usings.Add(new UsingModel("Geolocation.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Geolocation.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.Configuration"));
        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.Logging"));

        classModel.Implements.Add(new TypeModel("IGeocodingService"));

        classModel.Fields.Add(new FieldModel { Name = "configuration", Type = new TypeModel("IConfiguration"), AccessModifier = AccessModifier.Private, Readonly = true });
        classModel.Fields.Add(new FieldModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("GeocodingService")] }, AccessModifier = AccessModifier.Private, Readonly = true });
        classModel.Fields.Add(new FieldModel { Name = "httpClient", Type = new TypeModel("HttpClient"), AccessModifier = AccessModifier.Private, Readonly = true });

        var constructor = new ConstructorModel(classModel, "GeocodingService")
        {
            AccessModifier = AccessModifier.Public,
            Params =
            [
                new ParamModel { Name = "configuration", Type = new TypeModel("IConfiguration") },
                new ParamModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("GeocodingService")] } },
                new ParamModel { Name = "httpClient", Type = new TypeModel("HttpClient") }
            ],
            Body = new ExpressionModel(@"this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "GeocodeAddressAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Location") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "address", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"logger.LogInformation(""Geocoding address: {Address}"", address);

        // Placeholder implementation - integrate with actual geocoding provider
        // (e.g., Google Maps, Azure Maps, OpenStreetMap Nominatim)
        await Task.CompletedTask;

        return new Location
        {
            Address = address,
            Latitude = 0,
            Longitude = 0
        };")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "ReverseGeocodeAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("string") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "latitude", Type = new TypeModel("double") },
                new ParamModel { Name = "longitude", Type = new TypeModel("double") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"logger.LogInformation(""Reverse geocoding: {Latitude}, {Longitude}"", latitude, longitude);

        // Placeholder implementation - integrate with actual geocoding provider
        await Task.CompletedTask;

        return $""{latitude}, {longitude}"";")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "SearchAddressAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Location")] }] },
            Params =
            [
                new ParamModel { Name = "query", Type = new TypeModel("string") },
                new ParamModel { Name = "maxResults", Type = new TypeModel("int"), DefaultValue = "10" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"logger.LogInformation(""Searching addresses: {Query}"", query);

        // Placeholder implementation - integrate with actual geocoding provider
        await Task.CompletedTask;

        return Array.Empty<Location>();")
        });

        return new CodeFileModel<ClassModel>(classModel, "GeocodingService", directory, CSharp)
        {
            Namespace = "Geolocation.Infrastructure.Services"
        };
    }

    private static CodeFileModel<ClassModel> CreateRoutingServiceFile(string directory)
    {
        var classModel = new ClassModel("RoutingService");

        classModel.Usings.Add(new UsingModel("Geolocation.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Geolocation.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.Configuration"));
        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.Logging"));

        classModel.Implements.Add(new TypeModel("IRoutingService"));

        classModel.Fields.Add(new FieldModel { Name = "configuration", Type = new TypeModel("IConfiguration"), AccessModifier = AccessModifier.Private, Readonly = true });
        classModel.Fields.Add(new FieldModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("RoutingService")] }, AccessModifier = AccessModifier.Private, Readonly = true });
        classModel.Fields.Add(new FieldModel { Name = "httpClient", Type = new TypeModel("HttpClient"), AccessModifier = AccessModifier.Private, Readonly = true });

        var constructor = new ConstructorModel(classModel, "RoutingService")
        {
            AccessModifier = AccessModifier.Public,
            Params =
            [
                new ParamModel { Name = "configuration", Type = new TypeModel("IConfiguration") },
                new ParamModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("RoutingService")] } },
                new ParamModel { Name = "httpClient", Type = new TypeModel("HttpClient") }
            ],
            Body = new ExpressionModel(@"this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "CalculateRouteAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Route") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "startLat", Type = new TypeModel("double") },
                new ParamModel { Name = "startLon", Type = new TypeModel("double") },
                new ParamModel { Name = "endLat", Type = new TypeModel("double") },
                new ParamModel { Name = "endLon", Type = new TypeModel("double") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"logger.LogInformation(""Calculating route from ({StartLat}, {StartLon}) to ({EndLat}, {EndLon})"",
            startLat, startLon, endLat, endLon);

        var distance = await CalculateDistanceAsync(startLat, startLon, endLat, endLon, cancellationToken);

        return new Route
        {
            Name = ""Calculated Route"",
            StartLatitude = startLat,
            StartLongitude = startLon,
            EndLatitude = endLat,
            EndLongitude = endLon,
            DistanceMeters = distance,
            DurationSeconds = (int)(distance / 13.89) // Approximate driving speed of 50 km/h
        };")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "CalculateDistanceAsync",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("double")] },
            Params =
            [
                new ParamModel { Name = "lat1", Type = new TypeModel("double") },
                new ParamModel { Name = "lon1", Type = new TypeModel("double") },
                new ParamModel { Name = "lat2", Type = new TypeModel("double") },
                new ParamModel { Name = "lon2", Type = new TypeModel("double") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"// Haversine formula for calculating distance between two points
        const double EarthRadiusMeters = 6371000;

        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return Task.FromResult(EarthRadiusMeters * c);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "CalculateRouteWithWaypointsAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Route") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "waypoints", Type = new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("(double Latitude, double Longitude)")] } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var waypointList = waypoints.ToList();

        if (waypointList.Count < 2)
        {
            logger.LogWarning(""At least 2 waypoints are required to calculate a route"");
            return null;
        }

        var start = waypointList.First();
        var end = waypointList.Last();

        double totalDistance = 0;
        for (int i = 0; i < waypointList.Count - 1; i++)
        {
            totalDistance += await CalculateDistanceAsync(
                waypointList[i].Latitude, waypointList[i].Longitude,
                waypointList[i + 1].Latitude, waypointList[i + 1].Longitude,
                cancellationToken);
        }

        return new Route
        {
            Name = ""Multi-waypoint Route"",
            StartLatitude = start.Latitude,
            StartLongitude = start.Longitude,
            EndLatitude = end.Latitude,
            EndLongitude = end.Longitude,
            DistanceMeters = totalDistance,
            DurationSeconds = (int)(totalDistance / 13.89),
            Waypoints = System.Text.Json.JsonSerializer.Serialize(waypointList)
        };")
        });

        return new CodeFileModel<ClassModel>(classModel, "RoutingService", directory, CSharp)
        {
            Namespace = "Geolocation.Infrastructure.Services"
        };
    }

    private static CodeFileModel<ClassModel> CreateInfrastructureConfigureServicesFile(string directory)
    {
        var classModel = new ClassModel("ConfigureServices")
        {
            Static = true
        };

        classModel.Usings.Add(new UsingModel("Geolocation.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Geolocation.Infrastructure.Data"));
        classModel.Usings.Add(new UsingModel("Geolocation.Infrastructure.Repositories"));
        classModel.Usings.Add(new UsingModel("Geolocation.Infrastructure.Services"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.Configuration"));

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddGeolocationInfrastructure",
            AccessModifier = AccessModifier.Public,
            Static = true,
            ReturnType = new TypeModel("IServiceCollection"),
            Params =
            [
                new ParamModel { Name = "services", Type = new TypeModel("IServiceCollection"), ExtensionMethodParam = true },
                new ParamModel { Name = "configuration", Type = new TypeModel("IConfiguration") }
            ],
            Body = new ExpressionModel(@"services.AddDbContext<GeolocationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString(""GeolocationDb"") ??
                @""Server=.\SQLEXPRESS;Database=GeolocationDb;Trusted_Connection=True;TrustServerCertificate=True""));

        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddHttpClient<IGeocodingService, GeocodingService>();
        services.AddHttpClient<IRoutingService, RoutingService>();

        return services;")
        });

        return new CodeFileModel<ClassModel>(classModel, "ConfigureServices", directory, CSharp)
        {
            Namespace = "Microsoft.Extensions.DependencyInjection"
        };
    }

    #endregion

    #region API Layer Files

    private static CodeFileModel<ClassModel> CreateLocationsControllerFile(string directory)
    {
        var classModel = new ClassModel("LocationsController");

        classModel.Usings.Add(new UsingModel("Geolocation.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("Geolocation.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Geolocation.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.Authorization"));
        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.Mvc"));

        classModel.Implements.Add(new TypeModel("ControllerBase"));

        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ApiController" });
        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Route", Template = "\"api/[controller]\"" });
        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Authorize" });

        classModel.Fields.Add(new FieldModel { Name = "locationRepository", Type = new TypeModel("ILocationRepository"), AccessModifier = AccessModifier.Private, Readonly = true });
        classModel.Fields.Add(new FieldModel { Name = "geocodingService", Type = new TypeModel("IGeocodingService"), AccessModifier = AccessModifier.Private, Readonly = true });
        classModel.Fields.Add(new FieldModel { Name = "routingService", Type = new TypeModel("IRoutingService"), AccessModifier = AccessModifier.Private, Readonly = true });
        classModel.Fields.Add(new FieldModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("LocationsController")] }, AccessModifier = AccessModifier.Private, Readonly = true });

        var constructor = new ConstructorModel(classModel, "LocationsController")
        {
            AccessModifier = AccessModifier.Public,
            Params =
            [
                new ParamModel { Name = "locationRepository", Type = new TypeModel("ILocationRepository") },
                new ParamModel { Name = "geocodingService", Type = new TypeModel("IGeocodingService") },
                new ParamModel { Name = "routingService", Type = new TypeModel("IRoutingService") },
                new ParamModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("LocationsController")] } }
            ],
            Body = new ExpressionModel(@"this.locationRepository = locationRepository;
        this.geocodingService = geocodingService;
        this.routingService = routingService;
        this.logger = logger;")
        };
        classModel.Constructors.Add(constructor);

        var createMethod = new MethodModel
        {
            Name = "Create",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("LocationDto")] }] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("LocationDto"), Attribute = "[FromBody]" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var location = new Location
        {
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Altitude = request.Altitude,
            Accuracy = request.Accuracy,
            Address = request.Address,
            City = request.City,
            State = request.State,
            Country = request.Country,
            PostalCode = request.PostalCode,
            EntityId = request.EntityId,
            EntityType = request.EntityType,
            Timestamp = request.Timestamp
        };

        var createdLocation = await locationRepository.AddAsync(location, cancellationToken);

        logger.LogInformation(""Location {LocationId} created at ({Latitude}, {Longitude})"",
            createdLocation.LocationId, createdLocation.Latitude, createdLocation.Longitude);

        var response = new LocationDto
        {
            LocationId = createdLocation.LocationId,
            Latitude = createdLocation.Latitude,
            Longitude = createdLocation.Longitude,
            Altitude = createdLocation.Altitude,
            Accuracy = createdLocation.Accuracy,
            Address = createdLocation.Address,
            City = createdLocation.City,
            State = createdLocation.State,
            Country = createdLocation.Country,
            PostalCode = createdLocation.PostalCode,
            EntityId = createdLocation.EntityId,
            EntityType = createdLocation.EntityType,
            Timestamp = createdLocation.Timestamp
        };

        return CreatedAtAction(nameof(GetById), new { id = createdLocation.LocationId }, response);")
        };
        createMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpPost" });
        createMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "typeof(LocationDto), StatusCodes.Status201Created" });
        createMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "StatusCodes.Status400BadRequest" });
        classModel.Methods.Add(createMethod);

        var getByIdMethod = new MethodModel
        {
            Name = "GetById",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("LocationDto")] }] },
            Params =
            [
                new ParamModel { Name = "id", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var location = await locationRepository.GetByIdAsync(id, cancellationToken);

        if (location == null)
        {
            return NotFound();
        }

        return Ok(new LocationDto
        {
            LocationId = location.LocationId,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            Altitude = location.Altitude,
            Accuracy = location.Accuracy,
            Address = location.Address,
            City = location.City,
            State = location.State,
            Country = location.Country,
            PostalCode = location.PostalCode,
            EntityId = location.EntityId,
            EntityType = location.EntityType,
            Timestamp = location.Timestamp
        });")
        };
        getByIdMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"{id:guid}\"" });
        getByIdMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "typeof(LocationDto), StatusCodes.Status200OK" });
        getByIdMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "StatusCodes.Status404NotFound" });
        classModel.Methods.Add(getByIdMethod);

        var geocodeMethod = new MethodModel
        {
            Name = "Geocode",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("GeocodeResponse")] }] },
            Params =
            [
                new ParamModel { Name = "address", Type = new TypeModel("string"), Attribute = "[FromQuery]" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"if (string.IsNullOrWhiteSpace(address))
        {
            return BadRequest(new { error = ""Address is required"" });
        }

        var location = await geocodingService.GeocodeAddressAsync(address, cancellationToken);

        if (location == null)
        {
            return NotFound(new { error = ""Address not found"" });
        }

        return Ok(new GeocodeResponse
        {
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            FormattedAddress = location.Address,
            City = location.City,
            State = location.State,
            Country = location.Country,
            PostalCode = location.PostalCode
        });")
        };
        geocodeMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"geocode\"" });
        geocodeMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "typeof(GeocodeResponse), StatusCodes.Status200OK" });
        geocodeMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "StatusCodes.Status400BadRequest" });
        geocodeMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "StatusCodes.Status404NotFound" });
        classModel.Methods.Add(geocodeMethod);

        var getDistanceMethod = new MethodModel
        {
            Name = "GetDistance",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("DistanceResponse")] }] },
            Params =
            [
                new ParamModel { Name = "fromLatitude", Type = new TypeModel("double"), Attribute = "[FromQuery]" },
                new ParamModel { Name = "fromLongitude", Type = new TypeModel("double"), Attribute = "[FromQuery]" },
                new ParamModel { Name = "toLatitude", Type = new TypeModel("double"), Attribute = "[FromQuery]" },
                new ParamModel { Name = "toLongitude", Type = new TypeModel("double"), Attribute = "[FromQuery]" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var distanceMeters = await routingService.CalculateDistanceAsync(
            fromLatitude, fromLongitude, toLatitude, toLongitude, cancellationToken);

        return Ok(new DistanceResponse
        {
            DistanceMeters = distanceMeters,
            DistanceKilometers = distanceMeters / 1000,
            DistanceMiles = distanceMeters / 1609.344,
            EstimatedDurationSeconds = (int)(distanceMeters / 13.89) // ~50 km/h average speed
        });")
        };
        getDistanceMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"distance\"" });
        getDistanceMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "typeof(DistanceResponse), StatusCodes.Status200OK" });
        getDistanceMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "StatusCodes.Status400BadRequest" });
        classModel.Methods.Add(getDistanceMethod);

        var getAllMethod = new MethodModel
        {
            Name = "GetAll",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("LocationDto")] }] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var locations = await locationRepository.GetAllAsync(cancellationToken);

        var locationDtos = locations.Select(l => new LocationDto
        {
            LocationId = l.LocationId,
            Latitude = l.Latitude,
            Longitude = l.Longitude,
            Altitude = l.Altitude,
            Accuracy = l.Accuracy,
            Address = l.Address,
            City = l.City,
            State = l.State,
            Country = l.Country,
            PostalCode = l.PostalCode,
            EntityId = l.EntityId,
            EntityType = l.EntityType,
            Timestamp = l.Timestamp
        });

        return Ok(locationDtos);")
        };
        getAllMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet" });
        getAllMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "typeof(IEnumerable<LocationDto>), StatusCodes.Status200OK" });
        classModel.Methods.Add(getAllMethod);

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
            Body = new ExpressionModel(@"var location = await locationRepository.GetByIdAsync(id, cancellationToken);

        if (location == null)
        {
            return NotFound();
        }

        await locationRepository.DeleteAsync(id, cancellationToken);
        logger.LogInformation(""Location {LocationId} deleted"", id);

        return NoContent();")
        };
        deleteMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpDelete", Template = "\"{id:guid}\"" });
        deleteMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "StatusCodes.Status204NoContent" });
        deleteMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "StatusCodes.Status404NotFound" });
        classModel.Methods.Add(deleteMethod);

        return new CodeFileModel<ClassModel>(classModel, "LocationsController", directory, CSharp)
        {
            Namespace = "Geolocation.Api.Controllers"
        };
    }

    private static FileModel CreateProgramFile(string directory)
    {
        return new FileModel("Program", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.Text;
                using Microsoft.AspNetCore.Authentication.JwtBearer;
                using Microsoft.IdentityModel.Tokens;
                using Microsoft.OpenApi.Models;
                using Serilog;

                var builder = WebApplication.CreateBuilder(args);

                // Configure Serilog
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(builder.Configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .CreateLogger();

                builder.Host.UseSerilog();

                // Add services
                builder.Services.AddGeolocationInfrastructure(builder.Configuration);

                // Configure JWT authentication
                builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = builder.Configuration["Jwt:Issuer"],
                            ValidAudience = builder.Configuration["Jwt:Audience"],
                            IssuerSigningKey = new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured")))
                        };
                    });

                builder.Services.AddAuthorization();
                builder.Services.AddControllers();

                // Configure Swagger
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "Geolocation API",
                        Version = "v1",
                        Description = "Geolocation microservice for location tracking, geocoding, and routing"
                    });

                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Description = "JWT Authorization header using the Bearer scheme",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer"
                    });

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            Array.Empty<string>()
                        }
                    });
                });

                // Configure CORS
                builder.Services.AddCors(options =>
                {
                    options.AddDefaultPolicy(policy =>
                    {
                        policy.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
                });

                // Add health checks
                builder.Services.AddHealthChecks();

                var app = builder.Build();

                // Configure pipeline
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();
                app.UseCors();
                app.UseAuthentication();
                app.UseAuthorization();

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
                    "GeolocationDb": "Server=.\\SQLEXPRESS;Database=GeolocationDb;Trusted_Connection=True;TrustServerCertificate=True"
                  },
                  "Jwt": {
                    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
                    "Issuer": "Geolocation.Api",
                    "Audience": "Geolocation.Api",
                    "ExpirationHours": "24"
                  },
                  "Serilog": {
                    "MinimumLevel": {
                      "Default": "Information",
                      "Override": {
                        "Microsoft": "Warning",
                        "Microsoft.Hosting.Lifetime": "Information"
                      }
                    }
                  },
                  "AllowedHosts": "*"
                }
                """
        };
    }

    private static FileModel CreateAppSettingsDevelopmentFile(string directory)
    {
        return new FileModel("appsettings.Development", directory, ".json")
        {
            Body = """
                {
                  "Serilog": {
                    "MinimumLevel": {
                      "Default": "Debug"
                    }
                  }
                }
                """
        };
    }

    #endregion
}
