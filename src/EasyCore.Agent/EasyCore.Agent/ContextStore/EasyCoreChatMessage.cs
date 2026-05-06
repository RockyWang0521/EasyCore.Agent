using Microsoft.Extensions.AI;

namespace EasyCore.Agent.ContextStore
{
    public class EasyCoreChatMessage
    {
        public string? AuthorName { get; set; }

        public ChatRole Role { get; set; }

        public string? MessageId { get; set; }

        public string? Text { get; set; }
    }
}
