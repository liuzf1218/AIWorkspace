namespace AIWorkspace.Core.Models;

public class ChatRequest
{
    public string ModelId { get; set; } = "";
    public List<ChatMessage> Messages { get; set; } = new();
    public double Temperature { get; set; } = 0.7;
    public int? MaxTokens { get; set; }
}

public class ChatMessage
{
    public string Role { get; set; } = "user";
    public string Content { get; set; } = "";
    public List<ChatImageContent>? Images { get; set; }
}

public class ChatImageContent
{
    public string Base64Data { get; set; } = "";
    public string MimeType { get; set; } = "image/jpeg";
}

public class ChatChunk
{
    public string? ContentDelta { get; set; }
    public string? FinishReason { get; set; }
    public int? PromptTokens { get; set; }
    public int? CompletionTokens { get; set; }
}

public class AppSettings
{
    public string Theme { get; set; } = "dark";
    public string? DefaultProvider { get; set; }
    public string? DefaultModel { get; set; }
    public int WindowWidth { get; set; } = 1280;
    public int WindowHeight { get; set; } = 800;
    public string ShortcutScreenshot { get; set; } = "Ctrl+Shift+A";
    public string ShortcutActivate { get; set; } = "Ctrl+`";
    public int MaxFileSizeMb { get; set; } = 10;
    public int MaxImageLongEdge { get; set; } = 1536;
}
