// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Endpoint.DotNet.Artifacts.Solutions.Services;

// https://stackoverflow.com/questions/47637228/how-can-i-add-files-to-a-solution-folder
public interface ISolutionService
{
    Task AddSolutionItem(string path);

    Task Create(SolutionModel model);

    Task EventDrivenMicroservicesCreate(string name, string services, string directory);

    Task MessagingBuildingBlockAdd(string directory);

    Task IOCompressionBuildingBlockAdd(string directory);
}
