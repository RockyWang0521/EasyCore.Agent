namespace Demo.EasyCore.Pipeline.Dto;

/// <summary>
/// Pipeline run request.
/// </summary>
public sealed class PipelineRunRequest
{
    /// <summary>
    /// Branch selector: 1=code, 2=sql, otherwise chat.
    /// </summary>
    public string Input { get; set; } = "1";
}
