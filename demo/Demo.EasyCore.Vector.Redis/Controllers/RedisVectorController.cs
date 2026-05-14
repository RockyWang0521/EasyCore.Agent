using Demo.Common;
using Demo.Common.Agent;
using Demo.Common.Dto;
using Demo.EasyCore.Vector.Redis.Entities;
using EasyCore.Agent.RAG;
using EasyCore.Vector.Redis;
using Microsoft.AspNetCore.Mvc;

namespace Demo.EasyCore.Vector.Redis.Controllers;

/// <summary>
/// Standalone demo endpoints for EasyCore.Vector.Redis.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class RedisVectorController : ControllerBase
{
    private const string CollectionName = "demo_redis_collection";
    private const string VectorName = "documentVector";

    private readonly QianwenAgent _agent;
    private readonly IRedisVectorStore _store;

    public RedisVectorController(QianwenAgent agent, IRedisVectorStore store)
    {
        _agent = agent;
        _store = store;
    }

    /// <summary>
    /// Creates the collection and upserts sample document vectors.
    /// </summary>
    [HttpGet("upsert")]
    public async Task<object> Upsert()
    {
        await _store.CreateCollectionAsync(CollectionName, new RedisVectorCollectionDefinition
        {
            ScalarFields =
            {
                new RedisScalarFieldDefinition { Name = "DocumentId", FieldType = ScalarFieldType.VarChar, MaxLength = 128 },
                new RedisScalarFieldDefinition { Name = "Index", FieldType = ScalarFieldType.Int64 }
            },
            VectorFields =
            {
                new RedisVectorFieldDefinition
                {
                    Name = VectorName,
                    Dimension = 1024,
                    MetricType = RedisSimilarityMetricType.Cosine
                }
            }
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

    /// <summary>
    /// Vector similarity search demo.
    /// </summary>
    [HttpGet("search")]
    public async Task<IReadOnlyList<RedisVectorSearchResult<DemoTextVector>>> Search([FromQuery] VectorSearchRequest request)
    {
        var vector = await _agent.EmbedAsync(request.Query);
        return await _store.VectorSearchAsync<DemoTextVector>(
            CollectionName,
            VectorName,
            vector,
            new RedisVectorSearchOptions
            {
                Limit = request.Limit,
                IncludeMetadata = true,
                ScoreThreshold = 0.5f,
                IncludeVector = true
            });
    }

    /// <summary>
    /// Checks whether the demo collection exists.
    /// </summary>
    [HttpGet("exists")]
    public Task<bool> Exists() => _store.CollectionExistsAsync(CollectionName);

    /// <summary>
    /// Deletes the demo collection.
    /// </summary>
    [HttpDelete("collection")]
    public async Task DeleteCollection()
    {
        await _store.DeleteCollectionAsync(CollectionName);
    }
}
