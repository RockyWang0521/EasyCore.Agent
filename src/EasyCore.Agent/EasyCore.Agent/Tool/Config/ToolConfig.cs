namespace EasyCore.Agent.Tool.Config
{
    public sealed class ToolConfig
    {
        public string Name { get; set; } = default!;

        public string MethodName { get; set; } = default!;

        public string Description { get; set; } = default!;

        public Dictionary<string, ToolParameterConfig> Parameters { get; set; } = new();

        public Dictionary<string, ToolReturnConfig> Returns { get; set; } = new();
    }
}
