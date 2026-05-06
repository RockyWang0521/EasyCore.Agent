using Microsoft.Extensions.AI;

namespace EasyCore.Agent
{
    public interface IAIToolProvider
    {
        AITool? GetTool(string name);

        List<AITool> GetTools(params string[] names);

        List<AITool> GetTools();
    }
}
