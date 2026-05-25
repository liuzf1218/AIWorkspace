using AIWorkspace.Core.Models;

namespace AIWorkspace.Core.Providers;

public interface IProvider
{
    string Name { get; }
    string ApiBaseUrl { get; }
    bool SupportsVision { get; }

    IAsyncEnumerable<ChatChunk> StreamChatAsync(
        ChatRequest request,
        CancellationToken ct = default);

    Task<List<ModelInfo>> GetModelsAsync(CancellationToken ct = default);

    Task<bool> ValidateAsync(CancellationToken ct = default);
}
