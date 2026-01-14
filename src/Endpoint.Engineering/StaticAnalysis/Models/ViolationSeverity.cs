// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.StaticAnalysis.Models;

/// <summary>
/// Represents the severity level of a violation.
/// </summary>
public enum ViolationSeverity
{
    /// <summary>
    /// Informational message.
    /// </summary>
    Info,

    /// <summary>
    /// Warning that should be addressed.
    /// </summary>
    Warning,

    /// <summary>
    /// Error that must be fixed.
    /// </summary>
    Error
}
