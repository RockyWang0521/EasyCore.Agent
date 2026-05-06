namespace EasyCore.Agent
{
    /// <summary>
    /// Defines where the Agent context is stored.
    /// </summary>
    public enum AgentContextStoreType
    {
        /// <summary>
        /// Store context in local memory.
        /// Suitable for single-instance applications.
        /// Not recommended in load-balanced environments.
        /// </summary>
        Memory = 1,

        /// <summary>
        /// Store context in Redis (distributed cache).
        /// Suitable for multi-instance and load-balanced environments.
        /// </summary>
        Redis = 2
    }
}
