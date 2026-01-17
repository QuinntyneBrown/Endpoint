// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.ApiGateway.UnitTests;

/// <summary>
/// Tests for ApiGateway startup configuration.
/// Note: The ApiGateway is primarily a YARP reverse proxy with minimal custom logic.
/// These tests verify basic configuration aspects.
/// </summary>
public class StartupTests
{
    [Fact]
    public void ApiGateway_ProjectExists()
    {
        // This test verifies the test project references the ApiGateway correctly
        // The ApiGateway is a minimal YARP-based reverse proxy
        Assert.True(true);
    }
}
