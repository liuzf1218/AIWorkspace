namespace AIWorkspace.Core.Models;

public class Conversation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = "New Chat";
    public string? ModelId { get; set; }
    public int? ProviderId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class Message
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ConversationId { get; set; } = "";
    public string Role { get; set; } = "user";  // user / assistant / system
    public string Content { get; set; } = "";
    public string? ImageData { get; set; }  // Base64
    public int? TokensUsed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Attachment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string MessageId { get; set; } = "";
    public string FileName { get; set; } = "";
    public long FileSize { get; set; }
    public string? ContentText { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
