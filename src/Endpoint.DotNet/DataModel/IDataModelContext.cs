// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Endpoint.DotNet.DataModel;

public interface IDataModelContext
{
    List<ServiceModel> ServiceModels { get; }

}

