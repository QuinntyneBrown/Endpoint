// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using Endpoint.Core.Syntax.Entities;
using Endpoint.Core.Syntax.Interfaces;
using Endpoint.Core.Syntax.Properties;

namespace Endpoint.Core.Syntax.Classes.Factories;

public interface IClassFactory
{
    Task<ClassModel> CreateMessagePackMessageAsync(string name, List<KeyValuePair<string, string>> keyValuePairs);

    Task<ClassModel> CreateControllerAsync(string controllerName, string directory);

    Task<ClassModel> CreateEntityAsync(string name, List<KeyValuePair<string,string>> keyValuePairs);

    ClassModel CreateController(EntityModel model, string directory);

    ClassModel CreateEmptyController(string name, string directory);

    ClassModel CreateDbContext(string name, List<EntityModel> entities, string directory);

    ClassModel CreateMessageModel();

    ClassModel CreateHubModel(string name);

    InterfaceModel CreateHubInterfaceModel(string name);

    Task<ClassModel> CreateMessageProducerWorkerAsync(string name, string directory);

    Tuple<ClassModel, InterfaceModel> CreateClassAndInterface(string name);

    ClassModel CreateServiceBusMessageConsumer(string name, string messagesNamespace);

    ClassModel CreateConfigureServices(string serviceSuffix);

    Task<ClassModel> DtoExtensionsCreateAsync(ClassModel aggregate);

    Task<ClassModel> CreateRequestAsync(string requestName, string properties);

    Task<ClassModel> CreateRequestAsync(string requestName, string responseName, List<PropertyModel> properties);

    Task<ClassModel> CreateResponseAsync(string responseName, List<PropertyModel> properties);

    Task<ClassModel> CreateWorkerAsync(string name);

    Task<ClassModel> CreateUserDefinedEnumAsync(string name, string type, List<KeyValuePair<string,string>> keyValuePairs);

    Task<ClassModel> CreateUserDefinedTypeAsync(string name, string type);
}
