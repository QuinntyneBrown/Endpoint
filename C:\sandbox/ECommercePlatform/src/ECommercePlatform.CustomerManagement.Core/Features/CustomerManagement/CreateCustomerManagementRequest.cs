// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;
using ECommercePlatform.CustomerManagement.Core.Aggregates.CustomerManagement;

namespace ECommercePlatform.CustomerManagement.Core.Features.CustomerManagement;

public class CreateCustomerManagementRequest: IRequest<CreateCustomerManagementResponse>
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Status { get; set; }
}

