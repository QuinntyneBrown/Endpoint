// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using Endpoint.DotNet.Syntax.Entities;

namespace Endpoint.DotNet.Artifacts.Files.Factories;

public interface IFileFactory
{
    TemplatedFileModel CreateTemplate(string template, string name, string directory, string extension = ".cs", string filename = null, Dictionary<string, object> tokens = null);

    EntityFileModel Create(EntityModel model, string directory);

    CSharpTemplatedFileModel CreateCSharp(string template, string @namespace, string name, string directory, Dictionary<string, object> tokens = null);

    TemplatedFileModel LaunchSettingsJson(string projectDirectory, string projectName, int port);

    TemplatedFileModel AppSettings(string projectDirectory, string projectName, string dbContextName);

    FileModel CreateCSharp<T>(T classModel, string directory)
        where T : TypeDeclarationModel;

    FileModel CreateResponseBase(string directory);

    FileModel CreateLinqExtensions(string directory);

    FileModel CreateCoreUsings(string directory);

    FileModel CreateDbContextInterface(string directory);

    Task<FileModel> CreateUdpClientFactoryInterfaceAsync(string directory);

    Task<FileModel> CreateUdpMessageSenderInterfaceAsync(string directory);

    Task<FileModel> CreateUdpMessageReceiverInterfaceAsync(string directory);

    Task<FileModel> CreateUdpMessageSenderAsync(string directory);

    Task<FileModel> CreateUdpMessageReceiverAsync(string directory);

    Task<FileModel> CreateUdpServiceBusConfigureServicesAsync(string directory);

    Task<FileModel> CreateUdpServiceBusHostExtensionsAsync(string directory);

    Task<FileModel> CreateUdpServiceBusMessageAsync(string directory);
}
