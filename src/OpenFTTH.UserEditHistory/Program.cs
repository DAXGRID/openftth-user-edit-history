using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenFTTH.EventSourcing;

namespace OpenFTTH.UserEditHistory;

internal static class Program
{
    internal static async Task Main()
    {
        using var host = UserEditHistoryHostConfig.Configure();
        var logger =
            host.Services.GetService<ILoggerFactory>()?.CreateLogger(nameof(Program))
            ?? throw new InvalidOperationException($"Could not get {nameof(ILoggerFactory)}, it has most likely not been registered in the IOC container.");

        try
        {
            host.Services.GetService<IEventStore>()!.ScanForProjections();
            await host.StartAsync().ConfigureAwait(false);
            await host.WaitForShutdownAsync().ConfigureAwait(false);
            logger.LogInformation("User edit history host has been stopped, shutting down.");
        }
        catch (Exception ex)
        {
            logger.LogCritical("{Exception}", ex);
            throw;
        }
    }
}
