using Endpoint.Core.Messages;
using Endpoint.Core.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Endpoint.Core.MessageHandlers;

public class ServiceFileCreatedHandler : INotificationHandler<ServiceFileCreated>
{
    private readonly IDependencyInjectionService _dependencyInjectionService;
    private readonly ILogger<ServiceFileCreatedHandler> _logger;
    public ServiceFileCreatedHandler(
        IDependencyInjectionService dependencyInjectionService,
        ILogger<ServiceFileCreatedHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dependencyInjectionService = dependencyInjectionService ?? throw new ArgumentNullException(nameof(dependencyInjectionService));
    }

    public async Task Handle(ServiceFileCreated notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handled: {0}", nameof(ServiceFileCreatedHandler));

        _dependencyInjectionService.Add(notification.InterfaceName,notification.ClassName,notification.Directory);
    }
}
