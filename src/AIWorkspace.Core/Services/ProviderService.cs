using AIWorkspace.Core.Models;
using Microsoft.Data.Sqlite;

namespace AIWorkspace.Core.Services;

public class ProviderService
{
    private readonly DatabaseService _db;

    public ProviderService(DatabaseService db)
    {
        _db = db;
    }

    public List<Provider> GetAll()
    {
        var providers = new List<Provider>();
        using var cmd = new SqliteCommand(
            "SELECT id, name, api_base_url, api_key, proxy_url, is_enabled, supports_vision, created_at, updated_at FROM providers ORDER BY id",
            _db.GetConnection());
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            providers.Add(new Provider
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                ApiBaseUrl = reader.GetString(2),
                ApiKey = EncryptionService.Decrypt((byte[])reader["api_key"]),
                ProxyUrl = reader.IsDBNull(4) ? null : reader.GetString(4),
                IsEnabled = reader.GetInt32(5) == 1,
                SupportsVision = reader.GetInt32(6) == 1,
                CreatedAt = reader.GetDateTime(7),
                UpdatedAt = reader.GetDateTime(8)
            });
        }
        return providers;
    }

    public Provider? GetById(int id)
    {
        using var cmd = new SqliteCommand(
            "SELECT id, name, api_base_url, api_key, proxy_url, is_enabled, supports_vision, created_at, updated_at FROM providers WHERE id = @id",
            _db.GetConnection());
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new Provider
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                ApiBaseUrl = reader.GetString(2),
                ApiKey = EncryptionService.Decrypt((byte[])reader["api_key"]),
                ProxyUrl = reader.IsDBNull(4) ? null : reader.GetString(4),
                IsEnabled = reader.GetInt32(5) == 1,
                SupportsVision = reader.GetInt32(6) == 1,
                CreatedAt = reader.GetDateTime(7),
                UpdatedAt = reader.GetDateTime(8)
            };
        }
        return null;
    }

    public int Create(Provider provider)
    {
        using var cmd = new SqliteCommand(@"
            INSERT INTO providers (name, api_base_url, api_key, proxy_url, is_enabled, supports_vision)
            VALUES (@name, @api_base_url, @api_key, @proxy_url, @is_enabled, @supports_vision);
            SELECT last_insert_rowid();",
            _db.GetConnection());
        cmd.Parameters.AddWithValue("@name", provider.Name);
        cmd.Parameters.AddWithValue("@api_base_url", provider.ApiBaseUrl);
        cmd.Parameters.AddWithValue("@api_key", EncryptionService.Encrypt(provider.ApiKey));
        cmd.Parameters.AddWithValue("@proxy_url", (object?)provider.ProxyUrl ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@is_enabled", provider.IsEnabled ? 1 : 0);
        cmd.Parameters.AddWithValue("@supports_vision", provider.SupportsVision ? 1 : 0);
        return (int)(long)cmd.ExecuteScalar()!;
    }

    public void Update(Provider provider)
    {
        using var cmd = new SqliteCommand(@"
            UPDATE providers SET
                name = @name,
                api_base_url = @api_base_url,
                api_key = @api_key,
                proxy_url = @proxy_url,
                is_enabled = @is_enabled,
                supports_vision = @supports_vision,
                updated_at = CURRENT_TIMESTAMP
            WHERE id = @id",
            _db.GetConnection());
        cmd.Parameters.AddWithValue("@id", provider.Id);
        cmd.Parameters.AddWithValue("@name", provider.Name);
        cmd.Parameters.AddWithValue("@api_base_url", provider.ApiBaseUrl);
        cmd.Parameters.AddWithValue("@api_key", EncryptionService.Encrypt(provider.ApiKey));
        cmd.Parameters.AddWithValue("@proxy_url", (object?)provider.ProxyUrl ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@is_enabled", provider.IsEnabled ? 1 : 0);
        cmd.Parameters.AddWithValue("@supports_vision", provider.SupportsVision ? 1 : 0);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var cmd = new SqliteCommand("DELETE FROM providers WHERE id = @id", _db.GetConnection());
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }
}
