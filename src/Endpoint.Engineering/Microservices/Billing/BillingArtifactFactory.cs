// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.DotNet.Artifacts.Projects;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.Billing;

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
        project.Files.Add(new FileModel("Subscription", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Billing.Core.Entities;

                public class Subscription
                {
                    public Guid SubscriptionId { get; set; }
                    public Guid TenantId { get; set; }
                    public Guid PlanId { get; set; }
                    public Plan Plan { get; set; } = null!;
                    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
                    public DateTime StartDate { get; set; } = DateTime.UtcNow;
                    public DateTime? EndDate { get; set; }
                    public DateTime? CancelledAt { get; set; }
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
                }

                public enum SubscriptionStatus { Active, Cancelled, Expired, Suspended }
                """
        });

        project.Files.Add(new FileModel("Invoice", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Billing.Core.Entities;

                public class Invoice
                {
                    public Guid InvoiceId { get; set; }
                    public Guid SubscriptionId { get; set; }
                    public Subscription Subscription { get; set; } = null!;
                    public required string InvoiceNumber { get; set; }
                    public decimal Amount { get; set; }
                    public decimal TaxAmount { get; set; }
                    public decimal TotalAmount { get; set; }
                    public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;
                    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
                    public DateTime DueDate { get; set; }
                    public DateTime? PaidAt { get; set; }
                    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
                }

                public enum InvoiceStatus { Pending, Paid, Overdue, Cancelled }
                """
        });

        project.Files.Add(new FileModel("Payment", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Billing.Core.Entities;

                public class Payment
                {
                    public Guid PaymentId { get; set; }
                    public Guid InvoiceId { get; set; }
                    public Invoice Invoice { get; set; } = null!;
                    public decimal Amount { get; set; }
                    public required string PaymentMethod { get; set; }
                    public string? TransactionId { get; set; }
                    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? ProcessedAt { get; set; }
                    public string? FailureReason { get; set; }
                }

                public enum PaymentStatus { Pending, Completed, Failed, Refunded }
                """
        });

        project.Files.Add(new FileModel("Plan", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Billing.Core.Entities;

                public class Plan
                {
                    public Guid PlanId { get; set; }
                    public required string Name { get; set; }
                    public string? Description { get; set; }
                    public decimal Price { get; set; }
                    public BillingInterval Interval { get; set; } = BillingInterval.Monthly;
                    public bool IsActive { get; set; } = true;
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
                }

                public enum BillingInterval { Monthly, Quarterly, Yearly }
                """
        });

        // Interfaces
        project.Files.Add(new FileModel("ISubscriptionRepository", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Billing.Core.Entities;

                namespace Billing.Core.Interfaces;

                public interface ISubscriptionRepository
                {
                    Task<Subscription?> GetByIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
                    Task<Subscription?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
                    Task<IEnumerable<Subscription>> GetAllAsync(CancellationToken cancellationToken = default);
                    Task<Subscription> AddAsync(Subscription subscription, CancellationToken cancellationToken = default);
                    Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default);
                    Task DeleteAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
                }
                """
        });

        project.Files.Add(new FileModel("IPaymentGateway", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Billing.Core.Entities;

                namespace Billing.Core.Interfaces;

                public interface IPaymentGateway
                {
                    Task<PaymentResult> ProcessPaymentAsync(Payment payment, CancellationToken cancellationToken = default);
                    Task<PaymentResult> RefundPaymentAsync(string transactionId, decimal amount, CancellationToken cancellationToken = default);
                    Task<bool> ValidatePaymentMethodAsync(string paymentMethod, CancellationToken cancellationToken = default);
                }

                public class PaymentResult
                {
                    public bool Success { get; init; }
                    public string? TransactionId { get; init; }
                    public string? ErrorMessage { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("IInvoiceGenerator", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Billing.Core.Entities;

                namespace Billing.Core.Interfaces;

                public interface IInvoiceGenerator
                {
                    Task<Invoice> GenerateInvoiceAsync(Subscription subscription, CancellationToken cancellationToken = default);
                    Task<byte[]> GenerateInvoicePdfAsync(Invoice invoice, CancellationToken cancellationToken = default);
                    string GenerateInvoiceNumber();
                }
                """
        });

        // Events
        project.Files.Add(new FileModel("SubscriptionCreatedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Billing.Core.Events;

                public sealed class SubscriptionCreatedEvent
                {
                    public Guid SubscriptionId { get; init; }
                    public Guid TenantId { get; init; }
                    public Guid PlanId { get; init; }
                    public DateTime StartDate { get; init; }
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        project.Files.Add(new FileModel("PaymentProcessedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Billing.Core.Events;

                public sealed class PaymentProcessedEvent
                {
                    public Guid PaymentId { get; init; }
                    public Guid InvoiceId { get; init; }
                    public decimal Amount { get; init; }
                    public required string TransactionId { get; init; }
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        project.Files.Add(new FileModel("PaymentFailedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Billing.Core.Events;

                public sealed class PaymentFailedEvent
                {
                    public Guid PaymentId { get; init; }
                    public Guid InvoiceId { get; init; }
                    public decimal Amount { get; init; }
                    public required string Reason { get; init; }
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        project.Files.Add(new FileModel("InvoiceGeneratedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Billing.Core.Events;

                public sealed class InvoiceGeneratedEvent
                {
                    public Guid InvoiceId { get; init; }
                    public Guid SubscriptionId { get; init; }
                    public required string InvoiceNumber { get; init; }
                    public decimal TotalAmount { get; init; }
                    public DateTime DueDate { get; init; }
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        // DTOs
        project.Files.Add(new FileModel("SubscriptionDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Billing.Core.DTOs;

                public sealed class SubscriptionDto
                {
                    public Guid SubscriptionId { get; init; }
                    public Guid TenantId { get; init; }
                    public Guid PlanId { get; init; }
                    public string? PlanName { get; init; }
                    public string Status { get; init; } = "Active";
                    public DateTime StartDate { get; init; }
                    public DateTime? EndDate { get; init; }
                    public DateTime CreatedAt { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("CreateSubscriptionRequest", dtosDir, CSharp)
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
        });

        project.Files.Add(new FileModel("InvoiceDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Billing.Core.DTOs;

                public sealed class InvoiceDto
                {
                    public Guid InvoiceId { get; init; }
                    public Guid SubscriptionId { get; init; }
                    public required string InvoiceNumber { get; init; }
                    public decimal Amount { get; init; }
                    public decimal TaxAmount { get; init; }
                    public decimal TotalAmount { get; init; }
                    public string Status { get; init; } = "Pending";
                    public DateTime IssuedAt { get; init; }
                    public DateTime DueDate { get; init; }
                    public DateTime? PaidAt { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("CreatePaymentRequest", dtosDir, CSharp)
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
        });
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Billing.Infrastructure files");
        var dataDir = Path.Combine(project.Directory, "Data");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");

        project.Files.Add(new FileModel("BillingDbContext", dataDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Billing.Core.Entities;

                namespace Billing.Infrastructure.Data;

                public class BillingDbContext : DbContext
                {
                    public BillingDbContext(DbContextOptions<BillingDbContext> options) : base(options) { }

                    public DbSet<Subscription> Subscriptions => Set<Subscription>();
                    public DbSet<Invoice> Invoices => Set<Invoice>();
                    public DbSet<Payment> Payments => Set<Payment>();
                    public DbSet<Plan> Plans => Set<Plan>();

                    protected override void OnModelCreating(ModelBuilder modelBuilder)
                    {
                        modelBuilder.Entity<Plan>(entity =>
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
                        });
                    }
                }
                """
        });

        project.Files.Add(new FileModel("SubscriptionRepository", repositoriesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Billing.Core.Entities;
                using Billing.Core.Interfaces;
                using Billing.Infrastructure.Data;

                namespace Billing.Infrastructure.Repositories;

                public class SubscriptionRepository : ISubscriptionRepository
                {
                    private readonly BillingDbContext context;

                    public SubscriptionRepository(BillingDbContext context)
                    {
                        this.context = context;
                    }

                    public async Task<Subscription?> GetByIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
                        => await context.Subscriptions.Include(s => s.Plan).Include(s => s.Invoices).FirstOrDefaultAsync(s => s.SubscriptionId == subscriptionId, cancellationToken);

                    public async Task<Subscription?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
                        => await context.Subscriptions.Include(s => s.Plan).FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Status == SubscriptionStatus.Active, cancellationToken);

                    public async Task<IEnumerable<Subscription>> GetAllAsync(CancellationToken cancellationToken = default)
                        => await context.Subscriptions.Include(s => s.Plan).ToListAsync(cancellationToken);

                    public async Task<Subscription> AddAsync(Subscription subscription, CancellationToken cancellationToken = default)
                    {
                        subscription.SubscriptionId = Guid.NewGuid();
                        await context.Subscriptions.AddAsync(subscription, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                        return subscription;
                    }

                    public async Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default)
                    {
                        context.Subscriptions.Update(subscription);
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    public async Task DeleteAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
                    {
                        var subscription = await context.Subscriptions.FindAsync(new object[] { subscriptionId }, cancellationToken);
                        if (subscription != null)
                        {
                            context.Subscriptions.Remove(subscription);
                            await context.SaveChangesAsync(cancellationToken);
                        }
                    }
                }
                """
        });

        project.Files.Add(new FileModel("InvoiceGenerator", servicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Billing.Core.Entities;
                using Billing.Core.Interfaces;
                using Billing.Infrastructure.Data;

                namespace Billing.Infrastructure.Services;

                public class InvoiceGenerator : IInvoiceGenerator
                {
                    private readonly BillingDbContext context;

                    public InvoiceGenerator(BillingDbContext context)
                    {
                        this.context = context;
                    }

                    public async Task<Invoice> GenerateInvoiceAsync(Subscription subscription, CancellationToken cancellationToken = default)
                    {
                        var invoice = new Invoice
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
                        return invoice;
                    }

                    public Task<byte[]> GenerateInvoicePdfAsync(Invoice invoice, CancellationToken cancellationToken = default)
                    {
                        // Placeholder for PDF generation logic
                        return Task.FromResult(Array.Empty<byte>());
                    }

                    public string GenerateInvoiceNumber()
                    {
                        return $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
                    }
                }
                """
        });

        project.Files.Add(new FileModel("ConfigureServices", project.Directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Microsoft.Extensions.Configuration;
                using Billing.Core.Interfaces;
                using Billing.Infrastructure.Data;
                using Billing.Infrastructure.Repositories;
                using Billing.Infrastructure.Services;

                namespace Microsoft.Extensions.DependencyInjection;

                public static class ConfigureServices
                {
                    public static IServiceCollection AddBillingInfrastructure(this IServiceCollection services, IConfiguration configuration)
                    {
                        services.AddDbContext<BillingDbContext>(options =>
                            options.UseSqlServer(configuration.GetConnectionString("BillingDb") ??
                                @"Server=.\SQLEXPRESS;Database=BillingDb;Trusted_Connection=True;TrustServerCertificate=True"));

                        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
                        services.AddScoped<IInvoiceGenerator, InvoiceGenerator>();
                        return services;
                    }
                }
                """
        });
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Billing.Api files");
        var controllersDir = Path.Combine(project.Directory, "Controllers");

        project.Files.Add(new FileModel("SubscriptionsController", controllersDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.AspNetCore.Mvc;
                using Billing.Core.DTOs;
                using Billing.Core.Entities;
                using Billing.Core.Interfaces;

                namespace Billing.Api.Controllers;

                [ApiController]
                [Route("api/[controller]")]
                public class SubscriptionsController : ControllerBase
                {
                    private readonly ISubscriptionRepository repository;

                    public SubscriptionsController(ISubscriptionRepository repository)
                    {
                        this.repository = repository;
                    }

                    [HttpGet("{id:guid}")]
                    public async Task<ActionResult<SubscriptionDto>> GetById(Guid id, CancellationToken cancellationToken)
                    {
                        var subscription = await repository.GetByIdAsync(id, cancellationToken);
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
                        });
                    }

                    [HttpPost]
                    public async Task<ActionResult<SubscriptionDto>> Create([FromBody] CreateSubscriptionRequest request, CancellationToken cancellationToken)
                    {
                        var existing = await repository.GetByTenantIdAsync(request.TenantId, cancellationToken);
                        if (existing != null) return BadRequest("Tenant already has an active subscription");

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
                        });
                    }
                }
                """
        });

        project.Files.Add(new FileModel("PaymentsController", controllersDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.AspNetCore.Mvc;
                using Billing.Core.DTOs;
                using Billing.Core.Entities;
                using Billing.Core.Interfaces;
                using Billing.Infrastructure.Data;
                using Microsoft.EntityFrameworkCore;

                namespace Billing.Api.Controllers;

                [ApiController]
                [Route("api/[controller]")]
                public class PaymentsController : ControllerBase
                {
                    private readonly BillingDbContext context;
                    private readonly IPaymentGateway? paymentGateway;

                    public PaymentsController(BillingDbContext context, IPaymentGateway? paymentGateway = null)
                    {
                        this.context = context;
                        this.paymentGateway = paymentGateway;
                    }

                    [HttpPost]
                    public async Task<ActionResult> Create([FromBody] CreatePaymentRequest request, CancellationToken cancellationToken)
                    {
                        var invoice = await context.Invoices.FindAsync(new object[] { request.InvoiceId }, cancellationToken);
                        if (invoice == null) return NotFound("Invoice not found");

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
                        return Ok(new { payment.PaymentId, payment.Status });
                    }
                }
                """
        });

        project.Files.Add(new FileModel("InvoicesController", controllersDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.AspNetCore.Mvc;
                using Billing.Core.DTOs;
                using Billing.Infrastructure.Data;
                using Microsoft.EntityFrameworkCore;

                namespace Billing.Api.Controllers;

                [ApiController]
                [Route("api/[controller]")]
                public class InvoicesController : ControllerBase
                {
                    private readonly BillingDbContext context;

                    public InvoicesController(BillingDbContext context)
                    {
                        this.context = context;
                    }

                    [HttpGet("{id:guid}")]
                    public async Task<ActionResult<InvoiceDto>> GetById(Guid id, CancellationToken cancellationToken)
                    {
                        var invoice = await context.Invoices.FirstOrDefaultAsync(i => i.InvoiceId == id, cancellationToken);
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
                        });
                    }
                }
                """
        });

        project.Files.Add(new FileModel("Program", project.Directory, CSharp)
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
        });

        project.Files.Add(new FileModel("appsettings", project.Directory, ".json")
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
        });
    }
}
