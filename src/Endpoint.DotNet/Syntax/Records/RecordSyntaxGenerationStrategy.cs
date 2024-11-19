using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Syntax.Records.RecordType;

namespace Endpoint.DotNet.Syntax.Records;

public class RecordSyntaxGenerationStrategy : ISyntaxGenerationStrategy<RecordModel>
{
    private readonly ILogger<RecordSyntaxGenerationStrategy> logger;

    public RecordSyntaxGenerationStrategy(
        ILogger<RecordSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GenerateAsync(RecordModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating Record. {name}", model.Name);

        var sb = StringBuilderCache.Acquire();

        sb.AppendLine($"public record {model.Type switch { Struct => "struct", Class => "class" } } {model.Name}");

        sb.AppendLine("{");

        sb.AppendLine("}");

        return StringBuilderCache.GetStringAndRelease(sb);
    }
}