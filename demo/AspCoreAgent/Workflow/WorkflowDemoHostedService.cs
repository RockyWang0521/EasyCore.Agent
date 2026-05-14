using EasyCore.Workflow;

namespace AspCoreAgent.Workflow;

/// <summary>
/// Registers demo workflows when the application starts.
/// </summary>
public sealed class WorkflowDemoHostedService : IHostedService
{
    private readonly IWorkflowEngine _workflowEngine;

    public WorkflowDemoHostedService(IWorkflowEngine workflowEngine)
    {
        _workflowEngine = workflowEngine;
    }

    public Task StartAsync(CancellationToken cancellationToken) =>
        _workflowEngine.RegisterAsync(OrderWorkflowDemo.Create(), cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
