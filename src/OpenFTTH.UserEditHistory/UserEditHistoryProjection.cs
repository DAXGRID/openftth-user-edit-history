using OpenFTTH.EventSourcing;
using OpenFTTH.Events.Core;
using OpenFTTH.Events.RouteNetwork;

namespace OpenFTTH.UserEditHistory;

internal sealed class UserEditHistoryProjection : ProjectionBase
{
    public UserEditHistoryProjection()
    {
        ProjectEventAsync<RouteNetworkEditOperationOccuredEvent>(ProjectAsync);
    }

    private static Task ProjectAsync(IEventEnvelope eventEnvelope)
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

    private static void Handle(RouteNetworkEditOperationOccuredEvent editOperation)
    {
        foreach (var command in editOperation.RouteNetworkCommands)
        {
            foreach (var routeNetworkEvent in command.RouteNetworkEvents)
            {
                switch (routeNetworkEvent)
                {
                    case RouteNodeAdded domainEvent:
                        break;
                    case RouteNodeInfoModified domainEvent:
                        break;
                    case RouteNodeGeometryModified domainEvent:
                        break;
                    case RouteNodeMarkedForDeletion domainEvent:
                        break;

                    case RouteSegmentAdded domainEvent:
                        break;
                    case RouteSegmentGeometryModified domainEvent:
                        break;
                    case RouteSegmentInfoModified domainEvent:
                        break;
                    case RouteSegmentMarkedForDeletion domainEvent:
                        break;

                    case ObjectInfoModified domainEvent:
                        break;
                }
            }
        }
    }
}
