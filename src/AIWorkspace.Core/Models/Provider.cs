namespace AIWorkspace.Core.Models;

public class Provider
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string ApiBaseUrl { get; set; } = "";
    public string ApiKey { get; set; } = "";  // 明文（仅在内存中使用）
    public string? ProxyUrl { get; set; }
    public bool IsEnabled { get; set; } = true;
    public bool SupportsVision { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class ModelInfo
{
    public string ModelId { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public bool SupportsVision { get; set; } = false;
    public bool IsDefault { get; set; } = false;
}

public class ProviderConfiguration
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string ApiBaseUrl { get; set; } = "";
    public byte[] EncryptedApiKey { get; set; } = Array.Empty<byte>();
    public string? ProxyUrl { get; set; }
    public bool IsEnabled { get; set; } = true;
    public bool SupportsVision { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
