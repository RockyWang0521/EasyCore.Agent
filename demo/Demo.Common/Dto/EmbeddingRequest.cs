namespace Demo.Common.Dto;

/// <summary>
/// Embedding generation request.
/// </summary>
public sealed class EmbeddingRequest
{
    /// <summary>
    /// Text to embed.
    /// </summary>
    public string Text { get; set; } = "Does EasyCore.Agent support RAG?";
}
