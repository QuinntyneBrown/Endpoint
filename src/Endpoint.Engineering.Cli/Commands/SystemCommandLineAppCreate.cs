// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.DotNet.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Cli.Commands;

[Verb("system-commandline-app-create")]
public class SystemCommandLineAppCreateRequest : IRequest
{
    [Option('n', "name", HelpText = "The name of the CLI application")]
    public string Name { get; set; } = "MyCli";

    [Option('c', "command", HelpText = "The name of the initial command component")]
    public string CommandName { get; set; } = "HelloWorld";

    [Option('d', Required = false, HelpText = "The target directory")]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class SystemCommandLineAppCreateRequestHandler : IRequestHandler<SystemCommandLineAppCreateRequest>
{
    private readonly ILogger<SystemCommandLineAppCreateRequestHandler> _logger;
    private readonly IFileSystem _fileSystem;
    private readonly ICommandService _commandService;

    public SystemCommandLineAppCreateRequestHandler(
        ILogger<SystemCommandLineAppCreateRequestHandler> logger,
        IFileSystem fileSystem,
        ICommandService commandService)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(fileSystem);
        ArgumentNullException.ThrowIfNull(commandService);

        _logger = logger;
        _fileSystem = fileSystem;
        _commandService = commandService;
    }

    public async Task Handle(SystemCommandLineAppCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating System.CommandLine CLI application: {Name}", request.Name);

        var solutionDirectory = _fileSystem.Path.Combine(request.Directory, request.Name);
        var srcDirectory = _fileSystem.Path.Combine(solutionDirectory, "src");
        var testDirectory = _fileSystem.Path.Combine(solutionDirectory, "test");
        var appProjectDirectory = _fileSystem.Path.Combine(srcDirectory, request.Name);
        var testProjectDirectory = _fileSystem.Path.Combine(testDirectory, $"{request.Name}.Tests");
        var commandsDirectory = _fileSystem.Path.Combine(appProjectDirectory, "Commands");

        // Create directory structure
        _fileSystem.Directory.CreateDirectory(solutionDirectory);
        _fileSystem.Directory.CreateDirectory(srcDirectory);
        _fileSystem.Directory.CreateDirectory(testDirectory);
        _fileSystem.Directory.CreateDirectory(appProjectDirectory);
        _fileSystem.Directory.CreateDirectory(testProjectDirectory);
        _fileSystem.Directory.CreateDirectory(commandsDirectory);

        // Create solution file
        _commandService.Start($"dotnet new sln -n {request.Name}", solutionDirectory);

        // Create console project
        _commandService.Start("dotnet new console", appProjectDirectory);

        // Add System.CommandLine and DI packages
        _commandService.Start("dotnet add package System.CommandLine", appProjectDirectory);
        _commandService.Start("dotnet add package Microsoft.Extensions.DependencyInjection", appProjectDirectory);
        _commandService.Start("dotnet add package Microsoft.Extensions.Hosting", appProjectDirectory);

        // Create test project
        _commandService.Start("dotnet new xunit", testProjectDirectory);
        _commandService.Start($"dotnet add reference ../../src/{request.Name}/{request.Name}.csproj", testProjectDirectory);

        // Add projects to solution
        _commandService.Start($"dotnet sln add src/{request.Name}/{request.Name}.csproj", solutionDirectory);
        _commandService.Start($"dotnet sln add test/{request.Name}.Tests/{request.Name}.Tests.csproj", solutionDirectory);

        // Generate source files
        await GenerateProgramCsAsync(request.Name, request.CommandName, appProjectDirectory);
        await GenerateConfigureServicesAsync(request.Name, request.CommandName, appProjectDirectory);
        await GenerateCommandFileAsync(request.Name, request.CommandName, commandsDirectory);
        await GenerateCommandTestAsync(request.Name, request.CommandName, testProjectDirectory);

        // Open in VS Code
        _commandService.Start($"code .", solutionDirectory);

        _logger.LogInformation("System.CommandLine CLI application created at: {Directory}", solutionDirectory);
    }

    private async Task GenerateProgramCsAsync(string appName, string commandName, string projectDirectory)
    {
        var content = $@"// Copyright (c) {appName}. All Rights Reserved.

using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using {appName}.Commands;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddServices();

var host = builder.Build();

var rootCommand = new RootCommand(""{appName} CLI Application"");

var {ToCamelCase(commandName)}Command = host.Services.GetRequiredService<{commandName}Command>();
rootCommand.AddCommand({ToCamelCase(commandName)}Command.Create());

return await rootCommand.InvokeAsync(args);
";
        var filePath = _fileSystem.Path.Combine(projectDirectory, "Program.cs");
        await _fileSystem.File.WriteAllTextAsync(filePath, content);
    }

    private async Task GenerateConfigureServicesAsync(string appName, string commandName, string projectDirectory)
    {
        var content = $@"// Copyright (c) {appName}. All Rights Reserved.

using {appName}.Commands;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {{
        services.AddSingleton<{commandName}Command>();

        return services;
    }}
}}
";
        var filePath = _fileSystem.Path.Combine(projectDirectory, "ConfigureServices.cs");
        await _fileSystem.File.WriteAllTextAsync(filePath, content);
    }

    private async Task GenerateCommandFileAsync(string appName, string commandName, string commandsDirectory)
    {
        var content = $@"// Copyright (c) {appName}. All Rights Reserved.

using System.CommandLine;

namespace {appName}.Commands;

public class {commandName}Command
{{
    public Command Create()
    {{
        var command = new Command(""{ToKebabCase(commandName)}"", ""Displays a greeting message"");

        var nameOption = new Option<string>(
            aliases: [""-n"", ""--name""],
            description: ""The name to greet"",
            getDefaultValue: () => ""World"");

        command.AddOption(nameOption);

        command.SetHandler(Execute, nameOption);

        return command;
    }}

    public void Execute(string name)
    {{
        Console.WriteLine(GetGreeting(name));
    }}

    public string GetGreeting(string name)
    {{
        return $""Hello, {{name}}!"";
    }}
}}
";
        var filePath = _fileSystem.Path.Combine(commandsDirectory, $"{commandName}Command.cs");
        await _fileSystem.File.WriteAllTextAsync(filePath, content);
    }

    private async Task GenerateCommandTestAsync(string appName, string commandName, string testProjectDirectory)
    {
        var content = $@"// Copyright (c) {appName}. All Rights Reserved.

using {appName}.Commands;

namespace {appName}.Tests;

public class {commandName}CommandTests
{{
    [Fact]
    public void GetGreeting_WithName_ReturnsExpectedGreeting()
    {{
        // Arrange
        var command = new {commandName}Command();
        var name = ""Test"";

        // Act
        var result = command.GetGreeting(name);

        // Assert
        Assert.Equal(""Hello, Test!"", result);
    }}

    [Fact]
    public void GetGreeting_WithDefaultName_ReturnsHelloWorld()
    {{
        // Arrange
        var command = new {commandName}Command();
        var name = ""World"";

        // Act
        var result = command.GetGreeting(name);

        // Assert
        Assert.Equal(""Hello, World!"", result);
    }}
}}
";
        var filePath = _fileSystem.Path.Combine(testProjectDirectory, $"{commandName}CommandTests.cs");
        await _fileSystem.File.WriteAllTextAsync(filePath, content);

        // Delete default test file
        var defaultTestFile = _fileSystem.Path.Combine(testProjectDirectory, "UnitTest1.cs");
        if (_fileSystem.File.Exists(defaultTestFile))
        {
            _fileSystem.File.Delete(defaultTestFile);
        }
    }

    private static string ToCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return char.ToLowerInvariant(value[0]) + value[1..];
    }

    private static string ToKebabCase(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var result = new System.Text.StringBuilder();
        for (int i = 0; i < value.Length; i++)
        {
            if (char.IsUpper(value[i]))
            {
                if (i > 0)
                    result.Append('-');
                result.Append(char.ToLowerInvariant(value[i]));
            }
            else
            {
                result.Append(value[i]);
            }
        }
        return result.ToString();
    }
}
