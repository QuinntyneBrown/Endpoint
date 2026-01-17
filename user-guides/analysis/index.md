# Analysis Commands

Commands for code review, static analysis, and code parsing for AI/LLM consumption.

## Available Commands

| Command | Description |
|---------|-------------|
| [code-review](./code-review.user-guide.md) | Perform code review with diff analysis |
| [static-analysis](./static-analysis.user-guide.md) | Run general static analysis |
| [csharp-static-analysis](./csharp-static-analysis.user-guide.md) | C# specific static analysis |
| [angular-static-analysis](./angular-static-analysis.user-guide.md) | Angular workspace analysis |
| [scss-static-analysis](./scss-static-analysis.user-guide.md) | SCSS static analysis |
| [code-parse](./code-parse.user-guide.md) | Parse code for LLM consumption |
| [html-parse](./html-parse.user-guide.md) | Parse HTML for LLM consumption |

## Quick Examples

```bash
# Run code review against main branch
endpoint code-review -t main

# Run C# static analysis
endpoint csharp-static-analysis -p ./src

# Parse code for LLM with high efficiency
endpoint code-parse -e 80 --ignore-tests

# Parse HTML with medium stripping
endpoint html-parse -u https://example.com -s 5
```

[Back to Index](../index.md)
