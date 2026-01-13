// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ConfigurationManagement.Core.Events;

public record ConfigurationFileUpdatedEvent(Guid ConfigurationFileId, string Name, DateTime UpdatedAt);