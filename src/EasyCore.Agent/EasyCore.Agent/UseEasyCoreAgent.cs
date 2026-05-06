using EasyCore.Agent.ContextStore;
using EasyCore.Agent.Tool;
using EasyCore.DistributedCache;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EasyCore.Agent
{
    public static class UseEasyCoreAgent
    {
        public static IServiceCollection EasyCoreAgent(this IServiceCollection services, Action<AgentConfigOptions>? action = null)
        {
            var options = new AgentConfigOptions();

            if (action != null) action(options);

            services.AddSingleton(options);

            services.AddSingleton<IAIToolProvider, AIToolProvider>();

            if (options.AgentContextStoreType == AgentContextStoreType.Redis)
            {
                services.EasyCoreDistributedCache(o =>
                {
                    o.User = options.User;
                    o.Password = options.Password;
                    o.EndPoints = options.EndPoints;
                    o.ConnectTimeout = options.ConnectTimeout;
                    o.SyncTimeout = options.SyncTimeout;
                    o.DistributedName = options.DistributedName;
                });

                services.AddSingleton<IAgentContextStore, RedisAgentContextStore>();
            }
            else if (options.AgentContextStoreType == AgentContextStoreType.Memory)
            {
                services.AddSingleton<IAgentContextStore, MemoryAgentContextStore>();
            }

            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;

            string[] dllFiles = Directory.GetFiles(rootDirectory, "*.dll", SearchOption.TopDirectoryOnly).Where(path =>
            {
                string fileName = Path.GetFileName(path);
                return !(fileName.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase) || fileName.StartsWith("System.", StringComparison.OrdinalIgnoreCase));
            }).ToArray();

            var assemblys = dllFiles.Select(u => Assembly.LoadFrom(u)).ToList();

            services.AddAITools(assemblys);

            return services;
        }
    }
}
