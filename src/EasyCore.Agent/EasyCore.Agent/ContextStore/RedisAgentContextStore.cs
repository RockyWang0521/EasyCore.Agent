using EasyCore.Agent;
using EasyCore.Agent.ContextStore;
using EasyCore.DistributedCache.Cache;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

public sealed class RedisAgentContextStore : IAgentContextStore
{
    private readonly AgentConfigOptions _options;
    private readonly IDistributedCache _redisCache;

    public RedisAgentContextStore(
        IOptions<AgentConfigOptions> options,
        IDistributedCache redisCache)
    {
        _options = options.Value;
        _redisCache = redisCache;
    }

    public IList<ChatMessage> GetAsync(string sessionId)
    {
        var key = BuildKey(sessionId);

        var messages = _redisCache.Get<IList<EasyCoreChatMessage>>(key);

        if (messages == null || messages.Count == 0)
        {
            return new List<ChatMessage>();
        }

        return messages
            .Where(x => !string.IsNullOrWhiteSpace(x.Text))
            .Select(x => new ChatMessage(x.Role, x.Text!)
            {
                AuthorName = x.AuthorName,
                MessageId = x.MessageId
            })
            .ToList();
    }

    public void SaveAsync(string sessionId, IList<ChatMessage> messages)
    {
        var key = BuildKey(sessionId);

        var list = messages
            .TakeLast(_options.MaxContextCount)
            .Select(x => new EasyCoreChatMessage
            {
                AuthorName = x.AuthorName,
                Role = x.Role,
                MessageId = x.MessageId,
                Text = x.Text
            })
            .ToList();

        _redisCache.Set(key, list);
    }

    public void ClearAsync(string sessionId)
    {
        var key = BuildKey(sessionId);

        _redisCache.Remove(key);
    }

    private string BuildKey(string sessionId)
    {
        return $"{_options.DistributedName}{sessionId}";
    }
}