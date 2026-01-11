// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;
using Microsoft.EntityFrameworkCore;
using ECommercePlatform.CustomerManagement.Core.Data;
using CustomerManagementExt = ECommercePlatform.CustomerManagement.Core.Aggregates.CustomerManagement.CustomerManagementExtensions;

namespace ECommercePlatform.CustomerManagement.Core.Features.CustomerManagement;

public class UpdateCustomerManagementHandler : IRequestHandler<UpdateCustomerManagementRequest, UpdateCustomerManagementResponse>
{
    private readonly ICustomerManagementContext _context;

    public UpdateCustomerManagementHandler(ICustomerManagementContext context)
    {
        _context = context;
    }

    public async Task<UpdateCustomerManagementResponse> Handle(UpdateCustomerManagementRequest request, CancellationToken cancellationToken)
    {
        var customerManagement = await _context.CustomerManagements
            .FirstOrDefaultAsync(x => x.CustomerManagementId == request.CustomerManagementId, cancellationToken);

        if (customerManagement == null)
        {
            return new UpdateCustomerManagementResponse { CustomerManagement = null };
        }

        customerManagement.Name = request.Name;
        customerManagement.Description = request.Description;
        customerManagement.Status = request.Status;
        customerManagement.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateCustomerManagementResponse
        {
            CustomerManagement = CustomerManagementExt.ToDto(customerManagement)
        };
    }
}
