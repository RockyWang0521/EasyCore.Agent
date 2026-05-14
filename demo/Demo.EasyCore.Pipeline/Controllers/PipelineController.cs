using Demo.EasyCore.Pipeline.Dto;
using EasyCore.Pipeline;
using Microsoft.AspNetCore.Mvc;

namespace Demo.EasyCore.Pipeline.Controllers;

/// <summary>
/// Standalone demo endpoints for EasyCore.Pipeline.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class PipelineController : ControllerBase
{
    /// <summary>
    /// Runs a sequential, branch, and parallel pipeline demo.
    /// </summary>
    [HttpGet("run")]
    public async Task<PipelineRunResponse> Run([FromQuery] PipelineRunRequest request)
    {
        var input = request.Input;
        var pipeline = global::EasyCore.Pipeline.Pipeline.Create()
            .AddFunc(ctx =>
            {
                var intent = input switch
                {
                    "1" => "code",
                    "2" => "sql",
                    _ => "chat"
                };
                ctx.Set("intent", intent);
                ctx.Next(intent);
            })
            .AddBranch(branch => branch
                .If(ctx => ctx.Get<string>("intent") == "code", flow => flow
                    .AddFunc(ctx => ctx.Set("plan", "Generate Controller and DTO"))
                    .AddParallel(p => p
                        .AddFunc(ctx => ctx.Set("controller", "ProductController"))
                        .AddFunc(ctx => ctx.Set("dto", "ProductDto")))
                    .AddFunc(ctx => ctx.Next($"Merged: {ctx.Get<string>("controller")} + {ctx.Get<string>("dto")}")))
                .ElseIf(ctx => ctx.Get<string>("intent") == "sql", flow => flow
                    .AddFunc(ctx => ctx.Next("SELECT * FROM Products;")))
                .Else(flow => flow
                    .AddFunc(ctx => ctx.Next("Fallback chat branch"))))
            .AddFunc(ctx =>
            {
                ctx.Output = $"Pipeline completed, intent={ctx.Get<string>("intent")}, result={ctx.Output}";
            });

        var context = new PipelineContext { Input = input };
        await PipelineRunner.RunAsync(pipeline, context);

        return new PipelineRunResponse
        {
            Input = input,
            Output = context.Output,
            Intent = context.Get<string>("intent"),
            Traces = context.Traces.Select(t => new PipelineTraceDto
            {
                StepName = t.StepName,
                StepType = t.StepType,
                Success = t.Success,
                ElapsedMilliseconds = t.ElapsedMilliseconds
            }).ToList()
        };
    }
}
