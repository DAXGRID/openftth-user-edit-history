using Npgsql;

namespace OpenFTTH.UserEditHistory.Database;

internal sealed class UserEditHistoryPostgres : IUserEditHistoryDatabase
{
    private readonly string _connectionString;

    public UserEditHistoryPostgres(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void InitSchema()
    {
        const string schemaSql = @"
CREATE TABLE IF NOT EXISTS route_network.user_edit_history (
  route_network_element_id uuid PRIMARY KEY,
  created_username varchar(255) NULL,
  created_timestamp timestamptz NOT NULL,
  edited_username varchar(255) NULL,
  edited_timestamp timestamptz NULL)
 ";

        using var connection = new NpgsqlConnection(_connectionString);
        using var command = new NpgsqlCommand(schemaSql, connection);

        connection.Open();

        command.ExecuteNonQuery();
    }

    public void BulkUpsert(IReadOnlyCollection<UserEditHistory> userEditHistories)
    {
        using var connection = new NpgsqlConnection(_connectionString);

        const string tempTableSql = @"
CREATE TEMP TABLE user_edit_history_tmp (
  route_network_element_id uuid PRIMARY KEY,
  created_username varchar(255) NULL,
  created_timestamp timestamptz NOT NULL,
  edited_username varchar(255) NULL,
  edited_timestamp timestamptz NULL)
";

        connection.Open();
        // Create a temporary table to hold the data
        using (NpgsqlCommand createTempTableCmd = new NpgsqlCommand(tempTableSql, connection))
        {
            createTempTableCmd.ExecuteNonQuery();
        }

        const string binaryImportSql = @"
COPY user_edit_history_tmp (
  route_network_element_id,
  created_username,
  created_timestamp,
  edited_username,
  edited_timestamp
) FROM STDIN (FORMAT BINARY)";

        // Use the COPY command to insert data into the temporary table
        using var writer = connection.BeginBinaryImport(binaryImportSql);
        foreach (var userEditHistory in userEditHistories)
        {
            writer.WriteRow(
                userEditHistory.Id,
                userEditHistory.CreatedUsername is null ? DBNull.Value : userEditHistory.CreatedUsername,
                userEditHistory.CreatedTimestamp,
                userEditHistory.EditedUsername is null ? DBNull.Value : userEditHistory.EditedUsername,
                userEditHistory.EditedTimestamp is null ? DBNull.Value : userEditHistory.EditedTimestamp
            );
        }

        writer.Complete();
        writer.Close();

        // Perform upsert from the temporary table to the target table
        const string updatePrimaryTableSql = @"
INSERT INTO route_network.user_edit_history (
  route_network_element_id,
  created_username,
  created_timestamp,
  edited_username,
  edited_timestamp)
SELECT * FROM user_edit_history_tmp
ON CONFLICT (route_network_element_id) DO UPDATE
SET
  edited_username = EXCLUDED.edited_username,
  edited_timestamp = EXCLUDED.edited_timestamp";

        using (var upsertCmd = new NpgsqlCommand(updatePrimaryTableSql, connection))
        {
            upsertCmd.ExecuteNonQuery();
        }
    }

    public void Upsert(IReadOnlyCollection<UserEditHistory> userEditHistories)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        const string upsertCommand = @"
    INSERT INTO route_network.user_edit_history (
      route_network_element_id,
      created_username,
      created_timestamp,
      edited_username,
      edited_timestamp)
    VALUES (
      @route_network_element_id,
      @created_username,
      @created_timestamp,
      @edited_username,
      @edited_timestamp)
    ON CONFLICT (route_network_element_id) DO UPDATE
    SET
      edited_username = @edited_username,
      edited_timestamp = @edited_timestamp;
    ";

        foreach (var userEditHistory in userEditHistories)
        {
            using var command = new NpgsqlCommand(upsertCommand, connection);

            command.Parameters.AddWithValue(
                "@route_network_element_id",
                userEditHistory.Id);

            command.Parameters.AddWithValue(
                "@created_username",
                userEditHistory.CreatedUsername is null
                ? DBNull.Value
                : userEditHistory.CreatedUsername);

            command.Parameters.AddWithValue(
                "@created_timestamp",
                userEditHistory.CreatedTimestamp
            );

            command.Parameters.AddWithValue(
                "@edited_username",
                userEditHistory.EditedUsername is null
                ? DBNull.Value
                : userEditHistory.EditedUsername
            );

            command.Parameters.AddWithValue(
                "@edited_timestamp",
                userEditHistory.EditedTimestamp is null
                ? DBNull.Value
                : userEditHistory.EditedTimestamp
            );

            command.ExecuteNonQuery();
        }
    }
}
