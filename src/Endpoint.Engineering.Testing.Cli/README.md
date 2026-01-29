# Endpoint Engineering Testing CLI

A command-line tool for generating Jest unit tests for Angular/TypeScript code.

## Installation

```bash
dotnet tool install --global Quinntyne.Endpoint.Engineering.Testing.Cli
```

## Usage

### Generate unit tests for a single file

```bash
endpoint-testing unit-tests-generate path/to/component.ts
```

### Generate unit tests for a directory

```bash
endpoint-testing unit-tests-generate path/to/angular/src
```

### Options

- `-o, --output <directory>` - Output directory for generated test files (defaults to same directory as source)
- `--overwrite` - Overwrite existing test files
- `--dry-run` - Preview what would be generated without writing files

## Supported Angular Types

- Components (`*.component.ts`)
- Services (`*.service.ts`)
- Directives (`*.directive.ts`)
- Pipes (`*.pipe.ts`)
- Guards (`*.guard.ts`)
- Interceptors (`*.interceptor.ts`)
- Resolvers (`*.resolver.ts`)
- Plain TypeScript classes

## Features

- Automatically detects Angular file types
- Generates appropriate test setup (TestBed configuration)
- Creates mock objects for dependencies
- Generates tests for public methods
- Handles @Input and @Output decorators
- Supports standalone components
- Generates HTTP testing setup for services using HttpClient

## License

MIT
