using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OpenFTTH.UserEditHistory;

internal sealed class UserEditHistoryHost : BackgroundService
{
    private readonly ILogger<UserEditHistoryHost> _logger;

    public UserEditHistoryHost(ILogger<UserEditHistoryHost> logger)
    {
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Starting {BackgroundServiceName}",
            nameof(BackgroundService));

        return Task.CompletedTask;
    }
}
