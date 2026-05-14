using AspCoreAgent.Workflow;
using EasyCore.Workflow;
using Microsoft.AspNetCore.Mvc;

namespace AspCoreAgent.Controllers;

/// <summary>
/// EasyCore.Workflow 工作流测试接口。
/// </summary>
/// <remarks>
/// 演示流程：校验 → 处理 → 审批（步骤内发布 order.submitted、等待 order.approved 后继续） → 发货 → 完成。
/// <para>推荐测试顺序：</para>
/// <list type="number">
/// <item>调用 <c>启动</c>，记录返回的 instanceId</item>
/// <item>调用 <c>发布事件</c>，传入 instanceId 继续执行</item>
/// <item>调用 <c>查询实例</c> 查看最终状态与轨迹</item>
/// </list>
/// </remarks>
[Route("api/[controller]")]
[ApiController]
public class WorkflowController : ControllerBase
{
    private readonly IWorkflowEngine _workflowEngine;
    private readonly IWorkflowEventPublisher _eventPublisher;

    public WorkflowController(
        IWorkflowEngine workflowEngine,
        IWorkflowEventPublisher eventPublisher)
    {
        _workflowEngine = workflowEngine;
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// 启动订单审批演示流程。
    /// </summary>
    /// <param name="订单号">业务订单号，默认 order-001。</param>
    /// <param name="慢速模式">为 true 时处理节点会等待 30 秒，便于测试暂停。</param>
    /// <param name="关联Id">可选业务关联标识。</param>
    /// <returns>流程实例状态；在 step-approval 步骤内等待审批事件时停下，status 为 WaitingEvent。</returns>
    [HttpGet("启动")]
    public async Task<WorkflowInstanceState> 启动(
        [FromQuery(Name = "订单号")] string 订单号 = "order-001",
        [FromQuery(Name = "慢速模式")] bool 慢速模式 = false,
        [FromQuery(Name = "关联Id")] string? 关联Id = null)
    {
        var context = new WorkflowContext
        {
            Input = 订单号,
            CorrelationId = 关联Id
        };

        if (慢速模式)
            context.Set("slow", true);

        return await _workflowEngine.StartAsync(OrderWorkflowDemo.WorkflowId, context);
    }

    /// <summary>
    /// 查询流程实例详情。
    /// </summary>
    /// <param name="实例Id">流程实例标识。</param>
    [HttpGet("查询实例")]
    public async Task<ActionResult<WorkflowInstanceState>> 查询实例([FromQuery(Name = "实例Id")] string 实例Id)
    {
        if (string.IsNullOrWhiteSpace(实例Id))
            return BadRequest("请提供实例Id。");

        var state = await _workflowEngine.GetInstanceAsync(实例Id);
        if (state == null)
            return NotFound("未找到对应的流程实例。");

        return state;
    }

    /// <summary>
    /// 暂停流程实例（当前节点执行完成后生效）。
    /// </summary>
    /// <param name="实例Id">流程实例标识。</param>
    /// <remarks>可配合「启动」接口的慢速模式，在处理节点执行期间调用本接口。</remarks>
    [HttpGet("暂停")]
    public async Task<IActionResult> 暂停([FromQuery(Name = "实例Id")] string 实例Id)
    {
        if (string.IsNullOrWhiteSpace(实例Id))
            return BadRequest("请提供实例Id。");

        await _workflowEngine.PauseAsync(实例Id);
        var state = await _workflowEngine.GetInstanceAsync(实例Id);
        return Ok(state);
    }

    /// <summary>
    /// 恢复已暂停或等待事件的流程实例。
    /// </summary>
    /// <param name="实例Id">流程实例标识。</param>
    [HttpGet("恢复")]
    public async Task<ActionResult<WorkflowInstanceState>> 恢复([FromQuery(Name = "实例Id")] string 实例Id)
    {
        if (string.IsNullOrWhiteSpace(实例Id))
            return BadRequest("请提供实例Id。");

        try
        {
            return await _workflowEngine.ResumeAsync(实例Id);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// 终止流程实例。
    /// </summary>
    /// <param name="实例Id">流程实例标识。</param>
    [HttpGet("终止")]
    public async Task<IActionResult> 终止([FromQuery(Name = "实例Id")] string 实例Id)
    {
        if (string.IsNullOrWhiteSpace(实例Id))
            return BadRequest("请提供实例Id。");

        await _workflowEngine.TerminateAsync(实例Id);
        var state = await _workflowEngine.GetInstanceAsync(实例Id);
        return Ok(state);
    }

    /// <summary>
    /// 跳转到指定流程节点。
    /// </summary>
    /// <param name="实例Id">流程实例标识。</param>
    /// <param name="节点Id">目标节点，例如 step-ship、step-done。</param>
    /// <remarks>
    /// 可用节点：step-validate、step-process、step-wait-approval、step-ship、step-done。
    /// </remarks>
    [HttpGet("跳转节点")]
    public async Task<IActionResult> 跳转节点(
        [FromQuery(Name = "实例Id")] string 实例Id,
        [FromQuery(Name = "节点Id")] string 节点Id)
    {
        if (string.IsNullOrWhiteSpace(实例Id))
            return BadRequest("请提供实例Id。");

        if (string.IsNullOrWhiteSpace(节点Id))
            return BadRequest("请提供节点Id。");

        try
        {
            await _workflowEngine.JumpToNodeAsync(实例Id, 节点Id);
            var state = await _workflowEngine.GetInstanceAsync(实例Id);
            return Ok(state);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// 发布工作流事件，并恢复正在等待该事件的实例。
    /// </summary>
    /// <param name="事件名">事件名称，默认 order.approved。</param>
    /// <param name="实例Id">可选；指定则只恢复该实例，否则恢复所有等待该事件的实例。</param>
    /// <param name="审批人">审批人姓名，会写入流程上下文。</param>
    [HttpGet("发布事件")]
    public async Task<IActionResult> 发布事件(
        [FromQuery(Name = "事件名")] string 事件名 = OrderWorkflowDemo.ApprovalEventName,
        [FromQuery(Name = "实例Id")] string? 实例Id = null,
        [FromQuery(Name = "审批人")] string? 审批人 = "admin")
    {
        if (string.IsNullOrWhiteSpace(事件名))
            return BadRequest("请提供事件名。");

        await _eventPublisher.PublishAsync(new WorkflowEventMessage
        {
            EventName = 事件名,
            InstanceId = 实例Id,
            Payload =
            {
                ["approver"] = 审批人,
                ["approvedAt"] = DateTimeOffset.UtcNow
            }
        });

        if (!string.IsNullOrWhiteSpace(实例Id))
        {
            var state = await _workflowEngine.GetInstanceAsync(实例Id);
            return Ok(state);
        }

        return Ok(new
        {
            事件名,
            实例Id,
            消息 = "事件已发布。"
        });
    }
}
