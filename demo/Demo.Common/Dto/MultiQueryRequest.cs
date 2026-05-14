namespace Demo.Common.Dto;

/// <summary>
/// Multi-query generation request.
/// </summary>
public sealed class MultiQueryRequest
{
    /// <summary>
    /// Original user query.
    /// </summary>
    public string Message { get; set; } = "What can EasyCore RAG do?";
}
