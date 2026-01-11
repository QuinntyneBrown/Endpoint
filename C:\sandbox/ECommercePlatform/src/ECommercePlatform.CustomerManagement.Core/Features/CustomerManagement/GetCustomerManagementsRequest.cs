// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;

namespace ECommercePlatform.CustomerManagement.Core.Features.CustomerManagement;

public class GetCustomerManagementsRequest: IRequest<GetCustomerManagementsResponse>
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public string SearchTerm { get; set; }
    public string SortBy { get; set; }
    public bool SortDescending { get; set; }
}

