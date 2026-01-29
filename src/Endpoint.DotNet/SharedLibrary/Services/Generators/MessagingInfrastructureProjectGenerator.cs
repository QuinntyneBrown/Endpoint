// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using Endpoint.Services;
using Endpoint.DotNet.SharedLibrary.Configuration;
using Endpoint.Internal;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.SharedLibrary.Services.Generators;

/// <summary>
/// Generates the messaging infrastructure project with retry policies, circuit breakers, etc.
/// </summary>
public class MessagingInfrastructureProjectGenerator : ProjectGeneratorBase
{
    public MessagingInfrastructureProjectGenerator(
        ILogger<MessagingInfrastructureProjectGenerator> logger,
        IFileSystem fileSystem,
        ICommandService commandService,
        ITemplateLocator templateLocator,
        ITemplateProcessor templateProcessor)
        : base(logger, fileSystem, commandService, templateLocator, templateProcessor)
    {
    }

    public override int Order => 3; // After abstractions and domain

    public override bool ShouldGenerate(SharedLibraryConfig config)
    {
        var infra = config.MessagingInfrastructure;
        return infra.IncludeRetryPolicies ||
               infra.IncludeCircuitBreaker ||
               infra.IncludeDeadLetterQueue ||
               infra.IncludeMessageValidation ||
               infra.IncludeDistributedTracing ||
               infra.IncludeMessageVersioning ||
               infra.IncludeSerializationHelpers ||
               infra.IncludeRepositoryInterfaces;
    }

    public override async Task GenerateAsync(GeneratorContext context, CancellationToken cancellationToken = default)
    {
        var projectName = $"{context.LibraryName}.Messaging.Infrastructure";
        var projectDirectory = CreateProjectDirectory(context, projectName);
        var ns = $"{context.Namespace}.{context.LibraryName}.Messaging.Infrastructure";
        var infra = context.Config.MessagingInfrastructure;

        Logger.LogInformation("Generating {ProjectName}", projectName);

        var abstractionsProject = $"{context.LibraryName}.Messaging.Abstractions";
        var domainProject = $"{context.LibraryName}.Domain";

        // Generate csproj
        await GenerateCsprojAsync(
            projectDirectory,
            projectName,
            context.TargetFramework,
            packageReferences: new List<string>
            {
                "<PackageReference Include=\"Microsoft.Extensions.DependencyInjection.Abstractions\" Version=\"9.0.0\" />",
                "<PackageReference Include=\"Microsoft.Extensions.Logging.Abstractions\" Version=\"9.0.0\" />",
                "<PackageReference Include=\"System.Text.Json\" Version=\"9.0.0\" />",
            },
            projectReferences: new List<string>
            {
                $"../{abstractionsProject}/{abstractionsProject}.csproj",
            },
            cancellationToken: cancellationToken);

        // Generate retry policies
        if (infra.IncludeRetryPolicies)
        {
            await WriteFileAsync(
                FileSystem.Path.Combine(projectDirectory, "RetryPolicy.cs"),
                GenerateRetryPolicy(ns),
                cancellationToken);

            await WriteFileAsync(
                FileSystem.Path.Combine(projectDirectory, "IRetryExecutor.cs"),
                GenerateRetryExecutorInterface(ns),
                cancellationToken);

            await WriteFileAsync(
                FileSystem.Path.Combine(projectDirectory, "RetryExecutor.cs"),
                GenerateRetryExecutor(ns),
                cancellationToken);
        }

        // Generate circuit breaker
        if (infra.IncludeCircuitBreaker)
        {
            await WriteFileAsync(
                FileSystem.Path.Combine(projectDirectory, "CircuitBreaker.cs"),
                GenerateCircuitBreaker(ns),
                cancellationToken);
        }

        // Generate dead letter queue
        if (infra.IncludeDeadLetterQueue)
        {
            await WriteFileAsync(
                FileSystem.Path.Combine(projectDirectory, "DeadLetterQueue.cs"),
                GenerateDeadLetterQueue(ns),
                cancellationToken);
        }

        // Generate message validation
        if (infra.IncludeMessageValidation)
        {
            await WriteFileAsync(
                FileSystem.Path.Combine(projectDirectory, "MessageValidation.cs"),
                GenerateMessageValidation(ns),
                cancellationToken);
        }

        // Generate distributed tracing
        if (infra.IncludeDistributedTracing)
        {
            await WriteFileAsync(
                FileSystem.Path.Combine(projectDirectory, "DistributedTracing.cs"),
                GenerateDistributedTracing(ns),
                cancellationToken);
        }

        // Generate message versioning
        if (infra.IncludeMessageVersioning)
        {
            await WriteFileAsync(
                FileSystem.Path.Combine(projectDirectory, "MessageVersioning.cs"),
                GenerateMessageVersioning(ns),
                cancellationToken);
        }

        // Generate serialization helpers
        if (infra.IncludeSerializationHelpers)
        {
            await WriteFileAsync(
                FileSystem.Path.Combine(projectDirectory, "SerializationHelpers.cs"),
                GenerateSerializationHelpers(ns),
                cancellationToken);

            await WriteFileAsync(
                FileSystem.Path.Combine(projectDirectory, "MessageEnvelope.cs"),
                GenerateMessageEnvelope(ns),
                cancellationToken);
        }

        // Generate repository interfaces
        if (infra.IncludeRepositoryInterfaces)
        {
            var interfacesDir = FileSystem.Path.Combine(projectDirectory, "Interfaces");
            FileSystem.Directory.CreateDirectory(interfacesDir);

            await WriteFileAsync(
                FileSystem.Path.Combine(interfacesDir, "IRepository.cs"),
                GenerateRepositoryInterface(ns),
                cancellationToken);

            await WriteFileAsync(
                FileSystem.Path.Combine(interfacesDir, "IService.cs"),
                GenerateServiceInterface(ns),
                cancellationToken);

            await WriteFileAsync(
                FileSystem.Path.Combine(interfacesDir, "IUnitOfWork.cs"),
                GenerateUnitOfWorkInterface(ns),
                cancellationToken);
        }

        // Generate ServiceCollectionExtensions
        await WriteFileAsync(
            FileSystem.Path.Combine(projectDirectory, "ServiceCollectionExtensions.cs"),
            GenerateServiceCollectionExtensions(ns, infra),
            cancellationToken);

        // Add project to solution
        var projectPath = FileSystem.Path.Combine(projectDirectory, $"{projectName}.csproj");
        AddProjectToSolution(context, projectPath);
    }

    public override Task<GenerationPreview> PreviewAsync(GeneratorContext context, CancellationToken cancellationToken = default)
    {
        var preview = new GenerationPreview();
        var projectName = $"{context.LibraryName}.Messaging.Infrastructure";
        var basePath = $"src/{context.LibraryName}/{projectName}";
        var infra = context.Config.MessagingInfrastructure;

        preview.Projects.Add(projectName);
        preview.Files.Add($"{basePath}/{projectName}.csproj");

        if (infra.IncludeRetryPolicies)
        {
            preview.Files.Add($"{basePath}/RetryPolicy.cs");
            preview.Files.Add($"{basePath}/IRetryExecutor.cs");
            preview.Files.Add($"{basePath}/RetryExecutor.cs");
        }

        if (infra.IncludeCircuitBreaker)
        {
            preview.Files.Add($"{basePath}/CircuitBreaker.cs");
        }

        if (infra.IncludeDeadLetterQueue)
        {
            preview.Files.Add($"{basePath}/DeadLetterQueue.cs");
        }

        if (infra.IncludeMessageValidation)
        {
            preview.Files.Add($"{basePath}/MessageValidation.cs");
        }

        if (infra.IncludeDistributedTracing)
        {
            preview.Files.Add($"{basePath}/DistributedTracing.cs");
        }

        if (infra.IncludeMessageVersioning)
        {
            preview.Files.Add($"{basePath}/MessageVersioning.cs");
        }

        if (infra.IncludeSerializationHelpers)
        {
            preview.Files.Add($"{basePath}/SerializationHelpers.cs");
            preview.Files.Add($"{basePath}/MessageEnvelope.cs");
        }

        if (infra.IncludeRepositoryInterfaces)
        {
            preview.Files.Add($"{basePath}/Interfaces/IRepository.cs");
            preview.Files.Add($"{basePath}/Interfaces/IService.cs");
            preview.Files.Add($"{basePath}/Interfaces/IUnitOfWork.cs");
        }

        preview.Files.Add($"{basePath}/ServiceCollectionExtensions.cs");

        return Task.FromResult(preview);
    }

    private string GenerateRetryPolicy(string ns)
    {
        return $@"// Auto-generated code
namespace {ns};

/// <summary>
/// Configures retry behavior for operations.
/// </summary>
public class RetryPolicy
{{
    /// <summary>Maximum number of retry attempts.</summary>
    public int MaxRetries {{ get; set; }} = 3;

    /// <summary>Initial delay between retries.</summary>
    public TimeSpan InitialDelay {{ get; set; }} = TimeSpan.FromMilliseconds(100);

    /// <summary>Maximum delay between retries.</summary>
    public TimeSpan MaxDelay {{ get; set; }} = TimeSpan.FromSeconds(30);

    /// <summary>Multiplier for exponential backoff.</summary>
    public double BackoffMultiplier {{ get; set; }} = 2.0;

    /// <summary>Whether to add jitter to delays.</summary>
    public bool UseJitter {{ get; set; }} = true;

    /// <summary>Exception types that should trigger a retry.</summary>
    public List<Type> RetryableExceptions {{ get; set; }} = new()
    {{
        typeof(TimeoutException),
        typeof(TaskCanceledException)
    }};

    /// <summary>Exception types that should not trigger a retry.</summary>
    public List<Type> NonRetryableExceptions {{ get; set; }} = new()
    {{
        typeof(ArgumentException),
        typeof(InvalidOperationException)
    }};

    /// <summary>
    /// Calculates the delay for the given retry attempt.
    /// </summary>
    public TimeSpan CalculateDelay(int attempt)
    {{
        var delay = TimeSpan.FromMilliseconds(
            InitialDelay.TotalMilliseconds * Math.Pow(BackoffMultiplier, attempt));

        if (delay > MaxDelay)
        {{
            delay = MaxDelay;
        }}

        if (UseJitter)
        {{
            var jitter = Random.Shared.NextDouble() * 0.3; // Up to 30% jitter
            delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * (1 + jitter));
        }}

        return delay;
    }}

    /// <summary>
    /// Determines whether the exception should trigger a retry.
    /// </summary>
    public bool ShouldRetry(Exception exception, int attempt)
    {{
        if (attempt >= MaxRetries)
        {{
            return false;
        }}

        var exceptionType = exception.GetType();

        if (NonRetryableExceptions.Any(t => t.IsAssignableFrom(exceptionType)))
        {{
            return false;
        }}

        if (RetryableExceptions.Count == 0)
        {{
            return true; // Retry all exceptions if no specific list
        }}

        return RetryableExceptions.Any(t => t.IsAssignableFrom(exceptionType));
    }}

    /// <summary>
    /// Creates a default retry policy.
    /// </summary>
    public static RetryPolicy Default => new();

    /// <summary>
    /// Creates a retry policy with no retries.
    /// </summary>
    public static RetryPolicy NoRetry => new() {{ MaxRetries = 0 }};
}}
";
    }

    private string GenerateRetryExecutorInterface(string ns)
    {
        return $@"// Auto-generated code
namespace {ns};

/// <summary>
/// Executes operations with retry logic.
/// </summary>
public interface IRetryExecutor
{{
    /// <summary>
    /// Executes an async operation with retry logic.
    /// </summary>
    Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        RetryPolicy? policy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an async operation with retry logic (no return value).
    /// </summary>
    Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        RetryPolicy? policy = null,
        CancellationToken cancellationToken = default);
}}
";
    }

    private string GenerateRetryExecutor(string ns)
    {
        return $@"// Auto-generated code
using Microsoft.Extensions.Logging;

namespace {ns};

/// <summary>
/// Default implementation of retry executor.
/// </summary>
public class RetryExecutor : IRetryExecutor
{{
    private readonly ILogger<RetryExecutor> _logger;

    public RetryExecutor(ILogger<RetryExecutor> logger)
    {{
        _logger = logger;
    }}

    /// <inheritdoc />
    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        RetryPolicy? policy = null,
        CancellationToken cancellationToken = default)
    {{
        policy ??= RetryPolicy.Default;
        var attempt = 0;
        var exceptions = new List<Exception>();

        while (true)
        {{
            try
            {{
                return await operation(cancellationToken);
            }}
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {{
                exceptions.Add(ex);

                if (!policy.ShouldRetry(ex, attempt))
                {{
                    _logger.LogError(ex, ""Operation failed after {{Attempts}} attempts"", attempt + 1);
                    throw new AggregateException(""All retry attempts failed"", exceptions);
                }}

                var delay = policy.CalculateDelay(attempt);
                _logger.LogWarning(ex, ""Attempt {{Attempt}} failed, retrying in {{Delay}}ms"",
                    attempt + 1, delay.TotalMilliseconds);

                await Task.Delay(delay, cancellationToken);
                attempt++;
            }}
        }}
    }}

    /// <inheritdoc />
    public async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        RetryPolicy? policy = null,
        CancellationToken cancellationToken = default)
    {{
        await ExecuteAsync(async ct =>
        {{
            await operation(ct);
            return true;
        }}, policy, cancellationToken);
    }}
}}
";
    }

    private string GenerateCircuitBreaker(string ns)
    {
        return $@"// Auto-generated code
using Microsoft.Extensions.Logging;

namespace {ns};

/// <summary>
/// Circuit breaker states.
/// </summary>
public enum CircuitState
{{
    Closed,
    Open,
    HalfOpen
}}

/// <summary>
/// Circuit breaker configuration.
/// </summary>
public class CircuitBreakerOptions
{{
    /// <summary>Number of failures before opening the circuit.</summary>
    public int FailureThreshold {{ get; set; }} = 5;

    /// <summary>Duration to keep the circuit open.</summary>
    public TimeSpan OpenDuration {{ get; set; }} = TimeSpan.FromSeconds(30);

    /// <summary>Number of successful calls to close the circuit from half-open state.</summary>
    public int SuccessThreshold {{ get; set; }} = 2;
}}

/// <summary>
/// Implements the circuit breaker pattern for fault tolerance.
/// </summary>
public class CircuitBreaker
{{
    private readonly ILogger<CircuitBreaker> _logger;
    private readonly CircuitBreakerOptions _options;
    private readonly object _lock = new();

    private CircuitState _state = CircuitState.Closed;
    private int _failureCount;
    private int _successCount;
    private DateTime _openedAt;

    public CircuitBreaker(ILogger<CircuitBreaker> logger, CircuitBreakerOptions? options = null)
    {{
        _logger = logger;
        _options = options ?? new CircuitBreakerOptions();
    }}

    /// <summary>Gets the current circuit state.</summary>
    public CircuitState State
    {{
        get
        {{
            lock (_lock)
            {{
                if (_state == CircuitState.Open &&
                    DateTime.UtcNow - _openedAt >= _options.OpenDuration)
                {{
                    _state = CircuitState.HalfOpen;
                    _successCount = 0;
                    _logger.LogInformation(""Circuit breaker transitioned to HalfOpen"");
                }}
                return _state;
            }}
        }}
    }}

    /// <summary>
    /// Executes an operation through the circuit breaker.
    /// </summary>
    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default)
    {{
        if (State == CircuitState.Open)
        {{
            throw new CircuitBreakerOpenException(""Circuit breaker is open"");
        }}

        try
        {{
            var result = await operation(cancellationToken);
            OnSuccess();
            return result;
        }}
        catch (Exception ex) when (ex is not CircuitBreakerOpenException)
        {{
            OnFailure();
            throw;
        }}
    }}

    private void OnSuccess()
    {{
        lock (_lock)
        {{
            if (_state == CircuitState.HalfOpen)
            {{
                _successCount++;
                if (_successCount >= _options.SuccessThreshold)
                {{
                    _state = CircuitState.Closed;
                    _failureCount = 0;
                    _logger.LogInformation(""Circuit breaker closed"");
                }}
            }}
            else
            {{
                _failureCount = 0;
            }}
        }}
    }}

    private void OnFailure()
    {{
        lock (_lock)
        {{
            _failureCount++;
            if (_state == CircuitState.HalfOpen || _failureCount >= _options.FailureThreshold)
            {{
                _state = CircuitState.Open;
                _openedAt = DateTime.UtcNow;
                _logger.LogWarning(""Circuit breaker opened after {{Failures}} failures"", _failureCount);
            }}
        }}
    }}

    /// <summary>
    /// Forces the circuit breaker to reset to closed state.
    /// </summary>
    public void Reset()
    {{
        lock (_lock)
        {{
            _state = CircuitState.Closed;
            _failureCount = 0;
            _successCount = 0;
            _logger.LogInformation(""Circuit breaker manually reset"");
        }}
    }}
}}

/// <summary>
/// Exception thrown when the circuit breaker is open.
/// </summary>
public class CircuitBreakerOpenException : Exception
{{
    public CircuitBreakerOpenException(string message) : base(message) {{ }}
}}
";
    }

    private string GenerateDeadLetterQueue(string ns)
    {
        return $@"// Auto-generated code
using Microsoft.Extensions.Logging;

namespace {ns};

/// <summary>
/// Represents a failed message in the dead letter queue.
/// </summary>
public class DeadLetterMessage
{{
    /// <summary>Unique identifier for this dead letter entry.</summary>
    public Guid Id {{ get; set; }} = Guid.NewGuid();

    /// <summary>Original message content.</summary>
    public byte[] MessageContent {{ get; set; }} = Array.Empty<byte>();

    /// <summary>Message type name.</summary>
    public string MessageType {{ get; set; }} = string.Empty;

    /// <summary>Reason for failure.</summary>
    public string Reason {{ get; set; }} = string.Empty;

    /// <summary>Exception details if available.</summary>
    public string? ExceptionDetails {{ get; set; }}

    /// <summary>When the message was dead-lettered.</summary>
    public DateTimeOffset DeadLetteredAt {{ get; set; }} = DateTimeOffset.UtcNow;

    /// <summary>Number of processing attempts.</summary>
    public int AttemptCount {{ get; set; }}

    /// <summary>Original correlation ID.</summary>
    public string? CorrelationId {{ get; set; }}

    /// <summary>Additional metadata.</summary>
    public Dictionary<string, string> Metadata {{ get; set; }} = new();
}}

/// <summary>
/// Interface for dead letter queue operations.
/// </summary>
public interface IDeadLetterQueue
{{
    /// <summary>Adds a message to the dead letter queue.</summary>
    Task EnqueueAsync(DeadLetterMessage message, CancellationToken cancellationToken = default);

    /// <summary>Gets messages from the dead letter queue.</summary>
    Task<IReadOnlyList<DeadLetterMessage>> GetMessagesAsync(int maxCount = 100, CancellationToken cancellationToken = default);

    /// <summary>Removes a message from the dead letter queue.</summary>
    Task RemoveAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Attempts to reprocess a dead letter message.</summary>
    Task<bool> ReprocessAsync(Guid id, CancellationToken cancellationToken = default);
}}

/// <summary>
/// In-memory implementation of dead letter queue (for development/testing).
/// </summary>
public class InMemoryDeadLetterQueue : IDeadLetterQueue
{{
    private readonly ILogger<InMemoryDeadLetterQueue> _logger;
    private readonly List<DeadLetterMessage> _messages = new();
    private readonly object _lock = new();

    public InMemoryDeadLetterQueue(ILogger<InMemoryDeadLetterQueue> logger)
    {{
        _logger = logger;
    }}

    public Task EnqueueAsync(DeadLetterMessage message, CancellationToken cancellationToken = default)
    {{
        lock (_lock)
        {{
            _messages.Add(message);
            _logger.LogWarning(""Message dead-lettered: {{Reason}}"", message.Reason);
        }}
        return Task.CompletedTask;
    }}

    public Task<IReadOnlyList<DeadLetterMessage>> GetMessagesAsync(int maxCount = 100, CancellationToken cancellationToken = default)
    {{
        lock (_lock)
        {{
            return Task.FromResult<IReadOnlyList<DeadLetterMessage>>(
                _messages.Take(maxCount).ToList());
        }}
    }}

    public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {{
        lock (_lock)
        {{
            _messages.RemoveAll(m => m.Id == id);
        }}
        return Task.CompletedTask;
    }}

    public Task<bool> ReprocessAsync(Guid id, CancellationToken cancellationToken = default)
    {{
        // In a real implementation, this would resubmit to the message bus
        _logger.LogInformation(""Reprocess requested for message {{Id}}"", id);
        return Task.FromResult(false);
    }}
}}
";
    }

    private string GenerateMessageValidation(string ns)
    {
        return $@"// Auto-generated code
namespace {ns};

/// <summary>
/// Result of message validation.
/// </summary>
public class MessageValidationResult
{{
    public bool IsValid {{ get; set; }} = true;
    public List<string> Errors {{ get; set; }} = new();

    public static MessageValidationResult Success() => new() {{ IsValid = true }};

    public static MessageValidationResult Failure(params string[] errors) =>
        new() {{ IsValid = false, Errors = errors.ToList() }};
}}

/// <summary>
/// Interface for message validators.
/// </summary>
public interface IMessageValidator<T>
{{
    MessageValidationResult Validate(T message);
}}

/// <summary>
/// Base class for message validators with fluent rule building.
/// </summary>
public abstract class MessageValidatorBase<T> : IMessageValidator<T>
{{
    private readonly List<Func<T, (bool isValid, string? error)>> _rules = new();

    protected void AddRule(Func<T, bool> predicate, string errorMessage)
    {{
        _rules.Add(msg => (predicate(msg), predicate(msg) ? null : errorMessage));
    }}

    protected void RequireNotNull<TValue>(Func<T, TValue?> selector, string propertyName)
        where TValue : class
    {{
        AddRule(msg => selector(msg) != null, $""{{propertyName}} is required"");
    }}

    protected void RequireNotEmpty(Func<T, string?> selector, string propertyName)
    {{
        AddRule(msg => !string.IsNullOrWhiteSpace(selector(msg)), $""{{propertyName}} cannot be empty"");
    }}

    protected void RequireInRange(Func<T, int> selector, int min, int max, string propertyName)
    {{
        AddRule(msg =>
        {{
            var value = selector(msg);
            return value >= min && value <= max;
        }}, $""{{propertyName}} must be between {{min}} and {{max}}"");
    }}

    public MessageValidationResult Validate(T message)
    {{
        var errors = new List<string>();

        foreach (var rule in _rules)
        {{
            var (isValid, error) = rule(message);
            if (!isValid && error != null)
            {{
                errors.Add(error);
            }}
        }}

        return errors.Count > 0
            ? MessageValidationResult.Failure(errors.ToArray())
            : MessageValidationResult.Success();
    }}
}}
";
    }

    private string GenerateDistributedTracing(string ns)
    {
        return $@"// Auto-generated code
namespace {ns};

/// <summary>
/// Represents trace context for distributed tracing.
/// </summary>
public class TraceContext
{{
    /// <summary>Unique trace identifier spanning multiple services.</summary>
    public string TraceId {{ get; set; }} = Guid.NewGuid().ToString(""N"");

    /// <summary>Current span identifier.</summary>
    public string SpanId {{ get; set; }} = Guid.NewGuid().ToString(""N"").Substring(0, 16);

    /// <summary>Parent span identifier.</summary>
    public string? ParentSpanId {{ get; set; }}

    /// <summary>Sampling flag.</summary>
    public bool IsSampled {{ get; set; }} = true;

    /// <summary>Baggage items (key-value pairs propagated across services).</summary>
    public Dictionary<string, string> Baggage {{ get; set; }} = new();

    /// <summary>
    /// Creates a child span context.
    /// </summary>
    public TraceContext CreateChild()
    {{
        return new TraceContext
        {{
            TraceId = TraceId,
            ParentSpanId = SpanId,
            SpanId = Guid.NewGuid().ToString(""N"").Substring(0, 16),
            IsSampled = IsSampled,
            Baggage = new Dictionary<string, string>(Baggage)
        }};
    }}

    /// <summary>
    /// Creates a new root trace context.
    /// </summary>
    public static TraceContext CreateRoot() => new();
}}

/// <summary>
/// Interface for distributed tracing operations.
/// </summary>
public interface ITracer
{{
    /// <summary>Gets the current trace context.</summary>
    TraceContext? Current {{ get; }}

    /// <summary>Starts a new span.</summary>
    IDisposable StartSpan(string operationName, TraceContext? parent = null);

    /// <summary>Adds a tag to the current span.</summary>
    void AddTag(string key, string value);

    /// <summary>Logs an event in the current span.</summary>
    void LogEvent(string message);
}}

/// <summary>
/// No-op tracer implementation.
/// </summary>
public class NoOpTracer : ITracer
{{
    public TraceContext? Current => null;

    public IDisposable StartSpan(string operationName, TraceContext? parent = null)
        => new NoOpSpan();

    public void AddTag(string key, string value) {{ }}

    public void LogEvent(string message) {{ }}

    private class NoOpSpan : IDisposable
    {{
        public void Dispose() {{ }}
    }}
}}
";
    }

    private string GenerateMessageVersioning(string ns)
    {
        return $@"// Auto-generated code
namespace {ns};

/// <summary>
/// Attribute to mark message version.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class MessageVersionAttribute : Attribute
{{
    public int Version {{ get; }}
    public string? Description {{ get; set; }}

    public MessageVersionAttribute(int version)
    {{
        Version = version;
    }}
}}

/// <summary>
/// Interface for versioned messages.
/// </summary>
public interface IVersionedMessage
{{
    /// <summary>Gets the message schema version.</summary>
    int Version {{ get; }}
}}

/// <summary>
/// Helper for message version operations.
/// </summary>
public static class MessageVersioning
{{
    /// <summary>
    /// Gets the version of a message type.
    /// </summary>
    public static int GetVersion<T>() where T : class
    {{
        return GetVersion(typeof(T));
    }}

    /// <summary>
    /// Gets the version of a message type.
    /// </summary>
    public static int GetVersion(Type messageType)
    {{
        var attr = messageType.GetCustomAttributes(typeof(MessageVersionAttribute), false)
            .FirstOrDefault() as MessageVersionAttribute;

        return attr?.Version ?? 1;
    }}

    /// <summary>
    /// Checks if message types are compatible (same version).
    /// </summary>
    public static bool AreCompatible(Type type1, Type type2)
    {{
        return GetVersion(type1) == GetVersion(type2);
    }}
}}
";
    }

    private string GenerateSerializationHelpers(string ns)
    {
        return $@"// Auto-generated code
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace {ns};

/// <summary>
/// JSON serialization helper with configured options.
/// </summary>
public static class JsonSerializationHelper
{{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {{
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    }};

    /// <summary>Serializes an object to JSON string.</summary>
    public static string Serialize<T>(T value, JsonSerializerOptions? options = null)
    {{
        return JsonSerializer.Serialize(value, options ?? DefaultOptions);
    }}

    /// <summary>Serializes an object to UTF-8 bytes.</summary>
    public static byte[] SerializeToBytes<T>(T value, JsonSerializerOptions? options = null)
    {{
        return JsonSerializer.SerializeToUtf8Bytes(value, options ?? DefaultOptions);
    }}

    /// <summary>Deserializes a JSON string to an object.</summary>
    public static T? Deserialize<T>(string json, JsonSerializerOptions? options = null)
    {{
        return JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions);
    }}

    /// <summary>Deserializes UTF-8 bytes to an object.</summary>
    public static T? DeserializeFromBytes<T>(byte[] data, JsonSerializerOptions? options = null)
    {{
        return JsonSerializer.Deserialize<T>(data, options ?? DefaultOptions);
    }}

    /// <summary>Attempts to deserialize with error handling.</summary>
    public static bool TryDeserialize<T>(string json, out T? result, JsonSerializerOptions? options = null)
    {{
        try
        {{
            result = Deserialize<T>(json, options);
            return true;
        }}
        catch
        {{
            result = default;
            return false;
        }}
    }}

    /// <summary>Creates a deep clone via JSON serialization.</summary>
    public static T? Clone<T>(T value, JsonSerializerOptions? options = null)
    {{
        var json = Serialize(value, options);
        return Deserialize<T>(json, options);
    }}
}}

/// <summary>
/// Binary serialization helper for big-endian operations.
/// </summary>
public static class BinarySerializationHelper
{{
    /// <summary>Writes a boolean value.</summary>
    public static void WriteBool(byte[] buffer, int offset, bool value)
    {{
        buffer[offset] = value ? (byte)1 : (byte)0;
    }}

    /// <summary>Reads a boolean value.</summary>
    public static bool ReadBool(byte[] buffer, int offset)
    {{
        return buffer[offset] != 0;
    }}

    /// <summary>Writes a 16-bit unsigned integer in big-endian order.</summary>
    public static void WriteUInt16BE(byte[] buffer, int offset, ushort value)
    {{
        buffer[offset] = (byte)(value >> 8);
        buffer[offset + 1] = (byte)value;
    }}

    /// <summary>Reads a 16-bit unsigned integer in big-endian order.</summary>
    public static ushort ReadUInt16BE(byte[] buffer, int offset)
    {{
        return (ushort)((buffer[offset] << 8) | buffer[offset + 1]);
    }}

    /// <summary>Writes a 32-bit unsigned integer in big-endian order.</summary>
    public static void WriteUInt32BE(byte[] buffer, int offset, uint value)
    {{
        buffer[offset] = (byte)(value >> 24);
        buffer[offset + 1] = (byte)(value >> 16);
        buffer[offset + 2] = (byte)(value >> 8);
        buffer[offset + 3] = (byte)value;
    }}

    /// <summary>Reads a 32-bit unsigned integer in big-endian order.</summary>
    public static uint ReadUInt32BE(byte[] buffer, int offset)
    {{
        return (uint)((buffer[offset] << 24) | (buffer[offset + 1] << 16) |
                      (buffer[offset + 2] << 8) | buffer[offset + 3]);
    }}

    /// <summary>Writes a length-prefixed string (2-byte length + UTF-8 data).</summary>
    public static int WriteLengthPrefixedString(byte[] buffer, int offset, string value)
    {{
        var bytes = Encoding.UTF8.GetBytes(value);
        WriteUInt16BE(buffer, offset, (ushort)bytes.Length);
        Array.Copy(bytes, 0, buffer, offset + 2, bytes.Length);
        return 2 + bytes.Length;
    }}

    /// <summary>Reads a length-prefixed string.</summary>
    public static string ReadLengthPrefixedString(byte[] buffer, int offset, out int bytesRead)
    {{
        var length = ReadUInt16BE(buffer, offset);
        var str = Encoding.UTF8.GetString(buffer, offset + 2, length);
        bytesRead = 2 + length;
        return str;
    }}
}}
";
    }

    private string GenerateMessageEnvelope(string ns)
    {
        return $@"// Auto-generated code
using System.Text.Json;

namespace {ns};

/// <summary>
/// Standard message headers.
/// </summary>
public class MessageHeaders
{{
    /// <summary>Unique message identifier.</summary>
    public string MessageId {{ get; set; }} = Guid.NewGuid().ToString();

    /// <summary>Correlation ID for request/response tracking.</summary>
    public string? CorrelationId {{ get; set; }}

    /// <summary>Causation ID linking to the causing message.</summary>
    public string? CausationId {{ get; set; }}

    /// <summary>Message timestamp.</summary>
    public DateTimeOffset Timestamp {{ get; set; }} = DateTimeOffset.UtcNow;

    /// <summary>Source service/component.</summary>
    public string? Source {{ get; set; }}

    /// <summary>Message type name.</summary>
    public string? MessageType {{ get; set; }}

    /// <summary>Content type (e.g., application/json).</summary>
    public string ContentType {{ get; set; }} = ""application/json"";

    /// <summary>Schema version.</summary>
    public int Version {{ get; set; }} = 1;

    /// <summary>Custom headers.</summary>
    public Dictionary<string, string> Custom {{ get; set; }} = new();
}}

/// <summary>
/// Message envelope wrapping a typed payload.
/// </summary>
public class MessageEnvelope<TPayload>
{{
    /// <summary>Message headers.</summary>
    public MessageHeaders Headers {{ get; set; }} = new();

    /// <summary>Message payload.</summary>
    public TPayload? Payload {{ get; set; }}

    /// <summary>
    /// Creates a new envelope for the payload.
    /// </summary>
    public static MessageEnvelope<TPayload> Create(TPayload payload, string? source = null)
    {{
        return new MessageEnvelope<TPayload>
        {{
            Headers = new MessageHeaders
            {{
                Source = source,
                MessageType = typeof(TPayload).Name
            }},
            Payload = payload
        }};
    }}

    /// <summary>
    /// Creates a reply envelope linked to this message.
    /// </summary>
    public MessageEnvelope<TReply> CreateReply<TReply>(TReply payload, string? source = null)
    {{
        return new MessageEnvelope<TReply>
        {{
            Headers = new MessageHeaders
            {{
                CorrelationId = Headers.CorrelationId ?? Headers.MessageId,
                CausationId = Headers.MessageId,
                Source = source,
                MessageType = typeof(TReply).Name
            }},
            Payload = payload
        }};
    }}

    /// <summary>Serializes the envelope to JSON.</summary>
    public string ToJson() => JsonSerializationHelper.Serialize(this);

    /// <summary>Deserializes an envelope from JSON.</summary>
    public static MessageEnvelope<TPayload>? FromJson(string json)
        => JsonSerializationHelper.Deserialize<MessageEnvelope<TPayload>>(json);
}}

/// <summary>
/// Non-generic message envelope for dynamic payloads.
/// </summary>
public class MessageEnvelope
{{
    /// <summary>Message headers.</summary>
    public MessageHeaders Headers {{ get; set; }} = new();

    /// <summary>Message payload as JsonElement.</summary>
    public JsonElement Payload {{ get; set; }}

    /// <summary>
    /// Gets the payload as a typed object.
    /// </summary>
    public T? GetPayload<T>()
    {{
        return JsonSerializer.Deserialize<T>(Payload.GetRawText());
    }}
}}
";
    }

    private string GenerateRepositoryInterface(string ns)
    {
        return $@"// Auto-generated code
using System.Linq.Expressions;

namespace {ns}.Interfaces;

/// <summary>
/// Read-only repository interface.
/// </summary>
public interface IReadOnlyRepository<TEntity, TId>
    where TEntity : class
{{
    /// <summary>Gets an entity by ID.</summary>
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>Gets all entities.</summary>
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Finds entities matching a predicate.</summary>
    Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>Gets the first entity matching a predicate or null.</summary>
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>Checks if any entity matches the predicate.</summary>
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);

    /// <summary>Counts entities matching the predicate.</summary>
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);
}}

/// <summary>
/// Full repository interface with write operations.
/// </summary>
public interface IRepository<TEntity, TId> : IReadOnlyRepository<TEntity, TId>
    where TEntity : class
{{
    /// <summary>Adds an entity.</summary>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>Adds multiple entities.</summary>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>Updates an entity.</summary>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>Updates multiple entities.</summary>
    Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>Deletes an entity by ID.</summary>
    Task DeleteAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>Deletes an entity.</summary>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>Deletes multiple entities.</summary>
    Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
}}

/// <summary>
/// Convenience repository interface with Guid ID.
/// </summary>
public interface IRepository<TEntity> : IRepository<TEntity, Guid>
    where TEntity : class
{{
}}
";
    }

    private string GenerateServiceInterface(string ns)
    {
        return $@"// Auto-generated code
namespace {ns}.Interfaces;

/// <summary>
/// Base service interface for CRUD operations.
/// </summary>
public interface IService<TEntity, TDto, TId>
    where TEntity : class
    where TDto : class
{{
    /// <summary>Gets an entity by ID.</summary>
    Task<TDto?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>Gets all entities.</summary>
    Task<IReadOnlyList<TDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Creates a new entity.</summary>
    Task<TDto> CreateAsync(TDto dto, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing entity.</summary>
    Task<TDto> UpdateAsync(TId id, TDto dto, CancellationToken cancellationToken = default);

    /// <summary>Deletes an entity.</summary>
    Task DeleteAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>Checks if an entity exists.</summary>
    Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default);
}}

/// <summary>
/// Paged result for paginated queries.
/// </summary>
public class PagedResult<T>
{{
    /// <summary>The items in this page.</summary>
    public IReadOnlyList<T> Items {{ get; set; }} = Array.Empty<T>();

    /// <summary>Total number of items across all pages.</summary>
    public int TotalCount {{ get; set; }}

    /// <summary>Current page number (1-based).</summary>
    public int PageNumber {{ get; set; }}

    /// <summary>Page size.</summary>
    public int PageSize {{ get; set; }}

    /// <summary>Total number of pages.</summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>Whether there is a next page.</summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>Whether there is a previous page.</summary>
    public bool HasPreviousPage => PageNumber > 1;
}}

/// <summary>
/// Request for paged data.
/// </summary>
public class PagedRequest
{{
    /// <summary>Page number (1-based).</summary>
    public int PageNumber {{ get; set; }} = 1;

    /// <summary>Page size.</summary>
    public int PageSize {{ get; set; }} = 20;

    /// <summary>Sort field name.</summary>
    public string? SortBy {{ get; set; }}

    /// <summary>Sort direction.</summary>
    public bool SortDescending {{ get; set; }}

    /// <summary>Search/filter term.</summary>
    public string? SearchTerm {{ get; set; }}
}}

/// <summary>
/// Service interface with paging support.
/// </summary>
public interface IPagedService<TDto>
    where TDto : class
{{
    /// <summary>Gets a paged result.</summary>
    Task<PagedResult<TDto>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
}}
";
    }

    private string GenerateUnitOfWorkInterface(string ns)
    {
        return $@"// Auto-generated code
namespace {ns}.Interfaces;

/// <summary>
/// Unit of work interface for coordinating transactions.
/// </summary>
public interface IUnitOfWork : IDisposable
{{
    /// <summary>Saves all pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Begins a new transaction.</summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>Commits the current transaction.</summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>Rolls back the current transaction.</summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}}
";
    }

    private string GenerateServiceCollectionExtensions(string ns, MessagingInfrastructureConfig config)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("// Auto-generated code");
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine();
        sb.AppendLine($"namespace {ns};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Extension methods for service registration.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("public static class ServiceCollectionExtensions");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Adds messaging infrastructure to the service collection.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static IServiceCollection AddMessagingInfrastructure(this IServiceCollection services)");
        sb.AppendLine("    {");

        if (config.IncludeRetryPolicies)
        {
            sb.AppendLine("        services.AddSingleton<IRetryExecutor, RetryExecutor>();");
        }

        if (config.IncludeCircuitBreaker)
        {
            sb.AppendLine("        services.AddSingleton<CircuitBreaker>();");
        }

        if (config.IncludeDeadLetterQueue)
        {
            sb.AppendLine("        services.AddSingleton<IDeadLetterQueue, InMemoryDeadLetterQueue>();");
        }

        if (config.IncludeDistributedTracing)
        {
            sb.AppendLine("        services.AddSingleton<ITracer, NoOpTracer>();");
        }

        sb.AppendLine("        return services;");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }
}
