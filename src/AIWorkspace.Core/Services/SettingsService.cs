using Microsoft.Data.Sqlite;

namespace AIWorkspace.Core.Services;

public class SettingsService
{
    private readonly DatabaseService _db;

    public SettingsService(DatabaseService db)
    {
        _db = db;
    }

    public string? Get(string key)
    {
        using var cmd = new SqliteCommand("SELECT value FROM settings WHERE key = @key", _db.GetConnection());
        cmd.Parameters.AddWithValue("@key", key);
        var result = cmd.ExecuteScalar();
        return result == DBNull.Value ? null : (string?)result;
    }

    public void Set(string key, string value)
    {
        using var cmd = new SqliteCommand(
            "INSERT INTO settings (key, value) VALUES (@key, @value) ON CONFLICT(key) DO UPDATE SET value = excluded.value",
            _db.GetConnection());
        cmd.Parameters.AddWithValue("@key", key);
        cmd.Parameters.AddWithValue("@value", value);
        cmd.ExecuteNonQuery();
    }

    public Dictionary<string, string> GetAll()
    {
        var settings = new Dictionary<string, string>();
        using var cmd = new SqliteCommand("SELECT key, value FROM settings", _db.GetConnection());
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            settings[reader.GetString(0)] = reader.GetString(1);
        }
        return settings;
    }
}
