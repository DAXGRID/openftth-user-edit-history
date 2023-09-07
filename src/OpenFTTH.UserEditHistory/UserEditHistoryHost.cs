using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenFTTH.EventSourcing;
using OpenFTTH.UserEditHistory.Database;

namespace OpenFTTH.UserEditHistory;

internal sealed class UserEditHistoryHost : BackgroundService
{
    private readonly ILogger<UserEditHistoryHost> _logger;
    private readonly IEventStore _eventStore;
    private readonly IUserEditHistoryDatabase _userEditHistoryDatabase;

    public UserEditHistoryHost(
        ILogger<UserEditHistoryHost> logger,
        IEventStore eventStore,
        IUserEditHistoryDatabase userEditHistoryDatabase)
    {
        _logger = logger;
        _eventStore = eventStore;
        _userEditHistoryDatabase = userEditHistoryDatabase;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Starting {BackgroundServiceName}",
            nameof(BackgroundService));

        _logger.LogInformation("Init database.");
        _userEditHistoryDatabase.InitSchema();

        _logger.LogInformation("Starting initial dehydration of projections.");

        await _eventStore
            .DehydrateProjectionsAsync(stoppingToken)
            .ConfigureAwait(false);

        var userEditHistoryProjection = _eventStore.Projections.Get<UserEditHistoryProjection>();

        foreach (var x in userEditHistoryProjection.UserEditHistories)
        {
            Console.WriteLine(JsonConvert.SerializeObject(x));
        }

        _logger.LogInformation("Finished initial dehydration.");

        using var _ = File.Create("/tmp/healthy");
        _logger.LogInformation("The service is now marked as healthy.");
    }
}
