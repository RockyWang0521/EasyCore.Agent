using System.Reflection;

namespace EasyCore.Agent
{
    public class AIToolMethodDescriptor
    {
        public Type ToolType { get; set; } = default!;

        public MethodInfo Method { get; set; } = default!;

        public string Name { get; set; } = "";

        public string? Description { get; set; }
    }
}
