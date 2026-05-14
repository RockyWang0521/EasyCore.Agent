namespace Demo.Common.Dto;

/// <summary>
/// Document chunking request.
/// </summary>
public sealed class ChunkRequest
{
    /// <summary>
    /// Maximum chunk size in characters.
    /// </summary>
    public int ChunkSize { get; set; } = 120;

    /// <summary>
    /// Overlap size between consecutive chunks.
    /// </summary>
    public int Overlap { get; set; } = 30;
}
