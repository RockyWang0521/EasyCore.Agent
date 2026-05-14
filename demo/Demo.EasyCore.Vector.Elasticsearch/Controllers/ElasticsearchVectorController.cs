using Demo.Common;
using Demo.Common.Agent;
using Demo.Common.Dto;
using Demo.EasyCore.Vector.Elasticsearch.Entities;
using EasyCore.Agent.RAG;
using EasyCore.Vector.Elasticsearch;
using Microsoft.AspNetCore.Mvc;

namespace Demo.EasyCore.Vector.Elasticsearch.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ElasticsearchVectorController : ControllerBase
{
    private const string CollectionName = "demo_es_collection";
    private const string VectorName = "documentVector";

    private readonly QianwenAgent _agent;
    private readonly IElasticsearchVectorStore _store;

    public ElasticsearchVectorController(QianwenAgent agent, IElasticsearchVectorStore store)
    {
        _agent = agent;
        _store = store;
    }

    [HttpGet("upsert")]
    public async Task<object> Upsert()
    {
        await _store.CreateCollectionAsync(CollectionName, new ElasticsearchVectorCollectionDefinition
        {
            ScalarFields =
            {
                new ElasticsearchScalarFieldDefinition { Name = "DocumentId", FieldType = ScalarFieldType.VarChar, MaxLength = 128 },
                new ElasticsearchScalarFieldDefinition { Name = "Index", FieldType = ScalarFieldType.Int64 }
            },
            VectorFields =
            {
                new ElasticsearchVectorFieldDefinition
                {
                    Name = VectorName,
                    Dimension = 1024,
                    MetricType = ElasticsearchSimilarityMetricType.Cosine
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

    [HttpGet("search")]
    public async Task<IReadOnlyList<ElasticsearchVectorSearchResult<DemoTextVector>>> Search([FromQuery] VectorSearchRequest request)
    {
        var vector = await _agent.EmbedAsync(request.Query);
        return await _store.VectorSearchAsync<DemoTextVector>(
            CollectionName,
            VectorName,
            vector,
            new ElasticsearchVectorSearchOptions { Limit = request.Limit, IncludeMetadata = true, ScoreThreshold = 0.5f });
    }

    [HttpGet("exists")]
    public Task<bool> Exists() => _store.CollectionExistsAsync(CollectionName);

    [HttpDelete("collection")]
    public async Task DeleteCollection() => await _store.DeleteCollectionAsync(CollectionName);
}
