using OpenFTTH.EventSourcing;
using OpenFTTH.Events.Core;
using OpenFTTH.Events.RouteNetwork;

namespace OpenFTTH.UserEditHistory;

internal sealed record UserEditHistory
{
    public Guid Id { get; init; }
    public string? CreatedUsername { get; init; }
    public DateTime CreatedTimestamp { get; init; }
    public DateTime? EditedTimestamp { get; init; }
    public string? EditedUsername { get; init; }

    public UserEditHistory(
        Guid id,
        string? createdUsername,
        DateTime createdTimestamp)
    {
        Id = id;
        CreatedUsername = createdUsername;
        CreatedTimestamp = createdTimestamp;
    }
}

internal sealed class UserEditHistoryProjection : ProjectionBase
{
    private readonly Dictionary<Guid, UserEditHistory> _userEditHistories = new();
    private readonly HashSet<Guid> _changedEntityIds = new();

    public IReadOnlyDictionary<Guid, UserEditHistory> UserEditHistories => _userEditHistories;

    public HashSet<Guid> ChangedEntityIds => _changedEntityIds;

    public bool InitialDehydrationFinished { get; private set; }

    public UserEditHistoryProjection()
    {
        ProjectEventAsync<RouteNetworkEditOperationOccuredEvent>(ProjectAsync);
    }

    public void ClearChangedEntityIds()
    {
        _changedEntityIds.Clear();
    }

    public Task DehydrateFinishAsync()
    {
        InitialDehydrationFinished = true;
        return Task.CompletedTask;
    }

    private Task ProjectAsync(IEventEnvelope eventEnvelope)
    {
        ArgumentNullException.ThrowIfNull(eventEnvelope);

        switch (eventEnvelope.Data)
        {
            case (RouteNetworkEditOperationOccuredEvent @event):
                Handle(@event);
                break;
            default:
                throw new ArgumentException(
                    $"Could not handle type {eventEnvelope.GetType()}");
        }

        return Task.CompletedTask;
    }

    private void Handle(RouteNetworkEditOperationOccuredEvent editOperation)
    {
        foreach (var command in editOperation.RouteNetworkCommands)
        {
            foreach (var routeNetworkEvent in command.RouteNetworkEvents)
            {
                switch (routeNetworkEvent)
                {
                    case RouteNodeAdded domainEvent:
                        UpdateModified(
                            domainEvent.NodeId,
                            editOperation.UserName,
                            editOperation.EventTimestamp);
                        break;
                    case RouteNodeInfoModified domainEvent:
                        UpdateModified(
                            domainEvent.NodeId,
                            editOperation.UserName,
                            editOperation.EventTimestamp);
                        break;
                    case RouteNodeGeometryModified domainEvent:
                        UpdateModified(
                            domainEvent.NodeId,
                            editOperation.UserName,
                            editOperation.EventTimestamp);
                        break;
                    case RouteNodeMarkedForDeletion domainEvent:
                        UpdateModified(
                            domainEvent.NodeId,
                            editOperation.UserName,
                            editOperation.EventTimestamp);
                        break;
                    case RouteSegmentAdded domainEvent:
                        UpdateModified(
                            domainEvent.SegmentId,
                            editOperation.UserName,
                            editOperation.EventTimestamp);
                        break;
                    case RouteSegmentGeometryModified domainEvent:
                        UpdateModified(
                            domainEvent.SegmentId,
                            editOperation.UserName,
                            editOperation.EventTimestamp);
                        break;
                    case RouteSegmentInfoModified domainEvent:
                        UpdateModified(
                            domainEvent.SegmentId,
                            editOperation.UserName,
                            editOperation.EventTimestamp);
                        break;
                    case RouteSegmentMarkedForDeletion domainEvent:
                        UpdateModified(
                            domainEvent.SegmentId,
                            editOperation.UserName,
                            editOperation.EventTimestamp);
                        break;
                    case ObjectInfoModified domainEvent:
                        UpdateModified(
                            domainEvent.AggregateId,
                            editOperation.UserName,
                            editOperation.EventTimestamp);
                        break;
                }
            }
        }
    }

    private void UpdateModified(
        Guid elementId,
        string? username,
        DateTime timestamp)
    {
        #pragma warning disable CA1854
        // We do not do double lookup here, so the warning is not a problem.
        if (!_userEditHistories.ContainsKey(elementId))
        #pragma warning restore CA1854
        {
            _userEditHistories.Add(
                elementId,
                new UserEditHistory(
                    elementId,
                    string.IsNullOrWhiteSpace(username) ? null : username,
                    timestamp));
        }
        else
        {
            _userEditHistories[elementId] = _userEditHistories[elementId] with
            {
                EditedUsername = string.IsNullOrWhiteSpace(username) ? null : username,
                EditedTimestamp = timestamp
            };
        }

        if (InitialDehydrationFinished)
        {
            _changedEntityIds.Add(elementId);
        }
    }
}
