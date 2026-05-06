using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace EasyCore.Agent.ContextStore
{
    public sealed class MemoryAgentContextStore : IAgentContextStore
    {
        private readonly AgentConfigOptions _options;

        private static readonly ConcurrentDictionary<string, List<ChatMessage>> Store = new();

        public MemoryAgentContextStore(IOptions<AgentConfigOptions> options)
        {
            _options = options.Value;
        }

        public IList<ChatMessage> GetAsync(string sessionId)
        {
            var messages = Store.GetOrAdd(sessionId, _ => new List<ChatMessage>());

            lock (messages)
            {
                return messages.ToList();
            }
        }

        public void SaveAsync(string sessionId, IList<ChatMessage> messages)
        {
            var list = messages
                .TakeLast(_options.MaxContextCount)
                .ToList();

            Store.AddOrUpdate(
                sessionId,
                _ => list,
                (_, oldList) =>
                {
                    lock (oldList)
                    {
                        oldList.Clear();
                        oldList.AddRange(list);
                        return oldList;
                    }
                });
        }

        public void ClearAsync(string sessionId)
        {
            Store.TryRemove(sessionId, out _);
        }
    }
}
