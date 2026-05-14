using Demo.Common.Agent;
using Demo.Common.Dto;
using EasyCore.Agent;
using Microsoft.AspNetCore.Mvc;

namespace Demo.EasyCore.Agent.Controllers;

/// <summary>
/// Standalone demo endpoints for EasyCore.Agent.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AgentController : ControllerBase
{
    private readonly DeepSeekAgent _deepSeekAgent;
    private readonly QianwenAgent _qianwenAgent;
    private readonly IAIToolProvider _toolProvider;

    public AgentController(
        DeepSeekAgent deepSeekAgent,
        QianwenAgent qianwenAgent,
        IAIToolProvider toolProvider)
    {
        _deepSeekAgent = deepSeekAgent;
        _qianwenAgent = qianwenAgent;
        _toolProvider = toolProvider;
    }

    /// <summary>
    /// Single-turn chat demo.
    /// </summary>
    [HttpGet("chat")]
    public async Task<string> Chat([FromQuery] ChatRequest request)
    {
        var agent = _deepSeekAgent.CreateAgent("demo", "You are the EasyCore.Agent demo assistant. Answer concisely.");
        return await _deepSeekAgent.ChatRunAsync(request.SessionId, agent, request.Message);
    }

    /// <summary>
    /// Chat demo with registered AI tools.
    /// </summary>
    [HttpGet("chat-with-tools")]
    public async Task<string> ChatWithTools([FromQuery] ChatWithToolsRequest request)
    {
        var tools = _toolProvider.GetTools();
        var agent = _deepSeekAgent.CreateAgent(
            "tool-demo",
            "You can call tools to fetch weather information.",
            tools);

        return await _deepSeekAgent.ChatRunAsync(request.SessionId, agent, request.Message);
    }

    /// <summary>
    /// Embedding generation demo (Qwen Embedding).
    /// </summary>
    [HttpGet("embedding")]
    public async Task<object> Embedding([FromQuery] EmbeddingRequest request)
    {
        var vector = await _qianwenAgent.EmbedAsync(request.Text);
        return new
        {
            text = request.Text,
            dimension = vector.Length,
            preview = vector.Take(8).ToArray()
        };
    }

    /// <summary>
    /// Returns the stored chat context for a session.
    /// </summary>
    [HttpGet("context")]
    public object GetContext([FromQuery] SessionRequest request)
    {
        return _deepSeekAgent.GetChatContext(request.SessionId);
    }
}
