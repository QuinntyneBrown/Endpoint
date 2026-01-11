// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;
using Microsoft.EntityFrameworkCore;
using ECommercePlatform.CustomerManagement.Core.Data;

namespace ECommercePlatform.CustomerManagement.Core.Features.CustomerManagement;

public class DeleteCustomerManagementHandler : IRequestHandler<DeleteCustomerManagementRequest, DeleteCustomerManagementResponse>
{
    private readonly ICustomerManagementContext _context;

    public DeleteCustomerManagementHandler(ICustomerManagementContext context)
    {
        _context = context;
    }

    public async Task<DeleteCustomerManagementResponse> Handle(DeleteCustomerManagementRequest request, CancellationToken cancellationToken)
    {
        var customerManagement = await _context.CustomerManagements
            .FirstOrDefaultAsync(x => x.CustomerManagementId == request.CustomerManagementId, cancellationToken);

        if (customerManagement == null)
        {
            return new DeleteCustomerManagementResponse
            {
                Success = false,
                Message = $"CustomerManagement with ID '{request.CustomerManagementId}' was not found."
            };
        }

        _context.CustomerManagements.Remove(customerManagement);
        await _context.SaveChangesAsync(cancellationToken);

        return new DeleteCustomerManagementResponse
        {
            Success = true,
            Message = $"CustomerManagement with ID '{request.CustomerManagementId}' was successfully deleted."
        };
    }
}
