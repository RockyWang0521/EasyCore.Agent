namespace Demo.EasyCore.Pipeline.Dto;

/// <summary>
/// Pipeline run response.
/// </summary>
public sealed class PipelineRunResponse
{
    /// <summary>
    /// Original pipeline input.
    /// </summary>
    public string Input { get; set; } = string.Empty;

    /// <summary>
    /// Final pipeline output.
    /// </summary>
    public string? Output { get; set; }

    /// <summary>
    /// Resolved branch intent.
    /// </summary>
    public string? Intent { get; set; }

    /// <summary>
    /// Execution traces for each pipeline step.
    /// </summary>
    public List<PipelineTraceDto> Traces { get; set; } = new();
}
