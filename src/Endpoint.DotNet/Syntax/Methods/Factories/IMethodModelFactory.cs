// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Syntax.Classes;

namespace Endpoint.DotNet.Syntax.Methods.Factories;

public interface IMethodFactory
{
    MethodModel CreateControllerMethod(string name, string controller, RouteType routeType, string directory);

    Task<MethodModel> ToDtoCreateAsync(ClassModel aggregate);

    Task<MethodModel> ToDtosAsyncCreateAsync(ClassModel aggregate);

    Task<MethodModel> CreateWorkerExecuteAsync();
}
