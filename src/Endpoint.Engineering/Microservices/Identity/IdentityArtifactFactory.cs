// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.DotNet.Artifacts.Projects;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.Identity;

/// <summary>
/// Factory for creating Identity microservice artifacts according to identity-microservice.spec.md.
/// </summary>
public class IdentityArtifactFactory : IIdentityArtifactFactory
{
    private readonly ILogger<IdentityArtifactFactory> logger;

    public IdentityArtifactFactory(ILogger<IdentityArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding Identity.Core files");

        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var servicesDir = Path.Combine(project.Directory, "Services");

        // Entities
        project.Files.Add(CreateIAggregateRootFile(entitiesDir));
        project.Files.Add(CreateUserEntityFile(entitiesDir));
        project.Files.Add(CreateRoleEntityFile(entitiesDir));
        project.Files.Add(CreatePermissionEntityFile(entitiesDir));
        project.Files.Add(CreateUserRoleEntityFile(entitiesDir));
        project.Files.Add(CreateRolePermissionEntityFile(entitiesDir));

        // Interfaces
        project.Files.Add(CreateIDomainEventFile(interfacesDir));
        project.Files.Add(CreateIUserRepositoryFile(interfacesDir));
        project.Files.Add(CreateIRoleRepositoryFile(interfacesDir));
        project.Files.Add(CreateITokenServiceFile(interfacesDir));
        project.Files.Add(CreateIPasswordHasherFile(interfacesDir));

        // Events
        project.Files.Add(CreateUserCreatedEventFile(eventsDir));
        project.Files.Add(CreateUserAuthenticatedEventFile(eventsDir));
        project.Files.Add(CreateUserAuthenticationFailedEventFile(eventsDir));
        project.Files.Add(CreateUserUpdatedEventFile(eventsDir));
        project.Files.Add(CreateUserDeactivatedEventFile(eventsDir));
        project.Files.Add(CreateRoleAssignedEventFile(eventsDir));

        // DTOs
        var dtosDir = Path.Combine(project.Directory, "DTOs");
        project.Files.Add(CreateUserDtoFile(dtosDir));
        project.Files.Add(CreateRoleDtoFile(dtosDir));
        project.Files.Add(CreateLoginRequestFile(dtosDir));
        project.Files.Add(CreateLoginResponseFile(dtosDir));
        project.Files.Add(CreateRegisterRequestFile(dtosDir));
        project.Files.Add(CreateRegisterResponseFile(dtosDir));
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Identity.Infrastructure files");

        var dataDir = Path.Combine(project.Directory, "Data");
        var configurationsDir = Path.Combine(project.Directory, "Data", "Configurations");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");

        // DbContext
        project.Files.Add(CreateIdentityDbContextFile(dataDir));

        // Entity Configurations
        project.Files.Add(CreateUserConfigurationFile(configurationsDir));
        project.Files.Add(CreateRoleConfigurationFile(configurationsDir));
        project.Files.Add(CreatePermissionConfigurationFile(configurationsDir));
        project.Files.Add(CreateUserRoleConfigurationFile(configurationsDir));
        project.Files.Add(CreateRolePermissionConfigurationFile(configurationsDir));

        // Repositories
        project.Files.Add(CreateUserRepositoryFile(repositoriesDir));
        project.Files.Add(CreateRoleRepositoryFile(repositoriesDir));

        // Services
        project.Files.Add(CreateTokenServiceFile(servicesDir));
        project.Files.Add(CreatePasswordHasherFile(servicesDir));

        // ConfigureServices
        project.Files.Add(CreateInfrastructureConfigureServicesFile(project.Directory));
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Identity.Api files");

        var controllersDir = Path.Combine(project.Directory, "Controllers");

        // Controllers
        project.Files.Add(CreateAuthControllerFile(controllersDir));
        project.Files.Add(CreateUsersControllerFile(controllersDir));
        project.Files.Add(CreateRolesControllerFile(controllersDir));

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

                namespace Identity.Core.Entities;

                /// <summary>
                /// Marker interface for aggregate roots.
                /// </summary>
                public interface IAggregateRoot
                {
                }
                """
        };
    }

    private static FileModel CreateUserEntityFile(string directory)
    {
        return new FileModel("User", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Identity.Core.Entities;

                /// <summary>
                /// User entity representing a system user.
                /// </summary>
                public class User : IAggregateRoot
                {
                    public Guid UserId { get; set; }

                    public required string Username { get; set; }

                    public required string Email { get; set; }

                    public required string PasswordHash { get; set; }

                    public bool IsActive { get; set; } = true;

                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

                    public DateTime? LastLoginAt { get; set; }

                    public DateTime? UpdatedAt { get; set; }

                    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
                }
                """
        };
    }

    private static FileModel CreateRoleEntityFile(string directory)
    {
        return new FileModel("Role", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Identity.Core.Entities;

                /// <summary>
                /// Role entity representing a user role.
                /// </summary>
                public class Role : IAggregateRoot
                {
                    public Guid RoleId { get; set; }

                    public required string Name { get; set; }

                    public string? Description { get; set; }

                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

                    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

                    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
                }
                """
        };
    }

    private static FileModel CreatePermissionEntityFile(string directory)
    {
        return new FileModel("Permission", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Identity.Core.Entities;

                /// <summary>
                /// Permission entity representing a system permission.
                /// </summary>
                public class Permission : IAggregateRoot
                {
                    public Guid PermissionId { get; set; }

                    public required string Name { get; set; }

                    public required string Resource { get; set; }

                    public required string Action { get; set; }

                    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
                }
                """
        };
    }

    private static FileModel CreateUserRoleEntityFile(string directory)
    {
        return new FileModel("UserRole", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Identity.Core.Entities;

                /// <summary>
                /// Join entity for User-Role many-to-many relationship.
                /// </summary>
                public class UserRole
                {
                    public Guid UserId { get; set; }

                    public User User { get; set; } = null!;

                    public Guid RoleId { get; set; }

                    public Role Role { get; set; } = null!;

                    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
                }
                """
        };
    }

    private static FileModel CreateRolePermissionEntityFile(string directory)
    {
        return new FileModel("RolePermission", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Identity.Core.Entities;

                /// <summary>
                /// Join entity for Role-Permission many-to-many relationship.
                /// </summary>
                public class RolePermission
                {
                    public Guid RoleId { get; set; }

                    public Role Role { get; set; } = null!;

                    public Guid PermissionId { get; set; }

                    public Permission Permission { get; set; } = null!;
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

                namespace Identity.Core.Interfaces;

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

    private static FileModel CreateIUserRepositoryFile(string directory)
    {
        return new FileModel("IUserRepository", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Identity.Core.Entities;

                namespace Identity.Core.Interfaces;

                /// <summary>
                /// Repository interface for User entities.
                /// </summary>
                public interface IUserRepository
                {
                    Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);

                    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

                    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

                    Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);

                    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);

                    Task UpdateAsync(User user, CancellationToken cancellationToken = default);

                    Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default);

                    Task<bool> ExistsAsync(string username, string email, CancellationToken cancellationToken = default);
                }
                """
        };
    }

    private static FileModel CreateIRoleRepositoryFile(string directory)
    {
        return new FileModel("IRoleRepository", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Identity.Core.Entities;

                namespace Identity.Core.Interfaces;

                /// <summary>
                /// Repository interface for Role entities.
                /// </summary>
                public interface IRoleRepository
                {
                    Task<Role?> GetByIdAsync(Guid roleId, CancellationToken cancellationToken = default);

                    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

                    Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default);

                    Task<IEnumerable<Role>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

                    Task<Role> AddAsync(Role role, CancellationToken cancellationToken = default);

                    Task UpdateAsync(Role role, CancellationToken cancellationToken = default);

                    Task DeleteAsync(Guid roleId, CancellationToken cancellationToken = default);

                    Task AssignToUserAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);

                    Task RemoveFromUserAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
                }
                """
        };
    }

    private static FileModel CreateITokenServiceFile(string directory)
    {
        return new FileModel("ITokenService", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Identity.Core.Entities;

                namespace Identity.Core.Interfaces;

                /// <summary>
                /// Service interface for JWT token operations.
                /// </summary>
                public interface ITokenService
                {
                    string GenerateAccessToken(User user, IEnumerable<string> roles);

                    string GenerateRefreshToken();

                    bool ValidateToken(string token);
                }
                """
        };
    }

    private static FileModel CreateIPasswordHasherFile(string directory)
    {
        return new FileModel("IPasswordHasher", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Identity.Core.Interfaces;

                /// <summary>
                /// Service interface for password hashing operations.
                /// </summary>
                public interface IPasswordHasher
                {
                    string HashPassword(string password);

                    bool VerifyPassword(string password, string hash);
                }
                """
        };
    }

    private static FileModel CreateUserCreatedEventFile(string directory)
    {
        return new FileModel("UserCreatedEvent", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Identity.Core.Interfaces;

                namespace Identity.Core.Events;

                /// <summary>
                /// Event raised when a new user is created.
                /// </summary>
                public sealed class UserCreatedEvent : IDomainEvent
                {
                    public Guid AggregateId { get; init; }

                    public string AggregateType => "User";

                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

                    public required string CorrelationId { get; init; }

                    public required string Username { get; init; }

                    public required string Email { get; init; }
                }
                """
        };
    }

    private static FileModel CreateUserAuthenticatedEventFile(string directory)
    {
        return new FileModel("UserAuthenticatedEvent", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Identity.Core.Interfaces;

                namespace Identity.Core.Events;

                /// <summary>
                /// Event raised when a user successfully authenticates.
                /// </summary>
                public sealed class UserAuthenticatedEvent : IDomainEvent
                {
                    public Guid AggregateId { get; init; }

                    public string AggregateType => "User";

                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

                    public required string CorrelationId { get; init; }

                    public required string Username { get; init; }

                    public string? IpAddress { get; init; }
                }
                """
        };
    }

    private static FileModel CreateUserAuthenticationFailedEventFile(string directory)
    {
        return new FileModel("UserAuthenticationFailedEvent", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Identity.Core.Interfaces;

                namespace Identity.Core.Events;

                /// <summary>
                /// Event raised when a user authentication attempt fails.
                /// </summary>
                public sealed class UserAuthenticationFailedEvent : IDomainEvent
                {
                    public Guid AggregateId { get; init; }

                    public string AggregateType => "User";

                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

                    public required string CorrelationId { get; init; }

                    public required string Username { get; init; }

                    public required string Reason { get; init; }

                    public string? IpAddress { get; init; }
                }
                """
        };
    }

    private static FileModel CreateUserUpdatedEventFile(string directory)
    {
        return new FileModel("UserUpdatedEvent", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Identity.Core.Interfaces;

                namespace Identity.Core.Events;

                /// <summary>
                /// Event raised when a user profile is updated.
                /// </summary>
                public sealed class UserUpdatedEvent : IDomainEvent
                {
                    public Guid AggregateId { get; init; }

                    public string AggregateType => "User";

                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

                    public required string CorrelationId { get; init; }

                    public required IReadOnlyDictionary<string, object?> ChangedProperties { get; init; }
                }
                """
        };
    }

    private static FileModel CreateUserDeactivatedEventFile(string directory)
    {
        return new FileModel("UserDeactivatedEvent", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Identity.Core.Interfaces;

                namespace Identity.Core.Events;

                /// <summary>
                /// Event raised when a user is deactivated.
                /// </summary>
                public sealed class UserDeactivatedEvent : IDomainEvent
                {
                    public Guid AggregateId { get; init; }

                    public string AggregateType => "User";

                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

                    public required string CorrelationId { get; init; }

                    public required string Reason { get; init; }

                    public required string DeactivatedBy { get; init; }
                }
                """
        };
    }

    private static FileModel CreateRoleAssignedEventFile(string directory)
    {
        return new FileModel("RoleAssignedEvent", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Identity.Core.Interfaces;

                namespace Identity.Core.Events;

                /// <summary>
                /// Event raised when a role is assigned to a user.
                /// </summary>
                public sealed class RoleAssignedEvent : IDomainEvent
                {
                    public Guid AggregateId { get; init; }

                    public string AggregateType => "User";

                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

                    public required string CorrelationId { get; init; }

                    public required Guid RoleId { get; init; }

                    public required string RoleName { get; init; }

                    public required string AssignedBy { get; init; }
                }
                """
        };
    }

    private static FileModel CreateUserDtoFile(string directory)
    {
        return new FileModel("UserDto", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Identity.Core.DTOs;

                /// <summary>
                /// Data transfer object for User.
                /// </summary>
                public sealed class UserDto
                {
                    public Guid UserId { get; init; }

                    public required string Username { get; init; }

                    public required string Email { get; init; }

                    public bool IsActive { get; init; }

                    public DateTime CreatedAt { get; init; }

                    public DateTime? LastLoginAt { get; init; }

                    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
                }
                """
        };
    }

    private static FileModel CreateRoleDtoFile(string directory)
    {
        return new FileModel("RoleDto", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Identity.Core.DTOs;

                /// <summary>
                /// Data transfer object for Role.
                /// </summary>
                public sealed class RoleDto
                {
                    public Guid RoleId { get; init; }

                    public required string Name { get; init; }

                    public string? Description { get; init; }
                }
                """
        };
    }

    private static FileModel CreateLoginRequestFile(string directory)
    {
        return new FileModel("LoginRequest", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Identity.Core.DTOs;

                /// <summary>
                /// Request model for user login.
                /// </summary>
                public sealed class LoginRequest
                {
                    [Required]
                    public required string Username { get; init; }

                    [Required]
                    public required string Password { get; init; }
                }
                """
        };
    }

    private static FileModel CreateLoginResponseFile(string directory)
    {
        return new FileModel("LoginResponse", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Identity.Core.DTOs;

                /// <summary>
                /// Response model for successful login.
                /// </summary>
                public sealed class LoginResponse
                {
                    public required string Token { get; init; }

                    public required string RefreshToken { get; init; }

                    public required int ExpiresIn { get; init; }
                }
                """
        };
    }

    private static FileModel CreateRegisterRequestFile(string directory)
    {
        return new FileModel("RegisterRequest", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Identity.Core.DTOs;

                /// <summary>
                /// Request model for user registration.
                /// </summary>
                public sealed class RegisterRequest
                {
                    [Required]
                    [StringLength(100, MinimumLength = 3)]
                    public required string Username { get; init; }

                    [Required]
                    [EmailAddress]
                    public required string Email { get; init; }

                    [Required]
                    [StringLength(100, MinimumLength = 8)]
                    public required string Password { get; init; }
                }
                """
        };
    }

    private static FileModel CreateRegisterResponseFile(string directory)
    {
        return new FileModel("RegisterResponse", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Identity.Core.DTOs;

                /// <summary>
                /// Response model for successful registration.
                /// </summary>
                public sealed class RegisterResponse
                {
                    public Guid UserId { get; init; }

                    public required string Username { get; init; }

                    public required string Email { get; init; }
                }
                """
        };
    }

    #endregion

    #region Infrastructure Layer Files

    private static FileModel CreateIdentityDbContextFile(string directory)
    {
        return new FileModel("IdentityDbContext", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Identity.Core.Entities;
                using Microsoft.EntityFrameworkCore;

                namespace Identity.Infrastructure.Data;

                /// <summary>
                /// Entity Framework Core DbContext for Identity microservice.
                /// </summary>
                public class IdentityDbContext : DbContext
                {
                    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
                        : base(options)
                    {
                    }

                    public DbSet<User> Users => Set<User>();

                    public DbSet<Role> Roles => Set<Role>();

                    public DbSet<Permission> Permissions => Set<Permission>();

                    public DbSet<UserRole> UserRoles => Set<UserRole>();

                    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

                    protected override void OnModelCreating(ModelBuilder modelBuilder)
                    {
                        base.OnModelCreating(modelBuilder);
                        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
                    }
                }
                """
        };
    }

    private static FileModel CreateUserConfigurationFile(string directory)
    {
        return new FileModel("UserConfiguration", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Identity.Core.Entities;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.EntityFrameworkCore.Metadata.Builders;

                namespace Identity.Infrastructure.Data.Configurations;

                /// <summary>
                /// Entity configuration for User.
                /// </summary>
                public class UserConfiguration : IEntityTypeConfiguration<User>
                {
                    public void Configure(EntityTypeBuilder<User> builder)
                    {
                        builder.HasKey(u => u.UserId);

                        builder.Property(u => u.Username)
                            .IsRequired()
                            .HasMaxLength(100);

                        builder.Property(u => u.Email)
                            .IsRequired()
                            .HasMaxLength(255);

                        builder.Property(u => u.PasswordHash)
                            .IsRequired()
                            .HasMaxLength(255);

                        builder.Property(u => u.CreatedAt)
                            .IsRequired();

                        builder.HasIndex(u => u.Username)
                            .IsUnique();

                        builder.HasIndex(u => u.Email)
                            .IsUnique();
                    }
                }
                """
        };
    }

    private static FileModel CreateRoleConfigurationFile(string directory)
    {
        return new FileModel("RoleConfiguration", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Identity.Core.Entities;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.EntityFrameworkCore.Metadata.Builders;

                namespace Identity.Infrastructure.Data.Configurations;

                /// <summary>
                /// Entity configuration for Role.
                /// </summary>
                public class RoleConfiguration : IEntityTypeConfiguration<Role>
                {
                    public void Configure(EntityTypeBuilder<Role> builder)
                    {
                        builder.HasKey(r => r.RoleId);

                        builder.Property(r => r.Name)
                            .IsRequired()
                            .HasMaxLength(100);

                        builder.Property(r => r.Description)
                            .HasMaxLength(500);

                        builder.HasIndex(r => r.Name)
                            .IsUnique();
                    }
                }
                """
        };
    }

    private static FileModel CreatePermissionConfigurationFile(string directory)
    {
        return new FileModel("PermissionConfiguration", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Identity.Core.Entities;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.EntityFrameworkCore.Metadata.Builders;

                namespace Identity.Infrastructure.Data.Configurations;

                /// <summary>
                /// Entity configuration for Permission.
                /// </summary>
                public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
                {
                    public void Configure(EntityTypeBuilder<Permission> builder)
                    {
                        builder.HasKey(p => p.PermissionId);

                        builder.Property(p => p.Name)
                            .IsRequired()
                            .HasMaxLength(100);

                        builder.Property(p => p.Resource)
                            .IsRequired()
                            .HasMaxLength(100);

                        builder.Property(p => p.Action)
                            .IsRequired()
                            .HasMaxLength(50);

                        builder.HasIndex(p => new { p.Resource, p.Action })
                            .IsUnique();
                    }
                }
                """
        };
    }

    private static FileModel CreateUserRoleConfigurationFile(string directory)
    {
        return new FileModel("UserRoleConfiguration", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Identity.Core.Entities;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.EntityFrameworkCore.Metadata.Builders;

                namespace Identity.Infrastructure.Data.Configurations;

                /// <summary>
                /// Entity configuration for UserRole.
                /// </summary>
                public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
                {
                    public void Configure(EntityTypeBuilder<UserRole> builder)
                    {
                        builder.HasKey(ur => new { ur.UserId, ur.RoleId });

                        builder.HasOne(ur => ur.User)
                            .WithMany(u => u.UserRoles)
                            .HasForeignKey(ur => ur.UserId)
                            .OnDelete(DeleteBehavior.Cascade);

                        builder.HasOne(ur => ur.Role)
                            .WithMany(r => r.UserRoles)
                            .HasForeignKey(ur => ur.RoleId)
                            .OnDelete(DeleteBehavior.Cascade);
                    }
                }
                """
        };
    }

    private static FileModel CreateRolePermissionConfigurationFile(string directory)
    {
        return new FileModel("RolePermissionConfiguration", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Identity.Core.Entities;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.EntityFrameworkCore.Metadata.Builders;

                namespace Identity.Infrastructure.Data.Configurations;

                /// <summary>
                /// Entity configuration for RolePermission.
                /// </summary>
                public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
                {
                    public void Configure(EntityTypeBuilder<RolePermission> builder)
                    {
                        builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });

                        builder.HasOne(rp => rp.Role)
                            .WithMany(r => r.RolePermissions)
                            .HasForeignKey(rp => rp.RoleId)
                            .OnDelete(DeleteBehavior.Cascade);

                        builder.HasOne(rp => rp.Permission)
                            .WithMany(p => p.RolePermissions)
                            .HasForeignKey(rp => rp.PermissionId)
                            .OnDelete(DeleteBehavior.Cascade);
                    }
                }
                """
        };
    }

    private static FileModel CreateUserRepositoryFile(string directory)
    {
        return new FileModel("UserRepository", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Identity.Core.Entities;
                using Identity.Core.Interfaces;
                using Identity.Infrastructure.Data;
                using Microsoft.EntityFrameworkCore;

                namespace Identity.Infrastructure.Repositories;

                /// <summary>
                /// Repository implementation for User entities.
                /// </summary>
                public class UserRepository : IUserRepository
                {
                    private readonly IdentityDbContext context;

                    public UserRepository(IdentityDbContext context)
                    {
                        this.context = context ?? throw new ArgumentNullException(nameof(context));
                    }

                    public async Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
                    {
                        return await context.Users
                            .Include(u => u.UserRoles)
                            .ThenInclude(ur => ur.Role)
                            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
                    }

                    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
                    {
                        return await context.Users
                            .Include(u => u.UserRoles)
                            .ThenInclude(ur => ur.Role)
                            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
                    }

                    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
                    {
                        return await context.Users
                            .Include(u => u.UserRoles)
                            .ThenInclude(ur => ur.Role)
                            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
                    }

                    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
                    {
                        return await context.Users
                            .Include(u => u.UserRoles)
                            .ThenInclude(ur => ur.Role)
                            .ToListAsync(cancellationToken);
                    }

                    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
                    {
                        user.UserId = Guid.NewGuid();
                        user.CreatedAt = DateTime.UtcNow;
                        await context.Users.AddAsync(user, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                        return user;
                    }

                    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
                    {
                        user.UpdatedAt = DateTime.UtcNow;
                        context.Users.Update(user);
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    public async Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default)
                    {
                        var user = await context.Users.FindAsync(new object[] { userId }, cancellationToken);
                        if (user != null)
                        {
                            context.Users.Remove(user);
                            await context.SaveChangesAsync(cancellationToken);
                        }
                    }

                    public async Task<bool> ExistsAsync(string username, string email, CancellationToken cancellationToken = default)
                    {
                        return await context.Users.AnyAsync(
                            u => u.Username == username || u.Email == email,
                            cancellationToken);
                    }
                }
                """
        };
    }

    private static FileModel CreateRoleRepositoryFile(string directory)
    {
        return new FileModel("RoleRepository", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Identity.Core.Entities;
                using Identity.Core.Interfaces;
                using Identity.Infrastructure.Data;
                using Microsoft.EntityFrameworkCore;

                namespace Identity.Infrastructure.Repositories;

                /// <summary>
                /// Repository implementation for Role entities.
                /// </summary>
                public class RoleRepository : IRoleRepository
                {
                    private readonly IdentityDbContext context;

                    public RoleRepository(IdentityDbContext context)
                    {
                        this.context = context ?? throw new ArgumentNullException(nameof(context));
                    }

                    public async Task<Role?> GetByIdAsync(Guid roleId, CancellationToken cancellationToken = default)
                    {
                        return await context.Roles
                            .Include(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                            .FirstOrDefaultAsync(r => r.RoleId == roleId, cancellationToken);
                    }

                    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
                    {
                        return await context.Roles
                            .Include(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                            .FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
                    }

                    public async Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default)
                    {
                        return await context.Roles
                            .Include(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                            .ToListAsync(cancellationToken);
                    }

                    public async Task<IEnumerable<Role>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
                    {
                        return await context.UserRoles
                            .Where(ur => ur.UserId == userId)
                            .Select(ur => ur.Role)
                            .ToListAsync(cancellationToken);
                    }

                    public async Task<Role> AddAsync(Role role, CancellationToken cancellationToken = default)
                    {
                        role.RoleId = Guid.NewGuid();
                        role.CreatedAt = DateTime.UtcNow;
                        await context.Roles.AddAsync(role, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                        return role;
                    }

                    public async Task UpdateAsync(Role role, CancellationToken cancellationToken = default)
                    {
                        context.Roles.Update(role);
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    public async Task DeleteAsync(Guid roleId, CancellationToken cancellationToken = default)
                    {
                        var role = await context.Roles.FindAsync(new object[] { roleId }, cancellationToken);
                        if (role != null)
                        {
                            context.Roles.Remove(role);
                            await context.SaveChangesAsync(cancellationToken);
                        }
                    }

                    public async Task AssignToUserAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
                    {
                        var userRole = new UserRole
                        {
                            UserId = userId,
                            RoleId = roleId,
                            AssignedAt = DateTime.UtcNow
                        };
                        await context.UserRoles.AddAsync(userRole, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    public async Task RemoveFromUserAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
                    {
                        var userRole = await context.UserRoles
                            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId, cancellationToken);
                        if (userRole != null)
                        {
                            context.UserRoles.Remove(userRole);
                            await context.SaveChangesAsync(cancellationToken);
                        }
                    }
                }
                """
        };
    }

    private static FileModel CreateTokenServiceFile(string directory)
    {
        return new FileModel("TokenService", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.IdentityModel.Tokens.Jwt;
                using System.Security.Claims;
                using System.Security.Cryptography;
                using System.Text;
                using Identity.Core.Entities;
                using Identity.Core.Interfaces;
                using Microsoft.Extensions.Configuration;
                using Microsoft.IdentityModel.Tokens;

                namespace Identity.Infrastructure.Services;

                /// <summary>
                /// JWT token service implementation.
                /// </summary>
                public class TokenService : ITokenService
                {
                    private readonly IConfiguration configuration;

                    public TokenService(IConfiguration configuration)
                    {
                        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
                    }

                    public string GenerateAccessToken(User user, IEnumerable<string> roles)
                    {
                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                            configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured")));
                        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                        var claims = new List<Claim>
                        {
                            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                            new(JwtRegisteredClaimNames.UniqueName, user.Username),
                            new(JwtRegisteredClaimNames.Email, user.Email),
                            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                        };

                        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

                        var expirationHours = int.Parse(configuration["Jwt:ExpirationHours"] ?? "24");

                        var token = new JwtSecurityToken(
                            issuer: configuration["Jwt:Issuer"],
                            audience: configuration["Jwt:Audience"],
                            claims: claims,
                            expires: DateTime.UtcNow.AddHours(expirationHours),
                            signingCredentials: credentials);

                        return new JwtSecurityTokenHandler().WriteToken(token);
                    }

                    public string GenerateRefreshToken()
                    {
                        var randomBytes = new byte[64];
                        using var rng = RandomNumberGenerator.Create();
                        rng.GetBytes(randomBytes);
                        return Convert.ToBase64String(randomBytes);
                    }

                    public bool ValidateToken(string token)
                    {
                        var tokenHandler = new JwtSecurityTokenHandler();
                        var key = Encoding.UTF8.GetBytes(
                            configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured"));

                        try
                        {
                            tokenHandler.ValidateToken(token, new TokenValidationParameters
                            {
                                ValidateIssuerSigningKey = true,
                                IssuerSigningKey = new SymmetricSecurityKey(key),
                                ValidateIssuer = true,
                                ValidIssuer = configuration["Jwt:Issuer"],
                                ValidateAudience = true,
                                ValidAudience = configuration["Jwt:Audience"],
                                ValidateLifetime = true,
                                ClockSkew = TimeSpan.Zero
                            }, out _);
                            return true;
                        }
                        catch
                        {
                            return false;
                        }
                    }
                }
                """
        };
    }

    private static FileModel CreatePasswordHasherFile(string directory)
    {
        return new FileModel("PasswordHasher", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.Security.Cryptography;
                using Identity.Core.Interfaces;

                namespace Identity.Infrastructure.Services;

                /// <summary>
                /// Password hashing service using PBKDF2.
                /// </summary>
                public class PasswordHasher : IPasswordHasher
                {
                    private const int SaltSize = 16;
                    private const int HashSize = 32;
                    private const int Iterations = 100000;

                    public string HashPassword(string password)
                    {
                        using var rng = RandomNumberGenerator.Create();
                        var salt = new byte[SaltSize];
                        rng.GetBytes(salt);

                        var hash = Rfc2898DeriveBytes.Pbkdf2(
                            password,
                            salt,
                            Iterations,
                            HashAlgorithmName.SHA256,
                            HashSize);

                        var hashBytes = new byte[SaltSize + HashSize];
                        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
                        Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

                        return Convert.ToBase64String(hashBytes);
                    }

                    public bool VerifyPassword(string password, string hash)
                    {
                        var hashBytes = Convert.FromBase64String(hash);

                        var salt = new byte[SaltSize];
                        Array.Copy(hashBytes, 0, salt, 0, SaltSize);

                        var computedHash = Rfc2898DeriveBytes.Pbkdf2(
                            password,
                            salt,
                            Iterations,
                            HashAlgorithmName.SHA256,
                            HashSize);

                        for (int i = 0; i < HashSize; i++)
                        {
                            if (hashBytes[i + SaltSize] != computedHash[i])
                            {
                                return false;
                            }
                        }

                        return true;
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

                using Identity.Core.Interfaces;
                using Identity.Infrastructure.Data;
                using Identity.Infrastructure.Repositories;
                using Identity.Infrastructure.Services;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.Extensions.Configuration;

                namespace Microsoft.Extensions.DependencyInjection;

                /// <summary>
                /// Extension methods for configuring Identity infrastructure services.
                /// </summary>
                public static class ConfigureServices
                {
                    /// <summary>
                    /// Adds Identity infrastructure services to the service collection.
                    /// </summary>
                    public static IServiceCollection AddIdentityInfrastructure(
                        this IServiceCollection services,
                        IConfiguration configuration)
                    {
                        services.AddDbContext<IdentityDbContext>(options =>
                            options.UseSqlServer(
                                configuration.GetConnectionString("IdentityDb") ??
                                @"Server=.\SQLEXPRESS;Database=IdentityDb;Trusted_Connection=True;TrustServerCertificate=True"));

                        services.AddScoped<IUserRepository, UserRepository>();
                        services.AddScoped<IRoleRepository, RoleRepository>();
                        services.AddScoped<ITokenService, TokenService>();
                        services.AddScoped<IPasswordHasher, PasswordHasher>();

                        return services;
                    }
                }
                """
        };
    }

    #endregion

    #region API Layer Files

    private static FileModel CreateAuthControllerFile(string directory)
    {
        return new FileModel("AuthController", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Identity.Core.DTOs;
                using Identity.Core.Entities;
                using Identity.Core.Interfaces;
                using Microsoft.AspNetCore.Authorization;
                using Microsoft.AspNetCore.Mvc;

                namespace Identity.Api.Controllers;

                /// <summary>
                /// Controller for authentication operations.
                /// </summary>
                [ApiController]
                [Route("api/[controller]")]
                public class AuthController : ControllerBase
                {
                    private readonly IUserRepository userRepository;
                    private readonly ITokenService tokenService;
                    private readonly IPasswordHasher passwordHasher;
                    private readonly ILogger<AuthController> logger;

                    public AuthController(
                        IUserRepository userRepository,
                        ITokenService tokenService,
                        IPasswordHasher passwordHasher,
                        ILogger<AuthController> logger)
                    {
                        this.userRepository = userRepository;
                        this.tokenService = tokenService;
                        this.passwordHasher = passwordHasher;
                        this.logger = logger;
                    }

                    /// <summary>
                    /// Register a new user.
                    /// </summary>
                    [HttpPost("register")]
                    [AllowAnonymous]
                    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
                    [ProducesResponseType(StatusCodes.Status400BadRequest)]
                    public async Task<ActionResult<RegisterResponse>> Register(
                        [FromBody] RegisterRequest request,
                        CancellationToken cancellationToken)
                    {
                        if (await userRepository.ExistsAsync(request.Username, request.Email, cancellationToken))
                        {
                            return BadRequest(new { error = "Username or email already exists" });
                        }

                        var user = new User
                        {
                            Username = request.Username,
                            Email = request.Email,
                            PasswordHash = passwordHasher.HashPassword(request.Password)
                        };

                        var createdUser = await userRepository.AddAsync(user, cancellationToken);

                        logger.LogInformation("User {Username} registered successfully", user.Username);

                        var response = new RegisterResponse
                        {
                            UserId = createdUser.UserId,
                            Username = createdUser.Username,
                            Email = createdUser.Email
                        };

                        return CreatedAtAction(nameof(Register), new { id = createdUser.UserId }, response);
                    }

                    /// <summary>
                    /// Authenticate a user and return tokens.
                    /// </summary>
                    [HttpPost("login")]
                    [AllowAnonymous]
                    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
                    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
                    public async Task<ActionResult<LoginResponse>> Login(
                        [FromBody] LoginRequest request,
                        CancellationToken cancellationToken)
                    {
                        var user = await userRepository.GetByUsernameAsync(request.Username, cancellationToken);

                        if (user == null || !passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
                        {
                            logger.LogWarning("Failed login attempt for user {Username}", request.Username);
                            return Unauthorized(new { error = "Invalid username or password" });
                        }

                        if (!user.IsActive)
                        {
                            return Unauthorized(new { error = "User account is deactivated" });
                        }

                        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
                        var accessToken = tokenService.GenerateAccessToken(user, roles);
                        var refreshToken = tokenService.GenerateRefreshToken();

                        user.LastLoginAt = DateTime.UtcNow;
                        await userRepository.UpdateAsync(user, cancellationToken);

                        logger.LogInformation("User {Username} logged in successfully", user.Username);

                        return Ok(new LoginResponse
                        {
                            Token = accessToken,
                            RefreshToken = refreshToken,
                            ExpiresIn = 86400 // 24 hours in seconds
                        });
                    }

                    /// <summary>
                    /// Logout a user (invalidate refresh token).
                    /// </summary>
                    [HttpPost("logout")]
                    [Authorize]
                    [ProducesResponseType(StatusCodes.Status204NoContent)]
                    public IActionResult Logout()
                    {
                        // In a full implementation, you would invalidate the refresh token here
                        return NoContent();
                    }
                }
                """
        };
    }

    private static FileModel CreateUsersControllerFile(string directory)
    {
        return new FileModel("UsersController", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Identity.Core.DTOs;
                using Identity.Core.Interfaces;
                using Microsoft.AspNetCore.Authorization;
                using Microsoft.AspNetCore.Mvc;

                namespace Identity.Api.Controllers;

                /// <summary>
                /// Controller for user management operations.
                /// </summary>
                [ApiController]
                [Route("api/[controller]")]
                [Authorize]
                public class UsersController : ControllerBase
                {
                    private readonly IUserRepository userRepository;
                    private readonly ILogger<UsersController> logger;

                    public UsersController(
                        IUserRepository userRepository,
                        ILogger<UsersController> logger)
                    {
                        this.userRepository = userRepository;
                        this.logger = logger;
                    }

                    /// <summary>
                    /// Get a user by ID.
                    /// </summary>
                    [HttpGet("{id:guid}")]
                    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
                    [ProducesResponseType(StatusCodes.Status404NotFound)]
                    public async Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken cancellationToken)
                    {
                        var user = await userRepository.GetByIdAsync(id, cancellationToken);

                        if (user == null)
                        {
                            return NotFound();
                        }

                        return Ok(new UserDto
                        {
                            UserId = user.UserId,
                            Username = user.Username,
                            Email = user.Email,
                            IsActive = user.IsActive,
                            CreatedAt = user.CreatedAt,
                            LastLoginAt = user.LastLoginAt,
                            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
                        });
                    }

                    /// <summary>
                    /// Get all users.
                    /// </summary>
                    [HttpGet]
                    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
                    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll(CancellationToken cancellationToken)
                    {
                        var users = await userRepository.GetAllAsync(cancellationToken);

                        var userDtos = users.Select(user => new UserDto
                        {
                            UserId = user.UserId,
                            Username = user.Username,
                            Email = user.Email,
                            IsActive = user.IsActive,
                            CreatedAt = user.CreatedAt,
                            LastLoginAt = user.LastLoginAt,
                            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
                        });

                        return Ok(userDtos);
                    }

                    /// <summary>
                    /// Delete a user.
                    /// </summary>
                    [HttpDelete("{id:guid}")]
                    [ProducesResponseType(StatusCodes.Status204NoContent)]
                    [ProducesResponseType(StatusCodes.Status404NotFound)]
                    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
                    {
                        var user = await userRepository.GetByIdAsync(id, cancellationToken);

                        if (user == null)
                        {
                            return NotFound();
                        }

                        await userRepository.DeleteAsync(id, cancellationToken);
                        logger.LogInformation("User {UserId} deleted", id);

                        return NoContent();
                    }
                }
                """
        };
    }

    private static FileModel CreateRolesControllerFile(string directory)
    {
        return new FileModel("RolesController", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Identity.Core.DTOs;
                using Identity.Core.Entities;
                using Identity.Core.Interfaces;
                using Microsoft.AspNetCore.Authorization;
                using Microsoft.AspNetCore.Mvc;

                namespace Identity.Api.Controllers;

                /// <summary>
                /// Controller for role management operations.
                /// </summary>
                [ApiController]
                [Route("api/[controller]")]
                [Authorize]
                public class RolesController : ControllerBase
                {
                    private readonly IRoleRepository roleRepository;
                    private readonly ILogger<RolesController> logger;

                    public RolesController(
                        IRoleRepository roleRepository,
                        ILogger<RolesController> logger)
                    {
                        this.roleRepository = roleRepository;
                        this.logger = logger;
                    }

                    /// <summary>
                    /// Get all roles.
                    /// </summary>
                    [HttpGet]
                    [ProducesResponseType(typeof(IEnumerable<RoleDto>), StatusCodes.Status200OK)]
                    public async Task<ActionResult<IEnumerable<RoleDto>>> GetAll(CancellationToken cancellationToken)
                    {
                        var roles = await roleRepository.GetAllAsync(cancellationToken);

                        var roleDtos = roles.Select(role => new RoleDto
                        {
                            RoleId = role.RoleId,
                            Name = role.Name,
                            Description = role.Description
                        });

                        return Ok(roleDtos);
                    }

                    /// <summary>
                    /// Create a new role.
                    /// </summary>
                    [HttpPost]
                    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status201Created)]
                    [ProducesResponseType(StatusCodes.Status400BadRequest)]
                    public async Task<ActionResult<RoleDto>> Create(
                        [FromBody] RoleDto request,
                        CancellationToken cancellationToken)
                    {
                        var existingRole = await roleRepository.GetByNameAsync(request.Name, cancellationToken);
                        if (existingRole != null)
                        {
                            return BadRequest(new { error = "Role with this name already exists" });
                        }

                        var role = new Role
                        {
                            Name = request.Name,
                            Description = request.Description
                        };

                        var createdRole = await roleRepository.AddAsync(role, cancellationToken);
                        logger.LogInformation("Role {RoleName} created", role.Name);

                        var response = new RoleDto
                        {
                            RoleId = createdRole.RoleId,
                            Name = createdRole.Name,
                            Description = createdRole.Description
                        };

                        return CreatedAtAction(nameof(GetAll), response);
                    }

                    /// <summary>
                    /// Delete a role.
                    /// </summary>
                    [HttpDelete("{id:guid}")]
                    [ProducesResponseType(StatusCodes.Status204NoContent)]
                    [ProducesResponseType(StatusCodes.Status404NotFound)]
                    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
                    {
                        var role = await roleRepository.GetByIdAsync(id, cancellationToken);

                        if (role == null)
                        {
                            return NotFound();
                        }

                        await roleRepository.DeleteAsync(id, cancellationToken);
                        logger.LogInformation("Role {RoleId} deleted", id);

                        return NoContent();
                    }

                    /// <summary>
                    /// Assign roles to a user.
                    /// </summary>
                    [HttpPut("/api/users/{userId:guid}/roles")]
                    [ProducesResponseType(StatusCodes.Status200OK)]
                    public async Task<IActionResult> AssignRolesToUser(
                        Guid userId,
                        [FromBody] List<Guid> roleIds,
                        CancellationToken cancellationToken)
                    {
                        foreach (var roleId in roleIds)
                        {
                            await roleRepository.AssignToUserAsync(userId, roleId, cancellationToken);
                        }

                        logger.LogInformation("Roles assigned to user {UserId}", userId);
                        return Ok();
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
                builder.Services.AddIdentityInfrastructure(builder.Configuration);

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
                        Title = "Identity API",
                        Version = "v1",
                        Description = "Identity microservice for authentication and authorization"
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
                    "IdentityDb": "Server=.\\SQLEXPRESS;Database=IdentityDb;Trusted_Connection=True;TrustServerCertificate=True"
                  },
                  "Jwt": {
                    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
                    "Issuer": "Identity.Api",
                    "Audience": "Identity.Api",
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
