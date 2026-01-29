// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using Endpoint.Services;
using Endpoint.DotNet.SharedLibrary.Configuration;
using Endpoint.Internal;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.SharedLibrary.Services.Generators;

/// <summary>
/// Generates the Shared.Domain project.
/// </summary>
public class DomainProjectGenerator : ProjectGeneratorBase
{
    public DomainProjectGenerator(
        ILogger<DomainProjectGenerator> logger,
        IFileSystem fileSystem,
        ICommandService commandService,
        ITemplateLocator templateLocator,
        ITemplateProcessor templateProcessor)
        : base(logger, fileSystem, commandService, templateLocator, templateProcessor)
    {
    }

    public override int Order => 1;

    public override bool ShouldGenerate(SharedLibraryConfig config) => true;

    public override async Task GenerateAsync(GeneratorContext context, CancellationToken cancellationToken = default)
    {
        var projectName = $"{context.LibraryName}.Domain";
        var projectDirectory = CreateProjectDirectory(context, projectName);
        var ns = $"{context.Namespace}.{context.LibraryName}.Domain";

        Logger.LogInformation("Generating {ProjectName}", projectName);

        // Generate csproj
        await GenerateCsprojAsync(
            projectDirectory,
            projectName,
            context.TargetFramework,
            cancellationToken: cancellationToken);

        // Generate Result type
        await WriteFileAsync(
            FileSystem.Path.Combine(projectDirectory, "Result.cs"),
            GenerateResultClass(ns),
            cancellationToken);

        // Generate Error type
        await WriteFileAsync(
            FileSystem.Path.Combine(projectDirectory, "Error.cs"),
            GenerateErrorClass(ns),
            cancellationToken);

        // Generate StronglyTypedId base
        await WriteFileAsync(
            FileSystem.Path.Combine(projectDirectory, "StronglyTypedId.cs"),
            GenerateStronglyTypedIdBase(ns),
            cancellationToken);

        // Generate ValueObject base
        await WriteFileAsync(
            FileSystem.Path.Combine(projectDirectory, "ValueObject.cs"),
            GenerateValueObjectBase(ns),
            cancellationToken);

        // Generate configured strongly-typed IDs
        var idsDirectory = FileSystem.Path.Combine(projectDirectory, "Ids");
        FileSystem.Directory.CreateDirectory(idsDirectory);

        foreach (var id in context.Config.Domain.StronglyTypedIds)
        {
            await WriteFileAsync(
                FileSystem.Path.Combine(idsDirectory, $"{id.Name}.cs"),
                GenerateStronglyTypedId(ns, id),
                cancellationToken);
        }

        // Generate configured value objects
        var voDirectory = FileSystem.Path.Combine(projectDirectory, "ValueObjects");
        FileSystem.Directory.CreateDirectory(voDirectory);

        foreach (var vo in context.Config.Domain.ValueObjects)
        {
            await WriteFileAsync(
                FileSystem.Path.Combine(voDirectory, $"{vo.Name}.cs"),
                GenerateValueObject(ns, vo),
                cancellationToken);
        }

        // Add project to solution
        var projectPath = FileSystem.Path.Combine(projectDirectory, $"{projectName}.csproj");
        AddProjectToSolution(context, projectPath);
    }

    public override Task<GenerationPreview> PreviewAsync(GeneratorContext context, CancellationToken cancellationToken = default)
    {
        var preview = new GenerationPreview();
        var projectName = $"{context.LibraryName}.Domain";
        var basePath = $"src/{context.LibraryName}/{projectName}";

        preview.Projects.Add(projectName);
        preview.Files.Add($"{basePath}/{projectName}.csproj");
        preview.Files.Add($"{basePath}/Result.cs");
        preview.Files.Add($"{basePath}/Error.cs");
        preview.Files.Add($"{basePath}/StronglyTypedId.cs");
        preview.Files.Add($"{basePath}/ValueObject.cs");

        foreach (var id in context.Config.Domain.StronglyTypedIds)
        {
            preview.Files.Add($"{basePath}/Ids/{id.Name}.cs");
        }

        foreach (var vo in context.Config.Domain.ValueObjects)
        {
            preview.Files.Add($"{basePath}/ValueObjects/{vo.Name}.cs");
        }

        return Task.FromResult(preview);
    }

    private string GenerateResultClass(string ns)
    {
        return $@"// Auto-generated code
namespace {ns};

/// <summary>
/// Represents the result of an operation.
/// </summary>
public class Result
{{
    protected Result(bool isSuccess, Error error)
    {{
        if (isSuccess && error != Error.None)
        {{
            throw new InvalidOperationException();
        }}

        if (!isSuccess && error == Error.None)
        {{
            throw new InvalidOperationException();
        }}

        IsSuccess = isSuccess;
        Error = error;
    }}

    public bool IsSuccess {{ get; }}

    public bool IsFailure => !IsSuccess;

    public Error Error {{ get; }}

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}}

/// <summary>
/// Represents the result of an operation with a value.
/// </summary>
/// <typeparam name=""TValue"">The value type.</typeparam>
public class Result<TValue> : Result
{{
    private readonly TValue? _value;

    protected internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {{
        _value = value;
    }}

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException(""Cannot access value of a failed result."");

    public static implicit operator Result<TValue>(TValue value) => Success(value);
}}
";
    }

    private string GenerateErrorClass(string ns)
    {
        return $@"// Auto-generated code
namespace {ns};

/// <summary>
/// Represents an error.
/// </summary>
public sealed record Error(string Code, string Message)
{{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static readonly Error NullValue = new(""Error.NullValue"", ""A null value was provided."");

    public static Error NotFound(string entityName, object id) =>
        new($""{{entityName}}.NotFound"", $""{{entityName}} with id '{{id}}' was not found."");

    public static Error Validation(string propertyName, string message) =>
        new($""Validation.{{propertyName}}"", message);
}}
";
    }

    private string GenerateStronglyTypedIdBase(string ns)
    {
        return $@"// Auto-generated code
namespace {ns};

/// <summary>
/// Base class for strongly-typed IDs with Guid underlying type.
/// </summary>
public abstract class StronglyTypedId<TSelf> : IEquatable<TSelf>
    where TSelf : StronglyTypedId<TSelf>
{{
    public Guid Value {{ get; }}

    protected StronglyTypedId(Guid value)
    {{
        Value = value;
    }}

    public bool Equals(TSelf? other)
    {{
        if (other is null) return false;
        return Value == other.Value;
    }}

    public override bool Equals(object? obj)
    {{
        return obj is TSelf other && Equals(other);
    }}

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString();

    public static bool operator ==(StronglyTypedId<TSelf>? left, StronglyTypedId<TSelf>? right)
    {{
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }}

    public static bool operator !=(StronglyTypedId<TSelf>? left, StronglyTypedId<TSelf>? right)
    {{
        return !(left == right);
    }}
}}
";
    }

    private string GenerateValueObjectBase(string ns)
    {
        return $@"// Auto-generated code
namespace {ns};

/// <summary>
/// Base class for value objects.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public bool Equals(ValueObject? other)
    {{
        if (other is null || GetType() != other.GetType())
        {{
            return false;
        }}

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }}

    public override bool Equals(object? obj)
    {{
        return obj is ValueObject other && Equals(other);
    }}

    public override int GetHashCode()
    {{
        return GetEqualityComponents()
            .Aggregate(1, (current, obj) =>
                HashCode.Combine(current, obj?.GetHashCode() ?? 0));
    }}

    public static bool operator ==(ValueObject? left, ValueObject? right)
    {{
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }}

    public static bool operator !=(ValueObject? left, ValueObject? right)
    {{
        return !(left == right);
    }}
}}
";
    }

    private string GenerateStronglyTypedId(string ns, StronglyTypedIdConfig config)
    {
        return $@"// Auto-generated code
namespace {ns}.Ids;

/// <summary>
/// Strongly-typed ID for {config.Name}.
/// </summary>
public sealed class {config.Name} : StronglyTypedId<{config.Name}>
{{
    public {config.Name}(Guid value) : base(value)
    {{
    }}

    public static {config.Name} New() => new(Guid.NewGuid());

    public static {config.Name} From(Guid value) => new(value);

    public static {config.Name} Parse(string value) => new(Guid.Parse(value));

    public static bool TryParse(string value, out {config.Name}? result)
    {{
        if (Guid.TryParse(value, out var guid))
        {{
            result = new {config.Name}(guid);
            return true;
        }}

        result = null;
        return false;
    }}
}}
";
    }

    private string GenerateValueObject(string ns, ValueObjectConfig config)
    {
        var properties = string.Join("\n", config.Properties.Select(p =>
            $"    public {p.Type} {p.Name} {{ get; }}"));

        var constructorParams = string.Join(", ", config.Properties.Select(p =>
            $"{p.Type} {ToCamelCase(p.Name)}"));

        var constructorAssignments = string.Join("\n", config.Properties.Select(p =>
            $"        {p.Name} = {ToCamelCase(p.Name)};"));

        var equalityComponents = string.Join(",\n", config.Properties.Select(p =>
            $"            {p.Name}"));

        return $@"// Auto-generated code
namespace {ns}.ValueObjects;

/// <summary>
/// Value object for {config.Name}.
/// </summary>
public sealed class {config.Name} : ValueObject
{{
{properties}

    public {config.Name}({constructorParams})
    {{
{constructorAssignments}
    }}

    protected override IEnumerable<object?> GetEqualityComponents()
    {{
        yield return new object?[]
        {{
{equalityComponents}
        }};
    }}
}}
";
    }

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        return char.ToLowerInvariant(name[0]) + name.Substring(1);
    }
}
