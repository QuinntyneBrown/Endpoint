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
using Endpoint.DotNet.Syntax.Attributes;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.Workflow;

using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

public class WorkflowArtifactFactory : IWorkflowArtifactFactory
{
    private readonly ILogger<WorkflowArtifactFactory> logger;

    public WorkflowArtifactFactory(ILogger<WorkflowArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding Workflow.Core files");
        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");

        // Entities
        project.Files.Add(new FileModel("Workflow", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Workflow.Core.Entities;

                public class Workflow
                {
                    public Guid WorkflowId { get; set; }
                    public required string Name { get; set; }
                    public string? Description { get; set; }
                    public required string Version { get; set; }
                    public WorkflowStatus Status { get; set; } = WorkflowStatus.Draft;
                    public bool IsActive { get; set; } = true;
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? UpdatedAt { get; set; }
                    public ICollection<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
                    public ICollection<WorkflowInstance> Instances { get; set; } = new List<WorkflowInstance>();
                }

                public enum WorkflowStatus { Draft, Published, Archived }
                """
        });

        project.Files.Add(new FileModel("WorkflowInstance", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Workflow.Core.Entities;

                public class WorkflowInstance
                {
                    public Guid InstanceId { get; set; }
                    public Guid WorkflowId { get; set; }
                    public Workflow Workflow { get; set; } = null!;
                    public Guid? CurrentStepId { get; set; }
                    public WorkflowStep? CurrentStep { get; set; }
                    public InstanceStatus Status { get; set; } = InstanceStatus.Running;
                    public string? ContextData { get; set; }
                    public Guid? InitiatedBy { get; set; }
                    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? CompletedAt { get; set; }
                    public ICollection<WorkflowHistory> History { get; set; } = new List<WorkflowHistory>();
                    public ICollection<ApprovalRequest> ApprovalRequests { get; set; } = new List<ApprovalRequest>();
                }

                public enum InstanceStatus { Running, Paused, Completed, Failed, Cancelled }
                """
        });

        project.Files.Add(new FileModel("WorkflowStep", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Workflow.Core.Entities;

                public class WorkflowStep
                {
                    public Guid StepId { get; set; }
                    public Guid WorkflowId { get; set; }
                    public Workflow Workflow { get; set; } = null!;
                    public required string Name { get; set; }
                    public string? Description { get; set; }
                    public StepType Type { get; set; } = StepType.Task;
                    public int Order { get; set; }
                    public string? Configuration { get; set; }
                    public Guid? NextStepId { get; set; }
                    public bool RequiresApproval { get; set; } = false;
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                }

                public enum StepType { Task, Decision, Approval, Notification, Integration }
                """
        });

        project.Files.Add(new FileModel("ApprovalRequest", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Workflow.Core.Entities;

                public class ApprovalRequest
                {
                    public Guid ApprovalId { get; set; }
                    public Guid InstanceId { get; set; }
                    public WorkflowInstance Instance { get; set; } = null!;
                    public Guid StepId { get; set; }
                    public WorkflowStep Step { get; set; } = null!;
                    public Guid? AssignedTo { get; set; }
                    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
                    public string? Comments { get; set; }
                    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? RespondedAt { get; set; }
                    public Guid? RespondedBy { get; set; }
                }

                public enum ApprovalStatus { Pending, Approved, Rejected, Escalated }
                """
        });

        project.Files.Add(new FileModel("WorkflowHistory", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Workflow.Core.Entities;

                public class WorkflowHistory
                {
                    public Guid HistoryId { get; set; }
                    public Guid InstanceId { get; set; }
                    public WorkflowInstance Instance { get; set; } = null!;
                    public Guid? StepId { get; set; }
                    public required string Action { get; set; }
                    public required string FromState { get; set; }
                    public required string ToState { get; set; }
                    public string? Details { get; set; }
                    public Guid? PerformedBy { get; set; }
                    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
                }
                """
        });

        // Interfaces
        project.Files.Add(CreateIWorkflowRepositoryFile(interfacesDir));
        project.Files.Add(CreateIWorkflowEngineFile(interfacesDir));
        project.Files.Add(CreateIStateManagerFile(interfacesDir));

        // Events
        project.Files.Add(new FileModel("WorkflowStartedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Workflow.Core.Events;

                public sealed class WorkflowStartedEvent
                {
                    public Guid InstanceId { get; init; }
                    public Guid WorkflowId { get; init; }
                    public required string WorkflowName { get; init; }
                    public Guid? InitiatedBy { get; init; }
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        project.Files.Add(new FileModel("WorkflowCompletedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Workflow.Core.Events;

                public sealed class WorkflowCompletedEvent
                {
                    public Guid InstanceId { get; init; }
                    public Guid WorkflowId { get; init; }
                    public required string WorkflowName { get; init; }
                    public required string FinalStatus { get; init; }
                    public DateTime StartedAt { get; init; }
                    public DateTime CompletedAt { get; init; } = DateTime.UtcNow;
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        project.Files.Add(new FileModel("ApprovalRequestedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Workflow.Core.Events;

                public sealed class ApprovalRequestedEvent
                {
                    public Guid ApprovalId { get; init; }
                    public Guid InstanceId { get; init; }
                    public Guid StepId { get; init; }
                    public required string StepName { get; init; }
                    public Guid? AssignedTo { get; init; }
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        project.Files.Add(new FileModel("StateChangedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Workflow.Core.Events;

                public sealed class StateChangedEvent
                {
                    public Guid InstanceId { get; init; }
                    public Guid WorkflowId { get; init; }
                    public required string FromState { get; init; }
                    public required string ToState { get; init; }
                    public Guid? StepId { get; init; }
                    public Guid? PerformedBy { get; init; }
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        // DTOs
        project.Files.Add(new FileModel("WorkflowDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Workflow.Core.DTOs;

                public sealed class WorkflowDto
                {
                    public Guid WorkflowId { get; init; }
                    public required string Name { get; init; }
                    public string? Description { get; init; }
                    public required string Version { get; init; }
                    public string Status { get; init; } = "Draft";
                    public bool IsActive { get; init; }
                    public DateTime CreatedAt { get; init; }
                    public IEnumerable<WorkflowStepDto> Steps { get; init; } = [];
                }
                """
        });

        project.Files.Add(new FileModel("WorkflowStepDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Workflow.Core.DTOs;

                public sealed class WorkflowStepDto
                {
                    public Guid StepId { get; init; }
                    public required string Name { get; init; }
                    public string? Description { get; init; }
                    public string Type { get; init; } = "Task";
                    public int Order { get; init; }
                    public bool RequiresApproval { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("WorkflowInstanceDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Workflow.Core.DTOs;

                public sealed class WorkflowInstanceDto
                {
                    public Guid InstanceId { get; init; }
                    public Guid WorkflowId { get; init; }
                    public required string WorkflowName { get; init; }
                    public Guid? CurrentStepId { get; init; }
                    public string? CurrentStepName { get; init; }
                    public string Status { get; init; } = "Running";
                    public DateTime StartedAt { get; init; }
                    public DateTime? CompletedAt { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("CreateWorkflowRequest", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Workflow.Core.DTOs;

                public sealed class CreateWorkflowRequest
                {
                    [Required]
                    public required string Name { get; init; }

                    public string? Description { get; init; }

                    [Required]
                    public required string Version { get; init; }

                    public IEnumerable<CreateWorkflowStepRequest> Steps { get; init; } = [];
                }

                public sealed class CreateWorkflowStepRequest
                {
                    [Required]
                    public required string Name { get; init; }

                    public string? Description { get; init; }

                    public string Type { get; init; } = "Task";

                    public int Order { get; init; }

                    public bool RequiresApproval { get; init; }

                    public string? Configuration { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("TransitionRequest", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Workflow.Core.DTOs;

                public sealed class TransitionRequest
                {
                    public string? TransitionData { get; init; }
                    public Guid? PerformedBy { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("ApprovalResponse", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Workflow.Core.DTOs;

                public sealed class ApprovalResponse
                {
                    [Required]
                    public bool Approved { get; init; }

                    public string? Comments { get; init; }

                    public Guid? RespondedBy { get; init; }
                }
                """
        });
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Workflow.Infrastructure files");
        var dataDir = Path.Combine(project.Directory, "Data");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");

        project.Files.Add(CreateWorkflowDbContextFile(dataDir));

        project.Files.Add(CreateWorkflowRepositoryFile(repositoriesDir));

        project.Files.Add(CreateWorkflowEngineFile(servicesDir));

        project.Files.Add(CreateStateManagerFile(servicesDir));

        project.Files.Add(CreateConfigureServicesFile(project.Directory));
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Workflow.Api files");
        var controllersDir = Path.Combine(project.Directory, "Controllers");

        project.Files.Add(CreateWorkflowsControllerFile(controllersDir));
        project.Files.Add(CreateApprovalsControllerFile(controllersDir));

        project.Files.Add(new FileModel("Program", project.Directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                var builder = WebApplication.CreateBuilder(args);

                builder.Services.AddWorkflowInfrastructure(builder.Configuration);
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
        });

        project.Files.Add(new FileModel("appsettings", project.Directory, ".json")
        {
            Body = """
                {
                  "ConnectionStrings": {
                    "WorkflowDb": "Server=.\\SQLEXPRESS;Database=WorkflowDb;Trusted_Connection=True;TrustServerCertificate=True"
                  },
                  "Logging": {
                    "LogLevel": {
                      "Default": "Information"
                    }
                  },
                  "AllowedHosts": "*"
                }
                """
        });
    }

    private static CodeFileModel<InterfaceModel> CreateIWorkflowRepositoryFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IWorkflowRepository");
        interfaceModel.Usings.Add(new UsingModel("Workflow.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Entities.Workflow?")] },
            Params = [new ParamModel { Name = "workflowId", Type = new TypeModel("Guid") }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetAllAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Entities.Workflow")] }] },
            Params = [new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetActiveAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Entities.Workflow")] }] },
            Params = [new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Entities.Workflow")] },
            Params = [new ParamModel { Name = "workflow", Type = new TypeModel("Entities.Workflow") }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "UpdateAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params = [new ParamModel { Name = "workflow", Type = new TypeModel("Entities.Workflow") }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "DeleteAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params = [new ParamModel { Name = "workflowId", Type = new TypeModel("Guid") }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetInstanceByIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("WorkflowInstance?")] },
            Params = [new ParamModel { Name = "instanceId", Type = new TypeModel("Guid") }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "AddInstanceAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("WorkflowInstance")] },
            Params = [new ParamModel { Name = "instance", Type = new TypeModel("WorkflowInstance") }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "UpdateInstanceAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params = [new ParamModel { Name = "instance", Type = new TypeModel("WorkflowInstance") }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetApprovalByIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ApprovalRequest?")] },
            Params = [new ParamModel { Name = "approvalId", Type = new TypeModel("Guid") }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "UpdateApprovalAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params = [new ParamModel { Name = "approval", Type = new TypeModel("ApprovalRequest") }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IWorkflowRepository", directory, CSharp)
        {
            Namespace = "Workflow.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateIWorkflowEngineFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IWorkflowEngine");
        interfaceModel.Usings.Add(new UsingModel("Workflow.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "StartWorkflowAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("WorkflowInstance")] },
            Params = [new ParamModel { Name = "workflowId", Type = new TypeModel("Guid") }, new ParamModel { Name = "contextData", Type = new TypeModel("string?"), DefaultValue = "null" }, new ParamModel { Name = "initiatedBy", Type = new TypeModel("Guid?"), DefaultValue = "null" }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "TransitionAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("WorkflowInstance")] },
            Params = [new ParamModel { Name = "instanceId", Type = new TypeModel("Guid") }, new ParamModel { Name = "transitionData", Type = new TypeModel("string?"), DefaultValue = "null" }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "PauseAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("WorkflowInstance")] },
            Params = [new ParamModel { Name = "instanceId", Type = new TypeModel("Guid") }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "ResumeAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("WorkflowInstance")] },
            Params = [new ParamModel { Name = "instanceId", Type = new TypeModel("Guid") }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "CancelAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("WorkflowInstance")] },
            Params = [new ParamModel { Name = "instanceId", Type = new TypeModel("Guid") }, new ParamModel { Name = "reason", Type = new TypeModel("string?"), DefaultValue = "null" }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "CanTransitionAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("bool")] },
            Params = [new ParamModel { Name = "instanceId", Type = new TypeModel("Guid") }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IWorkflowEngine", directory, CSharp)
        {
            Namespace = "Workflow.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateIStateManagerFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IStateManager");
        interfaceModel.Usings.Add(new UsingModel("Workflow.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetNextStepAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("WorkflowStep?")] },
            Params = [new ParamModel { Name = "instance", Type = new TypeModel("WorkflowInstance") }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "ValidateTransitionAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("bool")] },
            Params = [new ParamModel { Name = "instance", Type = new TypeModel("WorkflowInstance") }, new ParamModel { Name = "targetStepId", Type = new TypeModel("Guid") }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "RecordStateChangeAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params = [new ParamModel { Name = "instanceId", Type = new TypeModel("Guid") }, new ParamModel { Name = "fromState", Type = new TypeModel("string") }, new ParamModel { Name = "toState", Type = new TypeModel("string") }, new ParamModel { Name = "action", Type = new TypeModel("string") }, new ParamModel { Name = "performedBy", Type = new TypeModel("Guid?"), DefaultValue = "null" }, new ParamModel { Name = "details", Type = new TypeModel("string?"), DefaultValue = "null" }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetHistoryAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("WorkflowHistory")] }] },
            Params = [new ParamModel { Name = "instanceId", Type = new TypeModel("Guid") }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IStateManager", directory, CSharp)
        {
            Namespace = "Workflow.Core.Interfaces"
        };
    }

    private static CodeFileModel<ClassModel> CreateWorkflowDbContextFile(string directory)
    {
        var classModel = new ClassModel("WorkflowDbContext")
        {
            BaseClass = "DbContext"
        };
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Workflow.Core.Entities"));

        classModel.Constructors.Add(new ConstructorModel(classModel, classModel.Name)
        {
            Params = [new ParamModel { Name = "options", Type = new TypeModel("DbContextOptions") { GenericTypeParameters = [new TypeModel("WorkflowDbContext")] } }],
            BaseParams = ["options"]
        });

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("Core.Entities.Workflow")] }, "Workflows", [new PropertyAccessorModel(PropertyAccessorType.Get)])
        {
            Body = new ExpressionModel("Set<Core.Entities.Workflow>()")
        });

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("WorkflowInstance")] }, "WorkflowInstances", [new PropertyAccessorModel(PropertyAccessorType.Get)])
        {
            Body = new ExpressionModel("Set<WorkflowInstance>()")
        });

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("WorkflowStep")] }, "WorkflowSteps", [new PropertyAccessorModel(PropertyAccessorType.Get)])
        {
            Body = new ExpressionModel("Set<WorkflowStep>()")
        });

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("ApprovalRequest")] }, "ApprovalRequests", [new PropertyAccessorModel(PropertyAccessorType.Get)])
        {
            Body = new ExpressionModel("Set<ApprovalRequest>()")
        });

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("WorkflowHistory")] }, "WorkflowHistories", [new PropertyAccessorModel(PropertyAccessorType.Get)])
        {
            Body = new ExpressionModel("Set<WorkflowHistory>()")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "OnModelCreating",
            Override = true,
            AccessModifier = AccessModifier.Protected,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "modelBuilder", Type = new TypeModel("ModelBuilder") }],
            Body = new ExpressionModel("""
                modelBuilder.Entity<Core.Entities.Workflow>(entity =>
                {
                    entity.HasKey(w => w.WorkflowId);
                    entity.Property(w => w.Name).IsRequired().HasMaxLength(200);
                    entity.Property(w => w.Version).IsRequired().HasMaxLength(50);
                    entity.HasMany(w => w.Steps).WithOne(s => s.Workflow).HasForeignKey(s => s.WorkflowId);
                    entity.HasMany(w => w.Instances).WithOne(i => i.Workflow).HasForeignKey(i => i.WorkflowId);
                });

                modelBuilder.Entity<WorkflowInstance>(entity =>
                {
                    entity.HasKey(i => i.InstanceId);
                    entity.HasOne(i => i.CurrentStep).WithMany().HasForeignKey(i => i.CurrentStepId).OnDelete(DeleteBehavior.Restrict);
                    entity.HasMany(i => i.History).WithOne(h => h.Instance).HasForeignKey(h => h.InstanceId);
                    entity.HasMany(i => i.ApprovalRequests).WithOne(a => a.Instance).HasForeignKey(a => a.InstanceId);
                });

                modelBuilder.Entity<WorkflowStep>(entity =>
                {
                    entity.HasKey(s => s.StepId);
                    entity.Property(s => s.Name).IsRequired().HasMaxLength(200);
                });

                modelBuilder.Entity<ApprovalRequest>(entity =>
                {
                    entity.HasKey(a => a.ApprovalId);
                    entity.HasOne(a => a.Step).WithMany().HasForeignKey(a => a.StepId).OnDelete(DeleteBehavior.Restrict);
                });

                modelBuilder.Entity<WorkflowHistory>(entity =>
                {
                    entity.HasKey(h => h.HistoryId);
                    entity.Property(h => h.Action).IsRequired().HasMaxLength(100);
                    entity.Property(h => h.FromState).IsRequired().HasMaxLength(100);
                    entity.Property(h => h.ToState).IsRequired().HasMaxLength(100);
                });
                """)
        });

        return new CodeFileModel<ClassModel>(classModel, "WorkflowDbContext", directory, CSharp)
        {
            Namespace = "Workflow.Infrastructure.Data"
        };
    }

    private static CodeFileModel<ClassModel> CreateWorkflowRepositoryFile(string directory)
    {
        var classModel = new ClassModel("WorkflowRepository")
        {
            Implements = [new TypeModel("IWorkflowRepository")]
        };
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Workflow.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Workflow.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Workflow.Infrastructure.Data"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "context",
            Type = new TypeModel("WorkflowDbContext"),
            AccessModifier = AccessModifier.Private,
            ReadOnly = true
        });

        classModel.Constructors.Add(new ConstructorModel(classModel, classModel.Name)
        {
            Params = [new ParamModel { Name = "context", Type = new TypeModel("WorkflowDbContext") }],
            Body = new ExpressionModel("this.context = context;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Core.Entities.Workflow?")] },
            Params = [new ParamModel { Name = "workflowId", Type = new TypeModel("Guid") }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }],
            Body = new ExpressionModel("return await context.Workflows.Include(w => w.Steps.OrderBy(s => s.Order)).FirstOrDefaultAsync(w => w.WorkflowId == workflowId, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetAllAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Core.Entities.Workflow")] }] },
            Params = [new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }],
            Body = new ExpressionModel("return await context.Workflows.Include(w => w.Steps.OrderBy(s => s.Order)).ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetActiveAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Core.Entities.Workflow")] }] },
            Params = [new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }],
            Body = new ExpressionModel("return await context.Workflows.Include(w => w.Steps.OrderBy(s => s.Order)).Where(w => w.IsActive && w.Status == WorkflowStatus.Published).ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Core.Entities.Workflow")] },
            Params = [new ParamModel { Name = "workflow", Type = new TypeModel("Core.Entities.Workflow") }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }],
            Body = new ExpressionModel("""
                workflow.WorkflowId = Guid.NewGuid();
                foreach (var step in workflow.Steps)
                {
                    step.StepId = Guid.NewGuid();
                    step.WorkflowId = workflow.WorkflowId;
                }
                await context.Workflows.AddAsync(workflow, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
                return workflow;
                """)
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "UpdateAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params = [new ParamModel { Name = "workflow", Type = new TypeModel("Core.Entities.Workflow") }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }],
            Body = new ExpressionModel("""
                workflow.UpdatedAt = DateTime.UtcNow;
                context.Workflows.Update(workflow);
                await context.SaveChangesAsync(cancellationToken);
                """)
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "DeleteAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params = [new ParamModel { Name = "workflowId", Type = new TypeModel("Guid") }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }],
            Body = new ExpressionModel("""
                var workflow = await context.Workflows.FindAsync(new object[] { workflowId }, cancellationToken);
                if (workflow != null)
                {
                    context.Workflows.Remove(workflow);
                    await context.SaveChangesAsync(cancellationToken);
                }
                """)
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetInstanceByIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("WorkflowInstance?")] },
            Params = [new ParamModel { Name = "instanceId", Type = new TypeModel("Guid") }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }],
            Body = new ExpressionModel("return await context.WorkflowInstances.Include(i => i.Workflow).Include(i => i.CurrentStep).Include(i => i.History).FirstOrDefaultAsync(i => i.InstanceId == instanceId, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddInstanceAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("WorkflowInstance")] },
            Params = [new ParamModel { Name = "instance", Type = new TypeModel("WorkflowInstance") }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }],
            Body = new ExpressionModel("""
                instance.InstanceId = Guid.NewGuid();
                await context.WorkflowInstances.AddAsync(instance, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
                return instance;
                """)
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "UpdateInstanceAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params = [new ParamModel { Name = "instance", Type = new TypeModel("WorkflowInstance") }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }],
            Body = new ExpressionModel("""
                context.WorkflowInstances.Update(instance);
                await context.SaveChangesAsync(cancellationToken);
                """)
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetApprovalByIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ApprovalRequest?")] },
            Params = [new ParamModel { Name = "approvalId", Type = new TypeModel("Guid") }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }],
            Body = new ExpressionModel("return await context.ApprovalRequests.Include(a => a.Instance).Include(a => a.Step).FirstOrDefaultAsync(a => a.ApprovalId == approvalId, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "UpdateApprovalAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params = [new ParamModel { Name = "approval", Type = new TypeModel("ApprovalRequest") }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }],
            Body = new ExpressionModel("""
                context.ApprovalRequests.Update(approval);
                await context.SaveChangesAsync(cancellationToken);
                """)
        });

        return new CodeFileModel<ClassModel>(classModel, "WorkflowRepository", directory, CSharp)
        {
            Namespace = "Workflow.Infrastructure.Repositories"
        };
    }

    private static CodeFileModel<ClassModel> CreateWorkflowEngineFile(string directory)
    {
        var classModel = new ClassModel("WorkflowEngine")
        {
            Implements = [new TypeModel("IWorkflowEngine")]
        };
        classModel.Usings.Add(new UsingModel("Workflow.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Workflow.Core.Interfaces"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "repository",
            Type = new TypeModel("IWorkflowRepository"),
            AccessModifier = AccessModifier.Private,
            ReadOnly = true
        });

        classModel.Fields.Add(new FieldModel
        {
            Name = "stateManager",
            Type = new TypeModel("IStateManager"),
            AccessModifier = AccessModifier.Private,
            ReadOnly = true
        });

        classModel.Constructors.Add(new ConstructorModel(classModel, classModel.Name)
        {
            Params = [new ParamModel { Name = "repository", Type = new TypeModel("IWorkflowRepository") }, new ParamModel { Name = "stateManager", Type = new TypeModel("IStateManager") }],
            Body = new ExpressionModel("""
                this.repository = repository;
                this.stateManager = stateManager;
                """)
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "StartWorkflowAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("WorkflowInstance")] },
            Params = [new ParamModel { Name = "workflowId", Type = new TypeModel("Guid") }, new ParamModel { Name = "contextData", Type = new TypeModel("string?"), DefaultValue = "null" }, new ParamModel { Name = "initiatedBy", Type = new TypeModel("Guid?"), DefaultValue = "null" }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }],
            Body = new ExpressionModel("""
                var workflow = await repository.GetByIdAsync(workflowId, cancellationToken)
                    ?? throw new InvalidOperationException($"Workflow {workflowId} not found");

                var firstStep = workflow.Steps.OrderBy(s => s.Order).FirstOrDefault();

                var instance = new WorkflowInstance
                {
                    WorkflowId = workflowId,
                    CurrentStepId = firstStep?.StepId,
                    ContextData = contextData,
                    InitiatedBy = initiatedBy,
                    Status = InstanceStatus.Running
                };

                var created = await repository.AddInstanceAsync(instance, cancellationToken);

                await stateManager.RecordStateChangeAsync(created.InstanceId, "None", "Started", "WorkflowStarted", initiatedBy, null, cancellationToken);

                return created;
                """)
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "TransitionAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("WorkflowInstance")] },
            Params = [new ParamModel { Name = "instanceId", Type = new TypeModel("Guid") }, new ParamModel { Name = "transitionData", Type = new TypeModel("string?"), DefaultValue = "null" }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }],
            Body = new ExpressionModel("""
                var instance = await repository.GetInstanceByIdAsync(instanceId, cancellationToken)
                    ?? throw new InvalidOperationException($"Instance {instanceId} not found");

                if (instance.Status != InstanceStatus.Running)
                    throw new InvalidOperationException($"Instance is not in Running state");

                var currentStepName = instance.CurrentStep?.Name ?? "Unknown";
                var nextStep = await stateManager.GetNextStepAsync(instance, cancellationToken);

                if (nextStep == null)
                {
                    instance.Status = InstanceStatus.Completed;
                    instance.CompletedAt = DateTime.UtcNow;
                    instance.CurrentStepId = null;
                    await stateManager.RecordStateChangeAsync(instanceId, currentStepName, "Completed", "WorkflowCompleted", null, transitionData, cancellationToken);
                }
                else
                {
                    instance.CurrentStepId = nextStep.StepId;
                    await stateManager.RecordStateChangeAsync(instanceId, currentStepName, nextStep.Name, "StepTransition", null, transitionData, cancellationToken);
                }

                await repository.UpdateInstanceAsync(instance, cancellationToken);
                return instance;
                """)
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "PauseAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("WorkflowInstance")] },
            Params = [new ParamModel { Name = "instanceId", Type = new TypeModel("Guid") }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }],
            Body = new ExpressionModel("""
                var instance = await repository.GetInstanceByIdAsync(instanceId, cancellationToken)
                    ?? throw new InvalidOperationException($"Instance {instanceId} not found");

                var previousStatus = instance.Status.ToString();
                instance.Status = InstanceStatus.Paused;
                await repository.UpdateInstanceAsync(instance, cancellationToken);
                await stateManager.RecordStateChangeAsync(instanceId, previousStatus, "Paused", "WorkflowPaused", null, null, cancellationToken);

                return instance;
                """)
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "ResumeAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("WorkflowInstance")] },
            Params = [new ParamModel { Name = "instanceId", Type = new TypeModel("Guid") }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }],
            Body = new ExpressionModel("""
                var instance = await repository.GetInstanceByIdAsync(instanceId, cancellationToken)
                    ?? throw new InvalidOperationException($"Instance {instanceId} not found");

                if (instance.Status != InstanceStatus.Paused)
                    throw new InvalidOperationException("Instance is not paused");

                instance.Status = InstanceStatus.Running;
                await repository.UpdateInstanceAsync(instance, cancellationToken);
                await stateManager.RecordStateChangeAsync(instanceId, "Paused", "Running", "WorkflowResumed", null, null, cancellationToken);

                return instance;
                """)
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "CancelAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("WorkflowInstance")] },
            Params = [new ParamModel { Name = "instanceId", Type = new TypeModel("Guid") }, new ParamModel { Name = "reason", Type = new TypeModel("string?"), DefaultValue = "null" }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }],
            Body = new ExpressionModel("""
                var instance = await repository.GetInstanceByIdAsync(instanceId, cancellationToken)
                    ?? throw new InvalidOperationException($"Instance {instanceId} not found");

                var previousStatus = instance.Status.ToString();
                instance.Status = InstanceStatus.Cancelled;
                instance.CompletedAt = DateTime.UtcNow;
                await repository.UpdateInstanceAsync(instance, cancellationToken);
                await stateManager.RecordStateChangeAsync(instanceId, previousStatus, "Cancelled", "WorkflowCancelled", null, reason, cancellationToken);

                return instance;
                """)
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "CanTransitionAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("bool")] },
            Params = [new ParamModel { Name = "instanceId", Type = new TypeModel("Guid") }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }],
            Body = new ExpressionModel("""
                var instance = await repository.GetInstanceByIdAsync(instanceId, cancellationToken);
                if (instance == null || instance.Status != InstanceStatus.Running) return false;

                if (instance.CurrentStep?.RequiresApproval == true)
                {
                    var approval = instance.ApprovalRequests.FirstOrDefault(a => a.StepId == instance.CurrentStepId && a.Status == ApprovalStatus.Pending);
                    if (approval != null) return false;
                }

                return true;
                """)
        });

        return new CodeFileModel<ClassModel>(classModel, "WorkflowEngine", directory, CSharp)
        {
            Namespace = "Workflow.Infrastructure.Services"
        };
    }

    private static CodeFileModel<ClassModel> CreateStateManagerFile(string directory)
    {
        var classModel = new ClassModel("StateManager")
        {
            Implements = [new TypeModel("IStateManager")]
        };
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Workflow.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Workflow.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Workflow.Infrastructure.Data"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "context",
            Type = new TypeModel("WorkflowDbContext"),
            AccessModifier = AccessModifier.Private,
            ReadOnly = true
        });

        classModel.Constructors.Add(new ConstructorModel(classModel, classModel.Name)
        {
            Params = [new ParamModel { Name = "context", Type = new TypeModel("WorkflowDbContext") }],
            Body = new ExpressionModel("this.context = context;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetNextStepAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("WorkflowStep?")] },
            Params = [new ParamModel { Name = "instance", Type = new TypeModel("WorkflowInstance") }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }],
            Body = new ExpressionModel("""
                if (instance.CurrentStep?.NextStepId != null)
                {
                    return await context.WorkflowSteps.FindAsync(new object[] { instance.CurrentStep.NextStepId }, cancellationToken);
                }

                var workflow = await context.Workflows.Include(w => w.Steps).FirstOrDefaultAsync(w => w.WorkflowId == instance.WorkflowId, cancellationToken);
                if (workflow == null) return null;

                var orderedSteps = workflow.Steps.OrderBy(s => s.Order).ToList();
                var currentIndex = orderedSteps.FindIndex(s => s.StepId == instance.CurrentStepId);

                if (currentIndex >= 0 && currentIndex < orderedSteps.Count - 1)
                {
                    return orderedSteps[currentIndex + 1];
                }

                return null;
                """)
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "ValidateTransitionAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("bool")] },
            Params = [new ParamModel { Name = "instance", Type = new TypeModel("WorkflowInstance") }, new ParamModel { Name = "targetStepId", Type = new TypeModel("Guid") }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }],
            Body = new ExpressionModel("""
                var targetStep = await context.WorkflowSteps.FindAsync(new object[] { targetStepId }, cancellationToken);
                if (targetStep == null || targetStep.WorkflowId != instance.WorkflowId) return false;

                if (instance.CurrentStep?.NextStepId == targetStepId) return true;

                var orderedSteps = await context.WorkflowSteps.Where(s => s.WorkflowId == instance.WorkflowId).OrderBy(s => s.Order).ToListAsync(cancellationToken);
                var currentIndex = orderedSteps.FindIndex(s => s.StepId == instance.CurrentStepId);
                var targetIndex = orderedSteps.FindIndex(s => s.StepId == targetStepId);

                return targetIndex == currentIndex + 1;
                """)
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "RecordStateChangeAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params = [new ParamModel { Name = "instanceId", Type = new TypeModel("Guid") }, new ParamModel { Name = "fromState", Type = new TypeModel("string") }, new ParamModel { Name = "toState", Type = new TypeModel("string") }, new ParamModel { Name = "action", Type = new TypeModel("string") }, new ParamModel { Name = "performedBy", Type = new TypeModel("Guid?"), DefaultValue = "null" }, new ParamModel { Name = "details", Type = new TypeModel("string?"), DefaultValue = "null" }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }],
            Body = new ExpressionModel("""
                var history = new WorkflowHistory
                {
                    HistoryId = Guid.NewGuid(),
                    InstanceId = instanceId,
                    FromState = fromState,
                    ToState = toState,
                    Action = action,
                    PerformedBy = performedBy,
                    Details = details
                };

                await context.WorkflowHistories.AddAsync(history, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
                """)
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetHistoryAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("WorkflowHistory")] }] },
            Params = [new ParamModel { Name = "instanceId", Type = new TypeModel("Guid") }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }],
            Body = new ExpressionModel("return await context.WorkflowHistories.Where(h => h.InstanceId == instanceId).OrderBy(h => h.OccurredAt).ToListAsync(cancellationToken);")
        });

        return new CodeFileModel<ClassModel>(classModel, "StateManager", directory, CSharp)
        {
            Namespace = "Workflow.Infrastructure.Services"
        };
    }

    private static CodeFileModel<ClassModel> CreateConfigureServicesFile(string directory)
    {
        var classModel = new ClassModel("ConfigureServices")
        {
            Static = true
        };
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.Configuration"));
        classModel.Usings.Add(new UsingModel("Workflow.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Workflow.Infrastructure.Data"));
        classModel.Usings.Add(new UsingModel("Workflow.Infrastructure.Repositories"));
        classModel.Usings.Add(new UsingModel("Workflow.Infrastructure.Services"));

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddWorkflowInfrastructure",
            AccessModifier = AccessModifier.Public,
            Static = true,
            ReturnType = new TypeModel("IServiceCollection"),
            Params = [new ParamModel { Name = "services", Type = new TypeModel("IServiceCollection"), ExtensionMethodParam = true }, new ParamModel { Name = "configuration", Type = new TypeModel("IConfiguration") }],
            Body = new ExpressionModel("""
                services.AddDbContext<WorkflowDbContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString("WorkflowDb") ??
                        @"Server=.\SQLEXPRESS;Database=WorkflowDb;Trusted_Connection=True;TrustServerCertificate=True"));

                services.AddScoped<IWorkflowRepository, WorkflowRepository>();
                services.AddScoped<IWorkflowEngine, WorkflowEngine>();
                services.AddScoped<IStateManager, StateManager>();
                return services;
                """)
        });

        return new CodeFileModel<ClassModel>(classModel, "ConfigureServices", directory, CSharp)
        {
            Namespace = "Microsoft.Extensions.DependencyInjection"
        };
    }

    private static CodeFileModel<ClassModel> CreateWorkflowsControllerFile(string directory)
    {
        var classModel = new ClassModel("WorkflowsController")
        {
            BaseClass = "ControllerBase",
            Attributes = [new AttributeModel { Name = "ApiController" }, new AttributeModel { Name = "Route", Template = "\"api/[controller]\"" }]
        };
        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.Mvc"));
        classModel.Usings.Add(new UsingModel("Workflow.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("Workflow.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Workflow.Core.Interfaces"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "repository",
            Type = new TypeModel("IWorkflowRepository"),
            AccessModifier = AccessModifier.Private,
            ReadOnly = true
        });

        classModel.Fields.Add(new FieldModel
        {
            Name = "engine",
            Type = new TypeModel("IWorkflowEngine"),
            AccessModifier = AccessModifier.Private,
            ReadOnly = true
        });

        classModel.Constructors.Add(new ConstructorModel(classModel, classModel.Name)
        {
            Params = [new ParamModel { Name = "repository", Type = new TypeModel("IWorkflowRepository") }, new ParamModel { Name = "engine", Type = new TypeModel("IWorkflowEngine") }],
            Body = new ExpressionModel("""
                this.repository = repository;
                this.engine = engine;
                """)
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "Create",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("WorkflowDto")] }] },
            Params = [new ParamModel { Name = "request", Type = new TypeModel("CreateWorkflowRequest"), Attributes = [new AttributeModel { Name = "FromBody" }] }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }],
            Attributes = [new AttributeModel { Name = "HttpPost" }],
            Body = new ExpressionModel("""
                var workflow = new Core.Entities.Workflow
                {
                    Name = request.Name,
                    Description = request.Description,
                    Version = request.Version,
                    Steps = request.Steps.Select(s => new WorkflowStep
                    {
                        Name = s.Name,
                        Description = s.Description,
                        Type = Enum.TryParse<StepType>(s.Type, out var type) ? type : StepType.Task,
                        Order = s.Order,
                        RequiresApproval = s.RequiresApproval,
                        Configuration = s.Configuration
                    }).ToList()
                };

                var created = await repository.AddAsync(workflow, cancellationToken);

                var dto = new WorkflowDto
                {
                    WorkflowId = created.WorkflowId,
                    Name = created.Name,
                    Description = created.Description,
                    Version = created.Version,
                    Status = created.Status.ToString(),
                    IsActive = created.IsActive,
                    CreatedAt = created.CreatedAt,
                    Steps = created.Steps.Select(s => new WorkflowStepDto
                    {
                        StepId = s.StepId,
                        Name = s.Name,
                        Description = s.Description,
                        Type = s.Type.ToString(),
                        Order = s.Order,
                        RequiresApproval = s.RequiresApproval
                    })
                };

                return CreatedAtAction(nameof(GetById), new { id = dto.WorkflowId }, dto);
                """)
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetById",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("WorkflowDto")] }] },
            Params = [new ParamModel { Name = "id", Type = new TypeModel("Guid") }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }],
            Attributes = [new AttributeModel { Name = "HttpGet", Template = "\"{id:guid}\"" }],
            Body = new ExpressionModel("""
                var workflow = await repository.GetByIdAsync(id, cancellationToken);
                if (workflow == null) return NotFound();

                return Ok(new WorkflowDto
                {
                    WorkflowId = workflow.WorkflowId,
                    Name = workflow.Name,
                    Description = workflow.Description,
                    Version = workflow.Version,
                    Status = workflow.Status.ToString(),
                    IsActive = workflow.IsActive,
                    CreatedAt = workflow.CreatedAt,
                    Steps = workflow.Steps.Select(s => new WorkflowStepDto
                    {
                        StepId = s.StepId,
                        Name = s.Name,
                        Description = s.Description,
                        Type = s.Type.ToString(),
                        Order = s.Order,
                        RequiresApproval = s.RequiresApproval
                    })
                });
                """)
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "Transition",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("WorkflowInstanceDto")] }] },
            Params = [new ParamModel { Name = "id", Type = new TypeModel("Guid") }, new ParamModel { Name = "request", Type = new TypeModel("TransitionRequest"), Attributes = [new AttributeModel { Name = "FromBody" }] }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }],
            Attributes = [new AttributeModel { Name = "HttpPost", Template = "\"{id:guid}/transition\"" }],
            Body = new ExpressionModel("""
                try
                {
                    var instance = await engine.TransitionAsync(id, request.TransitionData, cancellationToken);

                    return Ok(new WorkflowInstanceDto
                    {
                        InstanceId = instance.InstanceId,
                        WorkflowId = instance.WorkflowId,
                        WorkflowName = instance.Workflow?.Name ?? "Unknown",
                        CurrentStepId = instance.CurrentStepId,
                        CurrentStepName = instance.CurrentStep?.Name,
                        Status = instance.Status.ToString(),
                        StartedAt = instance.StartedAt,
                        CompletedAt = instance.CompletedAt
                    });
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest(new { error = ex.Message });
                }
                """)
        });

        return new CodeFileModel<ClassModel>(classModel, "WorkflowsController", directory, CSharp)
        {
            Namespace = "Workflow.Api.Controllers"
        };
    }

    private static CodeFileModel<ClassModel> CreateApprovalsControllerFile(string directory)
    {
        var classModel = new ClassModel("ApprovalsController")
        {
            BaseClass = "ControllerBase",
            Attributes = [new AttributeModel { Name = "ApiController" }, new AttributeModel { Name = "Route", Template = "\"api/[controller]\"" }]
        };
        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.Mvc"));
        classModel.Usings.Add(new UsingModel("Workflow.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("Workflow.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Workflow.Core.Interfaces"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "repository",
            Type = new TypeModel("IWorkflowRepository"),
            AccessModifier = AccessModifier.Private,
            ReadOnly = true
        });

        classModel.Fields.Add(new FieldModel
        {
            Name = "engine",
            Type = new TypeModel("IWorkflowEngine"),
            AccessModifier = AccessModifier.Private,
            ReadOnly = true
        });

        classModel.Constructors.Add(new ConstructorModel(classModel, classModel.Name)
        {
            Params = [new ParamModel { Name = "repository", Type = new TypeModel("IWorkflowRepository") }, new ParamModel { Name = "engine", Type = new TypeModel("IWorkflowEngine") }],
            Body = new ExpressionModel("""
                this.repository = repository;
                this.engine = engine;
                """)
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "Approve",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult")] },
            Params = [new ParamModel { Name = "id", Type = new TypeModel("Guid") }, new ParamModel { Name = "response", Type = new TypeModel("ApprovalResponse"), Attributes = [new AttributeModel { Name = "FromBody" }] }, new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }],
            Attributes = [new AttributeModel { Name = "HttpPost", Template = "\"{id:guid}/approve\"" }],
            Body = new ExpressionModel("""
                var approval = await repository.GetApprovalByIdAsync(id, cancellationToken);
                if (approval == null) return NotFound();

                if (approval.Status != ApprovalStatus.Pending)
                    return BadRequest(new { error = "Approval request is no longer pending" });

                approval.Status = response.Approved ? ApprovalStatus.Approved : ApprovalStatus.Rejected;
                approval.Comments = response.Comments;
                approval.RespondedBy = response.RespondedBy;
                approval.RespondedAt = DateTime.UtcNow;

                await repository.UpdateApprovalAsync(approval, cancellationToken);

                if (response.Approved)
                {
                    await engine.TransitionAsync(approval.InstanceId, null, cancellationToken);
                }

                return Ok(new { status = approval.Status.ToString(), message = response.Approved ? "Approval granted" : "Approval rejected" });
                """)
        });

        return new CodeFileModel<ClassModel>(classModel, "ApprovalsController", directory, CSharp)
        {
            Namespace = "Workflow.Api.Controllers"
        };
    }
}
