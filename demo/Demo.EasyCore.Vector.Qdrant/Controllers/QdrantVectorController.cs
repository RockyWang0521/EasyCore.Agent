using Demo.Common;
using Demo.Common.Agent;
using Demo.Common.Dto;
using Demo.EasyCore.Vector.Qdrant.Entities;
using EasyCore.Agent.RAG;
using EasyCore.Vector.Qdrant;
using Microsoft.AspNetCore.Mvc;

namespace Demo.EasyCore.Vector.Qdrant.Controllers;

[Route("api/[controller]")]
[ApiController]
public class QdrantVectorController : ControllerBase
{
    private const string CollectionName = "demo_qdrant_collection";
    private const string VectorName = "documentVector";

    private readonly QianwenAgent _agent;
    private readonly IQdrantVectorStore _store;

    public QdrantVectorController(QianwenAgent agent, IQdrantVectorStore store)
    {
        _agent = agent;
        _store = store;
    }

    [HttpGet("upsert")]
    public async Task<object> Upsert()
    {
        await _store.CreateCollectionAsync(CollectionName, new QdrantVectorCollectionDefinition
        {
            VectorFields = { new QdrantVectorFieldDefinition { Name = VectorName, Dimension = 1024 } }
        });

        var chunks = DocumentChunker.Chunk(DemoDocument.Content, DemoDocument.DocumentId, 120, 30);
        var count = 0;

        foreach (var chunk in chunks)
        {
            var embedding = await _agent.EmbedAsync(chunk.Content);
            var record = new DemoTextVector
            {
                Id = Guid.NewGuid().ToString("N"),
                DocumentId = chunk.DocumentId,
                Index = chunk.Index,
                Content = chunk.Content
            };
            record.SetVector(VectorName, embedding);
            await _store.UpsertAsync(CollectionName, record);
            count++;
        }

        return new { collection = CollectionName, upserted = count };
    }

    [HttpGet("search")]
    public async Task<IReadOnlyList<QdrantQdrantVectorSearchResult<DemoTextVector>>> Search([FromQuery] VectorSearchRequest request)
    {
        var vector = await _agent.EmbedAsync(request.Query);
        return await _store.VectorSearchAsync<DemoTextVector>(
            CollectionName,
            VectorName,
            vector,
            new QdrantVectorSearchOptions { Limit = request.Limit, IncludeMetadata = true, ScoreThreshold = 0.5f });
    }

    [HttpGet("exists")]
    public Task<bool> Exists() => _store.CollectionExistsAsync(CollectionName);

    [HttpDelete("collection")]
    public async Task DeleteCollection() => await _store.DeleteCollectionAsync(CollectionName);
}
