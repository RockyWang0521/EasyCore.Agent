namespace EasyCore.Agent
{
    public abstract class AgentClientOptions
    {
        public string BaseUrl { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;

        public string ApiKey { get; set; } = string.Empty;
    }
}
