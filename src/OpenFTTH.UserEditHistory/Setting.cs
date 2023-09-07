using System.Text.Json.Serialization;

namespace OpenFTTH.UserEditHistory;

internal sealed record Setting
{
    [JsonPropertyName("eventStoreConnectionString")]
    public string EventStoreConnectionString { get; init; }

    [JsonPropertyName("connectionString")]
    public string ConnectionString { get; init; }

    [JsonConstructor]
    public Setting(
        string eventStoreConnectionString,
        string connectionString)
    {
        EventStoreConnectionString = eventStoreConnectionString;
        ConnectionString = connectionString;
    }
}
