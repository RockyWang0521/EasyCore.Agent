namespace Demo.Common.Dto;

/// <summary>
/// Chat request for tool-enabled agent demo.
/// </summary>
public sealed class ChatWithToolsRequest
{
    /// <summary>
    /// User message content.
    /// </summary>
    public string Message { get; set; } = "What is the weather in Beijing today?";

    /// <summary>
    /// Conversation session identifier.
    /// </summary>
    public string SessionId { get; set; } = "demo-tools-session";
}
