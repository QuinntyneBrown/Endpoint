// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CommandLine;
using Endpoint.Artifacts.Abstractions;
using Endpoint.DotNet.Artifacts.Projects.Services;
using Endpoint.DotNet.Services;
using Endpoint.Engineering.Messaging.Artifacts;
using Endpoint.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Cli.Commands;

/// <summary>
/// Command to add a shared Messaging project to a solution.
/// </summary>
[Verb("messaging-project-add", HelpText = "Add a shared Messaging project with Redis pub/sub and MessagePack support")]
public class MessagingProjectAddRequest : IRequest
{
    /// <summary>
    /// Gets or sets the path to a JSON file containing message definitions.
    /// </summary>
    [Option('f', "file", Required = false, HelpText = "Path to a JSON file containing message definitions")]
    public string? FilePath { get; set; }

    /// <summary>
    /// Gets or sets the paths to multiple JSON files containing message definitions.
    /// </summary>
    [Option('p', "paths", Required = false, Separator = ',', HelpText = "Comma-separated paths to JSON files containing message definitions")]
    public IEnumerable<string>? Paths { get; set; }

    /// <summary>
    /// Gets or sets the directory where the messaging project will be created.
    /// </summary>
    [Option('d', "directory", Required = false, HelpText = "Directory where the messaging project will be created")]
    public string Directory { get; set; } = Environment.CurrentDirectory;

    /// <summary>
    /// Gets or sets the project name (without .Messaging suffix).
    /// </summary>
    [Option('n', "name", Required = false, HelpText = "Project name (without .Messaging suffix). Required if not using a definition file.")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets whether to include Redis pub/sub support.
    /// </summary>
    [Option("include-redis", Required = false, Default = true, HelpText = "Include Redis pub/sub support")]
    public bool IncludeRedisPubSub { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to use LZ4 compression with MessagePack.
    /// </summary>
    [Option("use-compression", Required = false, Default = true, HelpText = "Use LZ4 compression with MessagePack")]
    public bool UseLz4Compression { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to generate a sample definition file.
    /// </summary>
    [Option("generate-sample", Required = false, Default = false, HelpText = "Generate a sample message definition JSON file")]
    public bool GenerateSample { get; set; }
}

/// <summary>
/// Handler for the MessagingProjectAddRequest command.
/// </summary>
public class MessagingProjectAddRequestHandler : IRequestHandler<MessagingProjectAddRequest>
{
    private readonly ILogger<MessagingProjectAddRequestHandler> _logger;
    private readonly IMessagingArtifactFactory _messagingFactory;
    private readonly IProjectService _projectService;
    private readonly IFileSystem _fileSystem;
    private readonly IFileProvider _fileProvider;
    private readonly IArtifactGenerator _artifactGenerator;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessagingProjectAddRequestHandler"/> class.
    /// </summary>
    public MessagingProjectAddRequestHandler(
        ILogger<MessagingProjectAddRequestHandler> logger,
        IMessagingArtifactFactory messagingFactory,
        IProjectService projectService,
        IFileSystem fileSystem,
        IFileProvider fileProvider,
        IArtifactGenerator artifactGenerator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _messagingFactory = messagingFactory ?? throw new ArgumentNullException(nameof(messagingFactory));
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
        _artifactGenerator = artifactGenerator ?? throw new ArgumentNullException(nameof(artifactGenerator));
    }

    /// <inheritdoc/>
    public async Task Handle(MessagingProjectAddRequest request, CancellationToken cancellationToken)
    {
        if (request.GenerateSample)
        {
            await GenerateSampleDefinitionFile(request.Directory, cancellationToken);
            return;
        }

        var filePaths = GetFilePaths(request);

        if (filePaths.Count == 0 && string.IsNullOrWhiteSpace(request.Name))
        {
            _logger.LogError("Either a definition file path (-f or -p) or project name (-n) is required.");
            _logger.LogInformation("Use --generate-sample to create a sample message definition file.");
            return;
        }

        try
        {
            var outputDirectory = ResolveSrcRootDirectory(request.Directory);
            MessagingProjectModel projectModel;

            if (filePaths.Count > 0)
            {
                _logger.LogInformation("Creating messaging project from {FileCount} definition file(s)", filePaths.Count);
                projectModel = await _messagingFactory.CreateMessagingProjectFromFilesAsync(filePaths, outputDirectory, cancellationToken);
            }
            else
            {
                _logger.LogInformation("Creating messaging project: {Name}", request.Name);
                var definition = new Messaging.Models.MessagingProjectDefinition
                {
                    ProjectName = request.Name!,
                    IncludeRedisPubSub = request.IncludeRedisPubSub,
                    UseLz4Compression = request.UseLz4Compression
                };
                projectModel = await _messagingFactory.CreateMessagingProjectAsync(definition, outputDirectory, cancellationToken);
            }

            // Create project directory
            _fileSystem.Directory.CreateDirectory(projectModel.Directory);

            // Generate project and files
            await _artifactGenerator.GenerateAsync(projectModel);

            _logger.LogInformation("Successfully created messaging project: {ProjectName}", projectModel.Name);
            _logger.LogInformation("  Location: {Directory}", projectModel.Directory);
            _logger.LogInformation("  Messages: {MessageCount}", projectModel.Messages.Count);
            _logger.LogInformation("  Redis Pub/Sub: {IncludeRedisPubSub}", projectModel.IncludeRedisPubSub);
            _logger.LogInformation("  LZ4 Compression: {UseLz4Compression}", projectModel.UseLz4Compression);

            if (projectModel.Messages.Count > 0)
            {
                _logger.LogInformation("  Generated messages:");
                foreach (var message in projectModel.Messages)
                {
                    _logger.LogInformation("    - {MessageName} ({MessageKind})", message.Name, message.Kind);
                }
            }
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError("{Message}", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating messaging project");
        }
    }

    private List<string> GetFilePaths(MessagingProjectAddRequest request)
    {
        var paths = new List<string>();

        if (!string.IsNullOrWhiteSpace(request.FilePath))
        {
            paths.Add(Path.GetFullPath(request.FilePath));
        }

        if (request.Paths != null)
        {
            paths.AddRange(request.Paths.Select(p => Path.GetFullPath(p.Trim())));
        }

        return paths;
    }

    private string ResolveSrcRootDirectory(string directory)
    {
        var solutionPath = _fileProvider.Get("*.sln", directory, 0);

        var baseDirectory = solutionPath == Endpoint.Constants.FileNotFound
            ? directory
            : _fileSystem.Path.GetDirectoryName(solutionPath)!;

        var srcDirectory = _fileSystem.Path.Combine(baseDirectory, "src");

        if (!_fileSystem.Directory.Exists(srcDirectory))
        {
            _fileSystem.Directory.CreateDirectory(srcDirectory);
        }

        _logger.LogInformation("Creating messaging project under: {SrcDirectory}", srcDirectory);

        return srcDirectory;
    }

    private async Task GenerateSampleDefinitionFile(string directory, CancellationToken cancellationToken)
    {
        var sampleContent = """
            {
              "$schema": "https://endpoint.dev/schemas/messaging/v1.0.json",
              "projectName": "MyProject",
              "namespace": "MyProject.Messaging",
              "description": "Shared messaging project for microservices communication",
              "version": "1.0.0",
              "useLz4Compression": true,
              "includeRedisPubSub": true,
              "messages": [
                {
                  "name": "OrderCreated",
                  "kind": "Event",
                  "messageType": "com.example.orders.v1.OrderCreated",
                  "description": "Event raised when a new order is created",
                  "schemaVersion": 1,
                  "aggregateType": "Order",
                  "channel": "orders",
                  "properties": [
                    {
                      "name": "OrderId",
                      "type": "Guid",
                      "required": true,
                      "description": "The unique identifier of the order"
                    },
                    {
                      "name": "CustomerId",
                      "type": "Guid",
                      "required": true,
                      "description": "The customer who placed the order"
                    },
                    {
                      "name": "TotalAmount",
                      "type": "decimal",
                      "required": true,
                      "description": "The total amount of the order"
                    },
                    {
                      "name": "Items",
                      "type": "OrderItem",
                      "isCollection": true,
                      "collectionType": "List",
                      "description": "The items in the order"
                    }
                  ]
                },
                {
                  "name": "OrderShipped",
                  "kind": "Event",
                  "messageType": "com.example.orders.v1.OrderShipped",
                  "description": "Event raised when an order is shipped",
                  "aggregateType": "Order",
                  "channel": "orders",
                  "properties": [
                    {
                      "name": "OrderId",
                      "type": "Guid",
                      "required": true
                    },
                    {
                      "name": "TrackingNumber",
                      "type": "string",
                      "required": true
                    },
                    {
                      "name": "Carrier",
                      "type": "string",
                      "required": true
                    },
                    {
                      "name": "ShippedAt",
                      "type": "DateTimeOffset",
                      "required": true
                    }
                  ]
                },
                {
                  "name": "ProcessPayment",
                  "kind": "Command",
                  "messageType": "com.example.payments.v1.ProcessPayment",
                  "description": "Command to process a payment for an order",
                  "channel": "payments",
                  "properties": [
                    {
                      "name": "PaymentId",
                      "type": "Guid",
                      "required": true
                    },
                    {
                      "name": "OrderId",
                      "type": "Guid",
                      "required": true
                    },
                    {
                      "name": "Amount",
                      "type": "decimal",
                      "required": true
                    },
                    {
                      "name": "Currency",
                      "type": "string",
                      "required": true,
                      "defaultValue": "\"USD\""
                    }
                  ]
                }
              ],
              "channels": [
                {
                  "name": "orders",
                  "description": "Channel for order-related events",
                  "publishedMessages": ["OrderCreated", "OrderShipped"]
                },
                {
                  "name": "payments",
                  "description": "Channel for payment commands",
                  "subscribedMessages": ["ProcessPayment"]
                }
              ]
            }
            """;

        var filePath = Path.Combine(directory, "messages.json");
        await File.WriteAllTextAsync(filePath, sampleContent, cancellationToken);

        _logger.LogInformation("Sample message definition file created: {FilePath}", filePath);
        _logger.LogInformation("Use 'messaging-project-add -f {FilePath}' to create a messaging project from this file.", filePath);
    }
}
