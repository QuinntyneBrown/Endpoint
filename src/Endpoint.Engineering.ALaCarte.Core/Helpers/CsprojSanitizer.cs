// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.ALaCarte.Core.Helpers;

/// <summary>
/// Helper class for sanitizing .csproj files by removing directory-dependent references.
/// </summary>
public class CsprojSanitizer
{
    private readonly ILogger _logger;

    public CsprojSanitizer(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Sanitizes a .csproj file by removing directory-dependent references.
    /// </summary>
    /// <param name="csprojPath">Path to the .csproj file.</param>
    /// <returns>True if the file was modified; otherwise, false.</returns>
    public bool Sanitize(string csprojPath)
    {
        if (!File.Exists(csprojPath))
        {
            _logger.LogWarning("Csproj file not found: {Path}", csprojPath);
            return false;
        }

        try
        {
            var content = File.ReadAllText(csprojPath);
            var doc = XDocument.Parse(content);
            var modified = false;

            // Remove ProjectReference elements with relative paths
            var projectReferences = doc.Descendants()
                .Where(e => e.Name.LocalName == "ProjectReference")
                .ToList();

            foreach (var reference in projectReferences)
            {
                var include = reference.Attribute("Include")?.Value;
                if (!string.IsNullOrEmpty(include) && IsRelativePath(include))
                {
                    _logger.LogInformation("Removing ProjectReference: {Reference} from {File}", include, csprojPath);
                    reference.Remove();
                    modified = true;
                }
            }

            // Remove Import elements with relative paths to external .props/.targets files
            var imports = doc.Descendants()
                .Where(e => e.Name.LocalName == "Import")
                .ToList();

            foreach (var import in imports)
            {
                var project = import.Attribute("Project")?.Value;
                if (!string.IsNullOrEmpty(project) && IsRelativePath(project) && !IsLocalFile(project))
                {
                    _logger.LogInformation("Removing Import: {Import} from {File}", project, csprojPath);
                    import.Remove();
                    modified = true;
                }
            }

            // Remove Directory.Build.props and Directory.Build.targets relative imports
            var directoryBuildImports = doc.Descendants()
                .Where(e => e.Name.LocalName == "Import")
                .Where(e =>
                {
                    var project = e.Attribute("Project")?.Value;
                    return !string.IsNullOrEmpty(project) &&
                           (project.Contains("Directory.Build.props") || project.Contains("Directory.Build.targets")) &&
                           IsRelativePath(project);
                })
                .ToList();

            foreach (var import in directoryBuildImports)
            {
                _logger.LogInformation("Removing Directory.Build import: {Import} from {File}",
                    import.Attribute("Project")?.Value, csprojPath);
                import.Remove();
                modified = true;
            }

            // Remove Compile, Content, None, EmbeddedResource items with Link elements pointing outside
            var linkedItems = doc.Descendants()
                .Where(e => new[] { "Compile", "Content", "None", "EmbeddedResource" }.Contains(e.Name.LocalName))
                .Where(e =>
                {
                    var include = e.Attribute("Include")?.Value;
                    return !string.IsNullOrEmpty(include) && include.StartsWith("..");
                })
                .ToList();

            foreach (var item in linkedItems)
            {
                _logger.LogInformation("Removing linked item: {Item} from {File}",
                    item.Attribute("Include")?.Value, csprojPath);
                item.Remove();
                modified = true;
            }

            // Remove empty ItemGroup elements
            var emptyItemGroups = doc.Descendants()
                .Where(e => e.Name.LocalName == "ItemGroup" && !e.HasElements)
                .ToList();

            foreach (var group in emptyItemGroups)
            {
                group.Remove();
                modified = true;
            }

            if (modified)
            {
                File.WriteAllText(csprojPath, doc.ToString());
                _logger.LogInformation("Sanitized csproj file: {Path}", csprojPath);
            }

            return modified;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sanitizing csproj file: {Path}", csprojPath);
            return false;
        }
    }

    private static bool IsRelativePath(string path)
    {
        return path.StartsWith("..") || path.StartsWith(".\\") || path.StartsWith("./");
    }

    private static bool IsLocalFile(string path)
    {
        // Local files in the same directory or subdirectories are allowed
        return !path.StartsWith("..") && !Path.IsPathRooted(path);
    }
}
