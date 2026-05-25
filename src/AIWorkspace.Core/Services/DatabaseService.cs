using Microsoft.Data.Sqlite;

namespace AIWorkspace.Core.Services;

public class DatabaseService : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly string _dbPath;

    public DatabaseService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var dbDir = Path.Combine(appData, "AIWorkspace");
        Directory.CreateDirectory(dbDir);
        _dbPath = Path.Combine(dbDir, "aiworkspace.db");
        _connection = new SqliteConnection($"Data Source={_dbPath}");
        _connection.Open();
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        var sql = @"
CREATE TABLE IF NOT EXISTS providers (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    api_base_url TEXT NOT NULL,
    api_key BLOB NOT NULL,
    proxy_url TEXT,
    is_enabled INTEGER DEFAULT 1,
    supports_vision INTEGER DEFAULT 0,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS models (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    provider_id INTEGER NOT NULL REFERENCES providers(id) ON DELETE CASCADE,
    model_id TEXT NOT NULL,
    display_name TEXT,
    supports_vision INTEGER DEFAULT 0,
    is_default INTEGER DEFAULT 0,
    UNIQUE(provider_id, model_id)
);

CREATE TABLE IF NOT EXISTS conversations (
    id TEXT PRIMARY KEY,
    title TEXT,
    model_id TEXT,
    provider_id INTEGER REFERENCES providers(id),
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS messages (
    id TEXT PRIMARY KEY,
    conversation_id TEXT NOT NULL REFERENCES conversations(id) ON DELETE CASCADE,
    role TEXT NOT NULL CHECK(role IN ('user','assistant','system')),
    content TEXT NOT NULL,
    image_data TEXT,
    tokens_used INTEGER,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS attachments (
    id TEXT PRIMARY KEY,
    message_id TEXT NOT NULL REFERENCES messages(id) ON DELETE CASCADE,
    file_name TEXT NOT NULL,
    file_size INTEGER,
    content_text TEXT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS settings (
    key TEXT PRIMARY KEY,
    value TEXT
);

INSERT OR IGNORE INTO settings (key, value) VALUES
('theme', 'dark'),
('window_width', '1280'),
('window_height', '800'),
('shortcut_screenshot', 'Ctrl+Shift+A'),
('shortcut_activate', 'Ctrl+`'),
('max_file_size_mb', '10'),
('max_image_long_edge', '1536');
";
        using var cmd = new SqliteCommand(sql, _connection);
        cmd.ExecuteNonQuery();
    }

    public SqliteConnection GetConnection() => _connection;

    public void Dispose()
    {
        _connection.Dispose();
    }
}
