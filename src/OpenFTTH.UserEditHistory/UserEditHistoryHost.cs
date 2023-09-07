using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenFTTH.EventSourcing;
using OpenFTTH.UserEditHistory.Database;

namespace OpenFTTH.UserEditHistory;

internal sealed class UserEditHistoryHost : BackgroundService
{
    private readonly ILogger<UserEditHistoryHost> _logger;
    private readonly IEventStore _eventStore;
    private readonly IUserEditHistoryDatabase _userEditHistoryDatabase;
    private const int _catchUpTimeMs = 1000;

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

        _logger.LogInformation("Bulk upserting user edit histories.");
        _userEditHistoryDatabase.BulkUpsert(
            userEditHistoryProjection.UserEditHistories.Values.ToArray()
        );

        await userEditHistoryProjection.DehydrateFinishAsync().ConfigureAwait(false);
        _logger.LogInformation("Finished initial dehydration.");

        using var _ = File.Create("/tmp/healthy");
        _logger.LogInformation("The service is now marked as healthy.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_catchUpTimeMs, stoppingToken).ConfigureAwait(false);

            var changes = await _eventStore
                .CatchUpAsync(stoppingToken)
                .ConfigureAwait(false);

            if (changes > 0)
            {
                _logger.LogInformation(
                    "{ChangedEventCount} since last import, starting import.",
                    changes);

                var changedEntities = new List<UserEditHistory>(
                    userEditHistoryProjection.ChangedEntityIds.Count);

                foreach (var changedEntityId in userEditHistoryProjection.ChangedEntityIds)
                {
                    changedEntities.Add(
                        userEditHistoryProjection.UserEditHistories[changedEntityId]);
                }

                _userEditHistoryDatabase.Upsert(changedEntities);
                userEditHistoryProjection.ClearChangedEntityIds();
            }
            else
            {
                _logger.LogDebug("No changes since last run.");
            }
        }
    }
}
