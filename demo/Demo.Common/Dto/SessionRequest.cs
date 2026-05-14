namespace Demo.Common.Dto;

/// <summary>
/// Request that only requires a session identifier.
/// </summary>
public sealed class SessionRequest
{
    /// <summary>
    /// Conversation session identifier.
    /// </summary>
    public string SessionId { get; set; } = "demo-session";
}
