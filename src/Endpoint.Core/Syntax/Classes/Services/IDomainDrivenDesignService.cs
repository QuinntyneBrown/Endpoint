// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Core.Syntax.Classes.Services;

public interface IDomainDrivenDesignService
{
    ClassModel ServiceBusMessageConsumerCreate(string messagesNamespace, string directory);
}
