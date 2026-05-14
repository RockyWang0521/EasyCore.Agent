namespace Demo.Common.Dto;

/// <summary>
/// Vector similarity search request.
/// </summary>
public sealed class VectorSearchRequest
{
    /// <summary>
    /// Natural language search query.
    /// </summary>
    public string Query { get; set; } = "Which vector databases does EasyCore support?";

    /// <summary>
    /// Maximum number of results to return.
    /// </summary>
    public int Limit { get; set; } = 5;
}
