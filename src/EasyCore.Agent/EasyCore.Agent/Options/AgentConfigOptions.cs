namespace EasyCore.Agent
{
    /// <summary>
    /// Configuration options for Agent behavior.
    /// </summary>
    public class AgentConfigOptions
    {
        /// <summary>
        /// Maximum number of context messages to keep per session.
        /// For example: 20 means only the latest 20 messages are retained.
        /// </summary>
        public int MaxContextCount { get; set; } = 20;

        /// <summary>
        /// Context storage type: local memory or distributed Redis.
        /// </summary>
        public AgentContextStoreType AgentContextStoreType { get; set; } = AgentContextStoreType.Memory;

        /// <summary>
        /// Redis server endpoints.
        /// For example: 127.0.0.1:6379.
        /// </summary>
        public List<string> EndPoints { get; set; } = new();

        /// <summary>
        /// Redis connection timeout in milliseconds.
        /// </summary>
        public int ConnectTimeout { get; set; } = 10;

        /// <summary>
        /// Redis synchronous operation timeout in milliseconds.
        /// </summary>
        public int SyncTimeout { get; set; } = 10;

        /// <summary>
        /// Distributed cache instance name or key prefix.
        /// For example: agent:context:.
        /// </summary>
        public string DistributedName { get; set; } = "agent:context:";

        /// <summary>
        /// Username for Redis authentication.
        /// </summary>
        public string? User = null;

        /// <summary>
        /// Password for Redis authentication.
        /// </summary>
        public string? Password = null;
    }
}
