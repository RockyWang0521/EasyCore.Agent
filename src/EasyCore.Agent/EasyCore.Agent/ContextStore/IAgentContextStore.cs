namespace EasyCore.Agent.ContextStore
{
    public interface IAgentContextStore
    {
        IList<Microsoft.Extensions.AI.ChatMessage> Get(string sessionId);

        void Save(string sessionId, IList<Microsoft.Extensions.AI.ChatMessage> messages);

        void Clear(string sessionId);

        int GetMaxContextCount();
    }
}
