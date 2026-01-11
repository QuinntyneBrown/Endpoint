// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;
using Microsoft.EntityFrameworkCore;
using ECommercePlatform.CustomerManagement.Core.Data;
using CustomerManagementExt = ECommercePlatform.CustomerManagement.Core.Aggregates.CustomerManagement.CustomerManagementExtensions;

namespace ECommercePlatform.CustomerManagement.Core.Features.CustomerManagement;

public class GetCustomerManagementByIdHandler : IRequestHandler<GetCustomerManagementByIdRequest, GetCustomerManagementByIdResponse>
{
    private readonly ICustomerManagementContext _context;

    public GetCustomerManagementByIdHandler(ICustomerManagementContext context)
    {
        _context = context;
    }

    public async Task<GetCustomerManagementByIdResponse> Handle(GetCustomerManagementByIdRequest request, CancellationToken cancellationToken)
    {
        var customerManagement = await _context.CustomerManagements
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CustomerManagementId == request.CustomerManagementId, cancellationToken);

        return new GetCustomerManagementByIdResponse
        {
            CustomerManagement = customerManagement != null ? CustomerManagementExt.ToDto(customerManagement) : null
        };
    }
}
