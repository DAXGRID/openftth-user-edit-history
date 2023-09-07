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
  id uuid PRIMARY KEY,
  created_username varchar(255) NULL,
  created timestamptz NOT NULL,
  edited_username varchar(255) NULL,
  edited timestamptz NULL)
 ";

        using var connection = new NpgsqlConnection(_connectionString);
        using var command = new NpgsqlCommand(schemaSql, connection);

        connection.Open();

        command.ExecuteNonQuery();
    }
}
