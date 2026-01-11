// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;
using Microsoft.EntityFrameworkCore;
using ECommercePlatform.CustomerManagement.Core.Data;
using CustomerManagementExt = ECommercePlatform.CustomerManagement.Core.Aggregates.CustomerManagement.CustomerManagementExtensions;

namespace ECommercePlatform.CustomerManagement.Core.Features.CustomerManagement;

public class GetCustomerManagementsHandler : IRequestHandler<GetCustomerManagementsRequest, GetCustomerManagementsResponse>
{
    private readonly ICustomerManagementContext _context;

    public GetCustomerManagementsHandler(ICustomerManagementContext context)
    {
        _context = context;
    }

    public async Task<GetCustomerManagementsResponse> Handle(GetCustomerManagementsRequest request, CancellationToken cancellationToken)
    {
        var query = _context.CustomerManagements.AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var pageSize = request.PageSize > 0 ? request.PageSize : 10;
        var pageIndex = request.PageIndex > 0 ? request.PageIndex : 0;

        var items = await query
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new GetCustomerManagementsResponse
        {
            CustomerManagements = items.Select(CustomerManagementExt.ToDto).ToList(),
            TotalCount = totalCount
        };
    }
}
