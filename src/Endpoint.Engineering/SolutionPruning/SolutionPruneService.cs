// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.SolutionPruning;

/// <summary>
/// Service for pruning a .NET solution to only include a specified type and its dependencies/references.
/// </summary>
public class SolutionPruneService : ISolutionPruneService
{
    private readonly ILogger<SolutionPruneService> _logger;
    private static bool _msBuildRegistered;
    private static readonly object _registrationLock = new();

    public SolutionPruneService(ILogger<SolutionPruneService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        EnsureMSBuildRegistered();
    }

    private static void EnsureMSBuildRegistered()
    {
        lock (_registrationLock)
        {
            if (!_msBuildRegistered)
            {
                try
                {
                    MSBuildLocator.RegisterDefaults();
                    _msBuildRegistered = true;
                }
                catch (InvalidOperationException)
                {
                    // Already registered
                    _msBuildRegistered = true;
                }
            }
        }
    }

    /// <inheritdoc />
    public async Task<SolutionPruneResult> PruneAsync(
        string solutionPath,
        string fullyQualifiedTypeName,
        string? outputDirectory = null,
        CancellationToken cancellationToken = default)
    {
        var result = new SolutionPruneResult
        {
            TargetTypeName = fullyQualifiedTypeName
        };

        try
        {
            // Validate inputs
            if (!File.Exists(solutionPath))
            {
                result.ErrorMessage = $"Solution file not found: {solutionPath}";
                return result;
            }

            if (string.IsNullOrWhiteSpace(fullyQualifiedTypeName))
            {
                result.ErrorMessage = "Fully qualified type name is required.";
                return result;
            }

            _logger.LogInformation("Loading solution: {SolutionPath}", solutionPath);

            // Load the solution
            using var workspace = MSBuildWorkspace.Create();
            workspace.WorkspaceFailed += (sender, args) =>
            {
                if (args.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure)
                {
                    _logger.LogWarning("Workspace warning: {Message}", args.Diagnostic.Message);
                }
            };

            var solution = await workspace.OpenSolutionAsync(solutionPath, cancellationToken: cancellationToken);

            _logger.LogInformation("Solution loaded with {ProjectCount} projects", solution.Projects.Count());

            // Find the target type
            var (targetSymbol, targetDocument) = await FindTypeAsync(solution, fullyQualifiedTypeName, cancellationToken);

            if (targetSymbol == null || targetDocument == null)
            {
                result.ErrorMessage = $"Type '{fullyQualifiedTypeName}' not found in solution.";
                return result;
            }

            _logger.LogInformation("Found target type: {TypeName} in {FilePath}",
                targetSymbol.ToDisplayString(), targetDocument.FilePath);

            // Collect all related types (dependencies and references)
            var relatedTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            var processedTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

            // Add the target type
            relatedTypes.Add(targetSymbol);

            // Find all dependencies (types the target type depends on) - recursive
            await CollectDependenciesAsync(solution, targetSymbol, relatedTypes, processedTypes, cancellationToken);
            result.DependencyTypeCount = relatedTypes.Count - 1;

            // Find all references (types that reference the target type) - recursive
            var referencingTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            await CollectReferencesAsync(solution, targetSymbol, relatedTypes, referencingTypes, processedTypes, cancellationToken);
            result.DependentTypeCount = referencingTypes.Count;

            _logger.LogInformation("Found {TotalTypes} related types ({Dependencies} dependencies, {References} references)",
                relatedTypes.Count, result.DependencyTypeCount, result.DependentTypeCount);

            // Collect all files containing the related types
            var filesToInclude = await CollectFilesForTypesAsync(solution, relatedTypes, cancellationToken);

            // Collect projects that contain these files
            var projectsToInclude = filesToInclude
                .Select(f => f.Project)
                .Distinct()
                .ToList();

            // Determine output directory
            var solutionDirectory = Path.GetDirectoryName(solutionPath)!;
            var solutionName = Path.GetFileNameWithoutExtension(solutionPath);
            outputDirectory ??= Path.Combine(solutionDirectory, $"{solutionName}.Pruned");

            // Create the pruned solution
            await CreatePrunedSolutionAsync(
                solutionPath,
                outputDirectory,
                filesToInclude,
                projectsToInclude,
                relatedTypes,
                cancellationToken);

            result.Success = true;
            result.PrunedSolutionPath = Path.Combine(outputDirectory, Path.GetFileName(solutionPath));
            result.IncludedTypes = relatedTypes.Select(t => t.ToDisplayString()).OrderBy(t => t).ToList();
            result.IncludedFiles = filesToInclude.Select(d => d.FilePath!).Where(p => p != null).OrderBy(p => p).ToList();
            result.IncludedProjects = projectsToInclude.Select(p => p.Name).OrderBy(n => n).ToList();

            _logger.LogInformation("Pruned solution created at: {OutputPath}", result.PrunedSolutionPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to prune solution");
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    private async Task<(INamedTypeSymbol? Symbol, Document? Document)> FindTypeAsync(
        Solution solution,
        string fullyQualifiedTypeName,
        CancellationToken cancellationToken)
    {
        foreach (var project in solution.Projects)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var compilation = await project.GetCompilationAsync(cancellationToken);
            if (compilation == null)
                continue;

            // Try to find the type by fully qualified name
            var typeSymbol = compilation.GetTypeByMetadataName(fullyQualifiedTypeName);
            if (typeSymbol != null)
            {
                // Find the document containing this type
                foreach (var location in typeSymbol.Locations)
                {
                    if (location.SourceTree != null)
                    {
                        var document = solution.GetDocument(location.SourceTree);
                        if (document != null)
                        {
                            return (typeSymbol, document);
                        }
                    }
                }
            }
        }

        return (null, null);
    }

    private async Task CollectDependenciesAsync(
        Solution solution,
        INamedTypeSymbol targetType,
        HashSet<INamedTypeSymbol> collectedTypes,
        HashSet<INamedTypeSymbol> processedTypes,
        CancellationToken cancellationToken)
    {
        if (processedTypes.Contains(targetType))
            return;

        processedTypes.Add(targetType);

        var dependencies = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

        // Get dependencies from the type definition
        CollectTypeDependencies(targetType, dependencies);

        // Get dependencies from the type's members
        foreach (var member in targetType.GetMembers())
        {
            CollectMemberDependencies(member, dependencies);
        }

        // Get dependencies from the type's syntax (for local variables, etc.)
        foreach (var location in targetType.Locations)
        {
            if (location.SourceTree != null)
            {
                var document = solution.GetDocument(location.SourceTree);
                if (document != null)
                {
                    await CollectSyntaxDependenciesAsync(document, targetType, dependencies, cancellationToken);
                }
            }
        }

        // Filter to only types defined in the solution
        foreach (var dep in dependencies)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            if (IsTypeInSolution(solution, dep) && !collectedTypes.Contains(dep))
            {
                collectedTypes.Add(dep);
                // Recursively collect dependencies of this dependency
                await CollectDependenciesAsync(solution, dep, collectedTypes, processedTypes, cancellationToken);
            }
        }
    }

    private void CollectTypeDependencies(INamedTypeSymbol type, HashSet<INamedTypeSymbol> dependencies)
    {
        // Base type
        if (type.BaseType != null && type.BaseType.SpecialType == SpecialType.None)
        {
            AddTypeAndGenericArguments(type.BaseType, dependencies);
        }

        // Implemented interfaces
        foreach (var iface in type.Interfaces)
        {
            AddTypeAndGenericArguments(iface, dependencies);
        }

        // Type parameters and constraints
        foreach (var typeParam in type.TypeParameters)
        {
            foreach (var constraint in typeParam.ConstraintTypes)
            {
                if (constraint is INamedTypeSymbol namedConstraint)
                {
                    AddTypeAndGenericArguments(namedConstraint, dependencies);
                }
            }
        }

        // Attributes
        foreach (var attr in type.GetAttributes())
        {
            if (attr.AttributeClass != null)
            {
                AddTypeAndGenericArguments(attr.AttributeClass, dependencies);
            }
        }
    }

    private void CollectMemberDependencies(ISymbol member, HashSet<INamedTypeSymbol> dependencies)
    {
        switch (member)
        {
            case IFieldSymbol field:
                AddTypeAndGenericArguments(field.Type, dependencies);
                break;

            case IPropertySymbol property:
                AddTypeAndGenericArguments(property.Type, dependencies);
                break;

            case IMethodSymbol method:
                AddTypeAndGenericArguments(method.ReturnType, dependencies);
                foreach (var param in method.Parameters)
                {
                    AddTypeAndGenericArguments(param.Type, dependencies);
                }
                foreach (var typeParam in method.TypeParameters)
                {
                    foreach (var constraint in typeParam.ConstraintTypes)
                    {
                        if (constraint is INamedTypeSymbol namedConstraint)
                        {
                            AddTypeAndGenericArguments(namedConstraint, dependencies);
                        }
                    }
                }
                break;

            case IEventSymbol evt:
                AddTypeAndGenericArguments(evt.Type, dependencies);
                break;
        }

        // Attributes on members
        foreach (var attr in member.GetAttributes())
        {
            if (attr.AttributeClass != null)
            {
                AddTypeAndGenericArguments(attr.AttributeClass, dependencies);
            }
        }
    }

    private async Task CollectSyntaxDependenciesAsync(
        Document document,
        INamedTypeSymbol targetType,
        HashSet<INamedTypeSymbol> dependencies,
        CancellationToken cancellationToken)
    {
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
        var root = await document.GetSyntaxRootAsync(cancellationToken);

        if (semanticModel == null || root == null)
            return;

        // Find the type declaration
        var typeDeclarations = root.DescendantNodes()
            .OfType<TypeDeclarationSyntax>()
            .Where(td =>
            {
                var symbol = semanticModel.GetDeclaredSymbol(td);
                return SymbolEqualityComparer.Default.Equals(symbol, targetType);
            });

        foreach (var typeDecl in typeDeclarations)
        {
            // Collect all identifier names within the type declaration
            var identifiers = typeDecl.DescendantNodes().OfType<IdentifierNameSyntax>();
            foreach (var identifier in identifiers)
            {
                var symbolInfo = semanticModel.GetSymbolInfo(identifier);
                var symbol = symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault();

                if (symbol != null)
                {
                    var containingType = symbol.ContainingType;
                    if (containingType != null)
                    {
                        AddTypeAndGenericArguments(containingType, dependencies);
                    }

                    if (symbol is INamedTypeSymbol namedType)
                    {
                        AddTypeAndGenericArguments(namedType, dependencies);
                    }
                }
            }

            // Collect types from object creation expressions
            var objectCreations = typeDecl.DescendantNodes().OfType<ObjectCreationExpressionSyntax>();
            foreach (var creation in objectCreations)
            {
                var typeInfo = semanticModel.GetTypeInfo(creation);
                if (typeInfo.Type is INamedTypeSymbol createdType)
                {
                    AddTypeAndGenericArguments(createdType, dependencies);
                }
            }

            // Collect types from generic names
            var genericNames = typeDecl.DescendantNodes().OfType<GenericNameSyntax>();
            foreach (var generic in genericNames)
            {
                var symbolInfo = semanticModel.GetSymbolInfo(generic);
                if (symbolInfo.Symbol is INamedTypeSymbol namedType)
                {
                    AddTypeAndGenericArguments(namedType, dependencies);
                }
            }
        }
    }

    private async Task CollectReferencesAsync(
        Solution solution,
        INamedTypeSymbol targetType,
        HashSet<INamedTypeSymbol> collectedTypes,
        HashSet<INamedTypeSymbol> referencingTypes,
        HashSet<INamedTypeSymbol> processedForReferences,
        CancellationToken cancellationToken)
    {
        // Find all types that reference the target type
        var newReferences = new List<INamedTypeSymbol>();

        foreach (var project in solution.Projects)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var compilation = await project.GetCompilationAsync(cancellationToken);
            if (compilation == null)
                continue;

            foreach (var document in project.Documents)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
                var root = await document.GetSyntaxRootAsync(cancellationToken);

                if (semanticModel == null || root == null)
                    continue;

                // Find all type declarations in this document
                var typeDeclarations = root.DescendantNodes().OfType<TypeDeclarationSyntax>();

                foreach (var typeDecl in typeDeclarations)
                {
                    var declaredSymbol = semanticModel.GetDeclaredSymbol(typeDecl) as INamedTypeSymbol;
                    if (declaredSymbol == null)
                        continue;

                    // Skip if already collected or is the target type
                    if (collectedTypes.Contains(declaredSymbol) || SymbolEqualityComparer.Default.Equals(declaredSymbol, targetType))
                        continue;

                    // Check if this type references the target type
                    if (await TypeReferencesTargetAsync(document, typeDecl, targetType, semanticModel, cancellationToken))
                    {
                        if (!referencingTypes.Contains(declaredSymbol))
                        {
                            referencingTypes.Add(declaredSymbol);
                            newReferences.Add(declaredSymbol);
                        }
                    }
                }
            }
        }

        // Add the new references to collected types and recursively find their references
        foreach (var reference in newReferences)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            if (!collectedTypes.Contains(reference))
            {
                collectedTypes.Add(reference);

                // Also collect dependencies of the referencing type
                await CollectDependenciesAsync(solution, reference, collectedTypes, processedForReferences, cancellationToken);

                // Recursively find types that reference this referencing type
                await CollectReferencesAsync(solution, reference, collectedTypes, referencingTypes, processedForReferences, cancellationToken);
            }
        }
    }

    private async Task<bool> TypeReferencesTargetAsync(
        Document document,
        TypeDeclarationSyntax typeDecl,
        INamedTypeSymbol targetType,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        // Check all identifiers in the type declaration
        var identifiers = typeDecl.DescendantNodes()
            .OfType<IdentifierNameSyntax>()
            .Where(id => id.Identifier.Text == targetType.Name);

        foreach (var identifier in identifiers)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(identifier, cancellationToken);
            var symbol = symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault();

            if (symbol != null)
            {
                INamedTypeSymbol? referencedType = null;

                if (symbol is INamedTypeSymbol namedType)
                {
                    referencedType = namedType;
                }
                else if (symbol.ContainingType != null)
                {
                    referencedType = symbol.ContainingType;
                }

                if (referencedType != null && SymbolEqualityComparer.Default.Equals(referencedType.OriginalDefinition, targetType.OriginalDefinition))
                {
                    return true;
                }
            }
        }

        // Check generic names
        var genericNames = typeDecl.DescendantNodes()
            .OfType<GenericNameSyntax>()
            .Where(gn => gn.Identifier.Text == targetType.Name);

        foreach (var generic in genericNames)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(generic, cancellationToken);
            if (symbolInfo.Symbol is INamedTypeSymbol namedType &&
                SymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, targetType.OriginalDefinition))
            {
                return true;
            }
        }

        return false;
    }

    private void AddTypeAndGenericArguments(ITypeSymbol type, HashSet<INamedTypeSymbol> dependencies)
    {
        if (type is INamedTypeSymbol namedType)
        {
            if (namedType.SpecialType == SpecialType.None &&
                namedType.TypeKind != TypeKind.Error &&
                !namedType.IsAnonymousType)
            {
                dependencies.Add(namedType.OriginalDefinition);

                // Add generic type arguments
                foreach (var typeArg in namedType.TypeArguments)
                {
                    AddTypeAndGenericArguments(typeArg, dependencies);
                }
            }
        }
        else if (type is IArrayTypeSymbol arrayType)
        {
            AddTypeAndGenericArguments(arrayType.ElementType, dependencies);
        }
    }

    private bool IsTypeInSolution(Solution solution, INamedTypeSymbol type)
    {
        foreach (var location in type.Locations)
        {
            if (location.SourceTree != null)
            {
                var document = solution.GetDocument(location.SourceTree);
                if (document != null)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private async Task<List<Document>> CollectFilesForTypesAsync(
        Solution solution,
        HashSet<INamedTypeSymbol> types,
        CancellationToken cancellationToken)
    {
        var documents = new HashSet<Document>();

        foreach (var type in types)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            foreach (var location in type.Locations)
            {
                if (location.SourceTree != null)
                {
                    var document = solution.GetDocument(location.SourceTree);
                    if (document != null && document.FilePath != null)
                    {
                        documents.Add(document);
                    }
                }
            }
        }

        return documents.ToList();
    }

    private async Task CreatePrunedSolutionAsync(
        string originalSolutionPath,
        string outputDirectory,
        List<Document> filesToInclude,
        List<Project> projectsToInclude,
        HashSet<INamedTypeSymbol> includedTypes,
        CancellationToken cancellationToken)
    {
        var originalSolutionDirectory = Path.GetDirectoryName(originalSolutionPath)!;
        var solutionFileName = Path.GetFileName(originalSolutionPath);

        // Create output directory
        if (Directory.Exists(outputDirectory))
        {
            Directory.Delete(outputDirectory, true);
        }
        Directory.CreateDirectory(outputDirectory);

        // Group documents by project
        var documentsByProject = filesToInclude
            .GroupBy(d => d.Project)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Track which projects we're including
        var includedProjectPaths = new List<(string OriginalPath, string NewPath, string ProjectName)>();

        foreach (var project in projectsToInclude)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            if (project.FilePath == null)
                continue;

            var projectDirectory = Path.GetDirectoryName(project.FilePath)!;
            var relativeProjectDir = Path.GetRelativePath(originalSolutionDirectory, projectDirectory);
            var newProjectDir = Path.Combine(outputDirectory, relativeProjectDir);

            Directory.CreateDirectory(newProjectDir);

            // Copy the project file
            var projectFileName = Path.GetFileName(project.FilePath);
            var newProjectPath = Path.Combine(newProjectDir, projectFileName);

            // Read and modify the project file to only include the relevant files
            var projectContent = await File.ReadAllTextAsync(project.FilePath, cancellationToken);

            // Copy the project file (we'll let the user manage project references)
            await File.WriteAllTextAsync(newProjectPath, projectContent, cancellationToken);

            includedProjectPaths.Add((project.FilePath, newProjectPath, project.Name));

            // Copy the relevant source files
            if (documentsByProject.TryGetValue(project, out var documents))
            {
                foreach (var document in documents)
                {
                    if (document.FilePath == null)
                        continue;

                    var relativeFilePath = Path.GetRelativePath(projectDirectory, document.FilePath);
                    var newFilePath = Path.Combine(newProjectDir, relativeFilePath);
                    var newFileDir = Path.GetDirectoryName(newFilePath)!;

                    Directory.CreateDirectory(newFileDir);

                    // Copy the source file
                    File.Copy(document.FilePath, newFilePath, true);
                }
            }
        }

        // Create the solution file
        var newSolutionPath = Path.Combine(outputDirectory, solutionFileName);
        await CreateSolutionFileAsync(newSolutionPath, includedProjectPaths, originalSolutionPath, cancellationToken);

        _logger.LogInformation("Created pruned solution with {ProjectCount} projects and {FileCount} files",
            includedProjectPaths.Count, filesToInclude.Count);
    }

    private async Task CreateSolutionFileAsync(
        string solutionPath,
        List<(string OriginalPath, string NewPath, string ProjectName)> projects,
        string originalSolutionPath,
        CancellationToken cancellationToken)
    {
        var solutionDir = Path.GetDirectoryName(solutionPath)!;
        var solutionLines = new List<string>
        {
            "",
            "Microsoft Visual Studio Solution File, Format Version 12.00",
            "# Visual Studio Version 17",
            "VisualStudioVersion = 17.0.31903.59",
            "MinimumVisualStudioVersion = 10.0.40219.1"
        };

        // Read original solution to get project GUIDs
        var originalContent = await File.ReadAllTextAsync(originalSolutionPath, cancellationToken);
        var projectGuidMap = new Dictionary<string, Guid>();

        // Parse project GUIDs from original solution
        var projectPattern = @"Project\(""\{([^}]+)\}""\)\s*=\s*""([^""]+)""\s*,\s*""([^""]+)""\s*,\s*""\{([^}]+)\}""";
        var matches = System.Text.RegularExpressions.Regex.Matches(originalContent, projectPattern);
        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var projectPath = match.Groups[3].Value;
            var projectGuid = match.Groups[4].Value;
            projectGuidMap[Path.GetFileName(projectPath)] = Guid.Parse(projectGuid);
        }

        // Add projects
        foreach (var (originalPath, newPath, projectName) in projects)
        {
            var relativePath = Path.GetRelativePath(solutionDir, newPath);
            var projectFileName = Path.GetFileName(newPath);
            var projectGuid = projectGuidMap.GetValueOrDefault(projectFileName, Guid.NewGuid());
            var typeGuid = "FAE04EC0-301F-11D3-BF4B-00C04F79EFBC"; // C# project type GUID

            solutionLines.Add($"Project(\"{{{typeGuid}}}\") = \"{projectName}\", \"{relativePath}\", \"{{{projectGuid}}}\"");
            solutionLines.Add("EndProject");
        }

        solutionLines.Add("Global");
        solutionLines.Add("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");
        solutionLines.Add("\t\tDebug|Any CPU = Debug|Any CPU");
        solutionLines.Add("\t\tRelease|Any CPU = Release|Any CPU");
        solutionLines.Add("\tEndGlobalSection");
        solutionLines.Add("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");

        foreach (var (originalPath, newPath, projectName) in projects)
        {
            var projectFileName = Path.GetFileName(newPath);
            var projectGuid = projectGuidMap.GetValueOrDefault(projectFileName, Guid.NewGuid());

            solutionLines.Add($"\t\t{{{projectGuid}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU");
            solutionLines.Add($"\t\t{{{projectGuid}}}.Debug|Any CPU.Build.0 = Debug|Any CPU");
            solutionLines.Add($"\t\t{{{projectGuid}}}.Release|Any CPU.ActiveCfg = Release|Any CPU");
            solutionLines.Add($"\t\t{{{projectGuid}}}.Release|Any CPU.Build.0 = Release|Any CPU");
        }

        solutionLines.Add("\tEndGlobalSection");
        solutionLines.Add("EndGlobal");
        solutionLines.Add("");

        await File.WriteAllLinesAsync(solutionPath, solutionLines, cancellationToken);
    }
}
