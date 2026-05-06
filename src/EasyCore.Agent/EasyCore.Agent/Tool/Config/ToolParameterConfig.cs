namespace EasyCore.Agent.Tool.Config
{
    public sealed class ToolParameterConfig
    {
        public string Type { get; set; } = "string";

        public string Description { get; set; } = "";

        public bool Required { get; set; } = true;
    }
}
