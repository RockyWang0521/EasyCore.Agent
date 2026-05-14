namespace Demo.Common.Dto;

/// <summary>
/// MMR selection request.
/// </summary>
public sealed class MmrRequest
{
    /// <summary>
    /// Number of candidates to return.
    /// </summary>
    public int TopK { get; set; } = 2;
}
