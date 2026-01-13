// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.DotNet.Artifacts.Projects;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.Geolocation;

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

    private static FileModel CreateIAggregateRootFile(string directory)
    {
        return new FileModel("IAggregateRoot", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Geolocation.Core.Entities;

                /// <summary>
                /// Marker interface for aggregate roots.
                /// </summary>
                public interface IAggregateRoot
                {
                }
                """
        };
    }

    private static FileModel CreateLocationEntityFile(string directory)
    {
        return new FileModel("Location", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Geolocation.Core.Entities;

                /// <summary>
                /// Location entity representing a geographic location.
                /// </summary>
                public class Location : IAggregateRoot
                {
                    public Guid LocationId { get; set; }

                    public double Latitude { get; set; }

                    public double Longitude { get; set; }

                    public double? Altitude { get; set; }

                    public double? Accuracy { get; set; }

                    public string? Address { get; set; }

                    public string? City { get; set; }

                    public string? State { get; set; }

                    public string? Country { get; set; }

                    public string? PostalCode { get; set; }

                    public string? EntityId { get; set; }

                    public string? EntityType { get; set; }

                    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

                    public DateTime? UpdatedAt { get; set; }
                }
                """
        };
    }

    private static FileModel CreateGeoFenceEntityFile(string directory)
    {
        return new FileModel("GeoFence", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Geolocation.Core.Entities;

                /// <summary>
                /// GeoFence entity representing a geographic boundary.
                /// </summary>
                public class GeoFence : IAggregateRoot
                {
                    public Guid GeoFenceId { get; set; }

                    public required string Name { get; set; }

                    public string? Description { get; set; }

                    public double CenterLatitude { get; set; }

                    public double CenterLongitude { get; set; }

                    public double RadiusMeters { get; set; }

                    public string? Polygon { get; set; }

                    public bool IsActive { get; set; } = true;

                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

                    public DateTime? UpdatedAt { get; set; }
                }
                """
        };
    }

    private static FileModel CreateRouteEntityFile(string directory)
    {
        return new FileModel("Route", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Geolocation.Core.Entities;

                /// <summary>
                /// Route entity representing a path between locations.
                /// </summary>
                public class Route : IAggregateRoot
                {
                    public Guid RouteId { get; set; }

                    public required string Name { get; set; }

                    public string? Description { get; set; }

                    public double StartLatitude { get; set; }

                    public double StartLongitude { get; set; }

                    public double EndLatitude { get; set; }

                    public double EndLongitude { get; set; }

                    public double? DistanceMeters { get; set; }

                    public int? DurationSeconds { get; set; }

                    public string? EncodedPolyline { get; set; }

                    public string? Waypoints { get; set; }

                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

                    public DateTime? UpdatedAt { get; set; }
                }
                """
        };
    }

    private static FileModel CreateIDomainEventFile(string directory)
    {
        return new FileModel("IDomainEvent", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Geolocation.Core.Interfaces;

                /// <summary>
                /// Interface for domain events.
                /// </summary>
                public interface IDomainEvent
                {
                    Guid AggregateId { get; }

                    string AggregateType { get; }

                    DateTime OccurredAt { get; }

                    string CorrelationId { get; }
                }
                """
        };
    }

    private static FileModel CreateILocationRepositoryFile(string directory)
    {
        return new FileModel("ILocationRepository", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Geolocation.Core.Entities;

                namespace Geolocation.Core.Interfaces;

                /// <summary>
                /// Repository interface for Location entities.
                /// </summary>
                public interface ILocationRepository
                {
                    Task<Location?> GetByIdAsync(Guid locationId, CancellationToken cancellationToken = default);

                    Task<IEnumerable<Location>> GetAllAsync(CancellationToken cancellationToken = default);

                    Task<IEnumerable<Location>> GetByEntityAsync(string entityId, string entityType, CancellationToken cancellationToken = default);

                    Task<IEnumerable<Location>> GetWithinRadiusAsync(double latitude, double longitude, double radiusMeters, CancellationToken cancellationToken = default);

                    Task<Location> AddAsync(Location location, CancellationToken cancellationToken = default);

                    Task UpdateAsync(Location location, CancellationToken cancellationToken = default);

                    Task DeleteAsync(Guid locationId, CancellationToken cancellationToken = default);
                }
                """
        };
    }

    private static FileModel CreateIGeocodingServiceFile(string directory)
    {
        return new FileModel("IGeocodingService", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Geolocation.Core.Entities;

                namespace Geolocation.Core.Interfaces;

                /// <summary>
                /// Service interface for geocoding operations.
                /// </summary>
                public interface IGeocodingService
                {
                    Task<Location?> GeocodeAddressAsync(string address, CancellationToken cancellationToken = default);

                    Task<string?> ReverseGeocodeAsync(double latitude, double longitude, CancellationToken cancellationToken = default);

                    Task<IEnumerable<Location>> SearchAddressAsync(string query, int maxResults = 10, CancellationToken cancellationToken = default);
                }
                """
        };
    }

    private static FileModel CreateIRoutingServiceFile(string directory)
    {
        return new FileModel("IRoutingService", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Geolocation.Core.Entities;

                namespace Geolocation.Core.Interfaces;

                /// <summary>
                /// Service interface for routing operations.
                /// </summary>
                public interface IRoutingService
                {
                    Task<Route?> CalculateRouteAsync(double startLat, double startLon, double endLat, double endLon, CancellationToken cancellationToken = default);

                    Task<double> CalculateDistanceAsync(double lat1, double lon1, double lat2, double lon2, CancellationToken cancellationToken = default);

                    Task<Route?> CalculateRouteWithWaypointsAsync(IEnumerable<(double Latitude, double Longitude)> waypoints, CancellationToken cancellationToken = default);
                }
                """
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

    private static FileModel CreateLocationDtoFile(string directory)
    {
        return new FileModel("LocationDto", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Geolocation.Core.DTOs;

                /// <summary>
                /// Data transfer object for Location.
                /// </summary>
                public sealed class LocationDto
                {
                    public Guid LocationId { get; init; }

                    public double Latitude { get; init; }

                    public double Longitude { get; init; }

                    public double? Altitude { get; init; }

                    public double? Accuracy { get; init; }

                    public string? Address { get; init; }

                    public string? City { get; init; }

                    public string? State { get; init; }

                    public string? Country { get; init; }

                    public string? PostalCode { get; init; }

                    public string? EntityId { get; init; }

                    public string? EntityType { get; init; }

                    public DateTime Timestamp { get; init; }
                }
                """
        };
    }

    private static FileModel CreateGeoFenceDtoFile(string directory)
    {
        return new FileModel("GeoFenceDto", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Geolocation.Core.DTOs;

                /// <summary>
                /// Data transfer object for GeoFence.
                /// </summary>
                public sealed class GeoFenceDto
                {
                    public Guid GeoFenceId { get; init; }

                    public required string Name { get; init; }

                    public string? Description { get; init; }

                    public double CenterLatitude { get; init; }

                    public double CenterLongitude { get; init; }

                    public double RadiusMeters { get; init; }

                    public bool IsActive { get; init; }
                }
                """
        };
    }

    private static FileModel CreateRouteDtoFile(string directory)
    {
        return new FileModel("RouteDto", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Geolocation.Core.DTOs;

                /// <summary>
                /// Data transfer object for Route.
                /// </summary>
                public sealed class RouteDto
                {
                    public Guid RouteId { get; init; }

                    public required string Name { get; init; }

                    public string? Description { get; init; }

                    public double StartLatitude { get; init; }

                    public double StartLongitude { get; init; }

                    public double EndLatitude { get; init; }

                    public double EndLongitude { get; init; }

                    public double? DistanceMeters { get; init; }

                    public int? DurationSeconds { get; init; }

                    public string? EncodedPolyline { get; init; }
                }
                """
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

    private static FileModel CreateGeocodeResponseFile(string directory)
    {
        return new FileModel("GeocodeResponse", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Geolocation.Core.DTOs;

                /// <summary>
                /// Response model for geocoding result.
                /// </summary>
                public sealed class GeocodeResponse
                {
                    public double Latitude { get; init; }

                    public double Longitude { get; init; }

                    public string? FormattedAddress { get; init; }

                    public string? City { get; init; }

                    public string? State { get; init; }

                    public string? Country { get; init; }

                    public string? PostalCode { get; init; }
                }
                """
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

    private static FileModel CreateDistanceResponseFile(string directory)
    {
        return new FileModel("DistanceResponse", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Geolocation.Core.DTOs;

                /// <summary>
                /// Response model for distance calculation result.
                /// </summary>
                public sealed class DistanceResponse
                {
                    public double DistanceMeters { get; init; }

                    public double DistanceKilometers { get; init; }

                    public double DistanceMiles { get; init; }

                    public int? EstimatedDurationSeconds { get; init; }
                }
                """
        };
    }

    #endregion

    #region Infrastructure Layer Files

    private static FileModel CreateGeolocationDbContextFile(string directory)
    {
        return new FileModel("GeolocationDbContext", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Geolocation.Core.Entities;
                using Microsoft.EntityFrameworkCore;

                namespace Geolocation.Infrastructure.Data;

                /// <summary>
                /// Entity Framework Core DbContext for Geolocation microservice.
                /// </summary>
                public class GeolocationDbContext : DbContext
                {
                    public GeolocationDbContext(DbContextOptions<GeolocationDbContext> options)
                        : base(options)
                    {
                    }

                    public DbSet<Location> Locations => Set<Location>();

                    public DbSet<GeoFence> GeoFences => Set<GeoFence>();

                    public DbSet<Route> Routes => Set<Route>();

                    protected override void OnModelCreating(ModelBuilder modelBuilder)
                    {
                        base.OnModelCreating(modelBuilder);
                        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GeolocationDbContext).Assembly);
                    }
                }
                """
        };
    }

    private static FileModel CreateLocationConfigurationFile(string directory)
    {
        return new FileModel("LocationConfiguration", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Geolocation.Core.Entities;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.EntityFrameworkCore.Metadata.Builders;

                namespace Geolocation.Infrastructure.Data.Configurations;

                /// <summary>
                /// Entity configuration for Location.
                /// </summary>
                public class LocationConfiguration : IEntityTypeConfiguration<Location>
                {
                    public void Configure(EntityTypeBuilder<Location> builder)
                    {
                        builder.HasKey(l => l.LocationId);

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

                        builder.HasIndex(l => new { l.EntityId, l.EntityType });
                    }
                }
                """
        };
    }

    private static FileModel CreateGeoFenceConfigurationFile(string directory)
    {
        return new FileModel("GeoFenceConfiguration", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Geolocation.Core.Entities;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.EntityFrameworkCore.Metadata.Builders;

                namespace Geolocation.Infrastructure.Data.Configurations;

                /// <summary>
                /// Entity configuration for GeoFence.
                /// </summary>
                public class GeoFenceConfiguration : IEntityTypeConfiguration<GeoFence>
                {
                    public void Configure(EntityTypeBuilder<GeoFence> builder)
                    {
                        builder.HasKey(g => g.GeoFenceId);

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

                        builder.HasIndex(g => new { g.CenterLatitude, g.CenterLongitude });
                    }
                }
                """
        };
    }

    private static FileModel CreateRouteConfigurationFile(string directory)
    {
        return new FileModel("RouteConfiguration", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Geolocation.Core.Entities;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.EntityFrameworkCore.Metadata.Builders;

                namespace Geolocation.Infrastructure.Data.Configurations;

                /// <summary>
                /// Entity configuration for Route.
                /// </summary>
                public class RouteConfiguration : IEntityTypeConfiguration<Route>
                {
                    public void Configure(EntityTypeBuilder<Route> builder)
                    {
                        builder.HasKey(r => r.RouteId);

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

                        builder.HasIndex(r => r.Name);
                    }
                }
                """
        };
    }

    private static FileModel CreateLocationRepositoryFile(string directory)
    {
        return new FileModel("LocationRepository", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Geolocation.Core.Entities;
                using Geolocation.Core.Interfaces;
                using Geolocation.Infrastructure.Data;
                using Microsoft.EntityFrameworkCore;

                namespace Geolocation.Infrastructure.Repositories;

                /// <summary>
                /// Repository implementation for Location entities.
                /// </summary>
                public class LocationRepository : ILocationRepository
                {
                    private readonly GeolocationDbContext context;

                    public LocationRepository(GeolocationDbContext context)
                    {
                        this.context = context ?? throw new ArgumentNullException(nameof(context));
                    }

                    public async Task<Location?> GetByIdAsync(Guid locationId, CancellationToken cancellationToken = default)
                    {
                        return await context.Locations
                            .FirstOrDefaultAsync(l => l.LocationId == locationId, cancellationToken);
                    }

                    public async Task<IEnumerable<Location>> GetAllAsync(CancellationToken cancellationToken = default)
                    {
                        return await context.Locations
                            .OrderByDescending(l => l.Timestamp)
                            .ToListAsync(cancellationToken);
                    }

                    public async Task<IEnumerable<Location>> GetByEntityAsync(string entityId, string entityType, CancellationToken cancellationToken = default)
                    {
                        return await context.Locations
                            .Where(l => l.EntityId == entityId && l.EntityType == entityType)
                            .OrderByDescending(l => l.Timestamp)
                            .ToListAsync(cancellationToken);
                    }

                    public async Task<IEnumerable<Location>> GetWithinRadiusAsync(double latitude, double longitude, double radiusMeters, CancellationToken cancellationToken = default)
                    {
                        // Approximate conversion: 1 degree latitude = 111,000 meters
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
                            CalculateHaversineDistance(latitude, longitude, l.Latitude, l.Longitude) <= radiusMeters);
                    }

                    public async Task<Location> AddAsync(Location location, CancellationToken cancellationToken = default)
                    {
                        location.LocationId = Guid.NewGuid();
                        location.CreatedAt = DateTime.UtcNow;
                        await context.Locations.AddAsync(location, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                        return location;
                    }

                    public async Task UpdateAsync(Location location, CancellationToken cancellationToken = default)
                    {
                        location.UpdatedAt = DateTime.UtcNow;
                        context.Locations.Update(location);
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    public async Task DeleteAsync(Guid locationId, CancellationToken cancellationToken = default)
                    {
                        var location = await context.Locations.FindAsync(new object[] { locationId }, cancellationToken);
                        if (location != null)
                        {
                            context.Locations.Remove(location);
                            await context.SaveChangesAsync(cancellationToken);
                        }
                    }

                    private static double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
                    {
                        const double EarthRadiusMeters = 6371000;

                        var dLat = (lat2 - lat1) * Math.PI / 180;
                        var dLon = (lon2 - lon1) * Math.PI / 180;

                        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

                        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

                        return EarthRadiusMeters * c;
                    }
                }
                """
        };
    }

    private static FileModel CreateGeocodingServiceFile(string directory)
    {
        return new FileModel("GeocodingService", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Geolocation.Core.Entities;
                using Geolocation.Core.Interfaces;
                using Microsoft.Extensions.Configuration;
                using Microsoft.Extensions.Logging;

                namespace Geolocation.Infrastructure.Services;

                /// <summary>
                /// Geocoding service implementation.
                /// </summary>
                public class GeocodingService : IGeocodingService
                {
                    private readonly IConfiguration configuration;
                    private readonly ILogger<GeocodingService> logger;
                    private readonly HttpClient httpClient;

                    public GeocodingService(
                        IConfiguration configuration,
                        ILogger<GeocodingService> logger,
                        HttpClient httpClient)
                    {
                        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
                        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
                        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
                    }

                    public async Task<Location?> GeocodeAddressAsync(string address, CancellationToken cancellationToken = default)
                    {
                        logger.LogInformation("Geocoding address: {Address}", address);

                        // Placeholder implementation - integrate with actual geocoding provider
                        // (e.g., Google Maps, Azure Maps, OpenStreetMap Nominatim)
                        await Task.CompletedTask;

                        return new Location
                        {
                            Address = address,
                            Latitude = 0,
                            Longitude = 0
                        };
                    }

                    public async Task<string?> ReverseGeocodeAsync(double latitude, double longitude, CancellationToken cancellationToken = default)
                    {
                        logger.LogInformation("Reverse geocoding: {Latitude}, {Longitude}", latitude, longitude);

                        // Placeholder implementation - integrate with actual geocoding provider
                        await Task.CompletedTask;

                        return $"{latitude}, {longitude}";
                    }

                    public async Task<IEnumerable<Location>> SearchAddressAsync(string query, int maxResults = 10, CancellationToken cancellationToken = default)
                    {
                        logger.LogInformation("Searching addresses: {Query}", query);

                        // Placeholder implementation - integrate with actual geocoding provider
                        await Task.CompletedTask;

                        return Array.Empty<Location>();
                    }
                }
                """
        };
    }

    private static FileModel CreateRoutingServiceFile(string directory)
    {
        return new FileModel("RoutingService", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Geolocation.Core.Entities;
                using Geolocation.Core.Interfaces;
                using Microsoft.Extensions.Configuration;
                using Microsoft.Extensions.Logging;

                namespace Geolocation.Infrastructure.Services;

                /// <summary>
                /// Routing service implementation.
                /// </summary>
                public class RoutingService : IRoutingService
                {
                    private readonly IConfiguration configuration;
                    private readonly ILogger<RoutingService> logger;
                    private readonly HttpClient httpClient;

                    public RoutingService(
                        IConfiguration configuration,
                        ILogger<RoutingService> logger,
                        HttpClient httpClient)
                    {
                        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
                        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
                        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
                    }

                    public async Task<Route?> CalculateRouteAsync(double startLat, double startLon, double endLat, double endLon, CancellationToken cancellationToken = default)
                    {
                        logger.LogInformation("Calculating route from ({StartLat}, {StartLon}) to ({EndLat}, {EndLon})",
                            startLat, startLon, endLat, endLon);

                        var distance = await CalculateDistanceAsync(startLat, startLon, endLat, endLon, cancellationToken);

                        return new Route
                        {
                            Name = "Calculated Route",
                            StartLatitude = startLat,
                            StartLongitude = startLon,
                            EndLatitude = endLat,
                            EndLongitude = endLon,
                            DistanceMeters = distance,
                            DurationSeconds = (int)(distance / 13.89) // Approximate driving speed of 50 km/h
                        };
                    }

                    public Task<double> CalculateDistanceAsync(double lat1, double lon1, double lat2, double lon2, CancellationToken cancellationToken = default)
                    {
                        // Haversine formula for calculating distance between two points
                        const double EarthRadiusMeters = 6371000;

                        var dLat = (lat2 - lat1) * Math.PI / 180;
                        var dLon = (lon2 - lon1) * Math.PI / 180;

                        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

                        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

                        return Task.FromResult(EarthRadiusMeters * c);
                    }

                    public async Task<Route?> CalculateRouteWithWaypointsAsync(IEnumerable<(double Latitude, double Longitude)> waypoints, CancellationToken cancellationToken = default)
                    {
                        var waypointList = waypoints.ToList();

                        if (waypointList.Count < 2)
                        {
                            logger.LogWarning("At least 2 waypoints are required to calculate a route");
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
                            Name = "Multi-waypoint Route",
                            StartLatitude = start.Latitude,
                            StartLongitude = start.Longitude,
                            EndLatitude = end.Latitude,
                            EndLongitude = end.Longitude,
                            DistanceMeters = totalDistance,
                            DurationSeconds = (int)(totalDistance / 13.89),
                            Waypoints = System.Text.Json.JsonSerializer.Serialize(waypointList)
                        };
                    }
                }
                """
        };
    }

    private static FileModel CreateInfrastructureConfigureServicesFile(string directory)
    {
        return new FileModel("ConfigureServices", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Geolocation.Core.Interfaces;
                using Geolocation.Infrastructure.Data;
                using Geolocation.Infrastructure.Repositories;
                using Geolocation.Infrastructure.Services;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.Extensions.Configuration;

                namespace Microsoft.Extensions.DependencyInjection;

                /// <summary>
                /// Extension methods for configuring Geolocation infrastructure services.
                /// </summary>
                public static class ConfigureServices
                {
                    /// <summary>
                    /// Adds Geolocation infrastructure services to the service collection.
                    /// </summary>
                    public static IServiceCollection AddGeolocationInfrastructure(
                        this IServiceCollection services,
                        IConfiguration configuration)
                    {
                        services.AddDbContext<GeolocationDbContext>(options =>
                            options.UseSqlServer(
                                configuration.GetConnectionString("GeolocationDb") ??
                                @"Server=.\SQLEXPRESS;Database=GeolocationDb;Trusted_Connection=True;TrustServerCertificate=True"));

                        services.AddScoped<ILocationRepository, LocationRepository>();
                        services.AddHttpClient<IGeocodingService, GeocodingService>();
                        services.AddHttpClient<IRoutingService, RoutingService>();

                        return services;
                    }
                }
                """
        };
    }

    #endregion

    #region API Layer Files

    private static FileModel CreateLocationsControllerFile(string directory)
    {
        return new FileModel("LocationsController", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Geolocation.Core.DTOs;
                using Geolocation.Core.Entities;
                using Geolocation.Core.Interfaces;
                using Microsoft.AspNetCore.Authorization;
                using Microsoft.AspNetCore.Mvc;

                namespace Geolocation.Api.Controllers;

                /// <summary>
                /// Controller for location operations.
                /// </summary>
                [ApiController]
                [Route("api/[controller]")]
                [Authorize]
                public class LocationsController : ControllerBase
                {
                    private readonly ILocationRepository locationRepository;
                    private readonly IGeocodingService geocodingService;
                    private readonly IRoutingService routingService;
                    private readonly ILogger<LocationsController> logger;

                    public LocationsController(
                        ILocationRepository locationRepository,
                        IGeocodingService geocodingService,
                        IRoutingService routingService,
                        ILogger<LocationsController> logger)
                    {
                        this.locationRepository = locationRepository;
                        this.geocodingService = geocodingService;
                        this.routingService = routingService;
                        this.logger = logger;
                    }

                    /// <summary>
                    /// Create a new location.
                    /// </summary>
                    [HttpPost]
                    [ProducesResponseType(typeof(LocationDto), StatusCodes.Status201Created)]
                    [ProducesResponseType(StatusCodes.Status400BadRequest)]
                    public async Task<ActionResult<LocationDto>> Create(
                        [FromBody] LocationDto request,
                        CancellationToken cancellationToken)
                    {
                        var location = new Location
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

                        logger.LogInformation("Location {LocationId} created at ({Latitude}, {Longitude})",
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

                        return CreatedAtAction(nameof(GetById), new { id = createdLocation.LocationId }, response);
                    }

                    /// <summary>
                    /// Get a location by ID.
                    /// </summary>
                    [HttpGet("{id:guid}")]
                    [ProducesResponseType(typeof(LocationDto), StatusCodes.Status200OK)]
                    [ProducesResponseType(StatusCodes.Status404NotFound)]
                    public async Task<ActionResult<LocationDto>> GetById(Guid id, CancellationToken cancellationToken)
                    {
                        var location = await locationRepository.GetByIdAsync(id, cancellationToken);

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
                        });
                    }

                    /// <summary>
                    /// Geocode an address to coordinates.
                    /// </summary>
                    [HttpGet("geocode")]
                    [ProducesResponseType(typeof(GeocodeResponse), StatusCodes.Status200OK)]
                    [ProducesResponseType(StatusCodes.Status400BadRequest)]
                    [ProducesResponseType(StatusCodes.Status404NotFound)]
                    public async Task<ActionResult<GeocodeResponse>> Geocode(
                        [FromQuery] string address,
                        CancellationToken cancellationToken)
                    {
                        if (string.IsNullOrWhiteSpace(address))
                        {
                            return BadRequest(new { error = "Address is required" });
                        }

                        var location = await geocodingService.GeocodeAddressAsync(address, cancellationToken);

                        if (location == null)
                        {
                            return NotFound(new { error = "Address not found" });
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
                        });
                    }

                    /// <summary>
                    /// Calculate distance between two points.
                    /// </summary>
                    [HttpGet("distance")]
                    [ProducesResponseType(typeof(DistanceResponse), StatusCodes.Status200OK)]
                    [ProducesResponseType(StatusCodes.Status400BadRequest)]
                    public async Task<ActionResult<DistanceResponse>> GetDistance(
                        [FromQuery] double fromLatitude,
                        [FromQuery] double fromLongitude,
                        [FromQuery] double toLatitude,
                        [FromQuery] double toLongitude,
                        CancellationToken cancellationToken)
                    {
                        var distanceMeters = await routingService.CalculateDistanceAsync(
                            fromLatitude, fromLongitude, toLatitude, toLongitude, cancellationToken);

                        return Ok(new DistanceResponse
                        {
                            DistanceMeters = distanceMeters,
                            DistanceKilometers = distanceMeters / 1000,
                            DistanceMiles = distanceMeters / 1609.344,
                            EstimatedDurationSeconds = (int)(distanceMeters / 13.89) // ~50 km/h average speed
                        });
                    }

                    /// <summary>
                    /// Get all locations.
                    /// </summary>
                    [HttpGet]
                    [ProducesResponseType(typeof(IEnumerable<LocationDto>), StatusCodes.Status200OK)]
                    public async Task<ActionResult<IEnumerable<LocationDto>>> GetAll(CancellationToken cancellationToken)
                    {
                        var locations = await locationRepository.GetAllAsync(cancellationToken);

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

                        return Ok(locationDtos);
                    }

                    /// <summary>
                    /// Delete a location.
                    /// </summary>
                    [HttpDelete("{id:guid}")]
                    [ProducesResponseType(StatusCodes.Status204NoContent)]
                    [ProducesResponseType(StatusCodes.Status404NotFound)]
                    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
                    {
                        var location = await locationRepository.GetByIdAsync(id, cancellationToken);

                        if (location == null)
                        {
                            return NotFound();
                        }

                        await locationRepository.DeleteAsync(id, cancellationToken);
                        logger.LogInformation("Location {LocationId} deleted", id);

                        return NoContent();
                    }
                }
                """
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
