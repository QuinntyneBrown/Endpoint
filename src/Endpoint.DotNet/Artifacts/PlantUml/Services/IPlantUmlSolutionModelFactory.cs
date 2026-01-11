// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Endpoint.DotNet.Artifacts.PlantUml.Models;
using Endpoint.DotNet.Artifacts.Solutions;

namespace Endpoint.DotNet.Artifacts.PlantUml.Services;

public interface IPlantUmlSolutionModelFactory
{
    Task<SolutionModel> CreateAsync(PlantUmlSolutionModel plantUmlModel, string solutionName, string outputDirectory, CancellationToken cancellationToken = default);
}
