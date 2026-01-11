// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Artifacts.PlantUml.Services;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using Xunit;

namespace Endpoint.DotNet.UnitTests.Artifacts.PlantUml;

public class PlantUmlParserServiceSequenceTests
{
    [Fact]
    public void ParseContent_WithSequenceDiagram_ParsesParticipants()
    {
        // Arrange
        var content = @"@startuml
actor User as u

' Angular
participant ""Admin Dashboard"" as admin

' Worker
participant ""Email Sender"" as es

' Microservice
participant ""Order Management"" as om

@enduml";

        var logger = new LoggerFactory().CreateLogger<PlantUmlParserService>();
        var fileSystem = new FileSystem();
        var parser = new PlantUmlParserService(logger, fileSystem);

        // Act
        var document = parser.ParseContent(content);

        // Assert
        Assert.NotNull(document);
        Assert.Equal(4, document.Participants.Count);

        var user = document.Participants.Find(p => p.Alias == "u");
        Assert.NotNull(user);
        Assert.Equal("User", user.Name);
        Assert.Equal("actor", user.Type);

        var admin = document.Participants.Find(p => p.Alias == "admin");
        Assert.NotNull(admin);
        Assert.Equal("Admin Dashboard", admin.Name);
        Assert.Equal("Angular", admin.DotNetType);
        Assert.Equal("participant", admin.Type);

        var emailSender = document.Participants.Find(p => p.Alias == "es");
        Assert.NotNull(emailSender);
        Assert.Equal("Email Sender", emailSender.Name);
        Assert.Equal("Worker", emailSender.DotNetType);
        Assert.Equal("participant", emailSender.Type);

        var orderManagement = document.Participants.Find(p => p.Alias == "om");
        Assert.NotNull(orderManagement);
        Assert.Equal("Order Management", orderManagement.Name);
        Assert.Equal("Microservice", orderManagement.DotNetType);
        Assert.Equal("participant", orderManagement.Type);
    }

    [Fact]
    public void ParseContent_WithAngularAsTs_RecognizesAngularType()
    {
        // Arrange
        var content = @"@startuml

' ts
participant ""Frontend App"" as app

@enduml";

        var logger = new LoggerFactory().CreateLogger<PlantUmlParserService>();
        var fileSystem = new FileSystem();
        var parser = new PlantUmlParserService(logger, fileSystem);

        // Act
        var document = parser.ParseContent(content);

        // Assert
        Assert.NotNull(document);
        Assert.Single(document.Participants);

        var app = document.Participants[0];
        Assert.Equal("Frontend App", app.Name);
        Assert.Equal("ts", app.DotNetType);
        Assert.Equal("participant", app.Type);
    }

    [Fact]
    public void ParseContent_WithoutComment_ParticipantHasNoDotNetType()
    {
        // Arrange
        var content = @"@startuml
participant ""Some Service"" as svc
@enduml";

        var logger = new LoggerFactory().CreateLogger<PlantUmlParserService>();
        var fileSystem = new FileSystem();
        var parser = new PlantUmlParserService(logger, fileSystem);

        // Act
        var document = parser.ParseContent(content);

        // Assert
        Assert.NotNull(document);
        Assert.Single(document.Participants);

        var service = document.Participants[0];
        Assert.Equal("Some Service", service.Name);
        Assert.Null(service.DotNetType);
        Assert.Equal("participant", service.Type);
    }
}
