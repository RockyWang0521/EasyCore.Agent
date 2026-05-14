namespace Demo.EasyCore.Pipeline.Dto;

/// <summary>
/// Pipeline step trace information.
/// </summary>
public sealed class PipelineTraceDto
{
    /// <summary>
    /// Step name recorded in the trace.
    /// </summary>
    public string StepName { get; set; } = string.Empty;

    /// <summary>
    /// Step type recorded in the trace.
    /// </summary>
    public string StepType { get; set; } = string.Empty;

    /// <summary>
    /// Whether the step completed successfully.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Step elapsed time in milliseconds.
    /// </summary>
    public long ElapsedMilliseconds { get; set; }
}
