using Demo.Common;
using Demo.Common.Agent;
using Demo.Common.Dto;
using EasyCore.Agent.RAG;
using Microsoft.AspNetCore.Mvc;

namespace Demo.EasyCore.Agent.RAG.Controllers;

/// <summary>
/// Standalone demo endpoints for EasyCore.Agent.RAG.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class RagController : ControllerBase
{
    private readonly DeepSeekAgent _deepSeekAgent;

    public RagController(DeepSeekAgent deepSeekAgent)
    {
        _deepSeekAgent = deepSeekAgent;
    }

    /// <summary>
    /// Document chunking demo.
    /// </summary>
    [HttpGet("chunk")]
    public List<DocumentChunk> Chunk([FromQuery] ChunkRequest request)
    {
        return DocumentChunker.Chunk(
            DemoDocument.Content,
            DemoDocument.DocumentId,
            request.ChunkSize,
            request.Overlap);
    }

    /// <summary>
    /// Query rewrite demo (requires DeepSeek ApiKey).
    /// </summary>
    [HttpGet("rewrite")]
    public async Task<string> Rewrite([FromQuery] RewriteRequest request)
    {
        var agent = _deepSeekAgent.CreateAgent("You are the RAG demo assistant.");
        await _deepSeekAgent.ChatRunAsync(
            request.SessionId,
            agent,
            "EasyCore.Agent supports Qdrant, Milvus, PostgreSQL, Redis, and Elasticsearch vector stores.");
        var history = _deepSeekAgent.GetChatContext(request.SessionId);
        return await QueryRewrite.RewriteAsync(request.Message, agent, history);
    }

    /// <summary>
    /// Multi-query generation demo.
    /// </summary>
    [HttpGet("multi-query")]
    public async Task<List<string>> MultiQuery([FromQuery] MultiQueryRequest request)
    {
        var agent = _deepSeekAgent.CreateAgent();
        return await MultiQueryGenerator.GenerateAsync(request.Message, agent, 3);
    }

    /// <summary>
    /// MMR candidate selection demo using mock scores.
    /// </summary>
    [HttpGet("mmr")]
    public List<MmrCandidate> Mmr([FromQuery] MmrRequest request)
    {
        var chunks = DocumentChunker.Chunk(DemoDocument.Content, DemoDocument.DocumentId, 80, 20);
        var candidates = chunks.Select((chunk, index) => new MmrCandidate
        {
            Id = chunk.Index.ToString(),
            Content = chunk.Content,
            Score = 1.0f - index * 0.05f,
            Vector = Enumerable.Repeat((float)index * 0.01f, 8).ToArray()
        }).ToList();

        return MmrSelector.Select(candidates, request.TopK, 0.7);
    }
}
