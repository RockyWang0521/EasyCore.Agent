namespace Demo.Common.Dto;

/// <summary>
/// Chat request with session identifier.
/// </summary>
public sealed class ChatRequest
{
    /// <summary>
    /// User message content.
    /// </summary>
    public string Message { get; set; } = "Introduce EasyCore.Agent";

    /// <summary>
    /// Conversation session identifier.
    /// </summary>
    public string SessionId { get; set; } = "demo-session";
}
