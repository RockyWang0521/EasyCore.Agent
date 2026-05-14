using EasyCore.Agent;
using EasyCore.Dependencie;
using Microsoft.Extensions.Options;

namespace AspCoreAgent.Agent
{
    public class QianwenAgent : BasicAgentClient<QianwenClientOptions>, IScopedDependencie
    {
        public QianwenAgent(IOptions<QianwenClientOptions> options, IServiceProvider serviceProvider) : base(options, serviceProvider) { }
    }
}
