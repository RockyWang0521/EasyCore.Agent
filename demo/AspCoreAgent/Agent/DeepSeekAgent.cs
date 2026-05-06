using EasyCore.Agent;
using EasyCore.Dependencie;
using Microsoft.Extensions.Options;

namespace AspCoreAgent.Agent
{
    public class DeepSeekAgent : BasicAgentClient<DeepSeekClientOptions>, IScopedDependencie
    {
        public DeepSeekAgent(IOptions<DeepSeekClientOptions> options, IServiceProvider serviceProvider) : base(options, serviceProvider) { }
    }
}
