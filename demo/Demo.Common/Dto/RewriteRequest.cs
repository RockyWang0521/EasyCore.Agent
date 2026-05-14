namespace Demo.Common.Dto;

/// <summary>
/// Query rewrite request.
/// </summary>
public sealed class RewriteRequest
{
    /// <summary>
    /// User query to rewrite.
    /// </summary>
    public string Message { get; set; } = "Which vector databases does it support?";

    /// <summary>
    /// Conversation session identifier.
    /// </summary>
    public string SessionId { get; set; } = "rag-demo";
}
