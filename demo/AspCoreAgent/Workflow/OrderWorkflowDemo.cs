using EasyCore.Workflow;

namespace AspCoreAgent.Workflow;

/// <summary>
/// 订单审批演示流程：在步骤内发布事件、等待触发后继续执行。
/// </summary>
public static class OrderWorkflowDemo
{
    /// <summary>
    /// 流程定义标识。
    /// </summary>
    public const string WorkflowId = "order-demo";

    /// <summary>
    /// 步骤内发布的「订单已提交」事件。
    /// </summary>
    public const string SubmittedEventName = "order.submitted";

    /// <summary>
    /// 步骤内等待的「订单已审批」事件。
    /// </summary>
    public const string ApprovalEventName = "order.approved";

    /// <summary>
    /// 创建演示流程。
    /// </summary>
    public static EasyCore.Workflow.Workflow Create()
    {
        return EasyCore.Workflow.Workflow.Create(WorkflowId, "订单审批演示")
            .AddStep("step-validate", (ctx, ct) =>
            {
                if (string.IsNullOrWhiteSpace(ctx.Input))
                    throw new InvalidOperationException("订单号不能为空。");

                ctx.Set("orderNo", ctx.Input.Trim());
                ctx.Set("validatedAt", DateTimeOffset.UtcNow);
                ctx.Next($"validated:{ctx.Input.Trim()}");
                return Task.CompletedTask;
            })
            .AddStep("step-process", async (ctx, ct) =>
            {
                ctx.Set("status", "processing");
                ctx.Set("processedAt", DateTimeOffset.UtcNow);

                if (ctx.Get<bool>("slow"))
                    await Task.Delay(TimeSpan.FromSeconds(30), ct);

                ctx.Next("processed");
            })
            .AddStep("step-approval", async (ctx, ct) =>
            {
                // —— 步骤前半段：提交审批并发布事件 ——
                ctx.Set("status", "待审批");

                if (!ctx.Get<bool>("submitted"))
                {
                    await ctx.PublishEventAsync(SubmittedEventName, new Dictionary<string, object?>
                    {
                        ["orderNo"] = ctx.Get<string>("orderNo"),
                        ["submittedAt"] = DateTimeOffset.UtcNow
                    }, ct);

                    ctx.Set("submitted", true);
                }

                // —— 发布完成后等待外部触发；触发后在同一步骤内继续 ——
                await ctx.WaitEventAsync(ApprovalEventName, ct);

                // —— 步骤后半段：审批通过后继续 ——
                var approver = ctx.Get<string>("approver") ?? "system";
                ctx.Set("status", "已审批");
                ctx.Set("approvedAt", DateTimeOffset.UtcNow);
                ctx.Next($"approved by {approver}");
            })
            .AddStep("step-ship", (ctx, ct) =>
            {
                ctx.Set("status", "已发货");
                ctx.Set("shippedAt", DateTimeOffset.UtcNow);
                ctx.Next($"shipped:{ctx.Get<string>("orderNo")}");
                return Task.CompletedTask;
            })
            .AddStep("step-done", ctx =>
            {
                ctx.Output = $"订单 {ctx.Get<string>("orderNo")} 流程完成，结果：{ctx.Input}";
            });
    }
}
