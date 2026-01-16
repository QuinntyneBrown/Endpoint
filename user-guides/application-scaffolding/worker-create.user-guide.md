# worker-create

Create a background worker or console application.

## Synopsis

```bash
endpoint worker-create [options]
```

## Description

The `worker-create` command generates a .NET background worker service or console application. Workers are ideal for background processing, scheduled tasks, message consumers, and other long-running processes.

## Options

| Option | Short | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--name` | `-n` | Name of the worker service | No | - |
| `--directory` | `-d` | Target directory | No | Current directory |

## Examples

### Create a basic worker

```bash
endpoint worker-create -n EmailProcessor
```

### Specify output directory

```bash
endpoint worker-create -n BackgroundJobRunner -d ./services
```

## Generated Structure

```
EmailProcessor/
├── EmailProcessor.sln
├── src/
│   └── EmailProcessor/
│       ├── Worker.cs
│       ├── Program.cs
│       ├── appsettings.json
│       └── EmailProcessor.csproj
└── tests/
```

## Generated Worker Class

```csharp
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
    }
}
```

## Common Use Cases

### Message Consumer

Process messages from a queue:

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    await foreach (var message in _messageQueue.ConsumeAsync(stoppingToken))
    {
        await ProcessMessageAsync(message);
    }
}
```

### Scheduled Jobs

Run tasks on a schedule:

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        await RunScheduledTaskAsync();
        await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
    }
}
```

### Event Processor

Process events from an event stream:

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    await _eventProcessor.StartProcessingAsync(stoppingToken);
}
```

## Deployment Options

| Platform | Description |
|----------|-------------|
| **Windows Service** | Run as Windows service |
| **Linux Daemon** | Run as systemd service |
| **Docker** | Run in container |
| **Kubernetes** | Deploy as pod |
| **Azure Container Apps** | Serverless containers |

## Running as a Service

### Windows Service

```bash
sc create MyWorker binPath= "C:\path\to\Worker.exe"
sc start MyWorker
```

### Linux (systemd)

```ini
[Unit]
Description=My Worker Service

[Service]
ExecStart=/usr/bin/dotnet /path/to/Worker.dll
Restart=always

[Install]
WantedBy=multi-user.target
```

### Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "Worker.dll"]
```

## Common Patterns

1. **Queue Processors**: Process items from message queues
2. **File Watchers**: Monitor directories for new files
3. **Data Sync**: Synchronize data between systems
4. **Cleanup Jobs**: Remove old data, logs, temp files
5. **Health Monitors**: Check system health and alert

## Best Practices

- Implement graceful shutdown
- Add proper logging
- Handle exceptions appropriately
- Use cancellation tokens
- Configure appropriate restart policies

## Related Commands

- [solution-create](../project-management/solution-create.user-guide.md) - Create solutions
- [messaging-add](../messaging/messaging-add.user-guide.md) - Add messaging

[Back to Application Scaffolding](./index.md) | [Back to Index](../index.md)
