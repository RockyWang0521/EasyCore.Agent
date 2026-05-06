using EasyCore.Agent.ContextStore;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace EasyCore.Agent
{
    public abstract class BasicAgentClient<TOptions> where TOptions : AgentClientOptions
    {
        private readonly AgentClientOptions _options;
        private readonly IAgentContextStore _contextStore;

        public BasicAgentClient(IOptions<AgentClientOptions> options, IServiceProvider serviceProvider)
        {
            _options = options.Value;
            _contextStore = serviceProvider.GetRequiredService<IAgentContextStore>();
        }

        public AIAgent CreateAgent(string agentName, string instructions, IList<AITool>? tools = null)
        {
            var apiKey = NormalizeApiKey(_options.ApiKey);
            var baseUrl = NormalizeAsciiValue(_options.BaseUrl, "Agent BaseUrl");
            var model = NormalizeAsciiValue(_options.Model, "Agent Model");

            var client = new OpenAIClient(
                credential: new ApiKeyCredential(apiKey),
                options: new OpenAIClientOptions
                {
                    Endpoint = new Uri(baseUrl)
                });

            var chatClient = client.GetChatClient(model);

            return chatClient.AsAIAgent(
                name: agentName,
                tools: tools,
                instructions: instructions);
        }

        public AIAgent CreateAgent(string instructions, IList<AITool>? tools = null)
        {
            var apiKey = NormalizeApiKey(_options.ApiKey);
            var baseUrl = NormalizeAsciiValue(_options.BaseUrl, "Agent BaseUrl");
            var model = NormalizeAsciiValue(_options.Model, "Agent Model");

            var client = new OpenAIClient(
                credential: new ApiKeyCredential(apiKey),
                options: new OpenAIClientOptions
                {
                    Endpoint = new Uri(baseUrl)
                });

            var chatClient = client.GetChatClient(model);

            return chatClient.AsAIAgent(
                tools: tools,
                instructions: instructions);
        }

        public async Task<string> ChatRunAsync(string sessionId, AIAgent agent, string message, CancellationToken cancellationToken = default)
        {
            var messages = _contextStore.GetAsync(sessionId);

            messages.Add(new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, message));

            while (messages.Count > 20)
            {
                messages.RemoveAt(0);
            }

            var response = await agent.RunAsync(messages: messages, cancellationToken: cancellationToken);

            var answer = response.Text ?? string.Empty;

            messages.Add(new Microsoft.Extensions.AI.ChatMessage(ChatRole.Assistant, answer));

            _contextStore.SaveAsync(sessionId, messages);

            return answer;
        }

        public async Task<string> ChatRunAsync(AIAgent agent, string message, CancellationToken cancellationToken = default)
        {
            var response = await agent.RunAsync(message: message, cancellationToken: cancellationToken);

            var answer = response.Text ?? string.Empty;

            return answer;
        }

        public void ClearChatContext(string sessionId) => _contextStore.ClearAsync(sessionId);

        private string NormalizeApiKey(string? apiKey)
        {
            var value = NormalizeAsciiValue(apiKey, "Agent ApiKey");

            if (value.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                value = value["Bearer ".Length..].Trim();
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException("Agent ApiKey is not configured.");
            }

            return value;
        }

        private string NormalizeAsciiValue(string? value, string name)
        {
            var text = (value ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new InvalidOperationException($"{name} is not configured.");
            }

            var invalidChars = text
                .Where(c => c > 127 || char.IsControl(c))
                .Select(c => $"U+{(int)c:X4}('{DisplayChar(c)}')")
                .Distinct()
                .ToArray();

            if (invalidChars.Length > 0)
            {
                throw new InvalidOperationException(
                    $"{name} contains invalid characters: {string.Join(", ", invalidChars)}. " +
                    $"Please re-enter it manually and avoid using non-ASCII characters.");
            }

            return text;
        }

        private string DisplayChar(char c)
        {
            return c switch
            {
                '\r' => "\\r",
                '\n' => "\\n",
                '\t' => "\\t",
                _ => c.ToString()
            };
        }
    }
}