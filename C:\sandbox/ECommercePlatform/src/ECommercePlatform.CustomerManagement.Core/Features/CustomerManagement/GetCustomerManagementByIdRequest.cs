// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;

namespace ECommercePlatform.CustomerManagement.Core.Features.CustomerManagement;

public class GetCustomerManagementByIdRequest: IRequest<GetCustomerManagementByIdResponse>
{
    public string CustomerManagementId { get; set; }
}

