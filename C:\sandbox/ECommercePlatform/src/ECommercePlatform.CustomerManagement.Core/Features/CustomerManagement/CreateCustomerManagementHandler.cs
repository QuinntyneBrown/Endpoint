// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;
using Microsoft.EntityFrameworkCore;
using ECommercePlatform.CustomerManagement.Core.Data;
using CustomerManagementEntity = ECommercePlatform.CustomerManagement.Core.Aggregates.CustomerManagement.CustomerManagement;
using CustomerManagementExt = ECommercePlatform.CustomerManagement.Core.Aggregates.CustomerManagement.CustomerManagementExtensions;

namespace ECommercePlatform.CustomerManagement.Core.Features.CustomerManagement;

public class CreateCustomerManagementHandler : IRequestHandler<CreateCustomerManagementRequest, CreateCustomerManagementResponse>
{
    private readonly ICustomerManagementContext _context;

    public CreateCustomerManagementHandler(ICustomerManagementContext context)
    {
        _context = context;
    }

    public async Task<CreateCustomerManagementResponse> Handle(CreateCustomerManagementRequest request, CancellationToken cancellationToken)
    {
        var customerManagement = new CustomerManagementEntity
        {
            CustomerManagementId = Guid.NewGuid().ToString(),
            Name = request.Name,
            Description = request.Description,
            Status = request.Status,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        _context.CustomerManagements.Add(customerManagement);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateCustomerManagementResponse
        {
            CustomerManagement = CustomerManagementExt.ToDto(customerManagement)
        };
    }
}
