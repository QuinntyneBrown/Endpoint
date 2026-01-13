// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.DotNet.Artifacts.Files;
using Endpoint.DotNet.Artifacts.Projects;
using Endpoint.DotNet.Syntax;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Constructors;
using Endpoint.DotNet.Syntax.Expressions;
using Endpoint.DotNet.Syntax.Fields;
using Endpoint.DotNet.Syntax.Interfaces;
using Endpoint.DotNet.Syntax.Methods;
using Endpoint.DotNet.Syntax.Params;
using Endpoint.DotNet.Syntax.Properties;
using Endpoint.DotNet.Syntax.Attributes;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.Email;

using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

public class EmailArtifactFactory : IEmailArtifactFactory
{
    private readonly ILogger<EmailArtifactFactory> logger;

    public EmailArtifactFactory(ILogger<EmailArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding Email.Core files");
        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");

        // Entities
        project.Files.Add(CreateEmailFile(entitiesDir));
        project.Files.Add(CreateEmailTemplateFile(entitiesDir));
        project.Files.Add(CreateEmailAttachmentFile(entitiesDir));

        // Interfaces
        project.Files.Add(CreateIEmailRepositoryFile(interfacesDir));
        project.Files.Add(CreateIEmailSenderFile(interfacesDir));
        project.Files.Add(CreateITemplateEngineFile(interfacesDir));

        // Events
        project.Files.Add(CreateEmailSentEventFile(eventsDir));
        project.Files.Add(CreateEmailDeliveredEventFile(eventsDir));
        project.Files.Add(CreateEmailBouncedEventFile(eventsDir));

        // DTOs
        project.Files.Add(CreateEmailDtoFile(dtosDir));
        project.Files.Add(CreateEmailTemplateDtoFile(dtosDir));
        project.Files.Add(CreateSendEmailRequestFile(dtosDir));
        project.Files.Add(CreateSendTemplateEmailRequestFile(dtosDir));
        project.Files.Add(CreateSendEmailResultFile(dtosDir));
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Email.Infrastructure files");
        var dataDir = Path.Combine(project.Directory, "Data");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");

        project.Files.Add(CreateEmailDbContextFile(dataDir));
        project.Files.Add(CreateEmailRepositoryFile(repositoriesDir));
        project.Files.Add(CreateEmailSenderServiceFile(servicesDir));
        project.Files.Add(CreateTemplateEngineFile(servicesDir));
        project.Files.Add(CreateConfigureServicesFile(project.Directory));
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Email.Api files");
        var controllersDir = Path.Combine(project.Directory, "Controllers");

        project.Files.Add(CreateEmailControllerFile(controllersDir));
        project.Files.Add(CreateProgramFile(project.Directory));
        project.Files.Add(CreateAppSettingsFile(project.Directory));
    }

    #region Core Layer Files

    private static FileModel CreateEmailFile(string directory)
    {
        // Keep as FileModel because it includes an enum definition
        return new FileModel("Email", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Email.Core.Entities;

                public class Email
                {
                    public Guid EmailId { get; set; }
                    public Guid? TemplateId { get; set; }
                    public required string From { get; set; }
                    public required string To { get; set; }
                    public string? Cc { get; set; }
                    public string? Bcc { get; set; }
                    public required string Subject { get; set; }
                    public required string Body { get; set; }
                    public bool IsHtml { get; set; } = true;
                    public EmailStatus Status { get; set; } = EmailStatus.Pending;
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? SentAt { get; set; }
                    public DateTime? DeliveredAt { get; set; }
                    public string? FailureReason { get; set; }
                    public EmailTemplate? Template { get; set; }
                    public ICollection<EmailAttachment> Attachments { get; set; } = new List<EmailAttachment>();
                }

                public enum EmailStatus { Pending, Sent, Delivered, Bounced, Failed }
                """
        };
    }

    private static CodeFileModel<ClassModel> CreateEmailTemplateFile(string directory)
    {
        var classModel = new ClassModel("EmailTemplate");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "TemplateId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Subject", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "BodyTemplate", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsHtml", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "true" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsActive", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "true" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "UpdatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));

        return new CodeFileModel<ClassModel>(classModel, "EmailTemplate", directory, CSharp)
        {
            Namespace = "Email.Core.Entities"
        };
    }

    private static CodeFileModel<ClassModel> CreateEmailAttachmentFile(string directory)
    {
        var classModel = new ClassModel("EmailAttachment");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "AttachmentId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "EmailId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "FileName", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "ContentType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("byte[]"), "Content", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("long"), "Size", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Email"), "Email", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "null!" });

        return new CodeFileModel<ClassModel>(classModel, "EmailAttachment", directory, CSharp)
        {
            Namespace = "Email.Core.Entities"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateIEmailRepositoryFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IEmailRepository");

        interfaceModel.Usings.Add(new UsingModel("Email.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Entities.Email") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "emailId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetAllAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Entities.Email")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Entities.Email")] },
            Params =
            [
                new ParamModel { Name = "email", Type = new TypeModel("Entities.Email") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "UpdateAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "email", Type = new TypeModel("Entities.Email") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "DeleteAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "emailId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetTemplatesAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("EmailTemplate")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetTemplateByIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("EmailTemplate") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "templateId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IEmailRepository", directory, CSharp)
        {
            Namespace = "Email.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateIEmailSenderFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IEmailSender");

        interfaceModel.Usings.Add(new UsingModel("Email.Core.DTOs"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "SendAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("SendEmailResult")] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("SendEmailRequest") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "SendWithTemplateAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("SendEmailResult")] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("SendTemplateEmailRequest") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IEmailSender", directory, CSharp)
        {
            Namespace = "Email.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateITemplateEngineFile(string directory)
    {
        var interfaceModel = new InterfaceModel("ITemplateEngine");

        interfaceModel.Usings.Add(new UsingModel("Email.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "RenderTemplate",
            Interface = true,
            ReturnType = new TypeModel("string"),
            Params =
            [
                new ParamModel { Name = "template", Type = new TypeModel("EmailTemplate") },
                new ParamModel { Name = "parameters", Type = new TypeModel("IDictionary") { GenericTypeParameters = [new TypeModel("string"), new TypeModel("object")] } }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "RenderSubject",
            Interface = true,
            ReturnType = new TypeModel("string"),
            Params =
            [
                new ParamModel { Name = "template", Type = new TypeModel("EmailTemplate") },
                new ParamModel { Name = "parameters", Type = new TypeModel("IDictionary") { GenericTypeParameters = [new TypeModel("string"), new TypeModel("object")] } }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "ValidateTemplate",
            Interface = true,
            ReturnType = new TypeModel("bool"),
            Params =
            [
                new ParamModel { Name = "templateContent", Type = new TypeModel("string") }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "ITemplateEngine", directory, CSharp)
        {
            Namespace = "Email.Core.Interfaces"
        };
    }

    private static CodeFileModel<ClassModel> CreateEmailSentEventFile(string directory)
    {
        var classModel = new ClassModel("EmailSentEvent")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "EmailId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "To", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Subject", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "SentAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "EmailSentEvent", directory, CSharp)
        {
            Namespace = "Email.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreateEmailDeliveredEventFile(string directory)
    {
        var classModel = new ClassModel("EmailDeliveredEvent")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "EmailId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "To", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "DeliveredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "EmailDeliveredEvent", directory, CSharp)
        {
            Namespace = "Email.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreateEmailBouncedEventFile(string directory)
    {
        var classModel = new ClassModel("EmailBouncedEvent")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "EmailId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "To", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "BounceReason", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "BouncedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "EmailBouncedEvent", directory, CSharp)
        {
            Namespace = "Email.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreateEmailDtoFile(string directory)
    {
        var classModel = new ClassModel("EmailDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "EmailId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "From", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "To", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Cc", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Bcc", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Subject", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Body", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsHtml", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Status", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "\"Pending\"" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "SentAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "DeliveredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "AttachmentCount", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "EmailDto", directory, CSharp)
        {
            Namespace = "Email.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateEmailTemplateDtoFile(string directory)
    {
        var classModel = new ClassModel("EmailTemplateDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "TemplateId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Subject", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "BodyTemplate", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsHtml", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsActive", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "UpdatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "EmailTemplateDto", directory, CSharp)
        {
            Namespace = "Email.Core.DTOs"
        };
    }

    private static FileModel CreateSendEmailRequestFile(string directory)
    {
        // Keep as FileModel due to validation attributes on properties
        return new FileModel("SendEmailRequest", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Email.Core.DTOs;

                public sealed class SendEmailRequest
                {
                    [Required]
                    [EmailAddress]
                    public required string From { get; init; }

                    [Required]
                    [EmailAddress]
                    public required string To { get; init; }

                    [EmailAddress]
                    public string? Cc { get; init; }

                    [EmailAddress]
                    public string? Bcc { get; init; }

                    [Required]
                    public required string Subject { get; init; }

                    [Required]
                    public required string Body { get; init; }

                    public bool IsHtml { get; init; } = true;

                    public List<AttachmentRequest>? Attachments { get; init; }
                }

                public sealed class AttachmentRequest
                {
                    [Required]
                    public required string FileName { get; init; }

                    [Required]
                    public required string ContentType { get; init; }

                    [Required]
                    public required string Base64Content { get; init; }
                }
                """
        };
    }

    private static FileModel CreateSendTemplateEmailRequestFile(string directory)
    {
        // Keep as FileModel due to validation attributes on properties
        return new FileModel("SendTemplateEmailRequest", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Email.Core.DTOs;

                public sealed class SendTemplateEmailRequest
                {
                    [Required]
                    public Guid TemplateId { get; init; }

                    [Required]
                    [EmailAddress]
                    public required string From { get; init; }

                    [Required]
                    [EmailAddress]
                    public required string To { get; init; }

                    [EmailAddress]
                    public string? Cc { get; init; }

                    [EmailAddress]
                    public string? Bcc { get; init; }

                    public Dictionary<string, object> Parameters { get; init; } = new();

                    public List<AttachmentRequest>? Attachments { get; init; }
                }
                """
        };
    }

    private static CodeFileModel<ClassModel> CreateSendEmailResultFile(string directory)
    {
        var classModel = new ClassModel("SendEmailResult")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "EmailId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "Success", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "ErrorMessage", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "SentAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "SendEmailResult", directory, CSharp)
        {
            Namespace = "Email.Core.DTOs"
        };
    }

    #endregion

    #region Infrastructure Layer Files

    private static CodeFileModel<ClassModel> CreateEmailDbContextFile(string directory)
    {
        var classModel = new ClassModel("EmailDbContext");

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Email.Core.Entities"));

        classModel.Implements.Add(new TypeModel("DbContext"));

        var constructor = new ConstructorModel(classModel, "EmailDbContext")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "options", Type = new TypeModel("DbContextOptions") { GenericTypeParameters = [new TypeModel("EmailDbContext")] } }],
            BaseParams = ["options"]
        };
        classModel.Constructors.Add(constructor);

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("Core.Entities.Email")] }, "Emails", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("EmailTemplate")] }, "EmailTemplates", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("EmailAttachment")] }, "EmailAttachments", [new PropertyAccessorModel(PropertyAccessorType.Get)]));

        classModel.Methods.Add(new MethodModel
        {
            Name = "OnModelCreating",
            AccessModifier = AccessModifier.Protected,
            Override = true,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "modelBuilder", Type = new TypeModel("ModelBuilder") }],
            Body = new ExpressionModel(@"modelBuilder.Entity<Core.Entities.Email>(entity =>
        {
            entity.HasKey(e => e.EmailId);
            entity.Property(e => e.From).IsRequired().HasMaxLength(256);
            entity.Property(e => e.To).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Body).IsRequired();
            entity.HasOne(e => e.Template).WithMany().HasForeignKey(e => e.TemplateId);
            entity.HasMany(e => e.Attachments).WithOne(a => a.Email).HasForeignKey(a => a.EmailId);
        });

        modelBuilder.Entity<EmailTemplate>(entity =>
        {
            entity.HasKey(t => t.TemplateId);
            entity.Property(t => t.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(t => t.Name).IsUnique();
            entity.Property(t => t.Subject).IsRequired().HasMaxLength(500);
            entity.Property(t => t.BodyTemplate).IsRequired();
        });

        modelBuilder.Entity<EmailAttachment>(entity =>
        {
            entity.HasKey(a => a.AttachmentId);
            entity.Property(a => a.FileName).IsRequired().HasMaxLength(256);
            entity.Property(a => a.ContentType).IsRequired().HasMaxLength(100);
        });")
        });

        return new CodeFileModel<ClassModel>(classModel, "EmailDbContext", directory, CSharp)
        {
            Namespace = "Email.Infrastructure.Data"
        };
    }

    private static CodeFileModel<ClassModel> CreateEmailRepositoryFile(string directory)
    {
        var classModel = new ClassModel("EmailRepository");

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Email.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Email.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Email.Infrastructure.Data"));

        classModel.Implements.Add(new TypeModel("IEmailRepository"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "context",
            Type = new TypeModel("EmailDbContext"),
            AccessModifier = AccessModifier.Private,
            ReadOnly = true
        });

        var constructor = new ConstructorModel(classModel, "EmailRepository")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "context", Type = new TypeModel("EmailDbContext") }],
            Body = new ExpressionModel("this.context = context;")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Core.Entities.Email") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "emailId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.Emails
            .Include(e => e.Template)
            .Include(e => e.Attachments)
            .FirstOrDefaultAsync(e => e.EmailId == emailId, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetAllAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Core.Entities.Email")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.Emails
            .Include(e => e.Template)
            .Include(e => e.Attachments)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Core.Entities.Email")] },
            Params =
            [
                new ParamModel { Name = "email", Type = new TypeModel("Core.Entities.Email") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"email.EmailId = Guid.NewGuid();
        await context.Emails.AddAsync(email, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return email;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "UpdateAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "email", Type = new TypeModel("Core.Entities.Email") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"context.Emails.Update(email);
        await context.SaveChangesAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "DeleteAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "emailId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var email = await context.Emails.FindAsync(new object[] { emailId }, cancellationToken);
        if (email != null)
        {
            context.Emails.Remove(email);
            await context.SaveChangesAsync(cancellationToken);
        }")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetTemplatesAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("EmailTemplate")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.EmailTemplates
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetTemplateByIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("EmailTemplate") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "templateId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.EmailTemplates.FindAsync(new object[] { templateId }, cancellationToken);")
        });

        return new CodeFileModel<ClassModel>(classModel, "EmailRepository", directory, CSharp)
        {
            Namespace = "Email.Infrastructure.Repositories"
        };
    }

    private static CodeFileModel<ClassModel> CreateEmailSenderServiceFile(string directory)
    {
        var classModel = new ClassModel("EmailSender");

        classModel.Usings.Add(new UsingModel("Email.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("Email.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Email.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("IEmailSender"));

        classModel.Fields.Add(new FieldModel { Name = "repository", Type = new TypeModel("IEmailRepository"), AccessModifier = AccessModifier.Private, ReadOnly = true });
        classModel.Fields.Add(new FieldModel { Name = "templateEngine", Type = new TypeModel("ITemplateEngine"), AccessModifier = AccessModifier.Private, ReadOnly = true });

        var constructor = new ConstructorModel(classModel, "EmailSender")
        {
            AccessModifier = AccessModifier.Public,
            Params =
            [
                new ParamModel { Name = "repository", Type = new TypeModel("IEmailRepository") },
                new ParamModel { Name = "templateEngine", Type = new TypeModel("ITemplateEngine") }
            ],
            Body = new ExpressionModel(@"this.repository = repository;
        this.templateEngine = templateEngine;")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "SendAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("SendEmailResult")] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("SendEmailRequest") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"try
        {
            var email = new Core.Entities.Email
            {
                From = request.From,
                To = request.To,
                Cc = request.Cc,
                Bcc = request.Bcc,
                Subject = request.Subject,
                Body = request.Body,
                IsHtml = request.IsHtml,
                Status = EmailStatus.Sent,
                SentAt = DateTime.UtcNow
            };

            if (request.Attachments != null)
            {
                foreach (var attachment in request.Attachments)
                {
                    var content = Convert.FromBase64String(attachment.Base64Content);
                    email.Attachments.Add(new EmailAttachment
                    {
                        AttachmentId = Guid.NewGuid(),
                        FileName = attachment.FileName,
                        ContentType = attachment.ContentType,
                        Content = content,
                        Size = content.Length
                    });
                }
            }

            var created = await repository.AddAsync(email, cancellationToken);

            return new SendEmailResult
            {
                EmailId = created.EmailId,
                Success = true,
                SentAt = created.SentAt
            };
        }
        catch (Exception ex)
        {
            return new SendEmailResult
            {
                EmailId = Guid.Empty,
                Success = false,
                ErrorMessage = ex.Message
            };
        }")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "SendWithTemplateAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("SendEmailResult")] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("SendTemplateEmailRequest") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"try
        {
            var template = await repository.GetTemplateByIdAsync(request.TemplateId, cancellationToken);
            if (template == null)
            {
                return new SendEmailResult
                {
                    EmailId = Guid.Empty,
                    Success = false,
                    ErrorMessage = ""Template not found""
                };
            }

            var subject = templateEngine.RenderSubject(template, request.Parameters);
            var body = templateEngine.RenderTemplate(template, request.Parameters);

            var email = new Core.Entities.Email
            {
                From = request.From,
                To = request.To,
                Cc = request.Cc,
                Bcc = request.Bcc,
                Subject = subject,
                Body = body,
                IsHtml = template.IsHtml,
                TemplateId = template.TemplateId,
                Status = EmailStatus.Sent,
                SentAt = DateTime.UtcNow
            };

            if (request.Attachments != null)
            {
                foreach (var attachment in request.Attachments)
                {
                    var content = Convert.FromBase64String(attachment.Base64Content);
                    email.Attachments.Add(new EmailAttachment
                    {
                        AttachmentId = Guid.NewGuid(),
                        FileName = attachment.FileName,
                        ContentType = attachment.ContentType,
                        Content = content,
                        Size = content.Length
                    });
                }
            }

            var created = await repository.AddAsync(email, cancellationToken);

            return new SendEmailResult
            {
                EmailId = created.EmailId,
                Success = true,
                SentAt = created.SentAt
            };
        }
        catch (Exception ex)
        {
            return new SendEmailResult
            {
                EmailId = Guid.Empty,
                Success = false,
                ErrorMessage = ex.Message
            };
        }")
        });

        return new CodeFileModel<ClassModel>(classModel, "EmailSender", directory, CSharp)
        {
            Namespace = "Email.Infrastructure.Services"
        };
    }

    private static CodeFileModel<ClassModel> CreateTemplateEngineFile(string directory)
    {
        var classModel = new ClassModel("TemplateEngine");

        classModel.Usings.Add(new UsingModel("System.Text.RegularExpressions"));
        classModel.Usings.Add(new UsingModel("Email.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Email.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("ITemplateEngine"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "PlaceholderRegex",
            Type = new TypeModel("Regex"),
            AccessModifier = AccessModifier.Private,
            Static = true,
            ReadOnly = true,
            DefaultValue = "new(@\"\\{\\{(\\w+)\\}\\}\", RegexOptions.Compiled)"
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "RenderTemplate",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("string"),
            Params =
            [
                new ParamModel { Name = "template", Type = new TypeModel("EmailTemplate") },
                new ParamModel { Name = "parameters", Type = new TypeModel("IDictionary") { GenericTypeParameters = [new TypeModel("string"), new TypeModel("object")] } }
            ],
            Body = new ExpressionModel(@"return ReplacePlaceholders(template.BodyTemplate, parameters);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "RenderSubject",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("string"),
            Params =
            [
                new ParamModel { Name = "template", Type = new TypeModel("EmailTemplate") },
                new ParamModel { Name = "parameters", Type = new TypeModel("IDictionary") { GenericTypeParameters = [new TypeModel("string"), new TypeModel("object")] } }
            ],
            Body = new ExpressionModel(@"return ReplacePlaceholders(template.Subject, parameters);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "ValidateTemplate",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("bool"),
            Params =
            [
                new ParamModel { Name = "templateContent", Type = new TypeModel("string") }
            ],
            Body = new ExpressionModel(@"try
        {
            var matches = PlaceholderRegex.Matches(templateContent);
            return true;
        }
        catch
        {
            return false;
        }")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "ReplacePlaceholders",
            AccessModifier = AccessModifier.Private,
            Static = true,
            ReturnType = new TypeModel("string"),
            Params =
            [
                new ParamModel { Name = "template", Type = new TypeModel("string") },
                new ParamModel { Name = "parameters", Type = new TypeModel("IDictionary") { GenericTypeParameters = [new TypeModel("string"), new TypeModel("object")] } }
            ],
            Body = new ExpressionModel(@"return PlaceholderRegex.Replace(template, match =>
        {
            var key = match.Groups[1].Value;
            return parameters.TryGetValue(key, out var value) ? value?.ToString() ?? string.Empty : match.Value;
        });")
        });

        return new CodeFileModel<ClassModel>(classModel, "TemplateEngine", directory, CSharp)
        {
            Namespace = "Email.Infrastructure.Services"
        };
    }

    private static CodeFileModel<ClassModel> CreateConfigureServicesFile(string directory)
    {
        var classModel = new ClassModel("ConfigureServices")
        {
            Static = true
        };

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.Configuration"));
        classModel.Usings.Add(new UsingModel("Email.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Email.Infrastructure.Data"));
        classModel.Usings.Add(new UsingModel("Email.Infrastructure.Repositories"));
        classModel.Usings.Add(new UsingModel("Email.Infrastructure.Services"));

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddEmailInfrastructure",
            AccessModifier = AccessModifier.Public,
            Static = true,
            ReturnType = new TypeModel("IServiceCollection"),
            Params =
            [
                new ParamModel { Name = "services", Type = new TypeModel("IServiceCollection"), ExtensionMethodParam = true },
                new ParamModel { Name = "configuration", Type = new TypeModel("IConfiguration") }
            ],
            Body = new ExpressionModel(@"services.AddDbContext<EmailDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString(""EmailDb"") ??
                @""Server=.\SQLEXPRESS;Database=EmailDb;Trusted_Connection=True;TrustServerCertificate=True""));

        services.AddScoped<IEmailRepository, EmailRepository>();
        services.AddScoped<IEmailSender, EmailSender>();
        services.AddScoped<ITemplateEngine, TemplateEngine>();
        return services;")
        });

        return new CodeFileModel<ClassModel>(classModel, "ConfigureServices", directory, CSharp)
        {
            Namespace = "Microsoft.Extensions.DependencyInjection"
        };
    }

    #endregion

    #region API Layer Files

    private static CodeFileModel<ClassModel> CreateEmailControllerFile(string directory)
    {
        var classModel = new ClassModel("EmailController");

        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.Mvc"));
        classModel.Usings.Add(new UsingModel("Email.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("Email.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("ControllerBase"));

        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ApiController" });
        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Route", Template = "\"api/[controller]\"" });

        classModel.Fields.Add(new FieldModel { Name = "emailSender", Type = new TypeModel("IEmailSender"), AccessModifier = AccessModifier.Private, ReadOnly = true });
        classModel.Fields.Add(new FieldModel { Name = "repository", Type = new TypeModel("IEmailRepository"), AccessModifier = AccessModifier.Private, ReadOnly = true });

        var constructor = new ConstructorModel(classModel, "EmailController")
        {
            AccessModifier = AccessModifier.Public,
            Params =
            [
                new ParamModel { Name = "emailSender", Type = new TypeModel("IEmailSender") },
                new ParamModel { Name = "repository", Type = new TypeModel("IEmailRepository") }
            ],
            Body = new ExpressionModel(@"this.emailSender = emailSender;
        this.repository = repository;")
        };
        classModel.Constructors.Add(constructor);

        var sendMethod = new MethodModel
        {
            Name = "Send",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("SendEmailResult")] }] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("SendEmailRequest"), Attribute = new AttributeModel() { Name = "FromBody" } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var result = await emailSender.SendAsync(request, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetById), new { id = result.EmailId }, result);")
        };
        sendMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpPost", Template = "\"send\"" });
        classModel.Methods.Add(sendMethod);

        var getByIdMethod = new MethodModel
        {
            Name = "GetById",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("EmailDto")] }] },
            Params =
            [
                new ParamModel { Name = "id", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var email = await repository.GetByIdAsync(id, cancellationToken);
        if (email == null) return NotFound();

        return Ok(new EmailDto
        {
            EmailId = email.EmailId,
            From = email.From,
            To = email.To,
            Cc = email.Cc,
            Bcc = email.Bcc,
            Subject = email.Subject,
            Body = email.Body,
            IsHtml = email.IsHtml,
            Status = email.Status.ToString(),
            CreatedAt = email.CreatedAt,
            SentAt = email.SentAt,
            DeliveredAt = email.DeliveredAt,
            AttachmentCount = email.Attachments.Count
        });")
        };
        getByIdMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"{id:guid}\"" });
        classModel.Methods.Add(getByIdMethod);

        var getTemplatesMethod = new MethodModel
        {
            Name = "GetTemplates",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("EmailTemplateDto")] }] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var templates = await repository.GetTemplatesAsync(cancellationToken);
        return Ok(templates.Select(t => new EmailTemplateDto
        {
            TemplateId = t.TemplateId,
            Name = t.Name,
            Subject = t.Subject,
            BodyTemplate = t.BodyTemplate,
            IsHtml = t.IsHtml,
            IsActive = t.IsActive,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        }));")
        };
        getTemplatesMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"templates\"" });
        classModel.Methods.Add(getTemplatesMethod);

        return new CodeFileModel<ClassModel>(classModel, "EmailController", directory, CSharp)
        {
            Namespace = "Email.Api.Controllers"
        };
    }

    private static FileModel CreateProgramFile(string directory)
    {
        // Keep as FileModel for top-level statements
        return new FileModel("Program", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                var builder = WebApplication.CreateBuilder(args);

                builder.Services.AddEmailInfrastructure(builder.Configuration);
                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();
                builder.Services.AddHealthChecks();

                var app = builder.Build();

                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();
                app.MapControllers();
                app.MapHealthChecks("/health");
                app.Run();
                """
        };
    }

    private static FileModel CreateAppSettingsFile(string directory)
    {
        return new FileModel("appsettings", directory, ".json")
        {
            Body = """
                {
                  "ConnectionStrings": {
                    "EmailDb": "Server=.\\SQLEXPRESS;Database=EmailDb;Trusted_Connection=True;TrustServerCertificate=True"
                  },
                  "Logging": {
                    "LogLevel": {
                      "Default": "Information"
                    }
                  },
                  "AllowedHosts": "*"
                }
                """
        };
    }

    #endregion
}
