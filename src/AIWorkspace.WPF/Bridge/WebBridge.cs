using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Threading;
using AIWorkspace.Core.Models;
using AIWorkspace.Core.Providers;
using AIWorkspace.Core.Services;
using AIWorkspace.Core.Utils;
using AIWorkspace.WPF.Native;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Message = AIWorkspace.Core.Models.Message;

namespace AIWorkspace.WPF.Bridge;

public class WebBridge
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly WebView2 _webView;
    private readonly DatabaseService _db;
    private readonly ProviderService _providerService;
    private readonly ChatService _chatService;
    private readonly SettingsService _settingsService;
    private readonly PromptEngine _promptEngine;
    private readonly ScreenshotService _screenshotService;
    private CancellationTokenSource? _currentChatCts;

    private readonly System.Windows.Threading.Dispatcher _uiDispatcher;

    public event Action? ShortcutSettingsChanged;
    public event Action<string>? ThemeChanged;

    public WebBridge(WebView2 webView)
    {
        _webView = webView;
        _uiDispatcher = webView.Dispatcher;
        _db = new DatabaseService();
        _providerService = new ProviderService(_db);
        _chatService = new ChatService(_db);
        _settingsService = new SettingsService(_db);
        _promptEngine = new PromptEngine();
        _screenshotService = new ScreenshotService();
        _screenshotService.ScreenshotCaptured += OnScreenshotCaptured;
    }

    public void Register()
    {
        _webView.WebMessageReceived += OnWebMessageReceived;
    }

    public void SendScreenshotRequest()
    {
        CaptureScreenshotWithWindowHidden();
    }

    private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            var json = e.WebMessageAsJson;
            var msg = JsonSerializer.Deserialize<JsonObject>(json);
            if (msg == null) return;

            var id = msg["id"]?.GetValue<string>() ?? Guid.NewGuid().ToString();
            var channel = msg["channel"]?.GetValue<string>() ?? "";
            var payload = msg["payload"];

            _ = Task.Run(async () =>
            {
                try
                {
                    var response = await HandleMessage(channel, payload);
                    SendToWeb(channel, id, response);
                }
                catch (Exception ex)
                {
                    SendErrorToWeb(channel, id, ex.Message);
                }
            });
        }
        catch (Exception ex)
        {
            FileLogger.Error($"Bridge error: {ex}");
        }
    }

    private async Task<JsonNode?> HandleMessage(string channel, JsonNode? payload)
    {
        return channel switch
        {
            "provider:list" => await HandleProviderList(),
            "provider:save" => await HandleProviderSave(payload),
            "provider:delete" => await HandleProviderDelete(payload),
            "provider:models" => await HandleProviderModels(payload),
            "provider:validate" => await HandleProviderValidate(payload),
            "conversation:list" => await HandleConversationList(),
            "conversation:create" => await HandleConversationCreate(payload),
            "conversation:delete" => await HandleConversationDelete(payload),
            "conversation:load" => await HandleConversationLoad(payload),
            "conversation:export" => await HandleConversationExport(payload),
            "chat:send" => await HandleChatSend(payload),
            "chat:abort" => HandleChatAbort(),
            "file:read" => await HandleFileRead(payload),
            "file:open" => await HandleFileOpen(),
            "screenshot:capture" => HandleScreenshotCapture(),
            "settings:get" => await HandleSettingsGet(payload),
            "settings:set" => await HandleSettingsSet(payload),
            "settings:all" => await HandleSettingsAll(),
            _ => null
        };
    }

    private async Task<JsonNode?> HandleProviderList()
    {
        var providers = _providerService.GetAll();
        return JsonSerializer.SerializeToNode(providers, _jsonOptions);
    }

    private async Task<JsonNode?> HandleProviderSave(JsonNode? payload)
    {
        var provider = payload?.Deserialize<Provider>(_jsonOptions);
        if (provider == null) return null;

        if (provider.Id == 0)
        {
            var id = _providerService.Create(provider);
            provider.Id = id;
        }
        else
        {
            _providerService.Update(provider);
        }
        return JsonSerializer.SerializeToNode(provider, _jsonOptions);
    }

    private async Task<JsonNode?> HandleProviderDelete(JsonNode? payload)
    {
        var id = payload?["id"]?.GetValue<int>() ?? 0;
        if (id > 0) _providerService.Delete(id);
        return JsonSerializer.SerializeToNode(new { success = true }, _jsonOptions);
    }

    private async Task<JsonNode?> HandleProviderModels(JsonNode? payload)
    {
        var id = payload?["id"]?.GetValue<int>() ?? 0;
        var provider = _providerService.GetById(id);
        if (provider == null) return JsonSerializer.SerializeToNode(new List<ModelInfo>(), _jsonOptions);

        var p = new OpenAICompatibleProvider(provider.Name, provider.ApiBaseUrl, provider.ApiKey,
            provider.ProxyUrl, provider.SupportsVision);
        var models = await p.GetModelsAsync();
        return JsonSerializer.SerializeToNode(models, _jsonOptions);
    }

    private async Task<JsonNode?> HandleProviderValidate(JsonNode? payload)
    {
        var id = payload?["id"]?.GetValue<int>() ?? 0;
        var provider = _providerService.GetById(id);
        if (provider == null) return JsonSerializer.SerializeToNode(new { valid = false }, _jsonOptions);

        var p = new OpenAICompatibleProvider(provider.Name, provider.ApiBaseUrl, provider.ApiKey,
            provider.ProxyUrl, provider.SupportsVision);
        var valid = await p.ValidateAsync();
        return JsonSerializer.SerializeToNode(new { valid }, _jsonOptions);
    }

    private async Task<JsonNode?> HandleConversationList()
    {
        var conversations = _chatService.GetConversations();
        return JsonSerializer.SerializeToNode(conversations, _jsonOptions);
    }

    private async Task<JsonNode?> HandleConversationCreate(JsonNode? payload)
    {
        var title = payload?["title"]?.GetValue<string>() ?? "New Chat";
        var modelId = payload?["modelId"]?.GetValue<string>();
        var providerId = payload?["providerId"]?.GetValue<int?>();
        var conv = _chatService.CreateConversation(title, modelId, providerId);
        return JsonSerializer.SerializeToNode(conv, _jsonOptions);
    }

    private async Task<JsonNode?> HandleConversationDelete(JsonNode? payload)
    {
        var id = payload?["id"]?.GetValue<string>() ?? "";
        if (!string.IsNullOrEmpty(id)) _chatService.DeleteConversation(id);
        return JsonSerializer.SerializeToNode(new { success = true }, _jsonOptions);
    }

    private async Task<JsonNode?> HandleConversationLoad(JsonNode? payload)
    {
        var id = payload?["id"]?.GetValue<string>() ?? "";
        var messages = _chatService.GetMessages(id);
        return JsonSerializer.SerializeToNode(messages, _jsonOptions);
    }

    private async Task<JsonNode?> HandleConversationExport(JsonNode? payload)
    {
        var conversationId = payload?["conversationId"]?.GetValue<string>() ?? "";
        var format = payload?["format"]?.GetValue<string>() ?? "markdown";

        if (string.IsNullOrEmpty(conversationId))
            throw new Exception("No conversation selected");

        var messages = _chatService.GetMessages(conversationId);
        if (messages.Count == 0)
            throw new Exception("Conversation is empty");

        var conversation = _chatService.GetConversations().FirstOrDefault(c => c.Id == conversationId);
        var title = conversation?.Title ?? "Chat";

        // Sanitize filename
        var safeTitle = string.Join("_", title.Split(Path.GetInvalidFileNameChars())).Trim();
        if (string.IsNullOrEmpty(safeTitle)) safeTitle = "Chat";
        var defaultFileName = $"{safeTitle}_{DateTime.Now:yyyyMMdd_HHmmss}";

        var tcs = new TaskCompletionSource<string?>();
        _uiDispatcher.Invoke(() =>
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Export Conversation",
                FileName = defaultFileName,
                Filter = format == "json"
                    ? "JSON Files|*.json"
                    : "Markdown Files|*.md|Text Files|*.txt",
                DefaultExt = format == "json" ? ".json" : ".md"
            };

            var result = dialog.ShowDialog() == true ? dialog.FileName : null;
            tcs.SetResult(result);
        });

        var filePath = await tcs.Task;
        if (string.IsNullOrEmpty(filePath))
            return JsonSerializer.SerializeToNode(new { cancelled = true }, _jsonOptions);

        var content = format == "json" ? ExportAsJson(messages, conversation) : ExportAsMarkdown(messages, conversation);
        await File.WriteAllTextAsync(filePath, content);

        return JsonSerializer.SerializeToNode(new { success = true, path = filePath }, _jsonOptions);
    }

    private static string ExportAsMarkdown(List<Message> messages, Conversation? conversation)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"# {conversation?.Title ?? "Chat Export"}");
        sb.AppendLine();
        sb.AppendLine($"- **Exported**: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        if (conversation?.ModelId != null)
            sb.AppendLine($"- **Model**: {conversation.ModelId}");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();

        foreach (var msg in messages)
        {
            var roleLabel = msg.Role switch
            {
                "user" => "## User",
                "assistant" => "## Assistant",
                "system" => "## System",
                _ => $"## {msg.Role}"
            };
            sb.AppendLine($"{roleLabel}  ");
            sb.AppendLine($"*{msg.CreatedAt:yyyy-MM-dd HH:mm:ss}*  ");
            sb.AppendLine();
            sb.AppendLine(msg.Content);
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string ExportAsJson(List<Message> messages, Conversation? conversation)
    {
        var export = new
        {
            conversationId = conversation?.Id,
            title = conversation?.Title,
            modelId = conversation?.ModelId,
            exportedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            messages = messages.Select(m => new
            {
                m.Role,
                m.Content,
                createdAt = m.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
            }).ToList()
        };
        return JsonSerializer.Serialize(export, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    private async Task<JsonNode?> HandleChatSend(JsonNode? payload)
    {
        var conversationId = payload?["conversationId"]?.GetValue<string>() ?? "";
        var providerId = payload?["providerId"]?.GetValue<int>() ?? 0;
        var modelId = payload?["modelId"]?.GetValue<string>() ?? "";
        var userContent = payload?["content"]?.GetValue<string>() ?? "";
        var imageData = payload?["imageData"]?.GetValue<string>();
        var fileContent = payload?["fileContent"]?.GetValue<string>();

        var provider = _providerService.GetById(providerId);
        if (provider == null) throw new Exception("Provider not found");

        // PromptEngine temporarily disabled for testing
        var enhancedContent = userContent;
        if (!string.IsNullOrEmpty(fileContent))
            enhancedContent += "\n\n" + fileContent;

        // Save user message
        _chatService.AddMessage(conversationId, "user", userContent, imageData);

        // Build chat request
        var messages = new List<ChatMessage>();
        var history = _chatService.GetMessages(conversationId);
        foreach (var h in history.TakeLast(20))  // Keep last 20 messages as context
        {
            var msg = new ChatMessage { Role = h.Role, Content = h.Content };
            if (!string.IsNullOrEmpty(h.ImageData))
            {
                msg.Images = new List<ChatImageContent>
                {
                    new() { Base64Data = h.ImageData, MimeType = "image/jpeg" }
                };
            }
            messages.Add(msg);
        }

        // Ensure last message is the current one with any images
        if (messages.Count > 0 && messages[^1].Role == "user")
        {
            messages[^1].Content = enhancedContent;
            if (!string.IsNullOrEmpty(imageData) && messages[^1].Images == null)
            {
                messages[^1].Images = new List<ChatImageContent>
                {
                    new() { Base64Data = imageData, MimeType = "image/jpeg" }
                };
            }
        }

        var request = new ChatRequest
        {
            ModelId = modelId,
            Messages = messages,
            Temperature = 0.7
        };

        var p = new OpenAICompatibleProvider(provider.Name, provider.ApiBaseUrl, provider.ApiKey,
            provider.ProxyUrl, provider.SupportsVision);

        _currentChatCts = new CancellationTokenSource();
        var fullContent = "";
        var hasError = false;

        try
        {
            await foreach (var chunk in p.StreamChatAsync(request, _currentChatCts.Token))
            {
                if (!string.IsNullOrEmpty(chunk.ContentDelta))
                {
                    fullContent += chunk.ContentDelta;
                    SendStreamToWeb("chat:chunk", chunk.ContentDelta);
                }
                if (chunk.FinishReason != null)
                {
                    break;
                }
            }
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException || _currentChatCts?.Token.IsCancellationRequested != true)
        {
            hasError = true;
            SendStreamToWeb("chat:error", "Request timed out. The API took too long to respond. Please try again.");
        }
        catch (OperationCanceledException)
        {
            // User aborted
        }
        catch (HttpRequestException ex)
        {
            hasError = true;
            SendStreamToWeb("chat:error", $"Network error: {ex.Message}. Please check your connection and provider settings.");
        }
        catch (Exception ex)
        {
            hasError = true;
            SendStreamToWeb("chat:error", $"Chat error: {ex.Message}");
        }

        // Save assistant message
        if (!string.IsNullOrEmpty(fullContent))
        {
            _chatService.AddMessage(conversationId, "assistant", fullContent);

            // Auto-title on first assistant response
            var convMessages = _chatService.GetMessages(conversationId);
            if (convMessages.Count(m => m.Role == "assistant") == 1)
            {
                var title = GenerateTitle(userContent, fileContent, fullContent);
                _chatService.UpdateConversationTitle(conversationId, title);
            }
        }

        if (!hasError)
        {
            SendStreamToWeb("chat:done", "");
        }
        return null;
    }

    private JsonNode? HandleChatAbort()
    {
        _currentChatCts?.Cancel();
        return JsonSerializer.SerializeToNode(new { success = true }, _jsonOptions);
    }

    private async Task<JsonNode?> HandleFileRead(JsonNode? payload)
    {
        var filePath = payload?["path"]?.GetValue<string>() ?? "";
        if (!File.Exists(filePath)) throw new Exception("File not found");

        var ext = Path.GetExtension(filePath);
        var maxSize = (_settingsService.Get("max_file_size_mb") ?? "10");
        var maxBytes = int.Parse(maxSize) * 1024 * 1024;
        var fileInfo = new FileInfo(filePath);
        if (fileInfo.Length > maxBytes) throw new Exception($"File exceeds {maxSize}MB limit");

        if (!FileProcessor.IsSupported(filePath)) throw new Exception("Unsupported file type");

        var content = FileProcessor.ReadContent(filePath);
        var tokens = LogParser.EstimateTokens(content);

        return JsonSerializer.SerializeToNode(new
        {
            name = Path.GetFileName(filePath),
            size = fileInfo.Length,
            content,
            tokens
        }, _jsonOptions);
    }

    private JsonNode? HandleScreenshotCapture()
    {
        CaptureScreenshotWithWindowHidden();
        return JsonSerializer.SerializeToNode(new { success = true }, _jsonOptions);
    }

    private void CaptureScreenshotWithWindowHidden()
    {
        _uiDispatcher.Invoke(() =>
        {
            var mainWindow = System.Windows.Application.Current.MainWindow;
            mainWindow?.Hide();

            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                _screenshotService.CaptureArea(() =>
                {
                    _uiDispatcher.Invoke(() => mainWindow?.Show());
                });
            };
            timer.Start();
        });
    }

    private async Task<JsonNode?> HandleFileOpen()
    {
        var tcs = new TaskCompletionSource<string?>();

        _uiDispatcher.Invoke(() =>
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select a file to upload",
                Filter = "Text and Log Files|*.txt;*.log;*.json;*.xml;*.yaml;*.yml;*.sql;*.csv;*.md;*.ini;*.conf;*.config;*.properties|All Files|*.*"
            };

            var result = dialog.ShowDialog() == true ? dialog.FileName : null;
            tcs.SetResult(result);
        });

        var filePath = await tcs.Task;
        if (string.IsNullOrEmpty(filePath)) return null;

        if (!File.Exists(filePath)) throw new Exception("File not found");

        var ext = Path.GetExtension(filePath);
        var maxSize = _settingsService.Get("max_file_size_mb") ?? "10";
        var maxBytes = int.Parse(maxSize) * 1024 * 1024;
        var fileInfo = new FileInfo(filePath);
        if (fileInfo.Length > maxBytes) throw new Exception($"File exceeds {maxSize}MB limit");

        if (!FileProcessor.IsSupported(filePath)) throw new Exception("Unsupported file type");

        var content = FileProcessor.ReadContent(filePath);
        var tokens = LogParser.EstimateTokens(content);

        return JsonSerializer.SerializeToNode(new
        {
            name = Path.GetFileName(filePath),
            size = fileInfo.Length,
            content,
            tokens
        }, _jsonOptions);
    }

    private void OnScreenshotCaptured(object? sender, string base64)
    {
        SendToWeb("screenshot:ready", Guid.NewGuid().ToString(), JsonSerializer.SerializeToNode(new { base64 }, _jsonOptions));
    }

    private async Task<JsonNode?> HandleSettingsGet(JsonNode? payload)
    {
        var key = payload?["key"]?.GetValue<string>() ?? "";
        var value = _settingsService.Get(key);
        return JsonSerializer.SerializeToNode(new { key, value }, _jsonOptions);
    }

    private async Task<JsonNode?> HandleSettingsSet(JsonNode? payload)
    {
        var key = payload?["key"]?.GetValue<string>() ?? "";
        var value = payload?["value"]?.GetValue<string>() ?? "";
        _settingsService.Set(key, value);
        if (key.StartsWith("shortcut_", StringComparison.OrdinalIgnoreCase))
        {
            ShortcutSettingsChanged?.Invoke();
        }
        if (key.Equals("theme", StringComparison.OrdinalIgnoreCase))
        {
            ThemeChanged?.Invoke(value);
        }
        return JsonSerializer.SerializeToNode(new { success = true }, _jsonOptions);
    }

    private async Task<JsonNode?> HandleSettingsAll()
    {
        var settings = _settingsService.GetAll();
        return JsonSerializer.SerializeToNode(settings, _jsonOptions);
    }

    private void SendToWeb(string channel, string id, JsonNode? payload)
    {
        var msg = new JsonObject
        {
            ["id"] = id,
            ["channel"] = channel,
            ["payload"] = payload
        };
        _uiDispatcher.Invoke(() =>
        {
            _webView.CoreWebView2?.PostWebMessageAsJson(msg.ToJsonString());
        });
    }

    private void SendStreamToWeb(string channel, string data)
    {
        var msg = new JsonObject
        {
            ["id"] = Guid.NewGuid().ToString(),
            ["channel"] = channel,
            ["payload"] = data
        };
        _uiDispatcher.Invoke(() =>
        {
            _webView.CoreWebView2?.PostWebMessageAsJson(msg.ToJsonString());
        });
    }

    private void SendErrorToWeb(string channel, string id, string error)
    {
        var msg = new JsonObject
        {
            ["id"] = id,
            ["channel"] = channel,
            ["error"] = error
        };
        _uiDispatcher.Invoke(() =>
        {
            _webView.CoreWebView2?.PostWebMessageAsJson(msg.ToJsonString());
        });
    }

    private string GenerateTitle(string userContent, string? fileContent, string assistantContent)
    {
        var label = _promptEngine.GetStrategyLabel(userContent, fileContent);
        var firstLine = assistantContent.Split('\n').FirstOrDefault()?.Trim() ?? "";

        if (!string.IsNullOrEmpty(label))
        {
            var excerpt = firstLine.Length > 20 ? firstLine[..20] + "..." : firstLine;
            return $"{label} - {excerpt}";
        }

        if (firstLine.Length > 40)
            firstLine = firstLine[..40] + "...";
        return string.IsNullOrEmpty(firstLine) ? "New Chat" : firstLine;
    }
}
