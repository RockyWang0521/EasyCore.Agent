using EasyCore.Agent;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

public class AIToolProvider : IAIToolProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly List<AIToolMethodDescriptor> _descriptors;

    public AIToolProvider(
        IServiceProvider serviceProvider,
        List<AIToolMethodDescriptor> descriptors)
    {
        _serviceProvider = serviceProvider;
        _descriptors = descriptors;
    }

    public List<AITool> GetTools()
    {
        return _descriptors
            .Select(CreateTool)
            .ToList();
    }

    public AITool? GetTool(string name)
    {
        var descriptor = _descriptors.FirstOrDefault(x =>
            string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

        return descriptor == null ? null : CreateTool(descriptor);
    }

    public List<AITool> GetTools(params string[] names)
    {
        var nameSet = names.ToHashSet(StringComparer.OrdinalIgnoreCase);

        return _descriptors
            .Where(x => nameSet.Contains(x.Name))
            .Select(CreateTool)
            .ToList();
    }

    private AITool CreateTool(AIToolMethodDescriptor descriptor)
    {
        var instance = _serviceProvider.GetRequiredService(descriptor.ToolType);

        return AIFunctionFactory.Create(
            method: descriptor.Method,
            target: instance,
            name: descriptor.Name,
            description: descriptor.Description);
    }
}