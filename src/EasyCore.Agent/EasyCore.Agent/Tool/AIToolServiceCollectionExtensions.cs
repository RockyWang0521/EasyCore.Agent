using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EasyCore.Agent.Tool
{
    public static class AIToolServiceCollectionExtensions
    {
        public static IServiceCollection AddAITools(this IServiceCollection services, List<Assembly> assemblies)
        {
            var descriptors = new List<AIToolMethodDescriptor>();

            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes()
                    .Where(t => t is { IsClass: true, IsAbstract: false });

                foreach (var type in types)
                {
                    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .Where(m => m.GetCustomAttribute<AIToolAttribute>() != null);

                    foreach (var method in methods)
                    {
                        var aiToolAttr = method.GetCustomAttribute<AIToolAttribute>()!;
                        var descAttr = method.GetCustomAttribute<ToolDescriptionAttribute>();

                        services.AddScoped(type);

                        descriptors.Add(new AIToolMethodDescriptor
                        {
                            ToolType = type,
                            Method = method,
                            Name = aiToolAttr.AIToolName,
                            Description = descAttr?.Description
                        });
                    }
                }
            }

            services.AddSingleton(descriptors);

            services.AddScoped<IAIToolProvider, AIToolProvider>();

            return services;
        }
    }
}
