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

namespace Endpoint.Engineering.Microservices.Billing;

using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

public class BillingArtifactFactory : IBillingArtifactFactory
{
    private readonly ILogger<BillingArtifactFactory> logger;

    public BillingArtifactFactory(ILogger<BillingArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding Billing.Core files");
        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");

        // Entities
        project.Files.Add(CreateSubscriptionFile(entitiesDir));
        project.Files.Add(CreateSubscriptionStatusEnumFile(entitiesDir));
        project.Files.Add(CreateInvoiceFile(entitiesDir));
        project.Files.Add(CreateInvoiceStatusEnumFile(entitiesDir));
        project.Files.Add(CreatePaymentFile(entitiesDir));
        project.Files.Add(CreatePaymentStatusEnumFile(entitiesDir));
        project.Files.Add(CreatePlanFile(entitiesDir));
        project.Files.Add(CreateBillingIntervalEnumFile(entitiesDir));

        // Interfaces
        project.Files.Add(CreateISubscriptionRepositoryFile(interfacesDir));
        project.Files.Add(CreateIPaymentGatewayFile(interfacesDir));
        project.Files.Add(CreatePaymentResultFile(interfacesDir));
        project.Files.Add(CreateIInvoiceGeneratorFile(interfacesDir));

        // Events
        project.Files.Add(CreateSubscriptionCreatedEventFile(eventsDir));
        project.Files.Add(CreatePaymentProcessedEventFile(eventsDir));
        project.Files.Add(CreatePaymentFailedEventFile(eventsDir));
        project.Files.Add(CreateInvoiceGeneratedEventFile(eventsDir));

        // DTOs
        project.Files.Add(CreateSubscriptionDtoFile(dtosDir));
        project.Files.Add(CreateCreateSubscriptionRequestFile(dtosDir));
        project.Files.Add(CreateInvoiceDtoFile(dtosDir));
        project.Files.Add(CreateCreatePaymentRequestFile(dtosDir));
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Billing.Infrastructure files");
        var dataDir = Path.Combine(project.Directory, "Data");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");

        project.Files.Add(CreateBillingDbContextFile(dataDir));
        project.Files.Add(CreateSubscriptionRepositoryFile(repositoriesDir));
        project.Files.Add(CreateInvoiceGeneratorFile(servicesDir));
        project.Files.Add(CreateInfrastructureConfigureServicesFile(project.Directory));
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Billing.Api files");
        var controllersDir = Path.Combine(project.Directory, "Controllers");

        project.Files.Add(CreateSubscriptionsControllerFile(controllersDir));
        project.Files.Add(CreatePaymentsControllerFile(controllersDir));
        project.Files.Add(CreateInvoicesControllerFile(controllersDir));
        project.Files.Add(CreateProgramFile(project.Directory));
        project.Files.Add(CreateAppSettingsFile(project.Directory));
    }

    #region Core Layer - Entities

    private static CodeFileModel<ClassModel> CreateSubscriptionFile(string directory)
    {
        var classModel = new ClassModel("Subscription");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "SubscriptionId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "TenantId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "PlanId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Plan"), "Plan", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "null!" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("SubscriptionStatus"), "Status", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "SubscriptionStatus.Active" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "StartDate", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "EndDate", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "CancelledAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("ICollection") { GenericTypeParameters = [new TypeModel("Invoice")] }, "Invoices", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "new List<Invoice>()" });

        return new CodeFileModel<ClassModel>(classModel, "Subscription", directory, CSharp)
        {
            Namespace = "Billing.Core.Entities"
        };
    }

    private static FileModel CreateSubscriptionStatusEnumFile(string directory)
    {
        return new FileModel("SubscriptionStatus", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Billing.Core.Entities;

                public enum SubscriptionStatus { Active, Cancelled, Expired, Suspended }
                """
        };
    }

    private static CodeFileModel<ClassModel> CreateInvoiceFile(string directory)
    {
        var classModel = new ClassModel("Invoice");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "InvoiceId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "SubscriptionId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Subscription"), "Subscription", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "null!" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "InvoiceNumber", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("decimal"), "Amount", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("decimal"), "TaxAmount", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("decimal"), "TotalAmount", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("InvoiceStatus"), "Status", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "InvoiceStatus.Pending" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "IssuedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "DueDate", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "PaidAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("ICollection") { GenericTypeParameters = [new TypeModel("Payment")] }, "Payments", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "new List<Payment>()" });

        return new CodeFileModel<ClassModel>(classModel, "Invoice", directory, CSharp)
        {
            Namespace = "Billing.Core.Entities"
        };
    }

    private static FileModel CreateInvoiceStatusEnumFile(string directory)
    {
        return new FileModel("InvoiceStatus", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Billing.Core.Entities;

                public enum InvoiceStatus { Pending, Paid, Overdue, Cancelled }
                """
        };
    }

    private static CodeFileModel<ClassModel> CreatePaymentFile(string directory)
    {
        var classModel = new ClassModel("Payment");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "PaymentId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "InvoiceId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Invoice"), "Invoice", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "null!" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("decimal"), "Amount", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "PaymentMethod", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "TransactionId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("PaymentStatus"), "Status", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "PaymentStatus.Pending" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "ProcessedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "FailureReason", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));

        return new CodeFileModel<ClassModel>(classModel, "Payment", directory, CSharp)
        {
            Namespace = "Billing.Core.Entities"
        };
    }

    private static FileModel CreatePaymentStatusEnumFile(string directory)
    {
        return new FileModel("PaymentStatus", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Billing.Core.Entities;

                public enum PaymentStatus { Pending, Completed, Failed, Refunded }
                """
        };
    }

    private static CodeFileModel<ClassModel> CreatePlanFile(string directory)
    {
        var classModel = new ClassModel("Plan");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "PlanId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Description", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("decimal"), "Price", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("BillingInterval"), "Interval", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "BillingInterval.Monthly" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsActive", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "true" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("ICollection") { GenericTypeParameters = [new TypeModel("Subscription")] }, "Subscriptions", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "new List<Subscription>()" });

        return new CodeFileModel<ClassModel>(classModel, "Plan", directory, CSharp)
        {
            Namespace = "Billing.Core.Entities"
        };
    }

    private static FileModel CreateBillingIntervalEnumFile(string directory)
    {
        return new FileModel("BillingInterval", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Billing.Core.Entities;

                public enum BillingInterval { Monthly, Quarterly, Yearly }
                """
        };
    }

    #endregion

    #region Core Layer - Interfaces

    private static CodeFileModel<InterfaceModel> CreateISubscriptionRepositoryFile(string directory)
    {
        var interfaceModel = new InterfaceModel("ISubscriptionRepository");

        interfaceModel.Usings.Add(new UsingModel("Billing.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Subscription") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "subscriptionId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByTenantIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Subscription") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "tenantId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetAllAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Subscription")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Subscription")] },
            Params =
            [
                new ParamModel { Name = "subscription", Type = new TypeModel("Subscription") },
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
                new ParamModel { Name = "subscription", Type = new TypeModel("Subscription") },
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
                new ParamModel { Name = "subscriptionId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "ISubscriptionRepository", directory, CSharp)
        {
            Namespace = "Billing.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateIPaymentGatewayFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IPaymentGateway");

        interfaceModel.Usings.Add(new UsingModel("Billing.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "ProcessPaymentAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("PaymentResult")] },
            Params =
            [
                new ParamModel { Name = "payment", Type = new TypeModel("Payment") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "RefundPaymentAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("PaymentResult")] },
            Params =
            [
                new ParamModel { Name = "transactionId", Type = new TypeModel("string") },
                new ParamModel { Name = "amount", Type = new TypeModel("decimal") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "ValidatePaymentMethodAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("bool")] },
            Params =
            [
                new ParamModel { Name = "paymentMethod", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IPaymentGateway", directory, CSharp)
        {
            Namespace = "Billing.Core.Interfaces"
        };
    }

    private static CodeFileModel<ClassModel> CreatePaymentResultFile(string directory)
    {
        var classModel = new ClassModel("PaymentResult");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "Success", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "TransactionId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "ErrorMessage", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "PaymentResult", directory, CSharp)
        {
            Namespace = "Billing.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateIInvoiceGeneratorFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IInvoiceGenerator");

        interfaceModel.Usings.Add(new UsingModel("Billing.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GenerateInvoiceAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Invoice")] },
            Params =
            [
                new ParamModel { Name = "subscription", Type = new TypeModel("Subscription") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GenerateInvoicePdfAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("byte[]")] },
            Params =
            [
                new ParamModel { Name = "invoice", Type = new TypeModel("Invoice") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GenerateInvoiceNumber",
            Interface = true,
            ReturnType = new TypeModel("string"),
            Params = []
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IInvoiceGenerator", directory, CSharp)
        {
            Namespace = "Billing.Core.Interfaces"
        };
    }

    #endregion

    #region Core Layer - Events

    private static CodeFileModel<ClassModel> CreateSubscriptionCreatedEventFile(string directory)
    {
        var classModel = new ClassModel("SubscriptionCreatedEvent")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "SubscriptionId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "TenantId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "PlanId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "StartDate", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "SubscriptionCreatedEvent", directory, CSharp)
        {
            Namespace = "Billing.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreatePaymentProcessedEventFile(string directory)
    {
        var classModel = new ClassModel("PaymentProcessedEvent")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "PaymentId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "InvoiceId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("decimal"), "Amount", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "TransactionId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "PaymentProcessedEvent", directory, CSharp)
        {
            Namespace = "Billing.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreatePaymentFailedEventFile(string directory)
    {
        var classModel = new ClassModel("PaymentFailedEvent")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "PaymentId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "InvoiceId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("decimal"), "Amount", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Reason", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "PaymentFailedEvent", directory, CSharp)
        {
            Namespace = "Billing.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreateInvoiceGeneratedEventFile(string directory)
    {
        var classModel = new ClassModel("InvoiceGeneratedEvent")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "InvoiceId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "SubscriptionId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "InvoiceNumber", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("decimal"), "TotalAmount", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "DueDate", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "InvoiceGeneratedEvent", directory, CSharp)
        {
            Namespace = "Billing.Core.Events"
        };
    }

    #endregion

    #region Core Layer - DTOs

    private static CodeFileModel<ClassModel> CreateSubscriptionDtoFile(string directory)
    {
        var classModel = new ClassModel("SubscriptionDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "SubscriptionId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "TenantId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "PlanId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "PlanName", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Status", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "\"Active\"" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "StartDate", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "EndDate", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "SubscriptionDto", directory, CSharp)
        {
            Namespace = "Billing.Core.DTOs"
        };
    }

    private static FileModel CreateCreateSubscriptionRequestFile(string directory)
    {
        // Keep as FileModel due to validation attributes [Required]
        return new FileModel("CreateSubscriptionRequest", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Billing.Core.DTOs;

                public sealed class CreateSubscriptionRequest
                {
                    [Required]
                    public Guid TenantId { get; init; }

                    [Required]
                    public Guid PlanId { get; init; }

                    public DateTime? StartDate { get; init; }
                }
                """
        };
    }

    private static CodeFileModel<ClassModel> CreateInvoiceDtoFile(string directory)
    {
        var classModel = new ClassModel("InvoiceDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "InvoiceId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "SubscriptionId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "InvoiceNumber", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("decimal"), "Amount", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("decimal"), "TaxAmount", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("decimal"), "TotalAmount", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Status", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "\"Pending\"" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "IssuedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "DueDate", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "PaidAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "InvoiceDto", directory, CSharp)
        {
            Namespace = "Billing.Core.DTOs"
        };
    }

    private static FileModel CreateCreatePaymentRequestFile(string directory)
    {
        // Keep as FileModel due to validation attributes [Required], [Range]
        return new FileModel("CreatePaymentRequest", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Billing.Core.DTOs;

                public sealed class CreatePaymentRequest
                {
                    [Required]
                    public Guid InvoiceId { get; init; }

                    [Required]
                    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
                    public decimal Amount { get; init; }

                    [Required]
                    public required string PaymentMethod { get; init; }
                }
                """
        };
    }

    #endregion

    #region Infrastructure Layer

    private static CodeFileModel<ClassModel> CreateBillingDbContextFile(string directory)
    {
        var classModel = new ClassModel("BillingDbContext");

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Billing.Core.Entities"));

        classModel.Implements.Add(new TypeModel("DbContext"));

        var constructor = new ConstructorModel(classModel, "BillingDbContext")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "options", Type = new TypeModel("DbContextOptions") { GenericTypeParameters = [new TypeModel("BillingDbContext")] } }],
            BaseParams = ["options"]
        };
        classModel.Constructors.Add(constructor);

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("Subscription")] }, "Subscriptions", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("Invoice")] }, "Invoices", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("Payment")] }, "Payments", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("Plan")] }, "Plans", [new PropertyAccessorModel(PropertyAccessorType.Get)]));

        classModel.Methods.Add(new MethodModel
        {
            Name = "OnModelCreating",
            AccessModifier = AccessModifier.Protected,
            Override = true,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "modelBuilder", Type = new TypeModel("ModelBuilder") }],
            Body = new ExpressionModel(@"modelBuilder.Entity<Plan>(entity =>
        {
            entity.HasKey(p => p.PlanId);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Price).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(s => s.SubscriptionId);
            entity.HasOne(s => s.Plan).WithMany(p => p.Subscriptions).HasForeignKey(s => s.PlanId);
            entity.HasIndex(s => s.TenantId);
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(i => i.InvoiceId);
            entity.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(50);
            entity.Property(i => i.Amount).HasPrecision(18, 2);
            entity.Property(i => i.TaxAmount).HasPrecision(18, 2);
            entity.Property(i => i.TotalAmount).HasPrecision(18, 2);
            entity.HasOne(i => i.Subscription).WithMany(s => s.Invoices).HasForeignKey(i => i.SubscriptionId);
            entity.HasIndex(i => i.InvoiceNumber).IsUnique();
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(p => p.PaymentId);
            entity.Property(p => p.Amount).HasPrecision(18, 2);
            entity.Property(p => p.PaymentMethod).IsRequired().HasMaxLength(50);
            entity.HasOne(p => p.Invoice).WithMany(i => i.Payments).HasForeignKey(p => p.InvoiceId);
        });")
        });

        return new CodeFileModel<ClassModel>(classModel, "BillingDbContext", directory, CSharp)
        {
            Namespace = "Billing.Infrastructure.Data"
        };
    }

    private static CodeFileModel<ClassModel> CreateSubscriptionRepositoryFile(string directory)
    {
        var classModel = new ClassModel("SubscriptionRepository");

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Billing.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Billing.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Billing.Infrastructure.Data"));

        classModel.Implements.Add(new TypeModel("ISubscriptionRepository"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "context",
            Type = new TypeModel("BillingDbContext"),
            AccessModifier = AccessModifier.Private,
            ReadOnly = true
        });

        var constructor = new ConstructorModel(classModel, "SubscriptionRepository")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "context", Type = new TypeModel("BillingDbContext") }],
            Body = new ExpressionModel("this.context = context;")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Subscription") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "subscriptionId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.Subscriptions.Include(s => s.Plan).Include(s => s.Invoices).FirstOrDefaultAsync(s => s.SubscriptionId == subscriptionId, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByTenantIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Subscription") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "tenantId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.Subscriptions.Include(s => s.Plan).FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Status == SubscriptionStatus.Active, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetAllAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Subscription")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.Subscriptions.Include(s => s.Plan).ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Subscription")] },
            Params =
            [
                new ParamModel { Name = "subscription", Type = new TypeModel("Subscription") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"subscription.SubscriptionId = Guid.NewGuid();
        await context.Subscriptions.AddAsync(subscription, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return subscription;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "UpdateAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "subscription", Type = new TypeModel("Subscription") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"context.Subscriptions.Update(subscription);
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
                new ParamModel { Name = "subscriptionId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var subscription = await context.Subscriptions.FindAsync(new object[] { subscriptionId }, cancellationToken);
        if (subscription != null)
        {
            context.Subscriptions.Remove(subscription);
            await context.SaveChangesAsync(cancellationToken);
        }")
        });

        return new CodeFileModel<ClassModel>(classModel, "SubscriptionRepository", directory, CSharp)
        {
            Namespace = "Billing.Infrastructure.Repositories"
        };
    }

    private static CodeFileModel<ClassModel> CreateInvoiceGeneratorFile(string directory)
    {
        var classModel = new ClassModel("InvoiceGenerator");

        classModel.Usings.Add(new UsingModel("Billing.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Billing.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Billing.Infrastructure.Data"));

        classModel.Implements.Add(new TypeModel("IInvoiceGenerator"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "context",
            Type = new TypeModel("BillingDbContext"),
            AccessModifier = AccessModifier.Private,
            ReadOnly = true
        });

        var constructor = new ConstructorModel(classModel, "InvoiceGenerator")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "context", Type = new TypeModel("BillingDbContext") }],
            Body = new ExpressionModel("this.context = context;")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "GenerateInvoiceAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Invoice")] },
            Params =
            [
                new ParamModel { Name = "subscription", Type = new TypeModel("Subscription") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var invoice = new Invoice
        {
            InvoiceId = Guid.NewGuid(),
            SubscriptionId = subscription.SubscriptionId,
            InvoiceNumber = GenerateInvoiceNumber(),
            Amount = subscription.Plan.Price,
            TaxAmount = subscription.Plan.Price * 0.1m, // 10% tax
            TotalAmount = subscription.Plan.Price * 1.1m,
            DueDate = DateTime.UtcNow.AddDays(30)
        };

        await context.Invoices.AddAsync(invoice, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return invoice;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GenerateInvoicePdfAsync",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("byte[]")] },
            Params =
            [
                new ParamModel { Name = "invoice", Type = new TypeModel("Invoice") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"// Placeholder for PDF generation logic
        return Task.FromResult(Array.Empty<byte>());")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GenerateInvoiceNumber",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("string"),
            Params = [],
            Body = new ExpressionModel("return $\"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}\";")
        });

        return new CodeFileModel<ClassModel>(classModel, "InvoiceGenerator", directory, CSharp)
        {
            Namespace = "Billing.Infrastructure.Services"
        };
    }

    private static CodeFileModel<ClassModel> CreateInfrastructureConfigureServicesFile(string directory)
    {
        var classModel = new ClassModel("ConfigureServices")
        {
            Static = true
        };

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.Configuration"));
        classModel.Usings.Add(new UsingModel("Billing.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Billing.Infrastructure.Data"));
        classModel.Usings.Add(new UsingModel("Billing.Infrastructure.Repositories"));
        classModel.Usings.Add(new UsingModel("Billing.Infrastructure.Services"));

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddBillingInfrastructure",
            AccessModifier = AccessModifier.Public,
            Static = true,
            ReturnType = new TypeModel("IServiceCollection"),
            Params =
            [
                new ParamModel { Name = "services", Type = new TypeModel("IServiceCollection"), ExtensionMethodParam = true },
                new ParamModel { Name = "configuration", Type = new TypeModel("IConfiguration") }
            ],
            Body = new ExpressionModel(@"services.AddDbContext<BillingDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString(""BillingDb"") ??
                @""Server=.\SQLEXPRESS;Database=BillingDb;Trusted_Connection=True;TrustServerCertificate=True""));

        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IInvoiceGenerator, InvoiceGenerator>();
        return services;")
        });

        return new CodeFileModel<ClassModel>(classModel, "ConfigureServices", directory, CSharp)
        {
            Namespace = "Microsoft.Extensions.DependencyInjection"
        };
    }

    #endregion

    #region API Layer

    private static CodeFileModel<ClassModel> CreateSubscriptionsControllerFile(string directory)
    {
        var classModel = new ClassModel("SubscriptionsController");

        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.Mvc"));
        classModel.Usings.Add(new UsingModel("Billing.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("Billing.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Billing.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("ControllerBase"));

        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ApiController" });
        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Route", Template = "\"api/[controller]\"" });

        classModel.Fields.Add(new FieldModel
        {
            Name = "repository",
            Type = new TypeModel("ISubscriptionRepository"),
            AccessModifier = AccessModifier.Private,
            ReadOnly = true
        });

        var constructor = new ConstructorModel(classModel, "SubscriptionsController")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "repository", Type = new TypeModel("ISubscriptionRepository") }],
            Body = new ExpressionModel("this.repository = repository;")
        };
        classModel.Constructors.Add(constructor);

        var getByIdMethod = new MethodModel
        {
            Name = "GetById",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("SubscriptionDto")] }] },
            Params =
            [
                new ParamModel { Name = "id", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var subscription = await repository.GetByIdAsync(id, cancellationToken);
        if (subscription == null) return NotFound();
        return Ok(new SubscriptionDto
        {
            SubscriptionId = subscription.SubscriptionId,
            TenantId = subscription.TenantId,
            PlanId = subscription.PlanId,
            PlanName = subscription.Plan?.Name,
            Status = subscription.Status.ToString(),
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            CreatedAt = subscription.CreatedAt
        });")
        };
        getByIdMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"{id:guid}\"" });
        classModel.Methods.Add(getByIdMethod);

        var createMethod = new MethodModel
        {
            Name = "Create",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("SubscriptionDto")] }] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("CreateSubscriptionRequest"), Attribute = new AttributeModel() { Name = "FromBody" } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var existing = await repository.GetByTenantIdAsync(request.TenantId, cancellationToken);
        if (existing != null) return BadRequest(""Tenant already has an active subscription"");

        var subscription = new Subscription
        {
            TenantId = request.TenantId,
            PlanId = request.PlanId,
            StartDate = request.StartDate ?? DateTime.UtcNow
        };
        var created = await repository.AddAsync(subscription, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.SubscriptionId }, new SubscriptionDto
        {
            SubscriptionId = created.SubscriptionId,
            TenantId = created.TenantId,
            PlanId = created.PlanId,
            Status = created.Status.ToString(),
            StartDate = created.StartDate,
            CreatedAt = created.CreatedAt
        });")
        };
        createMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpPost" });
        classModel.Methods.Add(createMethod);

        return new CodeFileModel<ClassModel>(classModel, "SubscriptionsController", directory, CSharp)
        {
            Namespace = "Billing.Api.Controllers"
        };
    }

    private static CodeFileModel<ClassModel> CreatePaymentsControllerFile(string directory)
    {
        var classModel = new ClassModel("PaymentsController");

        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.Mvc"));
        classModel.Usings.Add(new UsingModel("Billing.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("Billing.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Billing.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Billing.Infrastructure.Data"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));

        classModel.Implements.Add(new TypeModel("ControllerBase"));

        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ApiController" });
        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Route", Template = "\"api/[controller]\"" });

        classModel.Fields.Add(new FieldModel { Name = "context", Type = new TypeModel("BillingDbContext"), AccessModifier = AccessModifier.Private, ReadOnly = true });
        classModel.Fields.Add(new FieldModel { Name = "paymentGateway", Type = new TypeModel("IPaymentGateway") { Nullable = true }, AccessModifier = AccessModifier.Private, ReadOnly = true });

        var constructor = new ConstructorModel(classModel, "PaymentsController")
        {
            AccessModifier = AccessModifier.Public,
            Params =
            [
                new ParamModel { Name = "context", Type = new TypeModel("BillingDbContext") },
                new ParamModel { Name = "paymentGateway", Type = new TypeModel("IPaymentGateway") { Nullable = true }, DefaultValue = "null" }
            ],
            Body = new ExpressionModel(@"this.context = context;
        this.paymentGateway = paymentGateway;")
        };
        classModel.Constructors.Add(constructor);

        var createMethod = new MethodModel
        {
            Name = "Create",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult")] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("CreatePaymentRequest"), Attribute = new AttributeModel() { Name = "FromBody" } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var invoice = await context.Invoices.FindAsync(new object[] { request.InvoiceId }, cancellationToken);
        if (invoice == null) return NotFound(""Invoice not found"");

        var payment = new Payment
        {
            PaymentId = Guid.NewGuid(),
            InvoiceId = request.InvoiceId,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod
        };

        if (paymentGateway != null)
        {
            var result = await paymentGateway.ProcessPaymentAsync(payment, cancellationToken);
            payment.Status = result.Success ? PaymentStatus.Completed : PaymentStatus.Failed;
            payment.TransactionId = result.TransactionId;
            payment.FailureReason = result.ErrorMessage;
            payment.ProcessedAt = DateTime.UtcNow;
        }
        else
        {
            payment.Status = PaymentStatus.Completed;
            payment.ProcessedAt = DateTime.UtcNow;
        }

        await context.Payments.AddAsync(payment, cancellationToken);

        if (payment.Status == PaymentStatus.Completed && payment.Amount >= invoice.TotalAmount)
        {
            invoice.Status = InvoiceStatus.Paid;
            invoice.PaidAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken);
        return Ok(new { payment.PaymentId, payment.Status });")
        };
        createMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpPost" });
        classModel.Methods.Add(createMethod);

        return new CodeFileModel<ClassModel>(classModel, "PaymentsController", directory, CSharp)
        {
            Namespace = "Billing.Api.Controllers"
        };
    }

    private static CodeFileModel<ClassModel> CreateInvoicesControllerFile(string directory)
    {
        var classModel = new ClassModel("InvoicesController");

        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.Mvc"));
        classModel.Usings.Add(new UsingModel("Billing.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("Billing.Infrastructure.Data"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));

        classModel.Implements.Add(new TypeModel("ControllerBase"));

        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ApiController" });
        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Route", Template = "\"api/[controller]\"" });

        classModel.Fields.Add(new FieldModel
        {
            Name = "context",
            Type = new TypeModel("BillingDbContext"),
            AccessModifier = AccessModifier.Private,
            ReadOnly = true
        });

        var constructor = new ConstructorModel(classModel, "InvoicesController")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "context", Type = new TypeModel("BillingDbContext") }],
            Body = new ExpressionModel("this.context = context;")
        };
        classModel.Constructors.Add(constructor);

        var getByIdMethod = new MethodModel
        {
            Name = "GetById",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("InvoiceDto")] }] },
            Params =
            [
                new ParamModel { Name = "id", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var invoice = await context.Invoices.FirstOrDefaultAsync(i => i.InvoiceId == id, cancellationToken);
        if (invoice == null) return NotFound();
        return Ok(new InvoiceDto
        {
            InvoiceId = invoice.InvoiceId,
            SubscriptionId = invoice.SubscriptionId,
            InvoiceNumber = invoice.InvoiceNumber,
            Amount = invoice.Amount,
            TaxAmount = invoice.TaxAmount,
            TotalAmount = invoice.TotalAmount,
            Status = invoice.Status.ToString(),
            IssuedAt = invoice.IssuedAt,
            DueDate = invoice.DueDate,
            PaidAt = invoice.PaidAt
        });")
        };
        getByIdMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"{id:guid}\"" });
        classModel.Methods.Add(getByIdMethod);

        return new CodeFileModel<ClassModel>(classModel, "InvoicesController", directory, CSharp)
        {
            Namespace = "Billing.Api.Controllers"
        };
    }

    private static FileModel CreateProgramFile(string directory)
    {
        // Keep as FileModel due to top-level statements
        return new FileModel("Program", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                var builder = WebApplication.CreateBuilder(args);

                builder.Services.AddBillingInfrastructure(builder.Configuration);
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
        // Keep as FileModel for JSON
        return new FileModel("appsettings", directory, ".json")
        {
            Body = """
                {
                  "ConnectionStrings": {
                    "BillingDb": "Server=.\\SQLEXPRESS;Database=BillingDb;Trusted_Connection=True;TrustServerCertificate=True"
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
