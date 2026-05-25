using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AIWorkspace.Core.Models;

namespace AIWorkspace.Core.Providers;

public class OpenAICompatibleProvider : IProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public string Name { get; }
    public string ApiBaseUrl { get; }
    public bool SupportsVision { get; set; }

    public OpenAICompatibleProvider(string name, string apiBaseUrl, string apiKey, string? proxyUrl = null, bool supportsVision = false)
    {
        Name = name;
        ApiBaseUrl = apiBaseUrl.TrimEnd('/');
        if (ApiBaseUrl.EndsWith("/v1", StringComparison.OrdinalIgnoreCase))
            ApiBaseUrl = ApiBaseUrl[..^3];
        _apiKey = apiKey;
        SupportsVision = supportsVision;

        var handler = new HttpClientHandler();
        if (!string.IsNullOrEmpty(proxyUrl))
        {
            handler.Proxy = new System.Net.WebProxy(proxyUrl);
            handler.UseProxy = true;
        }

        _httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(120)
        };
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async IAsyncEnumerable<ChatChunk> StreamChatAsync(
        ChatRequest request,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var messages = new List<JsonObject>();
        foreach (var msg in request.Messages)
        {
            var msgObj = new JsonObject();
            msgObj["role"] = msg.Role;

            if (msg.Images != null && msg.Images.Count > 0)
            {
                var content = new JsonArray();
                content.Add(new JsonObject
                {
                    ["type"] = "text",
                    ["text"] = msg.Content
                });
                foreach (var img in msg.Images)
                {
                    content.Add(new JsonObject
                    {
                        ["type"] = "image_url",
                        ["image_url"] = new JsonObject
                        {
                            ["url"] = $"data:{img.MimeType};base64,{img.Base64Data}"
                        }
                    });
                }
                msgObj["content"] = content;
            }
            else
            {
                msgObj["content"] = msg.Content;
            }
            messages.Add(msgObj);
        }

        var body = new JsonObject
        {
            ["model"] = request.ModelId,
            ["messages"] = JsonSerializer.SerializeToNode(messages),
            ["temperature"] = request.Temperature,
            ["stream"] = true
        };

        if (request.MaxTokens.HasValue)
            body["max_tokens"] = request.MaxTokens.Value;

        var json = body.ToJsonString();
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync(
            $"{ApiBaseUrl}/v1/chat/completions", httpContent, ct);

        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream, Encoding.UTF8);

        while (!reader.EndOfStream && !ct.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(ct);
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: ")) continue;
            var data = line.Substring(6);
            if (data == "[DONE]") yield break;

            ChatChunk? chunk = null;
            try
            {
                var node = JsonNode.Parse(data);
                var delta = node?["choices"]?[0]?["delta"]?["content"]?.GetValue<string?>();
                var finishReason = node?["choices"]?[0]?["finish_reason"]?.GetValue<string?>();
                var usage = node?["usage"];

                chunk = new ChatChunk
                {
                    ContentDelta = delta,
                    FinishReason = finishReason,
                    PromptTokens = usage?["prompt_tokens"]?.GetValue<int?>(),
                    CompletionTokens = usage?["completion_tokens"]?.GetValue<int?>()
                };
            }
            catch
            {
                // Ignore malformed SSE lines
            }

            if (chunk != null)
            {
                yield return chunk;
                if (chunk.FinishReason != null) yield break;
            }
        }
    }

    public async Task<List<ModelInfo>> GetModelsAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{ApiBaseUrl}/v1/models", ct);
            if (!response.IsSuccessStatusCode) return GetDefaultModels();

            var json = await response.Content.ReadFromJsonAsync<JsonObject>(ct);
            var models = new List<ModelInfo>();
            var data = json?["data"]?.AsArray();
            if (data != null)
            {
                foreach (var item in data)
                {
                    var modelId = item?["id"]?.GetValue<string>();
                    if (modelId != null)
                    {
                        models.Add(new ModelInfo
                        {
                            ModelId = modelId,
                            DisplayName = modelId,
                            SupportsVision = false // Will be updated by config
                        });
                    }
                }
            }
            return models.Count > 0 ? models : GetDefaultModels();
        }
        catch
        {
            return GetDefaultModels();
        }
    }

    public async Task<bool> ValidateAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{ApiBaseUrl}/v1/models", ct);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private List<ModelInfo> GetDefaultModels()
    {
        try
        {
            var configPath = Path.Combine(AppContext.BaseDirectory, "Assets", "default-models.json");
            if (File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                var models = JsonSerializer.Deserialize<List<ModelInfo>>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                if (models != null && models.Count > 0) return models;
            }
        }
        catch { /* Fallback to built-in list */ }

        // Fallback models for popular providers
        return new List<ModelInfo>
        {
            new() { ModelId = "gpt-4o", DisplayName = "GPT-4o", SupportsVision = true },
            new() { ModelId = "gpt-4o-mini", DisplayName = "GPT-4o Mini", SupportsVision = true },
            new() { ModelId = "claude-3-5-sonnet-20241022", DisplayName = "Claude 3.5 Sonnet", SupportsVision = true },
            new() { ModelId = "claude-3-opus-20240229", DisplayName = "Claude 3 Opus", SupportsVision = true },
            new() { ModelId = "gemini-1.5-pro", DisplayName = "Gemini 1.5 Pro", SupportsVision = true },
            new() { ModelId = "deepseek-chat", DisplayName = "DeepSeek V3", SupportsVision = false },
            new() { ModelId = "deepseek-coder", DisplayName = "DeepSeek Coder", SupportsVision = false },
            new() { ModelId = "qwen-max", DisplayName = "Qwen Max", SupportsVision = true },
            new() { ModelId = "qwen-coder-plus", DisplayName = "Qwen Coder Plus", SupportsVision = false },
        };
    }
}
