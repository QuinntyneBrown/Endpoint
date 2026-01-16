// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts.Abstractions;
using Endpoint.DotNet.Artifacts.Files;
using Endpoint.DotNet.Syntax;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Constructors;
using Endpoint.DotNet.Syntax.Expressions;
using Endpoint.DotNet.Syntax.Fields;
using Endpoint.DotNet.Syntax.Methods;
using Endpoint.DotNet.Syntax.Params;
using Endpoint.DotNet.Syntax.Properties;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.CyclicRandomizr;

using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

public class CyclicRandomizrService : ICyclicRandomizrService
{
    private readonly ILogger<CyclicRandomizrService> _logger;
    private readonly IArtifactGenerator _artifactGenerator;

    public CyclicRandomizrService(
        ILogger<CyclicRandomizrService> logger,
        IArtifactGenerator artifactGenerator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _artifactGenerator = artifactGenerator ?? throw new ArgumentNullException(nameof(artifactGenerator));
    }

    public async Task GenerateRandomizerAsync(string fullyQualifiedTypeName, string directory, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating cyclic randomizer for type: {TypeName}", fullyQualifiedTypeName);

        var typeParts = fullyQualifiedTypeName.Split('.');
        var typeName = typeParts.Last();
        var typeNamespace = string.Join(".", typeParts.Take(typeParts.Length - 1));

        var randomizerClassName = $"{typeName}Randomizer";

        var classModel = new ClassModel(randomizerClassName);

        // Add usings
        classModel.Usings.Add(new UsingModel("System"));
        classModel.Usings.Add(new UsingModel("System.Collections.Generic"));
        classModel.Usings.Add(new UsingModel("System.Reflection"));
        classModel.Usings.Add(new UsingModel("System.Threading"));
        classModel.Usings.Add(new UsingModel("System.Threading.Channels"));
        classModel.Usings.Add(new UsingModel("System.Threading.Tasks"));

        if (!string.IsNullOrEmpty(typeNamespace))
        {
            classModel.Usings.Add(new UsingModel(typeNamespace));
        }

        // Add fields
        classModel.Fields.Add(new FieldModel
        {
            Type = new TypeModel("Random"),
            Name = "_random",
            DefaultValue = "new()"
        });

        classModel.Fields.Add(new FieldModel
        {
            Type = new TypeModel("CancellationTokenSource") { Nullable = true },
            Name = "_cts",
            ReadOnly = false
        });

        classModel.Fields.Add(new FieldModel
        {
            Type = new TypeModel("bool"),
            Name = "_isRunning",
            ReadOnly = false
        });

        classModel.Fields.Add(new FieldModel
        {
            Type = new TypeModel("Task") { Nullable = true },
            Name = "_runningTask",
            ReadOnly = false
        });

        // Add properties
        var hzProperty = new PropertyModel(
            classModel,
            AccessModifier.Public,
            new TypeModel("double"),
            "Hz",
            PropertyAccessorModel.GetSet)
        {
            DefaultValue = "1.0"
        };
        classModel.Properties.Add(hzProperty);

        var excludedPropertiesProperty = new PropertyModel(
            classModel,
            AccessModifier.Public,
            new TypeModel("HashSet") { GenericTypeParameters = [new TypeModel("string")] },
            "ExcludedProperties",
            PropertyAccessorModel.GetSet)
        {
            DefaultValue = "new()"
        };
        classModel.Properties.Add(excludedPropertiesProperty);

        var isRunningProperty = new PropertyModel(
            classModel,
            AccessModifier.Public,
            new TypeModel("bool"),
            "IsRunning",
            [PropertyAccessorModel.Get])
        {
            Body = new ExpressionModel("_isRunning")
        };
        classModel.Properties.Add(isRunningProperty);

        // Add StartAsync method
        var startAsyncMethod = new MethodModel
        {
            Name = "StartAsync",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("Task"),
            Async = true,
            Params =
            [
                new ParamModel
                {
                    Name = "writer",
                    Type = new TypeModel("ChannelWriter") { GenericTypeParameters = [new TypeModel(typeName)] }
                },
                new ParamModel
                {
                    Name = "cancellationToken",
                    Type = new TypeModel("CancellationToken"),
                    DefaultValue = "default"
                }
            ],
            Body = new ExpressionModel($$"""
if (_isRunning)
{
    throw new InvalidOperationException("Randomizer is already running.");
}

_cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
_isRunning = true;

_runningTask = Task.Run(async () =>
{
    try
    {
        var delayMs = (int)(1000.0 / Hz);
        while (!_cts.Token.IsCancellationRequested)
        {
            var instance = CreateRandomInstance();
            await writer.WriteAsync(instance, _cts.Token);
            await Task.Delay(delayMs, _cts.Token);
        }
    }
    catch (OperationCanceledException)
    {
        // Expected when stopping
    }
    finally
    {
        _isRunning = false;
    }
}, _cts.Token);

await Task.CompletedTask;
""")
        };
        classModel.Methods.Add(startAsyncMethod);

        // Add Stop method
        var stopMethod = new MethodModel
        {
            Name = "Stop",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("void"),
            Body = new ExpressionModel("""
if (!_isRunning)
{
    return;
}

_cts?.Cancel();
""")
        };
        classModel.Methods.Add(stopMethod);

        // Add StopAsync method
        var stopAsyncMethod = new MethodModel
        {
            Name = "StopAsync",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("Task"),
            Async = true,
            Body = new ExpressionModel("""
Stop();

if (_runningTask != null)
{
    try
    {
        await _runningTask;
    }
    catch (OperationCanceledException)
    {
        // Expected
    }
}
""")
        };
        classModel.Methods.Add(stopAsyncMethod);

        // Add CreateRandomInstance method
        var createRandomInstanceMethod = new MethodModel
        {
            Name = "CreateRandomInstance",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel(typeName),
            Body = new ExpressionModel($$"""
var instance = Activator.CreateInstance<{{typeName}}>();
var properties = typeof({{typeName}}).GetProperties(BindingFlags.Public | BindingFlags.Instance);

foreach (var property in properties)
{
    if (ExcludedProperties.Contains(property.Name))
    {
        continue;
    }

    if (!property.CanWrite)
    {
        continue;
    }

    var randomValue = GenerateRandomValue(property.PropertyType);
    if (randomValue != null)
    {
        property.SetValue(instance, randomValue);
    }
}

return instance;
""")
        };
        classModel.Methods.Add(createRandomInstanceMethod);

        // Add GenerateRandomValue method
        var generateRandomValueMethod = new MethodModel
        {
            Name = "GenerateRandomValue",
            AccessModifier = AccessModifier.Private,
            ReturnType = new TypeModel("object") { Nullable = true },
            Params =
            [
                new ParamModel
                {
                    Name = "type",
                    Type = new TypeModel("Type")
                }
            ],
            Body = new ExpressionModel("""
var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

if (underlyingType == typeof(int))
{
    return _random.Next();
}
else if (underlyingType == typeof(long))
{
    return (long)_random.Next() << 32 | (uint)_random.Next();
}
else if (underlyingType == typeof(short))
{
    return (short)_random.Next(short.MinValue, short.MaxValue);
}
else if (underlyingType == typeof(byte))
{
    return (byte)_random.Next(0, 256);
}
else if (underlyingType == typeof(double))
{
    return _random.NextDouble() * 1000;
}
else if (underlyingType == typeof(float))
{
    return (float)(_random.NextDouble() * 1000);
}
else if (underlyingType == typeof(decimal))
{
    return (decimal)(_random.NextDouble() * 1000);
}
else if (underlyingType == typeof(bool))
{
    return _random.Next(2) == 1;
}
else if (underlyingType == typeof(string))
{
    return Guid.NewGuid().ToString("N")[..8];
}
else if (underlyingType == typeof(Guid))
{
    return Guid.NewGuid();
}
else if (underlyingType == typeof(DateTime))
{
    var range = (DateTime.MaxValue - DateTime.MinValue).Days;
    return DateTime.MinValue.AddDays(_random.Next(range));
}
else if (underlyingType == typeof(DateTimeOffset))
{
    var range = (DateTimeOffset.MaxValue - DateTimeOffset.MinValue).Days;
    return DateTimeOffset.MinValue.AddDays(_random.Next(range));
}
else if (underlyingType == typeof(TimeSpan))
{
    return TimeSpan.FromMilliseconds(_random.Next(0, 86400000));
}
else if (underlyingType.IsEnum)
{
    var values = Enum.GetValues(underlyingType);
    return values.GetValue(_random.Next(values.Length));
}

return null;
""")
        };
        classModel.Methods.Add(generateRandomValueMethod);

        // Create the file model
        var codeFile = new CodeFileModel<ClassModel>(
            classModel,
            classModel.Usings,
            randomizerClassName,
            directory,
            CSharp);

        await _artifactGenerator.GenerateAsync(codeFile);

        _logger.LogInformation("Generated {ClassName}.cs in {Directory}", randomizerClassName, directory);
    }
}
