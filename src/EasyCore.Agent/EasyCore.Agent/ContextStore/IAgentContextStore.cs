namespace EasyCore.Agent.ContextStore
{
    public interface IAgentContextStore
    {
        IList<Microsoft.Extensions.AI.ChatMessage> GetAsync(string sessionId);

        void SaveAsync(string sessionId, IList<Microsoft.Extensions.AI.ChatMessage> messages);

        void ClearAsync(string sessionId);
    }
}
