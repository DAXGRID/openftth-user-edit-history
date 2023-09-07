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

    public IReadOnlyDictionary<Guid, UserEditHistory> UserEditHistories => _userEditHistories;

    public UserEditHistoryProjection()
    {
        ProjectEventAsync<RouteNetworkEditOperationOccuredEvent>(ProjectAsync);
    }

    private Task ProjectAsync(IEventEnvelope eventEnvelope)
    {
        if (eventEnvelope is null)
        {
            throw new ArgumentNullException(nameof(eventEnvelope));
        }

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
        if (!_userEditHistories.ContainsKey(elementId))
        {
            _userEditHistories.Add(
                elementId,
                new UserEditHistory(
                    elementId,
                    username,
                    timestamp));
        }
        else
        {
            _userEditHistories[elementId] = _userEditHistories[elementId] with
            {
                EditedUsername = username,
                EditedTimestamp = timestamp
            };
        }
    }
}
