# html-parse

Parse HTML content and extract LLM-optimized summaries for AI consumption.

## Synopsis

```bash
endpoint html-parse [options]
```

## Description

The `html-parse` command parses HTML files and generates optimized content summaries designed for consumption by Large Language Models (LLMs) like ChatGPT, Claude, or other AI assistants. It extracts meaningful content while progressively stripping away non-essential elements based on the specified stripping level.

## Options

| Option | Short | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--path` | `-p` | Path to the HTML file to parse | No | - |
| `--url` | `-u` | URL to fetch and parse HTML from | No | - |
| `--urls` | - | Comma-separated list of URLs to fetch and parse | No | - |
| `--directories` | `-d` | Comma-separated list of directories containing HTML files | No | - |
| `--pattern` | - | Search pattern for HTML files in directories | No | `*.html` |
| `--recursive` | `-r` | Search directories recursively for HTML files | No | `false` |
| `--output` | `-o` | Output file path to write the parsed content | No | Console |
| `--strip-level` | `-s` | Stripping level (0-10) | No | `0` |

## Strip Levels

The `--strip-level` option controls how aggressively the HTML is stripped:

| Level | Description |
|-------|-------------|
| `0` | **Semantic extraction** (default) - Extract meaningful content with semantic markup preserved |
| `1-3` | **Light stripping** - Remove inline styles and event handlers (onclick, onload, etc.) |
| `4-6` | **Medium stripping** - Keep only semantic attributes (href, src, alt, title) |
| `7-9` | **Heavy stripping** - Remove all attributes but keep all tags |
| `10` | **Maximum stripping** - Only basic HTML tags with no attributes (p, div, span, h1-h6, ul, ol, li, table, tr, td, th, a, strong, em, blockquote, pre, code) |

## Examples

### Parse HTML from a file

```bash
endpoint html-parse -p ./index.html
```

### Parse with maximum stripping

```bash
endpoint html-parse -p ./page.html -s 10
```

### Fetch and parse HTML from URL

```bash
endpoint html-parse -u https://example.com
```

### Parse multiple URLs

```bash
endpoint html-parse --urls "https://example.com,https://example.org"
```

### Parse all HTML files in a directory

```bash
endpoint html-parse -d ./pages
```

### Parse recursively with pattern

```bash
endpoint html-parse -d ./docs -r --pattern "*.html"
```

### Save to file

```bash
endpoint html-parse -u https://example.com -o ./parsed.txt
```

### Parse HTML from stdin

```bash
cat page.html | endpoint html-parse
```

### Medium stripping for cleaner output

```bash
endpoint html-parse -p ./article.html -s 5
```

## Output Format

The output format varies based on the strip level:

### Level 0 (Default - Semantic Extraction)

```
=== Parsed HTML Content (LLM-Optimized) ===

## Source: File: article.html

# Article Title

## Introduction

- Link 1
- Link 2

This is the main content paragraph.

### Features

- Feature 1
- Feature 2
- Feature 3

| Column 1 | Column 2 |
| Data 1   | Data 2   |
```

### Level 10 (Maximum Stripping)

```
=== Parsed HTML Content (LLM-Optimized) ===

## Source: File: article.html

<h1>Article Title</h1>

<h2>Introduction</h2>

<ul>
<li>Link 1</li>
<li>Link 2</li>
</ul>

<p>This is the main content paragraph.</p>

<h3>Features</h3>

<ul>
<li>Feature 1</li>
<li>Feature 2</li>
<li>Feature 3</li>
</ul>

<table>
<tr>
<td>Column 1</td>
<td>Column 2</td>
</tr>
<tr>
<td>Data 1</td>
<td>Data 2</td>
</tr>
</table>
```

## Strip Level Selection Guide

Choose the appropriate strip level based on your needs:

- **Level 0**: Best for general AI assistance, preserves semantic structure
- **Level 1-3**: Remove styling while keeping structure and links
- **Level 4-6**: Keep only essential navigation and media attributes
- **Level 7-9**: Pure HTML structure, useful for layout analysis
- **Level 10**: Minimal HTML for content extraction and analysis

## Common Use Cases

1. **AI Content Analysis**: Extract webpage content for AI processing
2. **Documentation Generation**: Convert HTML docs to LLM-friendly format
3. **Web Scraping**: Extract structured content from websites
4. **Content Migration**: Convert HTML to simpler formats
5. **Token Optimization**: Reduce HTML token count for LLM context windows

## Best Practices

- Use level 0 for most AI queries where context is important
- Use level 10 for pure content extraction
- Use intermediate levels (4-6) when you need some attributes
- Combine with output files for large batch processing
- Use recursive mode carefully to avoid processing too many files

## Example Workflow

```bash
# Extract content from a documentation site
endpoint html-parse -u https://docs.example.com/api -s 5

# Paste into ChatGPT with your question
# "Based on this API documentation, how do I authenticate?"
```

```bash
# Parse all blog posts for content analysis
endpoint html-parse -d ./blog/posts -r -s 10 -o ./parsed-posts.txt

# Use the parsed content for further processing
```

## Related Commands

- [code-parse](./code-parse.user-guide.md) - Parse code files for AI consumption
- [static-analysis](./static-analysis.user-guide.md) - Analyze code quality

[Back to Analysis](./index.md) | [Back to Index](../index.md)
